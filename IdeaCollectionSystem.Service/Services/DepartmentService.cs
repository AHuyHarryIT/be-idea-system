using IdeaCollectionSystem.ApplicationCore.Entitites;
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IdeaCollectionSystem.Service.Services;

public class DepartmentService : IDepartmentService
{
	private readonly IdeaCollectionDbContext _context;

	public DepartmentService(IdeaCollectionDbContext context)
	{
		_context = context;
	}

	public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync()
	{
		return await _context.Departments
			.AsNoTracking()
			.Select(d => new DepartmentDto
			{
				Id = d.Id,
				Name = d.Name,
				Description = d.Description
			})
			.ToListAsync();
	}

	public async Task<DepartmentDto?> GetDepartmentByIdAsync(Guid id) // Đổi sang Guid
	{
		return await _context.Departments
			.AsNoTracking()
			.Where(d => d.Id == id)
			.Select(d => new DepartmentDto
			{
				Id = d.Id,
				Name = d.Name,
				Description = d.Description
			})
			.FirstOrDefaultAsync();
	}

	public async Task<bool> CreateDepartmentAsync(DepartmentCreateDto dto)
	{
		try
		{
			var department = new Department
			{
				Id = Guid.NewGuid(), // Tự động sinh Guid mới (Hoặc để DB tự sinh tuỳ cấu hình của bạn)
				Name = dto.Name,
				Description = dto.Description
			};

			await _context.Departments.AddAsync(department);
			var result = await _context.SaveChangesAsync();

			return result > 0;
		}
		catch
		{
			return false;
		}
	}

	public async Task<bool> UpdateDepartmentAsync(Guid id, DepartmentUpdateDto dto) // Đổi sang Guid
	{
		var department = await _context.Departments.FindAsync(id);
		if (department == null) return false;

		department.Name = dto.Name;
		department.Description = dto.Description;

		_context.Departments.Update(department);
		var result = await _context.SaveChangesAsync();

		return result > 0;
	}

	public async Task<bool> DeleteDepartmentAsync(Guid id) // Đổi sang Guid
	{
		var department = await _context.Departments.FindAsync(id);
		if (department == null) return false;

		_context.Departments.Remove(department);
		var result = await _context.SaveChangesAsync();

		return result > 0;
	}
}