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

		// Update user role
		public async Task<bool> UpdateUserRoleAsync(string userId, string newRole)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null) return false;

			var currentRoles = await _userManager.GetRolesAsync(user);
			await _userManager.RemoveFromRolesAsync(user, currentRoles);
			var result = await _userManager.AddToRoleAsync(user, newRole);
			return result.Succeeded;
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