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
	}
}