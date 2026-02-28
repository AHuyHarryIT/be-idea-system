using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.MVC.Controllers
{
	[Authorize(Policy = PolicyConstants.QAManagerOnly)]
	public class QAManagerController : Controller
	{
		private readonly IQAManagerService _qaService;
		private readonly ICategoryService _categoryService;

		public QAManagerController(
			IQAManagerService qaService,
			ICategoryService categoryService)
		{
			_qaService = qaService;
			_categoryService = categoryService;
		}

		public async Task<IActionResult> Dashboard()
		{
			var stats = await _qaService.GetDashboardStatsAsync();
			return View(stats);
		}

		public async Task<IActionResult> Categories()
		{
			var categories = await _categoryService.GetAllActiveAsync();
			return View(categories);
		}

		[HttpPost]
		public async Task<IActionResult> Create(string name)
		{
			await _categoryService.CreateAsync(name);
			return RedirectToAction(nameof(Categories));
		}

		[HttpPost]
		public async Task<IActionResult> DeleteCategory(Guid id)
		{
			var success = await _categoryService.DeleteIfUnusedAsync(id);

			if (!success)
				TempData["Error"] = "Category is in use and cannot be deleted.";

			return RedirectToAction(nameof(Categories));
		}

		public async Task<IActionResult> Export()
		{
			return View();
		}

		public async Task<IActionResult> ExportCsv()
		{
			var data = await _qaService.ExportIdeasToCsvAsync();
			return File(data, "text/csv", "Ideas.csv");
		}

		public async Task<IActionResult> ExportZip()
		{
			var data = await _qaService.ExportDocumentsToZipAsync();
			return File(data, "application/zip", "Documents.zip");
		}

		public async Task<IActionResult> DepartmentStats()
		{
			var stats = await _qaService.GetDepartmentStatisticsAsync();
			return View(stats);
		}
		public async Task<IActionResult> IdeasWithoutComments()
		{
			var ideas = await _qaService.GetIdeasWithoutCommentsAsync();
			return View(ideas);
		}
	}
}