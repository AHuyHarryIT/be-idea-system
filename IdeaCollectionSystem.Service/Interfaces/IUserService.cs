using IdeaCollectionSystem.Service.Models.DTOs;

namespace IdeaCollectionSystem.Service.Interfaces
{
	public interface IUserService
	{

		Task<PagedResult<UserDto>> GetAllUsersAsync(PaginationFilter filter);

		Task<string> CreateUserAsync(CreateUserRequest request, string roleToAssign);

		Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request);
		Task<bool> DeleteUserAsync(string userId);

	}
}