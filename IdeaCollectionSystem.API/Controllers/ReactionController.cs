using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdeaCollectionSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Bảo vệ toàn bộ endpoint
public class ReactionController : ControllerBase
{
    private readonly IReactionService _reactionService;

    public ReactionController(IReactionService reactionService)
    {
        _reactionService = reactionService;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // GET: api/reaction/idea/{ideaId}
    // Lấy tổng số lượt Like/Dislike (Có thể cho phép AllowAnonymous nếu muốn ai cũng xem được số like)
    [AllowAnonymous]
    [HttpGet("idea/{ideaId:guid}")]
    public async Task<IActionResult> GetIdeaReactions(Guid ideaId)
    {
        // Vẫn truyền CurrentUserId vào để biết User hiện tại đã thả tim chưa (nếu họ đã đăng nhập)
        var summary = await _reactionService.GetReactionSummaryAsync(ideaId, CurrentUserId);
        return Ok(summary);
    }

    // POST: api/reaction/toggle
    // Bật/Tắt cảm xúc (Bắt buộc đăng nhập)
    [HttpPost("toggle")]
    public async Task<IActionResult> ToggleReaction([FromBody] ReactionToggleDto dto)
    {
        if (string.IsNullOrEmpty(CurrentUserId)) return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.ReactionType))
            return BadRequest(new { message = "ReactionType không được để trống." });

        var result = await _reactionService.ToggleReactionAsync(dto, CurrentUserId);

        if (!result)
            return BadRequest(new { message = "Lỗi khi xử lý thao tác. Ý tưởng có thể không tồn tại." });

        return Ok(new { message = "Thao tác thành công." });
    }
}