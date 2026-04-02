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
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
		[HttpGet("dashboard")]
		public async Task<IActionResult> GetDashboard()
		{
			var stats = await _statsService.GetDashboardStatsAsync();
			return Ok(stats);
		}

		// GET api/stats/departments
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager + "," + RoleConstants.QACoordinator)]
		[HttpGet("departments")]
		public async Task<IActionResult> GetDepartmentStats([FromQuery] Guid? submissionId)
		{
			var stats = await _statsService.GetDepartmentStatsAsync(submissionId);
			return Ok(stats);
		}

		// GET api/stats/ideas-without-comments		
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
		[HttpGet("ideas-without-comments")]
		public async Task<IActionResult> GetIdeasWithoutComments()
		{
			var ideas = await _statsService.GetIdeasWithoutCommentsAsync();
			return Ok(ideas);
		}
	}
}