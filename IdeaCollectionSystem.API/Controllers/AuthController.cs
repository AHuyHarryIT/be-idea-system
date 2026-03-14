using IdeaCollectionIdea.Common.Constants; // Để lấy RoleConstants
using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;
using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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


		public AuthController(
			UserManager<IdeaUser> userManager,
			IConfiguration config,
			IEmailService emailService)
		{
			_userManager = userManager;
			_config = config;
			_emailService = emailService;
		}

		// POST: api/auth/login
		[AllowAnonymous]
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
		{
			var user = await _userManager.FindByEmailAsync(request.Email);
			if (user == null)
				return Unauthorized(new { message = "Email or password not match." });


			var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
			if (!isPasswordValid)
				return Unauthorized(new { message = "Email or password not match." });
			var roles = await _userManager.GetRolesAsync(user);

			var accessToken = GenerateJwtToken(user, roles);

			return Ok(new
			{
				access_token = accessToken,
				user = new
				{
					id = user.Id,
					email = user.Email,
					name = user.Name,
					departmentId = user.DepartmentId,
					roles = roles
				}
			});
		}

		//// POST: api/auth/register
		//[AllowAnonymous]
		//[HttpPost("register")]
		//public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
		//{
		//	var user = new IdeaUser
		//	{
		//		UserName = request.Email,
		//		Email = request.Email,
		//		Name = request.Name
		//	};

		//	var result = await _userManager.CreateAsync(user, request.Password);
		//	if (!result.Succeeded)
		//	{
		//		var errors = result.Errors.Select(e => e.Description);
		//		return BadRequest(new { message = "Đăng ký thất bại", errors });
		//	}


		//	await _userManager.AddToRoleAsync(user, RoleConstants.Staff);

		//	return Ok(new { message = "Đăng ký thành công. Vui lòng đăng nhập." });
		//}


		//  GenerateJwtToken
		private string GenerateJwtToken(IdeaUser user, IList<string> roles)
		{
			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Id), // ID chuẩn của JWT
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Mã chống lặp token
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

		//// Replace _configuration with _config in TestEmail method
		//[HttpGet("test-email")]
		//[AllowAnonymous] // test email không cần token
		//public async Task<IActionResult> TestEmail()
		//{
		//	try
		//	{
		//		string toEmail = "dangcuong9551@gmail.com";
		//		string subject = "Hệ thống Idea: Kiểm tra cấu hình Email";
		//		string body = $"<h1>Kết nối thành công!</h1><p>Email này được gửi lúc {DateTime.UtcNow:HH:mm:ss}. Hệ thống của bạn đã sẵn sàng!</p>";

		//		await _emailService.SendEmailAsync(toEmail, subject, body);

		//		return Ok(new
		//		{
		//			message = "Đã gửi",
		//			usingEmail = _config["EmailSettings:SenderEmail"] // Kiểm tra xem Azure đã nhận đúng Mail chưa
		//		});
		//	}
		//	catch (Exception ex)
		//	{
		//		return BadRequest(new { error = ex.Message, detail = ex.InnerException?.Message });
		//	}
		//}
		public class LoginRequestDto
		{
			public string Email { get; set; } = string.Empty;
			public string Password { get; set; } = string.Empty;
		}

		public class RegisterRequestDto
		{
			public string Email { get; set; } = string.Empty;
			public string Password { get; set; } = string.Empty;
			public string Name { get; set; } = string.Empty;
		}
	}
}