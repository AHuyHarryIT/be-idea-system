using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers
{
	[Route("api/stats")]
	[ApiController]
	[Authorize(Roles = RoleConstants.QAManager)] 
	public class StatsController : ControllerBase
	{
		private readonly IStatsService _statsService;

		public StatsController(IStatsService 
			statsService)
		{
			_statsService = statsService;
		}

		// GET api/stats/dashboard
		[HttpGet("dashboard")]
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
		public async Task<IActionResult> GetDashboard()
		{
			var stats = await _statsService.GetDashboardStatsAsync();
			return Ok(stats);
		}

		// GET api/stats/departments
		[HttpGet("departments")]
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager + "," + RoleConstants.QACoordinator)]
		public async Task<IActionResult> GetDepartmentStats([FromQuery] Guid? submissionId)
		{
			var stats = await _statsService.GetDepartmentStatsAsync(submissionId);
			return Ok(stats);
		}

		// GET api/stats/ideas-without-comments
		[HttpGet("ideas-without-comments")]
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
		public async Task<IActionResult> GetIdeasWithoutComments()
		{
			var ideas = await _statsService.GetIdeasWithoutCommentsAsync();
			return Ok(ideas);
		}
	}
}