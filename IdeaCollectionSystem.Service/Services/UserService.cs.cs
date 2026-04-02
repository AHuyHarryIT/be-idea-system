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

		// GetAllUsersAsync 
		public async Task<PagedResult<UserDto>> GetAllUsersAsync(PaginationFilter filter)
		{
			var query = _userManager.Users
				.Include(u => u.Department)
				.AsNoTracking();


			if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
			{
				var search = filter.SearchTerm.ToLower().Trim();
				query = query.Where(u => u.Name.ToLower().Contains(search) || u.Email.ToLower().Contains(search));
			}

			var totalCount = await query.CountAsync();

			//  Skip và Take
			var users = await query
				.OrderByDescending(u => u.Id) 
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
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
			return new PagedResult<UserDto>(result, totalCount, filter.PageNumber, filter.PageSize);
		}


		// Create user
		public async Task<string> CreateUserAsync(CreateUserRequest request, string roleToAssign)
		{
			var existingUser = await _userManager.FindByEmailAsync(request.Email);
			if (existingUser != null)
				throw new Exception("This email address has already been used.");

			var user = new IdeaUser
			{
				UserName = request.Email,
				Email = request.Email,
				Name = request.Name,
				DepartmentId = request.DepartmentId
			};


			var result = await _userManager.CreateAsync(user, request.Password);
			if (!result.Succeeded)
			{
				var errors = string.Join(", ", result.Errors.Select(e => e.Description));
				throw new Exception($"Account creation failed. Errors: {errors}");
			}

			await _userManager.AddToRoleAsync(user, roleToAssign);

			return user.Id;
		}

		// Update user 
		public async Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				throw new Exception("User not found in the system.");


			if (!string.IsNullOrWhiteSpace(request.Name))
			{
				user.Name = request.Name;
			}

			if (request.DepartmentId.HasValue && request.DepartmentId.Value != Guid.Empty)
			{
				user.DepartmentId = request.DepartmentId.Value;
			}

			var updateResult = await _userManager.UpdateAsync(user);
			if (!updateResult.Succeeded)
			{
				var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
				throw new Exception($"Failed to update user data: {errors}");
			}

			
			if (!string.IsNullOrWhiteSpace(request.Role))
			{
				var currentRoles = await _userManager.GetRolesAsync(user);

				if (currentRoles.Any())
				{
					await _userManager.RemoveFromRolesAsync(user, currentRoles);
				}

				var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
				if (!roleResult.Succeeded)
				{
					var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
					throw new Exception($"Failed to update user role. Errors: {errors}");
				}
			}

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