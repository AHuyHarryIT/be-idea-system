using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models.DTOs;
using IdeaCollectionSystem.ApplicationCore.Entitites;
using Microsoft.EntityFrameworkCore;

namespace IdeaCollectionSystem.Service.Services
{
	public class CategoryService : ICategoryService
	{
		private readonly IdeaCollectionDbContext _context;

		public CategoryService(IdeaCollectionDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<CategoryDto>> GetAllActiveAsync()
		{
			return await _context.Categories
				.Select(c => new CategoryDto
				{
					Id = c.Id,
					Name = c.Name
				})
				.ToListAsync();
		}

		public async Task<bool> CreateAsync (String name)
		{
			var category = new Category
			{
				Id = Guid.NewGuid(),
				Name = name,
			};
			_context.Categories.Add(category);	
			return await _context.SaveChangesAsync() > 0;
		}

        public async Task<bool> UpdateAsync(Guid id, string newName)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return false;

            var isNameExist = await _context.Categories.AnyAsync(c => c.Name.ToLower() == newName.ToLower() && c.Id != id);
            if (isNameExist)
                return false;

            category.Name = newName;
            _context.Categories.Update(category);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteIfUnusedAsync(Guid id)
		{
			var category = await _context.Categories
				.FirstOrDefaultAsync(c => c.Id == id);

			if (category == null) return false;

			// Check if any ideas reference this category
			var hasIdeas = await _context.Ideas.AnyAsync(i => i.CategoryId == id);
			if (hasIdeas) return false;

			_context.Categories.Remove(category);
			return await _context.SaveChangesAsync() > 0;
		}
	}
}