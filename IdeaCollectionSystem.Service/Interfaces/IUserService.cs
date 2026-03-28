using IdeaCollectionSystem.Service.Models.DTOs;

namespace IdeaCollectionSystem.Service.Interfaces
{
	public interface IUserService
	{
		Task<IEnumerable<UserDto>> GetAllUsersAsync();
		Task<bool> UpdateUserRoleAsync(string userId, string newRole);
        Task<bool> UpdateUserDepartmentAsync(string userId, Guid newDepartmentId);

        Task<bool> DeleteUserAsync(string userId);
	}
}