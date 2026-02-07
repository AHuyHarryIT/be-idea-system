using IdeaCollectionSystem.ApplicationCore.Entitites;
using IdeaCollectionSystem.Datalayer;
using Microsoft.EntityFrameworkCore;

namespace IdeaCollectionSystem.MVC.Services
{
	public class CategoryService
	{
		private readonly IdeaCollectionDbContext _dbContext;

		public CategoryService(IdeaCollectionDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<IReadOnlyList<Category>> GetCategoriesAsync()
		{
			return await _dbContext.Categories
				.AsNoTracking()
				.Where(category => category.DeletedAt == null)
				.OrderBy(category => category.Name)
				.ToListAsync();
		}

		public async Task<Category?> GetCategoryAsync(Guid id)
		{
			return await _dbContext.Categories
				.AsNoTracking()
				.FirstOrDefaultAsync(category => category.Id == id && category.DeletedAt == null);
		}

		public async Task<Category> CreateCategoryAsync(string name)
		{
			var now = DateTime.Now;
			var category = new Category
			{
				Id = Guid.NewGuid(),
				Name = name,
				CreatedAt = now,
				UpdateAt = now
			};

			_dbContext.Categories.Add(category);
			await _dbContext.SaveChangesAsync();

			return category;
		}

		public async Task<Category?> UpdateCategoryAsync(Guid id, string name)
		{
			var category = await _dbContext.Categories
				.FirstOrDefaultAsync(existing => existing.Id == id && existing.DeletedAt == null);

			if (category == null)
			{
				return null;
			}

			category.Name = name;
			category.UpdateAt = DateTime.Now;

			await _dbContext.SaveChangesAsync();

			return category;
		}

		public async Task<bool> DeleteCategoryAsync(Guid id)
		{
			var category = await _dbContext.Categories
				.FirstOrDefaultAsync(existing => existing.Id == id && existing.DeletedAt == null);

			if (category == null)
			{
				return false;
			}

			category.DeletedAt = DateTime.Now;
			await _dbContext.SaveChangesAsync();

			return true;
		}
	}
}
