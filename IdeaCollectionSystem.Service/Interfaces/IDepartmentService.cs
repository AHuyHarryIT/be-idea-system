using IdeaCollectionSystem.ApplicationCore.Entitites;

namespace IdeaCollectionSystem.Service.Interfaces
{
	public interface IDepartmentService
	{
		Task<IEnumerable<Department>> GetAllDepartmentsAsync();

	}
}