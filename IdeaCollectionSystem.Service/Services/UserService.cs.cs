using IdeaCollectionSystem.ApplicationCore.Entitites;
using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace IdeaCollectionSystem.Service.Services
{
	public class UserService : IUserService
	{
		private readonly UserManager<IdeaUser> _userManager;
		private readonly IdeaCollectionDbContext _context;

		public UserService(UserManager<IdeaUser> userManager, IdeaCollectionDbContext context)
		{
			_userManager = userManager;
			_context = context;
		}

		// Get all users
		public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
		{

			var users = await _userManager.Users
				.Include(u => u.Department)
				.AsNoTracking() 
				.ToListAsync();

			var result = new List<UserDto>();

			foreach (var user in users)
			{
	
				var roles = await _userManager.GetRolesAsync(user);

				result.Add(new UserDto
				{
					Id = user.Id,
					Email = user.Email,
					Name = user.Name,
					DepartmentId = user.DepartmentId,
					DepartmentName = user.Department?.Name ?? "No Department",
					Role = roles.FirstOrDefault() ?? "Staff"
				});
			}

			return result;
		}

		// Update user information 
		public async Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request)
		{
			
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null) return false;

			if (!string.IsNullOrWhiteSpace(request.Name))
			{
				user.Name = request.Name;
			}
			user.DepartmentId = request.DepartmentId;

			var updateResult = await _userManager.UpdateAsync(user);
			if (!updateResult.Succeeded) return false;
			if (!string.IsNullOrWhiteSpace(request.Role))
			{

				var currentRoles = await _userManager.GetRolesAsync(user);

				if (currentRoles.Any())
				{
					var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
					if (!removeResult.Succeeded) return false;
				}

				var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
				return roleResult.Succeeded;
			}

			// Nếu sửa thông tin thành công mà không đổi Role thì vẫn trả về true
			return true;
		}

		// Delete user
		public async Task<bool> DeleteUserAsync(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null) return false;

			var result = await _userManager.DeleteAsync(user);
			return result.Succeeded;
		}
	}
}