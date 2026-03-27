using IdeaCollectionSystem.ApplicationCore.Entitites; // Chú ý using Entity của bạn
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IdeaCollectionSystem.Service.Services;

public class ReactionService : IReactionService
{
    private readonly IdeaCollectionDbContext _context;

    public ReactionService(IdeaCollectionDbContext context)
    {
        _context = context;
    }

    public async Task<ReactionSummaryDto> GetReactionSummaryAsync(Guid ideaId, string? currentUserId)
    {
        // Lấy tất cả reaction của Idea này
        var reactions = await _context.IdeaReactions
            .AsNoTracking()
            .Where(r => r.IdeaId == ideaId)
            .ToListAsync();

        var summary = new ReactionSummaryDto
        {
            IdeaId = ideaId,
            TotalLikes = reactions.Count(r => r.Reaction.ToLower() == "like"),
            TotalDislikes = reactions.Count(r => r.Reaction.ToLower() == "dislike"),
        };

        // Nếu user đang đăng nhập, kiểm tra xem họ đã thả gì chưa
        if (!string.IsNullOrEmpty(currentUserId))
        {
            var userReaction = reactions.FirstOrDefault(r => r.UserId == currentUserId);
            summary.CurrentUserReaction = userReaction?.Reaction;
        }

        return summary;
    }

    public async Task<bool> ToggleReactionAsync(ReactionToggleDto dto, string userId)
    {
        // Kiểm tra xem user này đã thả cảm xúc cho Idea này chưa
        var existingReaction = await _context.IdeaReactions
            .FirstOrDefaultAsync(r => r.IdeaId == dto.IdeaId && r.UserId == userId);

        if (existingReaction != null)
        {
            // Nếu bấm lại y hệt cảm xúc cũ -> Hủy bỏ (Xóa)
            if (existingReaction.Reaction.Equals(dto.ReactionType, StringComparison.OrdinalIgnoreCase))
            {
                _context.IdeaReactions.Remove(existingReaction);
            }
            else
            {
                // Nếu đổi từ Like sang Dislike (hoặc ngược lại) -> Cập nhật
                existingReaction.Reaction = dto.ReactionType;
                existingReaction.UpdatedAt = DateTime.UtcNow;
                _context.IdeaReactions.Update(existingReaction);
            }
        }
        else
        {
            // Nếu chưa từng thả -> Thêm mới
            var newReaction = new IdeaReaction
            {
                Id = Guid.NewGuid(),
                IdeaId = dto.IdeaId,
                UserId = userId,
                Reaction = dto.ReactionType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _context.IdeaReactions.AddAsync(newReaction);
        }

        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}