using IdeaCollectionSystem.ApplicationCore.Entitites;
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdeaCollectionSystem.Service.Services
{
	public class DepartmentService : IDepartmentService
	{
		private readonly IdeaCollectionDbContext _context;

		public DepartmentService(IdeaCollectionDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
		{
		
			return await _context.Departments.ToListAsync();
		}

		// Add Department
		public async Task<bool> CreateDepartmentAsync(String name)
		{
			var Department = new Department
			{
				Id = Guid.NewGuid(),
				Name = name
			};
			await _context.SaveChangesAsync();
			return true;
		}
	
	}
}