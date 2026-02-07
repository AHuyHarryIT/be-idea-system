// IdeaCollectionSystem/Controllers/QAManagerController.cs
using IdeaCollectionIdea.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.MVC.Controllers
{
	[Authorize(Policy = PolicyConstants.QAManagerOnly)]
	public class QAManagerController : Controller
	{
		public IActionResult Dashboard()
		{
			ViewBag.PageTitle = "QA Manager Dashboard";
			return View();
		}

		public IActionResult AllIdeas()
		{
			return View();
		}

		public IActionResult Statistics()
		{
			return View();
		}

		public IActionResult ExportData()
		{
			return View();
		}

		public IActionResult Categories()
		{
			return View();
		}

		public IActionResult ClosureDates()
		{
			return View();
		}
	}
}