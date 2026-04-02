using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
public class DepartmentController : ControllerBase
{
	private readonly IDepartmentService _departmentService;

	public DepartmentController(IDepartmentService departmentService)
	{
		_departmentService = departmentService;
	}

	[HttpGet]
	public async Task<IActionResult> GetAllDepartments()
	{
		var departments = await _departmentService.GetAllDepartmentsAsync();
		return Ok(departments);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetDepartmentById(Guid id)
	{
		var department = await _departmentService.GetDepartmentByIdAsync(id);
		if (department == null)
			return NotFound(new { message = "Department not found" });

		return Ok(department);
	}

	[HttpPost]
	public async Task<IActionResult> CreateDepartment([FromBody] DepartmentCreateDto dto)
	{
		if (string.IsNullOrWhiteSpace(dto.Name))
			return BadRequest(new { message = "The department Name is required." });

		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		var result = await _departmentService.CreateDepartmentAsync(dto);
		if (!result)
			return BadRequest(new { message = "Can't create Department " });

		return Ok(new { message = "Create Department successfull.", data = dto });
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] DepartmentUpdateDto dto)
	{
		if (string.IsNullOrWhiteSpace(dto.Name))
			return BadRequest(new { message = "The department Name is required." });

		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		var result = await _departmentService.UpdateDepartmentAsync(id, dto);
		if (!result)
			return BadRequest(new { message = "Update failed. The department may not exist." });

		return Ok(new { message = "Room assignment update successful.." });
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteDepartment(Guid id)
	{
		var result = await _departmentService.DeleteDepartmentAsync(id);
		if (!result)
			return BadRequest(new { message = "Deletion failed. The department does not exist or is already occupied." });

		return Ok(new { message = "Department deletion successful." });
	}
}