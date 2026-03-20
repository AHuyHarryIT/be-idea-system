using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers
{
	[Route("api/[controller]")]
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

		//  4. Delete
		// DELETE: api/user/{userId}
		[HttpDelete("{userId}")]
		public async Task<IActionResult> DeleteUser([FromRoute] string userId)
		{
			await userService.DeleteUserAsync(userId);
			return Ok(new { message = "Đã xóa tài khoản." });
		}
	}

}