using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdeaCollectionSystem.MVC.Controllers
{
	[Authorize(Policy = PolicyConstants.QACoordinatorOnly)]
	public class QACoordinatorController : Controller
	{
		private readonly IIdeaService _ideaService;
		private readonly IQAManagerService _qaService;

		public QACoordinatorController(IIdeaService ideaService, IQAManagerService qaService)
		{
			_ideaService = ideaService;
			_qaService = qaService;
		}

		// GET /QACoordinator/Dashboard
		public IActionResult Dashboard()
		{
			ViewBag.PageTitle = "QA Coordinator Dashboard";
			return View();
		}

		//  Department Ideas (dept only) 
		public async Task<IActionResult> DepartmentIdeas()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var ideas = await _ideaService.GetIdeasByDepartmentAsync(userId!);
			return View(ideas);
		}

		//  Department Statistics 
		public async Task<IActionResult> DepartmentStatistics()
		{
			var stats = await _qaService.GetDepartmentStatisticsAsync();
			return View(stats);
		}

		//  Submit Idea (coordinator cũng được submit) 
		public IActionResult ManageStaff() => View();

		// ─── Vote (Thumbs Up/Down) via AJAX 
		[HttpPost]
		public async Task<IActionResult> Vote(int ideaId, bool isThumbsUp)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
				return Json(new { success = false });

			var result = await _ideaService.VoteIdeaAsync(ideaId, userId, isThumbsUp);
			return Json(new { success = result });
		}
	}
}