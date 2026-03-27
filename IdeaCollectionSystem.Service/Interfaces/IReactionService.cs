using IdeaCollectionSystem.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.Service.Interfaces
{
    public interface IReactionService
    {
        // Lấy thống kê lượt thả cảm xúc của 1 ý tưởng
        Task<ReactionSummaryDto> GetReactionSummaryAsync(Guid ideaId, string? currentUserId);

        // Xử lý logic Bật/Tắt/Đổi cảm xúc
        Task<bool> ToggleReactionAsync(ReactionToggleDto dto, string userId);
    }
}
