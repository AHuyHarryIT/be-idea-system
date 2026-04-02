//using IdeaCollectionIdea.Common.Constants;
//using IdeaCollectionSystem.Service.Interfaces;
//using IdeaCollectionSystem.Service.Models.DTOs;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace IdeaCollectionSystem.MVC.Controllers
//{
//	[Authorize(Roles = RoleConstants.QAManager)]
//	public class QAManagerController : Controller
//	{
//		// Tiêm đúng các Service chuyên biệt
//		private readonly ICategoryService _categoryService;
//		private readonly IIdeaService _ideaService;
//		private readonly IStatsService _statsService;
//		private readonly IExportService _exportService;
//		private readonly ISubmissionService _submissionService;

//		public QAManagerController(
//			ICategoryService categoryService,
//			IIdeaService ideaService,
//			IStatsService statsService,
//			IExportService exportService,
//			ISubmissionService submissionService)
//		{
//			_categoryService = categoryService;
//			_ideaService = ideaService;
//			_statsService = statsService;
//			_exportService = exportService;
//			_submissionService = submissionService;
//		}

//		//  DASHBOARD 
//		public async Task<IActionResult> Dashboard()
//		{
//			var stats = await _statsService.GetDashboardStatsAsync();
//			return View(stats);
//		}

//		//  VIEW ALL IDEAS 
//		//public async Task<IActionResult> AllIdeas()
//		//{
//		//	var ideas = await _ideaService.GetAllIdeasAsync();
//		//	return View(ideas);
//		//}

//		//  CATEGORIES MANAGEMENT 
//		public async Task<IActionResult> Categories()
//		{
//			var categories = await _categoryService.GetAllActiveAsync();
//			return View(categories);
//		}

//		[HttpPost]
//		public async Task<IActionResult> CreateCategory(string name)
//		{
//			if (!string.IsNullOrWhiteSpace(name))
//				await _categoryService.CreateAsync(name);
//			return RedirectToAction(nameof(Categories));
//		}

//		//  EXPORT DATA 
//		public IActionResult Export() => View();

//		public async Task<IActionResult> ExportCsv()
//		{
//			var data = await _exportService.ExportIdeasToCsvAsync();
//			return File(data, "text/csv", $"Ideas_Report_{DateTime.UtcNow:yyyyMMdd}.csv");
//		}

//		public async Task<IActionResult> ExportZip()
//		{
//			var data = await _exportService.ExportDocumentsToZipAsync();
//			return File(data, "application/zip", $"Attachments_{DateTime.UtcNow:yyyyMMdd}.zip");
//		}

//		//  ACADEMIC YEARS / CLOSURE DATES 
//		public async Task<IActionResult> ClosureDates()
//		{
//			var submissions = await _submissionService.GetAllSubmissionsAsync();
//			return View(submissions);
//		}

//		[HttpPost]
//		public async Task<IActionResult> CreateSubmission(SubmissionCreateDto dto)
//		{
//			if (ModelState.IsValid)
//			{
//				await _submissionService.CreateSubmissionAsync(dto);
//				TempData["Success"] = "New closure dates set successfully.";
//			}
//			return RedirectToAction(nameof(ClosureDates));
//		}

//		//  STATISTICS 
//		public async Task<IActionResult> Statistics()
//		{
//			var deptStats = await _statsService.GetDepartmentStatisticsAsync();
//			return View(deptStats);
//		}

//		//  EXCEPTION REPORT: IDEAS WITHOUT COMMENTS 
//		public async Task<IActionResult> IdeasWithoutComments()
//		{
			
//			var ideas = await _ideaService.GetIdeasWithoutCommentsAsync();
//			return View(ideas);
//		}
//	}
//}