using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CategoriesController : ControllerBase
	{
		private static readonly List<CategoryDto> Categories = new();
		private static readonly object CategoriesLock = new();

		[HttpGet]
		public ActionResult<IEnumerable<CategoryDto>> GetAll()
		{
			List<CategoryDto> categoriesSnapshot;
			lock (CategoriesLock)
			{
				categoriesSnapshot = Categories.ToList();
			}

			return Ok(categoriesSnapshot);
		}

		[HttpGet("{id:guid}")]
		public ActionResult<CategoryDto> GetById(Guid id)
		{
			CategoryDto? category;
			lock (CategoriesLock)
			{
				category = Categories.FirstOrDefault(c => c.Id == id);
			}
			return category is null ? NotFound() : Ok(category);
		}
	}
}
