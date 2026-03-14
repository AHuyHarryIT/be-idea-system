using IdeaCollectionIdea.Common.Constants; // Bắt buộc phải có để gọi RoleConstants
using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize] // Bảo vệ vòng ngoài (Yêu cầu có token)
	public class StatsController : ControllerBase
	{
		private readonly IStatsService statsService;

		public StatsController(IStatsService qaService)
		{
			statsService = qaService;
		}

		// GET api/stats/dashboard
		[HttpGet("dashboard")]
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
		public async Task<IActionResult> GetDashboard()
		{
			var stats = await statsService.GetDashboardStatsAsync();
			return Ok(stats);
		}

		// GET api/stats/departments
		[HttpGet("departments")]
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager + "," + RoleConstants.QACoordinator)]
		public async Task<IActionResult> GetDepartmentStats()
		{
			var stats = await statsService.GetDepartmentStatisticsAsync();
			return Ok(stats);
		}

	
		// GET api/stats/ideas-without-comments
		[HttpGet("ideas-without-comments")]
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
		public async Task<IActionResult> GetIdeasWithoutComments()
		{
			var ideas = await statsService.GetIdeasWithoutCommentsAsync();
			return Ok(ideas);
		}
	}
}