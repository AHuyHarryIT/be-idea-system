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

		// Get Categories with pagination and optional search
		public async Task<PagedResult<CategoryDto>> GetCategoriesPagedAsync(PaginationFilter filter)
		{
			var query = _context.Categories.AsNoTracking();
			if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
			{
				var search = filter.SearchTerm.ToLower().Trim();
				query = query.Where(c => c.Name.ToLower().Contains(search));
			}


			var totalCount = await query.CountAsync();


			var categories = await query
				.OrderByDescending(c => c.CreatedAt) 
				.OrderBy(c => c.Name)
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.ToListAsync();


			var result = categories.Select(c => new CategoryDto
			{
				Id = c.Id,
				Name = c.Name			
			}).ToList();

			return new PagedResult<CategoryDto>(result, totalCount, filter.PageNumber, filter.PageSize);
		}


		// Create Category
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