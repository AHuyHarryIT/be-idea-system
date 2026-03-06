using IdeaCollectionSystem.API.Models;
using IdeaCollectionSystem.ApplicationCore.Entitites.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdeaCollectionSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly UserManager<IdeaUser> _userManager;
	private readonly SignInManager<IdeaUser> _signInManager;
	private readonly IConfiguration _config;

	public AuthController(
		UserManager<IdeaUser> userManager,
		SignInManager<IdeaUser> signInManager,
		IConfiguration config)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_config = config;
	}

	// POST api/auth/login
	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] IdeaCollectionSystem.API.Models.LoginRequest request)
	{
		var user = await _userManager.FindByEmailAsync(request.Email);
		if (user == null)
			return Unauthorized(new { message = "Email hoặc mật khẩu không đúng." });

		var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
		if (!result.Succeeded)
			return Unauthorized(new { message = "Email hoặc mật khẩu không đúng." });

		var roles = await _userManager.GetRolesAsync(user);
		var token = GenerateJwtToken(user, roles);

		return Ok(new
		{
			token,
			user = new
			{
				id = user.Id,
				email = user.Email,
				name = user.Name,
				roles
			}
		});
	}

	// POST api/auth/register
	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] IdeaCollectionSystem.API.Models.RegisterRequest request)
	{
		var user = new IdeaUser
		{
			UserName = request.Email,
			Email = request.Email,
			Name = request.Name
		};

		var result = await _userManager.CreateAsync(user, request.Password);
		if (!result.Succeeded)
			return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

		return Ok(new { message = "Đăng ký thành công." });
	}

	private string GenerateJwtToken(IdeaUser user, IList<string> roles)
	{
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, user.Id),
			new(ClaimTypes.Email, user.Email!),
			new(ClaimTypes.Name, user.Name ?? ""),
		};
		claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
		var expires = DateTime.UtcNow.AddMinutes(
			double.Parse(_config["Jwt:ExpiresInMinutes"] ?? "120"));

		var token = new JwtSecurityToken(
			issuer: _config["Jwt:Issuer"],
			audience: _config["Jwt:Audience"],
			claims: claims,
			expires: expires,
			signingCredentials: creds);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}