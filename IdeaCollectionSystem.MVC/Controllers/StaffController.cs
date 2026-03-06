using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.ApplicationCore.Entitites;
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

		public IActionResult Dashboard()
		{
			return View();
		}

		// Terms
		[HttpGet]
		public IActionResult Terms()
		{
			return View();
		}

		[HttpPost]
		public IActionResult AcceptTerms(bool agree)
		{
			if (agree)
			{
				HttpContext.Session.SetString("AgreedTerms", "true");
				return RedirectToAction(nameof(SubmitIdea));
			}
			ModelState.AddModelError("", "You must agree to the Terms and Conditions to proceed.");
			return View("Terms");
		}

		// My Ideas
		public async Task<IActionResult> MyIdeas()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var ideas = await _ideaService.GetIdeasByStaffAsync(userId!);

			return View(ideas ?? new List<IdeaInfoDto>());
		}

		// Submit Idea
		[HttpGet]
		public async Task<IActionResult> SubmitIdea()
		{
		
			if (HttpContext.Session.GetString("AgreedTerms") != "true")
			{
				return RedirectToAction(nameof(Terms));
			}

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
			{
				return RedirectToAction("Login", "Account");
			}

			if (HttpContext.Session.GetString("AgreedTerms") != "true")
			{
				return RedirectToAction(nameof(Terms));
			}

			if (await _ideaService.IsClosureDatePassedAsync())
			{
				TempData["ErrorMessage"] = "The closure date for new ideas has passed.";
				return RedirectToAction(nameof(Dashboard));
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrEmpty(userId))
			{
				ModelState.AddModelError("", "User not found.");
			}

			if (ModelState.IsValid)
			{
				List<string>? filePaths = null;

				if (Request.Form.Files.Count > 0)
				{
					filePaths = new List<string>();

					foreach (var file in Request.Form.Files)
					{
						// Save the file to a location and get the saved path
						var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
						if (!Directory.Exists(uploadsFolder))
						{
							Directory.CreateDirectory(uploadsFolder);
						}
						var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
						var filePath = Path.Combine(uploadsFolder, uniqueFileName);

						using (var stream = new FileStream(filePath, FileMode.Create))
						{
							await file.CopyToAsync(stream);
						}
						var savedPath = Path.Combine("uploads", uniqueFileName);
						filePaths.Add(savedPath);
					}
				}

				var dto = new IdeaCreateDto
				{
					Text = model.Title,
					Description = model.Description,
					CategoryId = model.CategoryId,
					IsAnonymous = model.IsAnonymous,
					FilePaths = filePaths
				};

				var result = await _ideaService.CreateIdeaAsync(dto, userId);

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

	}
}