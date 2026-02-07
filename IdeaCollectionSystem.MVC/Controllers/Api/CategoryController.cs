using IdeaCollectionSystem.Models;
using IdeaCollectionSystem.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.MVC.Controllers.Api
{
	[ApiController]
	[Route("api/categories")]
	public class CategoryController : ControllerBase
	{
		private readonly CategoryService _categoryService;

		public CategoryController(CategoryService categoryService)
		{
			_categoryService = categoryService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
		{
			var categories = await _categoryService.GetCategoriesAsync();
			var results = categories
				.Select(category => new CategoryDto
				{
					Id = category.Id,
					Name = category.Name ?? string.Empty
				});

			return Ok(results);
		}

		[HttpGet("{id:guid}")]
		public async Task<ActionResult<CategoryDto>> GetCategory(Guid id)
		{
			var category = await _categoryService.GetCategoryAsync(id);

			if (category == null)
			{
				return NotFound();
			}

			return Ok(new CategoryDto
			{
				Id = category.Id,
				Name = category.Name ?? string.Empty
			});
		}

		[HttpPost]
		public async Task<ActionResult<CategoryDto>> CreateCategory(CategoryDto request)
		{
			if (request == null || string.IsNullOrWhiteSpace(request.Name))
			{
				return BadRequest("Name is required.");
			}

			var category = await _categoryService.CreateCategoryAsync(request.Name.Trim());
			var response = new CategoryDto
			{
				Id = category.Id,
				Name = category.Name ?? string.Empty
			};

			return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, response);
		}

		[HttpPut("{id:guid}")]
		public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, CategoryDto request)
		{
			if (request == null || string.IsNullOrWhiteSpace(request.Name))
			{
				return BadRequest("Name is required.");
			}

			var category = await _categoryService.UpdateCategoryAsync(id, request.Name.Trim());

			if (category == null)
			{
				return NotFound();
			}

			return Ok(new CategoryDto
			{
				Id = category.Id,
				Name = category.Name ?? string.Empty
			});
		}

		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> DeleteCategory(Guid id)
		{
			var deleted = await _categoryService.DeleteCategoryAsync(id);

			if (!deleted)
			{
				return NotFound();
			}

			return NoContent();
		}
	}
}
