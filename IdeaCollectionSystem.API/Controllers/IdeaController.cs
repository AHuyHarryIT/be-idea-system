using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using IdeaCollectionSystem.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdeaCollectionSystem.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class IdeaController : ControllerBase
	{
		private readonly IIdeaService _ideaService;

		public IdeaController(IIdeaService ideaService)
		{
			_ideaService = ideaService;
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> CreateIdea([FromForm] IdeaCreateDto dto)
		{

			if (!dto.HasAcceptedTerms)
			{
				return BadRequest(new
				{
					message = "You must agree to the Terms and Conditions before submitting an idea!"
				});
			}

			// 2. Lấy ID người dùng
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId)) return Unauthorized();

			// 3. Gọi Service xử lý
			var success = await _ideaService.CreateIdeaAsync(dto, userId);

			if (success)
			{
				return Ok(new { message = "The idea has been submitted successfully." });
			}

			return BadRequest(new { message = "Failed to submit idea. The submission period might be closed." });
		}

	

		// GET Idea 
		[HttpGet]
		public async Task<IActionResult> GetIdeasPaged([FromQuery] IdeaQueryParameters parameters)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId)) return Unauthorized();

			var pagedResult = await _ideaService.GetIdeasPagedAsync(parameters, userId);
			return Ok(pagedResult);
		}

		// 3. Get My Ideas
		[HttpGet("my-ideas")]
		public async Task<IActionResult> GetMyIdeas([FromQuery] IdeaQueryParameters parameters)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId)) return Unauthorized();

			var pagedResult = await _ideaService.GetIdeasPagedAsync(parameters, userId);
			return Ok(pagedResult);
		}

		// 4. Get Idea Details
		[HttpGet("{id}")]
		public async Task<IActionResult> GetIdeaDetails([FromRoute] Guid id)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var ideaDetail = await _ideaService.GetIdeaDetailAsync(id, userId!);

			if (ideaDetail == null) return NotFound(new { message = "No ideas found." });

			return Ok(ideaDetail);

		}

		// 5. Add Comment
		[HttpPost("{id}/comments")]
		[Authorize] 
		public async Task<IActionResult> CreateComment([FromRoute] Guid id, [FromBody] CommentDto request)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

	
			if (string.IsNullOrEmpty(userId))
				return Unauthorized(new { message = "Please log-in to comment" });

			if (string.IsNullOrWhiteSpace(request.Content))
				return BadRequest(new { message = "The comment section cannot be left blank." });

			var commentCreateDto = new CommentCreateDto
			{
				IdeaId = id,
				Content = request.Content,
				IsAnonymous = request.IsAnonymous
			};
			var success = await _ideaService.CreateCommentAsync(commentCreateDto, userId);

			if (success) return Ok(new { message = "Comment added successfully." });

			return BadRequest(new { message = "Unable to comment (The idea does not exist or is outdated)." });
		}

		// 6. Vote (Thumbs Up / Down)
		[HttpPost("{id}/vote")]
		public async Task<IActionResult> Vote([FromRoute] Guid id, [FromBody] VoteRequestDto request)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var result = await _ideaService.VoteIdeaAsync(id, userId!, request.IsThumbsUp);

			if (result) return Ok(new { success = true, message = "The votes have been recorded." });

			return BadRequest(new { success = false, message = "The vote was a failure." });
		}



		// PUT: api/Idea/{id}/review
		[HttpPut("{id}/review")]
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
		public async Task<IActionResult> ReviewIdea(Guid id, [FromBody] ReviewIdeaDto dto)
		{
			var result = await _ideaService.ReviewIdeaAsync(id, dto);

			if (!result) return NotFound(new { message = "Idea not found or update failed." });

			string action = dto.IsApproved ? "approved" : "rejected";
			return Ok(new { message = $"Idea has been {action} successfully." });
		}
	}
}