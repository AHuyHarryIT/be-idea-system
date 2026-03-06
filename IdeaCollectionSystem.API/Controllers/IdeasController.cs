using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdeaCollectionSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IdeasController : ControllerBase
{
	private readonly IIdeaService _ideaService;

	public IdeasController(IIdeaService ideaService)
	{
		_ideaService = ideaService;
	}

	// GET api/ideas/my
	[HttpGet("my")]
	public async Task<IActionResult> GetMyIdeas()
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
			return Unauthorized();

		var ideas = await _ideaService.GetIdeasByStaffAsync(userId);
		return Ok(ideas);
	}

	// POST api/ideas
	[HttpPost]
	public async Task<IActionResult> CreateIdea([FromBody] IdeaCreateDto dto)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
			return Unauthorized();

		if (await _ideaService.IsClosureDatePassedAsync())
			return BadRequest(new { message = "Đã qua ngày đóng cửa nộp ý tưởng." });

		var result = await _ideaService.CreateIdeaAsync(dto, userId);
		if (!result)
			return BadRequest(new { message = "Không thể tạo ý tưởng. Vui lòng thử lại." });

		return Ok(new { message = "Ý tưởng đã được gửi thành công." });
	}
}