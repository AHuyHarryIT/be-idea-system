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

		public IActionResult Dashboard()
		{
			return View();
		}

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

			// Kiểm tra session terms
			if (HttpContext.Session.GetString("AgreedTerms") != "true")
			{
				return RedirectToAction(nameof(Terms));
			}

			// Kiểm tra closure date
			if (await _ideaService.IsClosureDatePassedAsync())
			{
				TempData["ErrorMessage"] = "The closure date for new ideas has passed.";
				return RedirectToAction(nameof(Dashboard));
			}

			// Lấy claims từ user
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var departmentIdClaim = User.FindFirstValue("DepartmentId");

			// Kiểm tra claims
			if (string.IsNullOrEmpty(userIdClaim))
			{
				ModelState.AddModelError("", "User not found.");
			}

			if (string.IsNullOrEmpty(departmentIdClaim))
			{
				ModelState.AddModelError("", "Department not found. Please contact admin.");
			}

			// Kiểm tra ModelState sau khi đã kiểm tra claims
			if (ModelState.IsValid)
			{
				int departmentId = int.Parse(departmentIdClaim!);

				// Xử lý file upload nếu có
				List<string>? filePaths = null;
				if (Request.Form.Files.Count > 0)
				{
					filePaths = new List<string>();
					foreach (var file in Request.Form.Files)
					{
						// TODO: Xử lý lưu file và lấy đường dẫn
						// filePaths.Add(savedFilePath);
					}
				}

				var dto = new IdeaCreateDto
				{
					Title = model.Title,
					Description = model.Description,
					CategoryId = Guid.Parse(model.CategoryId.ToString()),
					DepartmentId = departmentId,
					IsAnonymous = model.IsAnonymous,
					FilePaths = filePaths // Gán file paths nếu có
				};

				var result = await _ideaService.CreateIdeaAsync(dto, userIdClaim!);

				if (result)
				{
					TempData["SuccessMessage"] = "Idea submitted successfully!";
					return RedirectToAction(nameof(MyIdea));
				}

				ModelState.AddModelError("", "Failed to submit idea.");
			}

			// Nếu có lỗi, reload categories
			var categories = await _categoryService.GetAllActiveAsync();
			ViewBag.Categories = new SelectList(categories, "Id", "Name", model.CategoryId);
			return View(model);
		}


		[HttpGet]
		public async Task<IActionResult> MyIdea()
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdClaim))
			{
				return RedirectToAction("Login", "Account");
			}

			var ideas = await _ideaService.GetIdeasByUserAsync(userIdClaim);
			return View(ideas);
		}
	}
}