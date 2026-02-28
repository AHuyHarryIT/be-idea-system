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

		public async Task<bool> IsClosureDatePassedAsync()
		{
			
			var latestSubmission = await _context.Submissions
				.OrderByDescending(s => s.ClousureDate)
				.FirstOrDefaultAsync();

			if (latestSubmission == null) return false;

			return DateTime.UtcNow > latestSubmission.ClousureDate;
		}

		public async Task<bool> CreateIdeaAsync(IdeaCreateDto dto, string userId)
		{
			
			if (await IsClosureDatePassedAsync()) return false;

			if (!Guid.TryParse(userId, out var userGuid)) return false;

			var newIdea = new Idea
			{
				Text = dto.Title,
				UserId = userGuid,  
				CategoryId = dto.CategoryId,
				IsAnonymous = dto.IsAnonymous,
				CreatedAt = DateTime.UtcNow
			};

			_context.Ideas.Add(newIdea);
			return await _context.SaveChangesAsync() > 0;
		}


		public async Task<IEnumerable<IdeaInfoDto>> GetIdeasByStaffAsync(string userId)
		{
			
			if (!Guid.TryParse(userId, out var userGuid))
			{
				return Enumerable.Empty<IdeaInfoDto>();
			}

			return await _context.Ideas
				.Where(i => i.UserId == userGuid)
				.Select(i => new IdeaInfoDto
				{
					Id = 0, 
					Title = i.Text,
					CategoryName = i.Category.Name,
					CreatedDate = i.CreatedAt,
					IsAnonymous = i.IsAnonymous
				})
				.ToListAsync();
		}

		public async Task<string?> GetIdeasByUserAsync(string userIdClaim)
		{
			if (!Guid.TryParse(userIdClaim, out var userGuid))
			{
				return null;
			}

			var ideas = await _context.Ideas
				.Where(i => i.UserId == userGuid)
				.Select(i => i.Text)
				.ToListAsync();

			return ideas.Count == 0 ? null : string.Join(", ", ideas);
		}

	}
}