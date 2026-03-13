using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatsController : ControllerBase
{
    private readonly IQAManagerService _qaService;

    public StatsController(IQAManagerService qaService)
    {
        _qaService = qaService;
    }

    // GET api/stats/dashboard  → Admin, QAManager
    [HttpGet("dashboard")]
    [Authorize(Roles = "Administrator,QA Manager")]
    public async Task<IActionResult> GetDashboard()
    {
        var stats = await _qaService.GetDashboardStatsAsync();
        return Ok(stats);
    }

    // GET api/stats/departments  → Admin, QAManager, QACoordinator
    [HttpGet("departments")]
    [Authorize(Roles = "Administrator,QA Manager,QA Coordinator")]
    public async Task<IActionResult> GetDepartmentStats()
    {
        var stats = await _qaService.GetDepartmentStatisticsAsync();
        return Ok(stats);
    }

    // GET api/stats/ideas-without-comments  → Admin, QAManager
    [HttpGet("ideas-without-comments")]
    [Authorize(Roles = "Administrator,QA Manager")]
    public async Task<IActionResult> GetIdeasWithoutComments()
    {
        var ideas = await _qaService.GetIdeasWithoutCommentsAsync();
        return Ok(ideas);
    }
}