using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
	private readonly ICategoryService _categoryService;

	public CategoriesController(ICategoryService categoryService)
	{
		_categoryService = categoryService;
	}

	// GET api/categories
	[HttpGet]
	[Authorize]
	public async Task<IActionResult> GetAll()
	{
		var categories = await _categoryService.GetAllActiveAsync();
		return Ok(categories);
	}

	// POST api/categories
	[HttpPost]
	[Authorize(Roles = "Administrator,QAManager")]
	public async Task<IActionResult> Create([FromBody] string name)
	{
		var result = await _categoryService.CreateAsync(name);
		if (!result)
			return BadRequest(new { message = "Không thể tạo category." });

		return Ok(new { message = "Category đã được tạo." });
	}

	// DELETE api/categories/{id}
	[HttpDelete("{id}")]
	[Authorize(Roles = "Administrator,QAManager")]
	public async Task<IActionResult> Delete(Guid id)
	{
		var result = await _categoryService.DeleteIfUnusedAsync(id);
		if (!result)
			return BadRequest(new { message = "Category đang được sử dụng, không thể xóa." });

		return Ok(new { message = "Category đã được xóa." });
	}
}