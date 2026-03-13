using IdeaCollectionSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaCollectionSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class UsersController : ControllerBase
{
    private readonly IQAManagerService _qaService;

    public UsersController(IQAManagerService qaService)
    {
        _qaService = qaService;
    }

    // GET api/users
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _qaService.GetAllUsersAsync();
        return Ok(users);
    }

    // PUT api/users/{id}/role
    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleDto dto)
    {
        var result = await _qaService.UpdateUserRoleAsync(id, dto.Role);
        if (!result) return NotFound(new { message = "Không tìm thấy user." });
        return Ok(new { message = "Cập nhật role thành công." });
    }

    // DELETE api/users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _qaService.DeleteUserAsync(id);
        if (!result) return NotFound(new { message = "Không tìm thấy user." });
        return Ok(new { message = "Xóa user thành công." });
    }
}

public class UpdateRoleDto
{
    public string Role { get; set; } = string.Empty;
}