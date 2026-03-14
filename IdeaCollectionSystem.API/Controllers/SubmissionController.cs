using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
	public class SubmissionController : ControllerBase
	{
		private readonly ISubmissionService _submissionService;

		public SubmissionController(ISubmissionService submissionService)
		{
			_submissionService = submissionService;
		}


		// Get
		[HttpGet]
		public async Task<IActionResult> GetClosureDates()
		{
			var submissions = await _submissionService.GetAllSubmissionsAsync();
			return Ok(submissions);
		}

		// create
		[HttpPost]
		public async Task<IActionResult> CreateSubmission([FromBody] SubmissionCreateDto dto)
		{
			await _submissionService.CreateSubmissionAsync(dto);
			return Ok(new { message = "Create a successful submission period." });
		}

		// put
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateSubmission([FromRoute] Guid id, [FromBody] SubmissionCreateDto dto)
		{
			await _submissionService.UpdateSubmissionAsync(id, dto);
			return Ok(new { message = "The submission period has been updated as successful." });
			}
	}
}