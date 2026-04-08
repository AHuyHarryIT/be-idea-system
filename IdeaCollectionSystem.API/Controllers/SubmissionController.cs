using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers
{
	[Route("api/submissions")]
	[ApiController]
	[Authorize]
	public class SubmissionController : ControllerBase
	{
		private readonly ISubmissionService _submissionService;

		public SubmissionController(ISubmissionService submissionService)
		{
			_submissionService = submissionService;
		}

		// GET: api/submissions
		[HttpGet]
		public async Task<IActionResult> GetSubmissions([FromQuery] PaginationFilter filter, [FromQuery] bool fetchAll = false)
		{
			try
			{
				if (fetchAll)
				{
					var allData = await _submissionService.GetSubmissionsPagedAsync(
						new PaginationFilter { PageNumber = 1, PageSize = int.MaxValue }
					);
					return Ok(allData.Items);
				}
				var pagedData = await _submissionService.GetSubmissionsPagedAsync(filter);

				return Ok(new
				{
					Submissions = pagedData.Items,
					Pagination = new
					{
						pagedData.TotalCount,
						pagedData.PageNumber,
						pagedData.PageSize,
						pagedData.TotalPages,
						pagedData.HasPreviousPage,
						pagedData.HasNextPage
					}
				});
			}
			catch (Exception ex)
			{
				var realError = ex.InnerException?.Message ?? ex.Message;
				return StatusCode(500, new { message = "Database error:", details = realError });
			}
		}


		// CREATE SUBMISSION
		[HttpPost]
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
		public async Task<IActionResult> CreateSubmission([FromBody] SubmissionCreateDto dto)
		{
			if (dto.FinalClosureDate < dto.ClosureDate)
			{
				return BadRequest(new { message = "Final Closure Date must be greater than or equal to Closure Date." });
			}

			var isCreated = await _submissionService.CreateSubmissionAsync(dto);

			if (!isCreated)
			{
				return BadRequest(new { message = "Failed to create submission. Please ensure the Name is not empty and Closure Date is not in the past." });
			}

			return Ok(new { message = "Create a successful submission period." });
		}

		// UPDATE SUBMISSION
		[HttpPut("{id}")]
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
		public async Task<IActionResult> UpdateSubmission([FromRoute] Guid id, [FromBody] SubmissionCreateDto dto)
		{
			if (dto.FinalClosureDate < dto.ClosureDate)
			{
				return BadRequest(new { message = "Final Closure Date must be greater than or equal to Closure Date." });
			}

			try
			{
	
				await _submissionService.UpdateSubmissionAsync(id, dto);
				return Ok(new { message = "The submission period has been updated successfully." });
			}
			catch (Exception ex)
			{
				
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
		public async Task<IActionResult> DeleteSubmission(Guid id)
		{
			var result = await _submissionService.DeleteSubmissionAsync(id);

			if (!result.Success)
			{
				if (result.Message.Contains("does not exist"))
					return NotFound(new { message = result.Message });

				return BadRequest(new { message = result.Message });
			}

			return Ok(new { message = result.Message });
		}
	}
}