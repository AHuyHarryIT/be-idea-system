using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubmittionController : ControllerBase
{
	private readonly IQAManagerService _qaManagerService;

	public SubmittionController(IQAManagerService qaManagerService)
	{
		_qaManagerService = qaManagerService;
	}

	// GET api/submittion
	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		var submissions = await _qaManagerService.GetAllSubmissionsAsync();
		return Ok(submissions);
	}

	// POST api/submittion
	[HttpPost]
	[Authorize(Roles = "Administrator,QAManager")]
	public async Task<IActionResult> Create([FromBody] SubmissionCreateDto dto)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		var result = await _qaManagerService.CreateSubmissionAsync(dto);
		if (!result)
			return BadRequest(new { message = "Không thể tạo đợt nộp bài." });

		return Ok(new { message = "Đợt nộp bài đã được tạo." });
	}

	// PUT api/submittion/{id}
	[HttpPut("{id}")]
	[Authorize(Roles = "Administrator,QAManager")]
	public async Task<IActionResult> Update(Guid id, [FromBody] SubmissionCreateDto dto)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		var result = await _qaManagerService.UpdateSubmissionAsync(id, dto);
		if (!result)
			return NotFound(new { message = "Không tìm thấy đợt nộp bài." });

		return Ok(new { message = "Đợt nộp bài đã được cập nhật." });
	}
}
