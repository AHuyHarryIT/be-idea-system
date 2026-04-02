using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdeaCollectionSystem.API.Controllers
{
	[Route("api/ideas")]
	[ApiController]
	[Authorize]
	public class IdeaController : ControllerBase
	{
		private readonly IIdeaService _ideaService;

		public IdeaController(IIdeaService ideaService)
		{
			_ideaService = ideaService;
		}

		// TRONG IdeaController.cs
		[HttpPost]
		[Authorize(Roles = RoleConstants.Staff)]
		public async Task<IActionResult> CreateIdea([FromForm] IdeaCreateDto request)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId)) return Unauthorized();

			try
			{
				var newIdeaId = await _ideaService.CreateIdeaAsync(request, userId);

				if (newIdeaId != null) 
				{
					
					return Ok(new
					{
						message = "Idea created successfully.",
						id = newIdeaId
					});
				}

				return BadRequest(new { message = "Unable to submit idea. Please check submission date or your department." });
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
			}
		}

		// GET Idea (Dùng chung cho cả Staff, QA Coordinator, QA Manager và Admin)
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetIdeasPaged([FromQuery] IdeaQueryParameters parameters)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { message = "You must be logged in to view ideas." });
			}

			bool isManager = User.IsInRole(RoleConstants.Administrator) || User.IsInRole(RoleConstants.QAManager);
			var pagedResult = await _ideaService.GetIdeasPagedAsync(parameters, userId, isManager);

			return Ok(pagedResult);
		}

		// 3. Get My Ideas
		[HttpGet("my-ideas")]
		[Authorize]
		public async Task<IActionResult> GetMyIdeas([FromQuery] IdeaQueryParameters parameters)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { message = "You must be logged in to view your ideas." });
			}

			var pagedResult = await _ideaService.GetMyIdeasPagedAsync(parameters, userId);
			return Ok(pagedResult);
		}

		// 4.  Get Idea Details
		[HttpGet("{id}")]
		[Authorize]
		public async Task<IActionResult> GetIdeaDetails([FromRoute] Guid id)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId)) return Unauthorized();

			var ideaDetail = await _ideaService.GetIdeaDetailAsync(id, userId);

			if (ideaDetail == null) return NotFound(new { message = "No idea found." });

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

			try
			{
				var commentCreateDto = new CommentCreateDto
				{
					IdeaId = id,
					Content = request.Content,
					IsAnonymous = request.IsAnonymous
				};

				// Gọi hàm service, hứng kết quả trả về là CommentDto
				var createdComment = await _ideaService.CreateCommentAsync(commentCreateDto, userId);

				// Nếu createdComment khác null nghĩa là tạo thành công
				if (createdComment != null)
				{
					return Ok(new
					{
						message = "Comment added successfully.",
						data = createdComment // Truyền trực tiếp CommentDto xuống cho Frontend hiển thị
					});
				}

				// Nếu trả về null
				return BadRequest(new { message = "Unable to comment (The idea does not exist or is outdated)." });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new
				{
					message = "Server crashed!",
					details = ex.InnerException?.Message ?? ex.Message
				});
			}
		}

		// 6. Vote (Thumbs Up / Down)
		[HttpPost("{id}/vote")]
		[Authorize] 
		public async Task<IActionResult> Vote([FromRoute] Guid id, [FromBody] VoteRequestDto request)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId)) return Unauthorized();

			var result = await _ideaService.VoteIdeaAsync(id, userId, request.IsThumbsUp);

			if (result) return Ok(new { success = true, message = "The votes have been recorded." });

			return BadRequest(new { success = false, message = "The vote was a failure." });
		}

		// PUT: api/Idea/{id}/review
		[HttpPut("{id}/review")]
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager + "," + RoleConstants.QACoordinator)]
		public async Task<IActionResult> ReviewIdea(Guid id, [FromBody] ReviewIdeaDto dto)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { message = "You must be logged in to perform this action." });
			}

			try
			{
				var result = await _ideaService.ReviewIdeaAsync(id, dto, userId);

				if (!result)
				{
					return NotFound(new { message = "Idea not found or update failed." });
				}

				string actionMessage = dto.Status switch
				{
					ReviewStatus.APPROVED => "approved", 
					ReviewStatus.REJECTED => "set back to pending (rejected)",
					ReviewStatus.PENDING => "set to pending",
					_ => "updated"
				};

				return Ok(new { message = $"Idea has been {actionMessage} successfully." });
			}
			catch (UnauthorizedAccessException ex)
			{
				return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred.", details = ex.Message });
			}
		}
	}
}