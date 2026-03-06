using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using IdeaCollectionSystem.ApplicationCore.Entitites;

namespace IdeaCollectionSystem.Service.Services
{
	public class IdeaService : IIdeaService
	{
		private readonly IdeaCollectionDbContext _context;

		public IdeaService(IdeaCollectionDbContext context)
		{
			_context = context;
		}

		// Check closure date
		public async Task<bool> IsClosureDatePassedAsync()
		{
			var latestSubmission = await _context.Submissions
				.OrderByDescending(s => s.ClousureDate)
				.FirstOrDefaultAsync();

			if (latestSubmission == null) return false;

			return DateTime.UtcNow > latestSubmission.ClousureDate;
		}

		// Create idea
		public async Task<bool> CreateIdeaAsync(IdeaCreateDto dto, string userId)
		{
			if (!Guid.TryParse(userId, out var userGuid))
				return false;

			var user = await _context.Users
				.FirstOrDefaultAsync(u => u.Id == userGuid);

			if (user == null)
				return false;

			var idea = new Idea
			{
				//Tex = dto.Title,
				Text = dto.Text,
				CategoryId = dto.CategoryId,
				DepartmentId = user.DepartmentId, 
				UserId = userGuid,
				IsAnonymous = dto.IsAnonymous,
				CreatedAt = DateTime.UtcNow
			};

			await _context.Ideas.AddAsync(idea);
			await _context.SaveChangesAsync();   

			return true;
		}

		// Get ideas of current staff
		public async Task<IEnumerable<IdeaInfoDto>> GetIdeasByStaffAsync(string userId)
		{
			if (!Guid.TryParse(userId, out var userGuid))
				return Enumerable.Empty<IdeaInfoDto>();

			return await _context.Ideas
				.Include(i => i.Category)
				.Where(i => i.UserId == userGuid)
				.Select(i => new IdeaInfoDto
				{
					Id = i.Id.GetHashCode(), 
					CategoryName = i.Category != null ? i.Category.Name : "No Category",
					CreatedDate = i.CreatedAt,
					IsAnonymous = i.IsAnonymous
				})
				.ToListAsync();
		}

		// Get idea titles by user
		public async Task<string?> GetIdeasByUserAsync(string userIdClaim)
		{
			if (!Guid.TryParse(userIdClaim, out var userGuid))
				return null;

			var ideas = await _context.Ideas
				.Where(i => i.UserId == userGuid)
				.Select(i => i.Text)
				.ToListAsync();

			return ideas.Count == 0 ? null : string.Join(", ", ideas);
		}
	}
}