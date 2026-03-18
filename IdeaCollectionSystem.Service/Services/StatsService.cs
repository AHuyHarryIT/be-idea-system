using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
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
								  && i.CreatedAt.Year == DateTime.UtcNow.Year)
			};
		}

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

		public async Task<IEnumerable<IdeaInfoDto>> GetIdeasWithoutCommentsAsync()
		{
			return await _context.Ideas
				.Include(i => i.Category)
				.Where(i => !i.Comments.Any())
				.Select(i => new IdeaInfoDto
				{
					Id = i.Id,
					Text = i.Text,
					CategoryName = i.Category != null ? i.Category.Name : "No Category",
					CreatedDate = i.CreatedAt,
					IsAnonymous = i.IsAnonymous
				})
				.ToListAsync();
		}

		public async Task<List<DepartmentStatDto>> GetDepartmentStatsAsync(Guid? submissionId = null)
		{
			// 1. Lấy toàn bộ ý tưởng (có thể lọc theo năm học nếu muốn)
			var ideasQuery = _context.Ideas.AsQueryable();
			if (submissionId.HasValue && submissionId.Value != Guid.Empty)
			{
				ideasQuery = ideasQuery.Where(i => i.SubmissionId == submissionId.Value);
			}

			// 2. Đếm TỔNG SỐ ý tưởng của toàn trường để làm mẫu số tính %
			var totalIdeas = await ideasQuery.CountAsync();

			var departments = await _context.Departments.ToListAsync();
			var stats = new List<DepartmentStatDto>();

			// 3. Vòng lặp tính toán cho TỪNG phòng ban
			foreach (var dept in departments)
			{
				var deptIdeas = await ideasQuery.Where(i => i.DepartmentId == dept.Id).ToListAsync();

				// Số lượng ý tưởng của khoa này
				int ideaCount = deptIdeas.Count;

				// Tính % (Tránh lỗi chia cho 0)
				double percentage = totalIdeas > 0
					? Math.Round((double)ideaCount / totalIdeas * 100, 2)
					: 0;

				// Đếm số người tham gia (Dùng Distinct để 1 người nộp 10 bài vẫn chỉ tính là 1 người)
				int contributorCount = deptIdeas.Select(i => i.UserId).Distinct().Count();

				stats.Add(new DepartmentStatDto
				{
					DepartmentName = dept.Name,
					IdeaCount = ideaCount,
					Percentage = percentage,
					ContributorCount = contributorCount
				});
			}

			return stats;
		}
	}
}