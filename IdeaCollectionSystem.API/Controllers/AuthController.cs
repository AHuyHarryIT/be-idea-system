using IdeaCollectionSystem.ApplicationCore.Entitites;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdeaCollectionSystem.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly UserManager<IdeaUser> _userManager;
		private readonly IConfiguration _config;
		private readonly IEmailService _emailService;
		private readonly IdeaCollectionIdentityDbContext _context;


		public AuthController(
			UserManager<IdeaUser> userManager,
			IConfiguration config,
			IEmailService emailService,
			IdeaCollectionIdentityDbContext context)
		{
			_userManager = userManager;
			_config = config;
			_emailService = emailService;
			_context = context;
		}

		// POST: api/auth/login
		[AllowAnonymous]
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
		{
	
			if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
			{
				return BadRequest(new { message = "Email and password are required fields." });
			}

			var user = await _userManager.FindByEmailAsync(request.Email);
			if (user == null)
				return Unauthorized(new { message = "Email or password not match." });


			var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
			if (!isPasswordValid)
				return Unauthorized(new { message = "Email or password not match." });

			var roles = await _userManager.GetRolesAsync(user);
			var singleRole = roles.FirstOrDefault() ?? "";

			var accessToken = GenerateJwtToken(user, roles);
			var deptName = await _context.Departments
					.Where(d => d.Id == user.DepartmentId)
					.Select(d => d.Name)
					.FirstOrDefaultAsync();

			return Ok(new
			{
				access_token = accessToken,
				user = new
				{
					id = user.Id,
					email = user.Email,
					name = user.Name,
					departmentId = user.DepartmentId,
					departmentName = deptName,
					role = singleRole
				}
			});
		}

		private string GenerateJwtToken(IdeaUser user, IList<string> roles)
		{
			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Id),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(ClaimTypes.Email, user.Email!),
				new Claim(ClaimTypes.Name, user.Name ?? "")
			};


			if (user.DepartmentId.HasValue)
			{
				claims.Add(new Claim("DepartmentId", user.DepartmentId.Value.ToString()));
			}

			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


			var expireMinutes = Convert.ToDouble(_config["Jwt:ExpiresInMinutes"] ?? "120");
			var expires = DateTime.UtcNow.AddMinutes(expireMinutes);

			var token = new JwtSecurityToken(
				issuer: _config["Jwt:Issuer"],
				audience: _config["Jwt:Audience"],
				claims: claims,
				expires: expires,
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		
		// POST: api/auth/change-password
		[HttpPost("change-password")]
		[Authorize] 
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
		{
			// 1. Validate DTO
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}


			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { message = "You must be logged in to perform this action." });
			}

			// 3. Tìm User trong Database
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound(new { message = "User account not found." });
			}

		
			var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

			if (!result.Succeeded)
			{
			
				var errors = result.Errors.Select(e => e.Description);
				return BadRequest(new
				{
					message = "Failed to change password. Please check your current password.",
					errors = errors
				});
			}

			return Ok(new { message = "Password has been changed successfully!" });
		}
	}
}