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
				AvailableRoles = RoleConstants.GetAllRoles() // Gửi kèm list Role cho FE làm dropdown
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
				DepartmentId = request.DepartmentId // phân khoa cho Staff/QA
			};

			// Tạo user với mật khẩu mặc định
			var result = await _userManager.CreateAsync(user, request.Password);
			if (!result.Succeeded)
			{
				var errors = result.Errors.Select(e => e.Description);
				return BadRequest(new { message = "Account creation failed.", errors });
			}

			// Gán Role cho user (Nếu Admin không chọn, mặc định là Staff)
			var roleToAssign = string.IsNullOrWhiteSpace(request.Role) ? RoleConstants.Staff : request.Role;
			await _userManager.AddToRoleAsync(user, roleToAssign);

			return Ok(new { message = "Account created successfully." });
		}

	
		// PUT: api/user/{userId}/role
		[HttpPut("{userId}/role")]
		public async Task<IActionResult> UpdateUserRole([FromRoute] string userId, [FromBody] UpdateRoleRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Role))
				return BadRequest(new { message = "The role cannot be left blank." });

			await userService.UpdateUserRoleAsync(userId, request.Role);
			return Ok(new { message = "Permissions successfully updated." });
		}


		// DELETE: api/user/delete/{id}
		[HttpDelete("delete/{id}")]
		// 1. Ensure only Administrators can access this endpoint
		[Authorize(Roles = RoleConstants.Administrator)]
		public async Task<IActionResult> DeleteUser(string id)
		{
			// 2. Get the ID of the CURRENTLY LOGGED IN Admin (the one making the request)
			var currentLoggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			// 3. VALIDATE 1: PREVENT SELF-DELETION
			if (currentLoggedInUserId == id)
			{
				return BadRequest(new
				{
					message = "Action denied! As an Administrator, you cannot delete your own account."
				});
			}

			// Find the target user to be deleted
			var userToDelete = await _userManager.FindByIdAsync(id);
			if (userToDelete == null)
			{
				return NotFound(new { message = "User not found in the system." });
			}

			// 4. VALIDATE 2: PREVENT ADMIN FROM DELETING ANOTHER ADMIN
			// Since Admin is the highest role, they are typically not allowed to delete each other to prevent internal conflicts.
			var isTargetUserAdmin = await _userManager.IsInRoleAsync(userToDelete, RoleConstants.Administrator);
			if (isTargetUserAdmin)
			{
				return BadRequest(new
				{
					message = "Action denied! The target account is also an Administrator. You are not allowed to delete them."
				});
			}

			// 5. EXECUTE DELETE (For Staff, QA Coordinator, QA Manager...)
			var result = await _userManager.DeleteAsync(userToDelete);

			if (result.Succeeded)
			{
				return Ok(new { message = $"Successfully deleted user: {userToDelete.Email}" });
			}

			// Return error if DB deletion fails (e.g., due to foreign key constraints)
			return BadRequest(new { message = "An error occurred while deleting this user's data. They might have related records in the system." });
		}
	}

}