using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CategoriesController : ControllerBase
	{
		private static readonly List<CategoryDto> Categories = new();

		[HttpGet]
		public ActionResult<IEnumerable<CategoryDto>> GetAll()
		{
			return Ok(Categories);
		}

		[HttpGet("{id:guid}")]
		public ActionResult<CategoryDto> GetById(Guid id)
		{
			var category = Categories.FirstOrDefault(c => c.Id == id);
			return category is null ? NotFound() : Ok(category);
		}

		[HttpPost]
		public ActionResult<CategoryDto> Create([FromBody] CategoryDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Name))
			{
				return BadRequest("Name is required.");
			}

			var category = new CategoryDto
			{
				Id = Guid.NewGuid(),
				Name = dto.Name.Trim(),
				CreatedAt = DateTime.UtcNow,
				UpdateAt = DateTime.UtcNow
			};

			Categories.Add(category);

			return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
		}
	}
}
