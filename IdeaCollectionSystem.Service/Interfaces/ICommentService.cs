using IdeaCollectionSystem.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.Service.Interfaces
{
    public interface ICommentService
    {
        // Lấy tất cả comment của 1 ý tưởng cụ thể
        Task<IEnumerable<CommentDto>> GetCommentsByIdeaIdAsync(Guid ideaId);

        // Tạo comment cần biết ai là người tạo (authorId)
        Task<bool> CreateCommentAsync(CommentCreateDto dto, string authorId);

        // Sửa/Xóa cần biết authorId để check xem họ có quyền không (có phải chủ comment không)
        Task<bool> UpdateCommentAsync(Guid commentId, CommentUpdateDto dto, string authorId);
        Task<bool> DeleteCommentAsync(Guid commentId, string authorId);
    }
}
