using IdeaCollectionSystem.Service.Models;
using IdeaCollectionSystem.Service.Models.DTOs;

namespace IdeaCollectionSystem.Service.Interfaces
{
	public interface IQAManagerService
	{
		Task<QADashboardDto> GetDashboardStatsAsync();

		Task<IEnumerable<DepartmentStatDto>> GetDepartmentStatisticsAsync();

		Task<byte[]> ExportIdeasToCsvAsync();

		Task<byte[]> ExportDocumentsToZipAsync();

		Task<IEnumerable<IdeaInfoDto>> GetIdeasWithoutCommentsAsync();
	}
}