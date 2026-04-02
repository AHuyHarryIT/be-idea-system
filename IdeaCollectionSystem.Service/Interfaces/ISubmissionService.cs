using IdeaCollectionSystem.Service.Models.DTOs;

namespace IdeaCollectionSystem.Service.Interfaces
{
	public interface ISubmissionService
	{
		Task<PagedResult<SubmissionDto>> GetSubmissionsPagedAsync(PaginationFilter filter);
		Task<bool> CreateSubmissionAsync(SubmissionCreateDto dto);
		Task<bool> UpdateSubmissionAsync(Guid id, SubmissionCreateDto dto);
		Task<(bool Success, string Message)> DeleteSubmissionAsync(Guid id);
	}
}