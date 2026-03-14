using IdeaCollectionIdea.Common.Constants;
using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize] 
	public class CategoriesController : ControllerBase
	{
		private readonly ICategoryService _categoryService;

		public CategoriesController(ICategoryService categoryService)
		{
			_categoryService = categoryService;
		}

		// GET: api/categories
		[HttpGet]
		public async Task<IActionResult> GetCategories()
		{
			// list staff when submit idea
			var categories = await _categoryService.GetAllActiveAsync();
			return Ok(categories);
		}

		// POST: api/categories
		[HttpPost]
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
		public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Name))
				return BadRequest(new { message = "The category name cannot be left blank." });

			await _categoryService.CreateAsync(request.Name);
			return Ok(new { message = "Create Categories Sucessfully"});
			 
		}

		// DELETE: api/categories/{id}
		[HttpDelete("{id}")]
		[Authorize(Roles = RoleConstants.Administrator + "," + RoleConstants.QAManager)]
		public async Task<IActionResult> DeleteCategory([FromRoute] Guid id)
		{
			var success = await _categoryService.DeleteIfUnusedAsync(id);
			if (!success)
				return BadRequest(new { message = "Cannot be deleted. This category is being used for existing ideas." });

			return Ok(new { message = "Category deletion successful." });
		}
	}

	public class CreateCategoryRequest
	{
		public string Name { get; set; } = string.Empty;
	}
}