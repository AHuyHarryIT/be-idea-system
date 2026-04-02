using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
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

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> CreateIdea([FromForm] IdeaCreateDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Title))
			{
				return BadRequest(new { message = "The Title field is required and cannot be empty." });
			}

			if (!dto.HasAcceptedTerms)
			{
				return BadRequest(new { message = "You must agree to the Terms and Conditions before submitting an idea!" });
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { message = "You must be logged in to perform this action." });
			}

			try
			{
				var newIdeaId = await _ideaService.CreateIdeaAsync(dto, userId);

				if (newIdeaId != null)
				{

					return Ok(new
					{
						message = "Idea created successfully.",
						id = newIdeaId
					});
				}

				return BadRequest(new { message = "Failed to submit idea. The submission period might be closed or the provided data is invalid." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}


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

				var createdComment = await _ideaService.CreateCommentAsync(commentCreateDto, userId);

				if (createdComment != null)
				{
					return Ok(new
					{
						message = "Comment added successfully.",
						data = createdComment
					});
				}

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