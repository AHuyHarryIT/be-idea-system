using IdeaCollectionSystem.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.Service.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDto?> GetDepartmentByIdAsync(Guid id);
        Task<bool> CreateDepartmentAsync(DepartmentCreateDto dto);
        Task<bool> UpdateDepartmentAsync(Guid id, DepartmentUpdateDto dto);
        Task<bool> DeleteDepartmentAsync(Guid id);
    }
}
