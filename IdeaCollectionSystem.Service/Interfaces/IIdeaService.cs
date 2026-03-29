using IdeaCollectionSystem.Service.Models.DTOs;

namespace IdeaCollectionSystem.Service.Interfaces
{
	public interface IIdeaService
	{
		Task<bool> IsFinalClosureDatePassedAsync(Guid ideaId);
		//Task<IdeaInfoDto?> GetIdeaDetailAsync(Guid ideaId, string userId); 
		//Task<bool> AddCommentAsync(Guid ideaId, string userId, string text, bool isAnonymous);
		Task<bool> CreateIdeaAsync(IdeaCreateDto dto, string userId);
		Task<IEnumerable<IdeaInfoDto>> GetIdeasByStaffAsync(string userId);
		//Task<IEnumerable<IdeaInfoDto>> GetAllIdeasAsync();
		Task<IEnumerable<IdeaInfoDto>> GetIdeasByDepartmentAsync(string userId);
		Task<string?> GetIdeasByUserAsync(string userIdClaim);
		Task<bool> IsClosureDatePassedAsync();
		Task<bool> VoteIdeaAsync(Guid ideaId, string userId, bool isThumbsUp);

		Task<IEnumerable<IdeaInfoDto>> GetIdeasWithoutCommentsAsync();

		Task<PagedResult<IdeaInfoDto>> GetIdeasPagedAsync(IdeaQueryParameters parameters, string userId, bool isManager = false);

		Task<bool> CreateCommentAsync(CommentCreateDto dto, string userId);

		Task<bool> ReviewIdeaAsync(Guid ideaId, ReviewIdeaDto dto, string reviewerId);

		Task<PagedResult<IdeaInfoDto>> GetMyIdeasPagedAsync(IdeaQueryParameters parameters, string userId);


	}
}