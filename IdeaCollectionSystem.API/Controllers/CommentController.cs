using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdeaCollectionSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Bắt buộc đăng nhập
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    // Tiện ích lấy UserId từ Token
    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // GET: api/comment/idea/{ideaId}
    [HttpGet("idea/{ideaId:guid}")]
    public async Task<IActionResult> GetCommentsByIdea(Guid ideaId)
    {
        var comments = await _commentService.GetCommentsByIdeaIdAsync(ideaId);
        return Ok(comments);
    }

    // POST: api/comment
    [HttpPost]
    public async Task<IActionResult> CreateComment([FromBody] CommentCreateDto dto)
    {
        if (string.IsNullOrEmpty(CurrentUserId)) return Unauthorized();

        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _commentService.CreateCommentAsync(dto, CurrentUserId);
        if (!result)
            return BadRequest(new { message = "Không thể tạo bình luận. Vui lòng thử lại." });

        return Ok(new { message = "Đã gửi bình luận thành công." });
    }

    // PUT: api/comment/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateComment(Guid id, [FromBody] CommentUpdateDto dto)
    {
        if (string.IsNullOrEmpty(CurrentUserId)) return Unauthorized();
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _commentService.UpdateCommentAsync(id, dto, CurrentUserId);
        if (!result)
            return BadRequest(new { message = "Cập nhật thất bại. Bạn không có quyền hoặc bình luận không tồn tại." });

        return Ok(new { message = "Cập nhật bình luận thành công." });
    }

    // DELETE: api/comment/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        if (string.IsNullOrEmpty(CurrentUserId)) return Unauthorized();

        var result = await _commentService.DeleteCommentAsync(id, CurrentUserId);
        if (!result)
            return BadRequest(new { message = "Xóa thất bại. Bạn không có quyền hoặc bình luận không tồn tại." });

        return Ok(new { message = "Xóa bình luận thành công." });
    }
}