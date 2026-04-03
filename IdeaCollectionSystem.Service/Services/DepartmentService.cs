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


	public async Task<PagedResult<DepartmentDto>> GetAllDepartmentsAsync(PaginationFilter filter)
	{
		var query = _context.Departments.AsNoTracking();


		if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
		{
			var search = filter.SearchTerm.ToLower().Trim();
			query = query.Where(d => d.Name.ToLower().Contains(search));
		}

		// 2. Count tổng số lượng record
		var totalCount = await query.CountAsync();

		
		var departments = await query
			.OrderBy(d => d.Name)
			.Skip((filter.PageNumber - 1) * filter.PageSize)
			.Take(filter.PageSize)
			.ToListAsync();

		// 4. Map sang DTO
		var result = departments.Select(d => new DepartmentDto
		{
			Id = d.Id,
			Name = d.Name,
			Description = d.Description
		}).ToList();

		// 5. Trả về PagedResult
		return new PagedResult<DepartmentDto>(result, totalCount, filter.PageNumber, filter.PageSize);
	}

	public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync()
	{
		var departments = await _context.Departments
			.AsNoTracking()
			.OrderBy(d => d.Name)
			.ToListAsync();

		return departments.Select(d => new DepartmentDto
		{
			Id = d.Id,
			Name = d.Name
		});
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