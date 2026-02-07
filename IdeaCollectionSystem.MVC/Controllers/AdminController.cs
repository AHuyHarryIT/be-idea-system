// IdeaCollectionSystem/Controllers/AdminController.cs
using IdeaCollectionIdea.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.MVC.Controllers
{
	[Authorize(Policy = PolicyConstants.AdminOnly)]
	public class AdminController : Controller
	{
		public IActionResult Dashboard()
		{
			ViewBag.PageTitle = "Administrator Dashboard";
			return View();
		}

		public IActionResult Users()
		{
			return View();
		}

		public IActionResult Departments()
		{
			return View();
		}

		public IActionResult Settings()
		{
			return View();
		}

		public IActionResult SystemLogs()
		{
			return View();
		}
	}
}