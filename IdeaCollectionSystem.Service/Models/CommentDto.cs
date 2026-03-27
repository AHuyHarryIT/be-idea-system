using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.Service.Models
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public Guid IdeaId { get; set; }

        // Nếu IsAnonymous = true, ta sẽ ẩn thông tin này đi khi trả về Frontend
        public string UserId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;

        public string? Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsAnonymous { get; set; }
    }

    public class CommentCreateDto
    {
        public Guid IdeaId { get; set; }
        public string? Text { get; set; }
        public bool IsAnonymous { get; set; } // Cho phép user chọn ẩn danh khi gửi
    }

    public class CommentUpdateDto
    {
        public string? Text { get; set; }
        public bool IsAnonymous { get; set; }
    }
}
