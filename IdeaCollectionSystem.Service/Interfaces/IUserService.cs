using IdeaCollectionSystem.Service.Models.DTOs;

namespace IdeaCollectionSystem.Service.Interfaces
{
	public interface IUserService
	{
		Task<IEnumerable<UserDto>> GetAllUsersAsync();
		Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request);
		Task<bool> DeleteUserAsync(string userId);

	}
}