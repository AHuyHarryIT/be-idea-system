//using IdeaCollectionIdea.Common.Constants;
//using IdeaCollectionSystem.Models;
//using IdeaCollectionSystem.Service.Interfaces;
//using IdeaCollectionSystem.Service.Models.DTOs;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using System.Security.Claims;

//namespace IdeaCollectionSystem.MVC.Controllers
//{
//	[Authorize] // Cho phép tất cả người dùng đã đăng nhập
//	public class StaffController : Controller
//	{
//		private readonly IIdeaService _ideaService;
//		private readonly ICategoryService _categoryService;
//		private readonly ISubmissionService _submissionService; // Thay thế IQAManagerService

//		public StaffController(
//			IIdeaService ideaService,
//			ICategoryService categoryService,
//			ISubmissionService submissionService)
//		{
//			_ideaService = ideaService;
//			_categoryService = categoryService;
//			_submissionService = submissionService;
//		}

//		// --- DASHBOARD ---
//		public IActionResult Dashboard() => View();

//		// --- TERMS & CONDITIONS (Yêu cầu bắt buộc của Coursework) ---
//		[HttpGet]
//		public IActionResult Terms() => View();

//		[HttpPost]
//		public IActionResult AcceptTerms(bool agree)
//		{
//			if (agree)
//			{
//				HttpContext.Session.SetString("AgreedTerms", "true");
//				return RedirectToAction(nameof(SubmitIdea));
//			}
//			ModelState.AddModelError("", "You must agree to the Terms and Conditions to proceed.");
//			return View("Terms");
//		}

//		// --- MY IDEAS ---
//		public async Task<IActionResult> MyIdeas()
//		{
//			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//			var ideas = await _ideaService.GetIdeasByStaffAsync(userId!);
//			return View(ideas ?? new List<IdeaInfoDto>());
//		}

//		// --- BROWSE IDEAS ---
//		public async Task<IActionResult> BrowseIdeas()
//		{
//			var ideas = await _ideaService.GetAllIdeasAsync();
//			return View(ideas);
//		}

//		// --- SUBMIT IDEA (GET) ---
//		[HttpGet]
//		public async Task<IActionResult> SubmitIdea()
//		{
//			// 1. Kiểm tra đã đồng ý điều khoản chưa
//			if (HttpContext.Session.GetString("AgreedTerms") != "true")
//				return RedirectToAction(nameof(Terms));

//			// 2. Load dữ liệu cho Dropdown
//			await LoadFormSelectionData();

//			return View();
//		}

//		// --- SUBMIT IDEA (POST) ---
//		[HttpPost]
//		[ValidateAntiForgeryToken]
//		public async Task<IActionResult> SubmitIdea(IdeaViewModel model)
//		{
//			// Kiểm tra điều khoản
//			if (HttpContext.Session.GetString("AgreedTerms") != "true")
//				return RedirectToAction(nameof(Terms));

//			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

//			if (ModelState.IsValid)
//			{
//				// Kiểm tra logic Closure Date tại tầng Service trước khi lưu
//				// (Giả sử CreateIdeaAsync đã có logic check date nội bộ)

//				List<string>? filePaths = await HandleFileUploads();

//				var dto = new IdeaCreateDto
//				{
//					Title = model.Title,
//					Description = model.Description,
//					CategoryId = model.CategoryId!.Value,
//					SubmissionId = model.SubmissionId!.Value,
//					IsAnonymous = model.IsAnonymous,
//					FilePaths = filePaths
//				};

//				try
//				{
//					var result = await _ideaService.CreateIdeaAsync(dto, userId!);
//					TempData["SuccessMessage"] = "Your idea has been submitted successfully!";
//					return RedirectToAction(nameof(MyIdeas));
//				}
//				catch (Exception ex)
//				{
//					ModelState.AddModelError("", ex.Message);
//				}
//			}

//			await LoadFormSelectionData(model.CategoryId, model.SubmissionId);
//			return View(model);
//		}

//		// --- VOTE (AJAX) ---
//		[HttpPost]
//		public async Task<IActionResult> Vote(Guid ideaId, bool isThumbsUp)
//		{
//			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//			if (string.IsNullOrEmpty(userId)) return Json(new { success = false });

//			var result = await _ideaService.VoteIdeaAsync(ideaId, userId, isThumbsUp);
//			return Json(new { success = result });
//		}

//		// --- HELPER METHODS ---
//		private async Task LoadFormSelectionData(Guid? selectedCat = null, Guid? selectedSub = null)
//		{
//			var categories = await _categoryService.GetAllActiveAsync();
//			ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCat);

//			// Chỉ lấy các đợt nộp bài đang còn hạn (IsActive)
//			var submissions = await _submissionService.GetAllSubmissionsAsync();
//			var activeSubmissions = submissions.Where(s => s.IsActive).ToList();
//			ViewBag.Submissions = new SelectList(activeSubmissions, "Id", "Name", selectedSub);
//		}

//		private async Task<List<string>?> HandleFileUploads()
//		{
//			if (Request.Form.Files.Count == 0) return null;

//			var filePaths = new List<string>();
//			var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

//			if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

//			foreach (var file in Request.Form.Files)
//			{
//				var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
//				var filePath = Path.Combine(uploadsFolder, uniqueFileName);

//				using (var stream = new FileStream(filePath, FileMode.Create))
//				{
//					await file.CopyToAsync(stream);
//				}
//				filePaths.Add(Path.Combine("uploads", uniqueFileName));
//			}
//			return filePaths;
//		}
//	}
//}