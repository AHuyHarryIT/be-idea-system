using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
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
		// GET: api/user
		[HttpGet]
		public async Task<IActionResult> GetUsers()
		{
			var users = await userService.GetAllUsersAsync();
			return Ok(new
			{
				Users = users,
				AvailableRoles = RoleConstants.GetAllRoles() 
			});
		}


		// POST: api/user
		[HttpPost]
		public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
		{
			
			var existingUser = await _userManager.FindByEmailAsync(request.Email);
			if (existingUser != null)
				return BadRequest(new { message = "This email address has already been used." });

			var user = new IdeaUser
			{
				UserName = request.Email,
				Email = request.Email,
				Name = request.Name,
				DepartmentId = request.DepartmentId 
			};

			// Tạo user với mật khẩu mặc định
			var result = await _userManager.CreateAsync(user, request.Password);
			if (!result.Succeeded)
			{
				var errors = result.Errors.Select(e => e.Description);
				return BadRequest(new { message = "Account creation failed.", errors });
			}


			var roleToAssign = string.IsNullOrWhiteSpace(request.Role) ? RoleConstants.Staff : request.Role;
			await _userManager.AddToRoleAsync(user, roleToAssign);

			return Ok(new { message = "Account created successfully." });
		}

		// PUT: api/users/{id}
		[HttpPut("{id}")]
		[Authorize(Roles = RoleConstants.Administrator)]
		public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromBody] UpdateUserRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Role))
				return BadRequest(new { message = "The role cannot be left blank." });

			if (string.IsNullOrWhiteSpace(request.Name))
				return BadRequest(new { message = "The name cannot be left blank." });

			try
			{
				// Gọi service mới
				var result = await userService.UpdateUserAsync(id, request);

				if (result)
				{
					return Ok(new { message = "User information updated successfully." });
				}

				return BadRequest(new { message = "Failed to update user (Unknown error)." });
			}
			catch (Exception ex)
			{
			
				return BadRequest(new { message = ex.Message });
			}
		}


		// DELETE: api/user/delete/{id}
		[HttpDelete("{id}")] 
		[Authorize(Roles = RoleConstants.Administrator)]
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