using IdeaCollectionSystem.Service.Models.DTOs;

namespace IdeaCollectionSystem.Service.Interfaces
{
	public interface IIdeaService
	{
		Task<bool> CreateIdeaAsync(IdeaCreateDto dto, string userId);
		Task<IEnumerable<IdeaInfoDto>> GetIdeasByStaffAsync(string userId);
		Task<IEnumerable<IdeaInfoDto>> GetAllIdeasAsync();
		Task<IEnumerable<IdeaInfoDto>> GetIdeasByDepartmentAsync(string userId);
		Task<string?> GetIdeasByUserAsync(string userIdClaim);
		Task<bool> IsClosureDatePassedAsync();
		Task<bool> VoteIdeaAsync(int ideaId, string userId, bool isThumbsUp);
		Task<IdeaInfoDto?> GetIdeaDetailAsync(int ideaId);
	}
}