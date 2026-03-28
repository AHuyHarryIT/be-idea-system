using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers
{
	[Route("api/submissions")]
	[ApiController]
	[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
	public class SubmissionController : ControllerBase
	{
		private readonly ISubmissionService _submissionService;

		public SubmissionController(ISubmissionService submissionService)
		{
			_submissionService = submissionService;
		}


		// Get clousure dates
		[HttpGet]
		public async Task<IActionResult> GetClosureDates()
		{
			var submissions = await _submissionService.GetAllSubmissionsAsync();
			return Ok(submissions);
		}

		// create submission
		[HttpPost]
		public async Task<IActionResult> CreateSubmission([FromBody] SubmissionCreateDto dto)
		{
			await _submissionService.CreateSubmissionAsync(dto);
			return Ok(new { message = "Create a successful submission period." });
		}

		// update submission
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateSubmission([FromRoute] Guid id, [FromBody] SubmissionCreateDto dto)
		{
			await _submissionService.UpdateSubmissionAsync(id, dto);
			return Ok(new { message = "The submission period has been updated as successful." });
		}

		// Xóa Submission
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

			// Trả về 200 OK nếu xóa thành công
			return Ok(new { message = result.Message });
		}
	}
}