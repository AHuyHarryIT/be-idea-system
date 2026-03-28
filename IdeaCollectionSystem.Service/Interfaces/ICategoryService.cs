using IdeaCollectionSystem.Service.Models.DTOs;

namespace IdeaCollectionSystem.Service.Interfaces
{
	public interface ICategoryService
	{
		Task<IEnumerable<CategoryDto>> GetAllActiveAsync();

		Task<bool> CreateAsync(string name);

		Task<bool> DeleteIfUnusedAsync(Guid id);
        Task<bool> UpdateAsync(Guid id, string newName);
    }
}