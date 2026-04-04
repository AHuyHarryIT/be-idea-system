using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using IdeaCollectionSystem.Service.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdeaCollectionSystem.Service.Services
{
	public class StatsService : IStatsService
	{
		private readonly IdeaCollectionDbContext _context;
		private readonly UserManager<IdeaUser> _userManager;

		public StatsService(IdeaCollectionDbContext context, UserManager<IdeaUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		// 1. Get stats dashboard
		public async Task<QaDashboardDto> GetDashboardStatsAsync()
		{
			return new QaDashboardDto
			{
				TotalIdeas = await _context.Ideas.CountAsync(),
				TotalCategories = await _context.Categories.CountAsync(),
				TotalDepartments = await _context.Departments.CountAsync(),
				TotalUsers = await _userManager.Users.CountAsync(),
				IdeasWithoutComments = await _context.Ideas.CountAsync(i => !i.Comments.Any()),
				IdeasThisMonth = await _context.Ideas
					.CountAsync(i => i.CreatedAt.Month == DateTime.UtcNow.Month
								  && i.CreatedAt.Year == DateTime.UtcNow.Year),

				// Đếm số lượng bài đang chờ duyệt
				TotalPendingIdeas = await _context.Ideas.CountAsync(i => i.ReviewStatus == ReviewStatus.PENDING)
			};
		}

		// 2. Get department statistics
		public async Task<IEnumerable<DepartmentStatDto>> GetDepartmentStatisticsAsync()
		{
			var totalIdeas = await _context.Ideas.CountAsync();

			return await _context.Departments
				.Select(d => new DepartmentStatDto
				{
					DepartmentName = d.Name,
					IdeaCount = d.Ideas.Count(),
					Percentage = totalIdeas > 0 ? Math.Round((double)d.Ideas.Count() / totalIdeas * 100, 1) : 0,
					ContributorCount = d.Ideas.Select(i => i.UserId).Distinct().Count()
				})
				.ToListAsync();
		}

		// 3. Get ideas without comments
		public async Task<IEnumerable<IdeaInfoDto>> GetIdeasWithoutCommentsAsync()
		{
			return await _context.Ideas
				.Include(i => i.Category)
				.Where(i => !i.Comments.Any())
				.Select(i => new IdeaInfoDto
				{
					Id = i.Id,
					Title = i.Title,
					CategoryName = i.Category != null ? i.Category.Name : "No Category",
					CreatedDate = i.CreatedAt,
					IsAnonymous = i.IsAnonymous
				})
				.ToListAsync();
		}

		// 4. Get department stats
		public async Task<List<DepartmentStatDto>> GetDepartmentStatsAsync(Guid? submissionId = null)
		{
			var ideasQuery = _context.Ideas.AsQueryable();
			if (submissionId.HasValue && submissionId.Value != Guid.Empty)
			{
				ideasQuery = ideasQuery.Where(i => i.SubmissionId == submissionId.Value);
			}

			var totalIdeas = await ideasQuery.CountAsync();
			var departments = await _context.Departments.ToListAsync();
			var stats = new List<DepartmentStatDto>();

			foreach (var dept in departments)
			{
				var deptIdeas = await ideasQuery.Where(i => i.DepartmentId == dept.Id).ToListAsync();
				int ideaCount = deptIdeas.Count;

				stats.Add(new DepartmentStatDto
				{
					DepartmentName = dept.Name,
					IdeaCount = ideaCount,
					Percentage = totalIdeas > 0 ? Math.Round((double)ideaCount / totalIdeas * 100, 1) : 0,
					ContributorCount = deptIdeas.Select(i => i.UserId).Distinct().Count()
				});
			}
			return stats;
		}

		// 5. THÊM HÀM NÀY ĐỂ TRẢ VỀ CHUẨN Task<List<ReviewIdeaDto>> THEO ĐÚNG INTERFACE
		public async Task<List<ReviewIdeaDto>> GetIdeasWithoutReviewsAsync(Guid? submissionId = null)
		{
			var query = _context.Ideas
				.Where(i => i.ReviewStatus == ReviewStatus.PENDING);

			if (submissionId.HasValue && submissionId.Value != Guid.Empty)
			{
				query = query.Where(i => i.SubmissionId == submissionId.Value);
			}

			return await query
				.OrderByDescending(i => i.CreatedAt)
				.Select(i => new ReviewIdeaDto
				{
					Status = i.ReviewStatus,
				
				})
				.ToListAsync();
		}
	}
}