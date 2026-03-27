using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDepartments()
    {
        var departments = await _departmentService.GetAllDepartmentsAsync();
        return Ok(departments);
    }

    // Ràng buộc id bắt buộc phải là chuẩn Guid
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDepartmentById(Guid id)
    {
        var department = await _departmentService.GetDepartmentByIdAsync(id);
        if (department == null)
            return NotFound(new { message = "Không tìm thấy phòng ban." });

        return Ok(department);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDepartment([FromBody] DepartmentCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _departmentService.CreateDepartmentAsync(dto);
        if (!result)
            return BadRequest(new { message = "Không thể tạo phòng ban. Vui lòng thử lại." });

        return Ok(new { message = "Tạo phòng ban thành công." });
    }

    // Ràng buộc id bắt buộc phải là chuẩn Guid
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] DepartmentUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _departmentService.UpdateDepartmentAsync(id, dto);
        if (!result)
            return BadRequest(new { message = "Cập nhật thất bại. Phòng ban có thể không tồn tại." });

        return Ok(new { message = "Cập nhật phòng ban thành công." });
    }

    // Ràng buộc id bắt buộc phải là chuẩn Guid
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        var result = await _departmentService.DeleteDepartmentAsync(id);
        if (!result)
            return BadRequest(new { message = "Xóa thất bại. Phòng ban không tồn tại hoặc đang chứa dữ liệu." });

        return Ok(new { message = "Xóa phòng ban thành công." });
    }
}