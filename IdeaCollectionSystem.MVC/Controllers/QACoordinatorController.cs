using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdeaCollectionSystem.MVC.Controllers
{

	[Authorize(Roles = RoleConstants.QACoordinator)]
	public class QACoordinatorController : Controller
	{
		private readonly IIdeaService _ideaService;
		private readonly IStatsService _statsService; 
		private readonly IUserService _userService;

		public QACoordinatorController(
			IIdeaService ideaService,
			IStatsService statsService,
			IUserService userService)
		{
			_ideaService = ideaService;
			_statsService = statsService;
			_userService = userService;
		}

		public IActionResult Dashboard()
		{
			ViewBag.PageTitle = "QA Coordinator Dashboard";
			return View();
		}

		//  Xem các ý tưởng thuộc phòng ban của mình 
		public async Task<IActionResult> DepartmentIdeas()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

			var ideas = await _ideaService.GetIdeasByDepartmentAsync(userId);
			return View(ideas);
		}

		//  Thống kê riêng cho phòng ban 
		public async Task<IActionResult> DepartmentStatistics()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var stats = await _statsService.GetDepartmentStatisticsAsync();

			return View(stats);
		}

		//  Quản lý Staff trong phòng ban 
		public async Task<IActionResult> ManageStaff()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var allUsers = await _userService.GetAllUsersAsync();
			return View(allUsers);
		}

		//  Vote (Hỗ trợ AJAX từ View) 
		[HttpPost]
		public async Task<IActionResult> Vote(Guid ideaId, bool isThumbsUp)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
				return Json(new { success = false, message = "Unauthorized" });

			try
			{
				var result = await _ideaService.VoteIdeaAsync(ideaId, userId, isThumbsUp);
				return Json(new { success = result });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}
	}
}