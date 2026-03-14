using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdeaCollectionSystem.Service.Services
{
	public class UserService : IUserService
	{
		private readonly UserManager<IdeaUser> _userManager;

		public UserService(UserManager<IdeaUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
		{
			var users = await _userManager.Users.ToListAsync();
			var result = new List<UserDto>();

			foreach (var user in users)
			{
				var roles = await _userManager.GetRolesAsync(user);
				result.Add(new UserDto
				{
					Id = user.Id,
					Name = user.Name,
					Email = user.Email ?? "",
					Role = roles.FirstOrDefault() ?? "No Role",
					Avatar = user.Avatar
				});
			}

			return result;
		}

		public async Task<bool> UpdateUserRoleAsync(string userId, string newRole)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null) return false;

			var currentRoles = await _userManager.GetRolesAsync(user);
			await _userManager.RemoveFromRolesAsync(user, currentRoles);
			var result = await _userManager.AddToRoleAsync(user, newRole);
			return result.Succeeded;
		}

		public async Task<bool> DeleteUserAsync(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null) return false;

			var result = await _userManager.DeleteAsync(user);
			return result.Succeeded;
		}
	}
}