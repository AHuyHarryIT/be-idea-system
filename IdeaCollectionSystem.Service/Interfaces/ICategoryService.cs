using IdeaCollectionSystem.Service.Models.DTOs;

namespace IdeaCollectionSystem.Service.Interfaces
{
	public interface ICategoryService
	{

		Task<PagedResult<CategoryDto>> GetCategoriesPagedAsync(PaginationFilter filter);
		//Task<PagedResult<CategoryDto>> GetAllActiveAsync(PaginationFilter filter);

		Task<bool> CreateAsync(string name);

		Task<bool> DeleteIfUnusedAsync(Guid id);
        Task<bool> UpdateAsync(Guid id, string newName);
    }
}