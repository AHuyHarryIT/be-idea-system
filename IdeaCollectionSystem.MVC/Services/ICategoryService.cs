using IdeaCollectionSystem.ApplicationCore.Entitites;

namespace IdeaCollectionSystem.MVC.Services
{
	public interface ICategoryService
	{
		Task<IReadOnlyList<Category>> GetCategoriesAsync();
		Task<Category?> GetCategoryAsync(Guid id);
		Task<Category> CreateCategoryAsync(string name);
		Task<Category?> UpdateCategoryAsync(Guid id, string name);
		Task<bool> DeleteCategoryAsync(Guid id);
	}
}
