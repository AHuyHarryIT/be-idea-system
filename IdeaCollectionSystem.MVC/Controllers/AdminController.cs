using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.MVC.Controllers
{
	[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
	public class AdminController : Controller
	{
		private readonly IUserService _userService;
		private readonly IIdeaService _ideaService;
		private readonly ICategoryService _categoryService;
		private readonly IStatsService _statsService;
		private readonly ISubmissionService _submissionService;
		private readonly IExportService _exportService;

		public AdminController(
			IUserService userService,
			IIdeaService ideaService,
			ICategoryService categoryService,
			IStatsService statsService,
			ISubmissionService submissionService,
			IExportService exportService)
		{
			_userService = userService;
			_ideaService = ideaService;
			_categoryService = categoryService;
			_statsService = statsService;
			_submissionService = submissionService;
			_exportService = exportService;
		}

		//  DASHBOARD & STATISTICS 
		public async Task<IActionResult> Dashboard()
		{
			var stats = await _statsService.GetDashboardStatsAsync();
			return View(stats);
		}

		public async Task<IActionResult> Statistics()
		{
			var deptStats = await _statsService.GetDepartmentStatisticsAsync();
			return View(deptStats); 
		}

		//  USERS MANAGEMENT 
		[Authorize(Roles = RoleConstants.Administrator)] 
		public async Task<IActionResult> Users()
		{
			var users = await _userService.GetAllUsersAsync();
			ViewBag.Roles = RoleConstants.GetAllRoles();
			return View(users);
		}

		[HttpPost]
		[Authorize(Roles = RoleConstants.Administrator)]
		public async Task<IActionResult> UpdateUserRole(string userId, string role)
		{
			try
			{
				await _userService.UpdateUserRoleAsync(userId, role);
				TempData["Success"] = "User role updated successfully.";
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Failed to update role: " + ex.Message;
			}
			return RedirectToAction(nameof(Users));
		}

		//  IDEAS VIEW 
		//public async Task<IActionResult> AllIdeas()
		//{
		//	var ideas = await _ideaService.GetAllIdeasAsync();
		//	return View(ideas);
		//}

		//  CATEGORIES MANAGEMENT 
		public async Task<IActionResult> Categories()
		{
			var categories = await _categoryService.GetAllActiveAsync();
			return View(categories);
		}

		[HttpPost]
		public async Task<IActionResult> CreateCategory(string name)
		{
			if (!string.IsNullOrWhiteSpace(name))
			{
				await _categoryService.CreateAsync(name);
				TempData["Success"] = "Category created!";
			}
			return RedirectToAction(nameof(Categories));
		}

		[HttpPost]
		public async Task<IActionResult> DeleteCategory(Guid id)
		{
			var success = await _categoryService.DeleteIfUnusedAsync(id);
			if (!success)
				TempData["Error"] = "Category is in use and cannot be deleted.";
			else
				TempData["Success"] = "Category deleted successfully.";

			return RedirectToAction(nameof(Categories));
		}

		//  SUBMISSIONS (CLOSURE DATES) 
		public async Task<IActionResult> ClosureDates()
		{
			var submissions = await _submissionService.GetAllSubmissionsAsync();
			return View(submissions);
		}

		[HttpPost]
		public async Task<IActionResult> CreateSubmission(SubmissionCreateDto dto)
		{
			if (ModelState.IsValid)
			{
				await _submissionService.CreateSubmissionAsync(dto);
				TempData["Success"] = "Academic Year (Submission) created.";
			}
			return RedirectToAction(nameof(ClosureDates));
		}

		//  EXPORT DATA 
		public IActionResult Export() => View();

		public async Task<IActionResult> ExportCsv()
		{
			var data = await _exportService.ExportIdeasToCsvAsync();
			return File(data, "text/csv", $"Ideas_Report_{DateTime.Now:yyyyMMdd}.csv");
		}

		public async Task<IActionResult> ExportZip()
		{
			var data = await _exportService.ExportDocumentsToZipAsync();
			return File(data, "application/zip", $"All_Attachments_{DateTime.Now:yyyyMMdd}.zip");
		}
	}
}