using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DepartmentController : ControllerBase
	{
		private readonly IDepartmentService _departmentService;

		public DepartmentController (IDepartmentService departmentService)
		{
			_departmentService = departmentService;
		}

		// GET: api/department
		[HttpGet("departments")] 
		public async Task<IActionResult> GetDepartments()
		{
			
			var departments = await _departmentService.GetAllDepartmentsAsync();

			if (departments == null || !departments.Any())
			{
				return NotFound(new { message = "No departments were found." });
			}

			return Ok(departments);
		}
	}
}