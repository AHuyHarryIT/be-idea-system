using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers;

[ApiController]
[Route("api/categories")]
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
	public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request) 
	{

		if (string.IsNullOrWhiteSpace(request.Name))
			return BadRequest(new { message = "Category name cannot be empty." });

		var result = await _categoryService.CreateAsync(request.Name);
		if (!result)
			return BadRequest(new { message = "Failed to create category." });

		return Ok(new { message = "Category created successfully." });
	}



	// PUT api/categories/{id}
	[HttpPut("{id}")]
	[Authorize(Roles = "Administrator,QAManager")]
	public async Task<IActionResult> Update(Guid id, [FromBody] CreateCategoryRequest request) 
	{
		if (string.IsNullOrWhiteSpace(request.Name))
			return BadRequest(new { message = "Category name cannot be empty." });

		var result = await _categoryService.UpdateAsync(id, request.Name);
		if (!result)
			return BadRequest(new { message = "Failed to update category. The category might not exist or the name is already in use." });

		return Ok(new { message = "Category updated successfully." });
	}

	// DELETE api/categories/{id}
	[HttpDelete("{id}")]
	[Authorize(Roles = "Administrator,QAManager")]
	public async Task<IActionResult> Delete(Guid id)
	{
		var result = await _categoryService.DeleteIfUnusedAsync(id);
		if (!result)
			return BadRequest(new { message = "Category is currently in use and cannot be deleted." });

		return Ok(new { message = "Category deleted successfully." });
	}
}