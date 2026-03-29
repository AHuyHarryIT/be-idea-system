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

		//  GET: ClosureDates
		[HttpGet]
		public async Task<IActionResult> GetClosureDates()
		{
			try
			{
				var submissions = await _submissionService.GetAllSubmissionsAsync();
				return Ok(submissions);
			}
			catch (Exception ex)
			{
				var realError = ex.InnerException?.Message ?? ex.Message;
				return StatusCode(500, new
				{
					message = "Database error on the server:",
					details = realError
				});
			}
		}

		//  POST: CreateSubmission
		[HttpPost]
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
		public async Task<IActionResult> CreateSubmission([FromBody] SubmissionCreateDto dto)
		{
			await _submissionService.CreateSubmissionAsync(dto);
			return Ok(new { message = "Create a successful submission period." });
		}

		//  PUT: UpdateSubmission
		[HttpPut("{id}")]
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
		public async Task<IActionResult> UpdateSubmission([FromRoute] Guid id, [FromBody] SubmissionCreateDto dto)
		{
			await _submissionService.UpdateSubmissionAsync(id, dto);
			return Ok(new { message = "The submission period has been updated as successful." });
		}

		//  DELETE: DeleteSubmission
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