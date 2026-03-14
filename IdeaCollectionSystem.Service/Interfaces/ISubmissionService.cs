using IdeaCollectionSystem.Service.Models.DTOs;

namespace IdeaCollectionSystem.Service.Interfaces
{
	public interface ISubmissionService
	{
		Task<IEnumerable<SubmissionDto>> GetAllSubmissionsAsync();
		Task<bool> CreateSubmissionAsync(SubmissionCreateDto dto);
		Task<bool> UpdateSubmissionAsync(Guid id, SubmissionCreateDto dto);
	}
}