using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Models;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace IdeaCollectionSystem.MVC.Controllers
{
	[Authorize(Policy = PolicyConstants.AllStaff)]
	public class StaffController : Controller
	{
		private readonly IIdeaService _ideaService;
		private readonly ICategoryService _categoryService;

		public StaffController(IIdeaService ideaService, ICategoryService categoryService)
		{
			_ideaService = ideaService;
			_categoryService = categoryService;
		}

		// GET /Staff/Dashboard
		public IActionResult Dashboard() => View();

		// ─── Terms ────────────────────────────────────────────────────────────
		[HttpGet]
		public IActionResult Terms() => View();

		[HttpPost]
		public IActionResult AcceptTerms(bool agree)
		{
			if (agree)
			{
				HttpContext.Session.SetString("AgreedTerms", "true");
				return RedirectToAction(nameof(SubmitIdea));
			}
			ModelState.AddModelError("", "You must agree to the Terms and Conditions.");
			return View("Terms");
		}

		//  My Ideas 
		public async Task<IActionResult> MyIdeas()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var ideas = await _ideaService.GetIdeasByStaffAsync(userId!);
			return View(ideas ?? new List<IdeaInfoDto>());
		}

		//  Browse All Ideas 
		public async Task<IActionResult> BrowseIdeas()
		{
			var ideas = await _ideaService.GetAllIdeasAsync();
			return View(ideas);
		}

		//  Submit Idea 
		[HttpGet]
		public async Task<IActionResult> SubmitIdea()
		{
			if (HttpContext.Session.GetString("AgreedTerms") != "true")
				return RedirectToAction(nameof(Terms));

			if (await _ideaService.IsClosureDatePassedAsync())
			{
				TempData["ErrorMessage"] = "The closure date for new ideas has passed.";
				return RedirectToAction(nameof(Dashboard));
			}

			var categories = await _categoryService.GetAllActiveAsync();
			ViewBag.Categories = new SelectList(categories, "Id", "Name");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SubmitIdea(IdeaViewModel model)
		{
			if (!User.Identity!.IsAuthenticated)
				return RedirectToAction("Login", "Account");

			if (HttpContext.Session.GetString("AgreedTerms") != "true")
				return RedirectToAction(nameof(Terms));

			if (await _ideaService.IsClosureDatePassedAsync())
			{
				TempData["ErrorMessage"] = "The closure date for new ideas has passed.";
				return RedirectToAction(nameof(Dashboard));
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
				ModelState.AddModelError("", "User not found.");

			if (ModelState.IsValid)
			{
				List<string>? filePaths = null;

				if (Request.Form.Files.Count > 0)
				{
					filePaths = new List<string>();
					var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
					if (!Directory.Exists(uploadsFolder))
						Directory.CreateDirectory(uploadsFolder);

					foreach (var file in Request.Form.Files)
					{
						var uniqueFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
						var filePath = Path.Combine(uploadsFolder, uniqueFileName);
						using var stream = new FileStream(filePath, FileMode.Create);
						await file.CopyToAsync(stream);
						filePaths.Add(Path.Combine("uploads", uniqueFileName));
					}
				}

				var dto = new IdeaCreateDto
				{
					Text = model.Text,
					Description = model.Description,
					CategoryId = model.CategoryId,
					IsAnonymous = model.IsAnonymous,
					FilePaths = filePaths
				};

				var result = await _ideaService.CreateIdeaAsync(dto, userId!);
				if (result)
				{
					TempData["SuccessMessage"] = "Idea submitted successfully!";
					return RedirectToAction(nameof(MyIdeas));
				}

				ModelState.AddModelError("", "Failed to submit idea.");
			}

			var categories = await _categoryService.GetAllActiveAsync();
			ViewBag.Categories = new SelectList(categories, "Id", "Name", model.CategoryId);
			return View(model);
		}

		//  Vote (Thumbs Up/Down) via AJAX 
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