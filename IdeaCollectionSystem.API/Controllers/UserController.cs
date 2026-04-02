using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using IdeaCollectionSystem.ApplicationCore.Entitites; // Cần đảm bảo có using entity chứa IdeaUser
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdeaCollectionSystem.API.Controllers
{
	[Route("api/users")]
	[ApiController]
	[Authorize(Roles = RoleConstants.Administrator)]
	public class UserController : ControllerBase
	{
		private readonly IUserService userService;
		private readonly UserManager<IdeaUser> _userManager;

		public UserController(
			IUserService qaService,
			UserManager<IdeaUser> userManager)
		{
			userService = qaService;
			_userManager = userManager;
		}

		#region HELPER METHOD
		private string MapRoleFromFrontend(string frontendRole)
		{
			if (string.IsNullOrWhiteSpace(frontendRole)) return "";

			var role = frontendRole.Trim().ToUpper();

			if (role == "QA COORDINATOR") return RoleConstants.QACoordinator;
			if (role == "QA MANAGER") return RoleConstants.QAManager;
			if (role == "ADMINISTRATOR" || role == "SYSTEM ADMINISTRATOR") return RoleConstants.Administrator;
			if (role == "STAFF") return RoleConstants.Staff;

			return frontendRole;
		}
		#endregion


		[HttpGet]
		public async Task<IActionResult> GetUsers([FromQuery] PaginationFilter filter)
		{
			var pagedData = await userService.GetAllUsersAsync(filter);

			return Ok(new
			{
				Users = pagedData.Items,
				Pagination = new
				{
					pagedData.TotalCount,
					pagedData.PageNumber,
					pagedData.PageSize,
					pagedData.TotalPages,
					pagedData.HasPreviousPage,
					pagedData.HasNextPage
				},
				AvailableRoles = RoleConstants.GetAllRoles()
			});
		}

		// POST: api/users
		[HttpPost]
		public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
		{
			var rawRole = string.IsNullOrWhiteSpace(request.Role) ? RoleConstants.Staff : request.Role;
			var roleToAssign = MapRoleFromFrontend(rawRole);

			if (!RoleConstants.GetAllRoles().Contains(roleToAssign))
			{
				return BadRequest(new { message = $"Invalid role provided: '{request.Role}'." });
			}

			try
			{
				var newUserId = await userService.CreateUserAsync(request, roleToAssign);
				return Ok(new { message = "Account created successfully.", id = newUserId });
			}
			catch (Exception ex)
			{
				
				return BadRequest(new { message = ex.Message });
			}
		}
		// PUT: api/users/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromBody] UpdateUserRequest request)
		{
			if (!string.IsNullOrWhiteSpace(request.Role))
			{
				request.Role = MapRoleFromFrontend(request.Role);

				if (!RoleConstants.GetAllRoles().Contains(request.Role))
				{
					return BadRequest(new { message = $"Role '{request.Role}' does not exist." });
				}
			}

			try
			{
				var result = await userService.UpdateUserAsync(id, request);

				if (result)
				{
					return Ok(new { message = "User information updated successfully." });
				}

				return BadRequest(new { message = "Failed to update user." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// DELETE: api/users/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUser(string id)
		{
			var currentLoggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (currentLoggedInUserId == id)
			{
				return BadRequest(new
				{
					message = "Action denied! As an Administrator, you cannot delete your own account."
				});
			}

			var userToDelete = await _userManager.FindByIdAsync(id);
			if (userToDelete == null)
			{
				return NotFound(new { message = "User not found in the system." });
			}

			var isTargetUserAdmin = await _userManager.IsInRoleAsync(userToDelete, RoleConstants.Administrator);
			if (isTargetUserAdmin)
			{
				return BadRequest(new
				{
					message = "Action denied! The target account is also an Administrator. You are not allowed to delete them."
				});
			}

			var result = await _userManager.DeleteAsync(userToDelete);

			if (result.Succeeded)
			{
				return Ok(new { message = $"Successfully deleted user: {userToDelete.Email}" });
			}
			return BadRequest(new { message = "An error occurred while deleting this user's data. They might have related records in the system." });
		}
	}
}