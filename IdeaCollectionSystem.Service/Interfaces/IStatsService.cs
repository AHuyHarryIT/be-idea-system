using IdeaCollectionSystem.Service.Models.DTOs;

namespace IdeaCollectionSystem.Service.Interfaces
{
	public interface IStatsService
	{
		Task<QaDashboardDto> GetDashboardStatsAsync();
		Task<IEnumerable<DepartmentStatDto>> GetDepartmentStatisticsAsync();
		Task<IEnumerable<IdeaInfoDto>> GetIdeasWithoutCommentsAsync();

		Task<List<DepartmentStatDto>> GetDepartmentStatsAsync(Guid? submissionId = null);

		Task<List<ReviewIdeaDto>> GetIdeasWithoutReviewsAsync(Guid? submissionId = null);
	}
}