using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.Service.Models
{
    // DTO dùng để nhận request thả reaction từ người dùng
    public class ReactionToggleDto
    {
        public Guid IdeaId { get; set; }

        // Ví dụ: "Like", "Dislike", "Heart" (Nên thống nhất ở Frontend)
        public string ReactionType { get; set; } = string.Empty;
    }

    // DTO dùng để trả về tổng hợp lượt thả cảm xúc cho 1 Idea (Rất cần cho Frontend)
    public class ReactionSummaryDto
    {
        public Guid IdeaId { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDislikes { get; set; }
        // Có thể thêm các loại Reaction khác nếu hệ thống bạn có

        // Biến này để UI biết người dùng đang đăng nhập đã thả cảm xúc gì chưa (để bôi màu nút Like)
        public string? CurrentUserReaction { get; set; }
    }
}
