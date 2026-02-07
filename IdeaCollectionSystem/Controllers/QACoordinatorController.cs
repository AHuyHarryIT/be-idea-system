// IdeaCollectionSystem/Controllers/QACoordinatorController.cs
using IdeaCollectionIdea.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.MVC.Controllers
{
	[Authorize(Policy = PolicyConstants.QACoordinatorOnly)]
	public class QACoordinatorController : Controller
	{
		public IActionResult Dashboard()
		{
			ViewBag.PageTitle = "QA Coordinator Dashboard";
			return View();
		}

		public IActionResult DepartmentIdeas()
		{
			return View();
		}

		public IActionResult DepartmentStatistics()
		{
			return View();
		}

		public IActionResult ManageStaff()
		{
			return View();
		}
	}
}