using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.MVC.Controllers
{
	[Authorize(Policy = PolicyConstants.QAManagerOnly)]
	public class QAManagerController : Controller
	{
		private readonly IQAManagerService _qaService;
		private readonly ICategoryService _categoryService;
		private readonly IIdeaService _ideaService;

		public QAManagerController(
			IQAManagerService qaService,
			ICategoryService categoryService,
			IIdeaService ideaService)
		{
			_qaService = qaService;
			_categoryService = categoryService;
			_ideaService = ideaService;
		}

		// GET /QAManager/Dashboard
		public async Task<IActionResult> Dashboard()
		{
			var stats = await _qaService.GetDashboardStatsAsync();
			return View(stats);
		}

		//  View All Ideas 
		public async Task<IActionResult> AllIdeas()
		{
			var ideas = await _ideaService.GetAllIdeasAsync();
			return View(ideas);
		}

		//  Categories 
		[Authorize(Policy = PolicyConstants.CanManageCategories)]
		public async Task<IActionResult> Categories()
		{
			var categories = await _categoryService.GetAllActiveAsync();
			if (TempData["Error"] != null) ModelState.AddModelError("", TempData["Error"]!.ToString()!);
			return View(categories);
		}

		[HttpPost]
		[Authorize(Policy = PolicyConstants.CanManageCategories)]
		public async Task<IActionResult> CreateCategory(string name)
		{
			if (!string.IsNullOrWhiteSpace(name))
				await _categoryService.CreateAsync(name);
			return RedirectToAction(nameof(Categories));
		}

		[HttpPost]
		[Authorize(Policy = PolicyConstants.CanManageCategories)]
		public async Task<IActionResult> DeleteCategory(Guid id)
		{
			var success = await _categoryService.DeleteIfUnusedAsync(id);
			if (!success)
				TempData["Error"] = "Category is in use and cannot be deleted.";
			return RedirectToAction(nameof(Categories));
		}

		//  Export 
		[Authorize(Policy = PolicyConstants.CanExportData)]
		public IActionResult Export() => View();

		[Authorize(Policy = PolicyConstants.CanExportData)]
		public async Task<IActionResult> ExportCsv()
		{
			var data = await _qaService.ExportIdeasToCsvAsync();
			return File(data, "text/csv", $"Ideas_{DateTime.UtcNow:yyyyMMdd}.csv");
		}

		[Authorize(Policy = PolicyConstants.CanExportData)]
		public async Task<IActionResult> ExportZip()
		{
			var data = await _qaService.ExportDocumentsToZipAsync();
			return File(data, "application/zip", $"Documents_{DateTime.UtcNow:yyyyMMdd}.zip");
		}
		//  Closure Dates 
		[Authorize(Policy = PolicyConstants.CanSetClosureDates)]
		public async Task<IActionResult> ClosureDates()
		{
			var submissions = await _qaService.GetAllSubmissionsAsync();
			return View(submissions);
		}

		[HttpPost]
		[Authorize(Policy = PolicyConstants.CanSetClosureDates)]
		public async Task<IActionResult> CreateSubmission(SubmissionCreateDto dto)
		{
			if (ModelState.IsValid)
			{
				await _qaService.CreateSubmissionAsync(dto);
				TempData["Success"] = "Submission period created.";
			}
			return RedirectToAction(nameof(ClosureDates));
		}

		[HttpPost]
		[Authorize(Policy = PolicyConstants.CanSetClosureDates)]
		public async Task<IActionResult> UpdateSubmission(Guid id, SubmissionCreateDto dto)
		{
			if (ModelState.IsValid)
			{
				await _qaService.UpdateSubmissionAsync(id, dto);
				TempData["Success"] = "Submission period updated.";
			}
			return RedirectToAction(nameof(ClosureDates));
		}

		//  Statistics 
		public async Task<IActionResult> Statistics()
		{
			var deptStats = await _qaService.GetDepartmentStatisticsAsync();
			return View(deptStats);
		}

		public async Task<IActionResult> IdeasWithoutComments()
		{
			var ideas = await _qaService.GetIdeasWithoutCommentsAsync();
			return View(ideas);
		}
	}
}