using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DepartmentController : ControllerBase
	{
		private readonly IUserService userService;

		public DepartmentController(IUserService departmentService)
		{
			userService = departmentService;
		}

		// GET: api/department
		[HttpGet]
		[Authorize] 
		public async Task<IActionResult> GetDepartments()
		{
			var users = await userService.GetAllUsersAsync();
			return Ok(users);
		}
	}
}