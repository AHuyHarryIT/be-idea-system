using IdeaCollectionSystem.Service.Models.DTOs;

namespace IdeaCollectionSystem.Service.Interfaces
{
	public interface IQAManagerService
	{
		Task<QaDashboardDto> GetDashboardStatsAsync();
		Task<IEnumerable<DepartmentStatDto>> GetDepartmentStatisticsAsync();
		Task<byte[]> ExportIdeasToCsvAsync();
		Task<byte[]> ExportDocumentsToZipAsync();
		Task<IEnumerable<IdeaInfoDto>> GetIdeasWithoutCommentsAsync();
		// Closure dates
		Task<IEnumerable<SubmissionDto>> GetAllSubmissionsAsync();
		Task<bool> CreateSubmissionAsync(SubmissionCreateDto dto);
		Task<bool> UpdateSubmissionAsync(Guid id, SubmissionCreateDto dto);
		// Users (Admin only - but interface shared)
		Task<IEnumerable<UserDto>> GetAllUsersAsync();
		Task<bool> UpdateUserRoleAsync(string userId, string newRole);
		Task<bool> DeleteUserAsync(string userId);
	}
}