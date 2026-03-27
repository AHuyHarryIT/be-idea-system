using IdeaCollectionSystem.ApplicationCore.Entitites; // Chú ý using đúng namespace chứa Entity của bạn
using IdeaCollectionSystem.Datalayer;
using IdeaCollectionSystem.Service.Interfaces;
using IdeaCollectionSystem.Service.Models;
using IdeaCollectionSystem.Service.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IdeaCollectionSystem.Service.Services;

public class CommentService : ICommentService
{
    private readonly IdeaCollectionDbContext _context;

    public CommentService(IdeaCollectionDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsByIdeaIdAsync(Guid ideaId)
    {
        return await _context.Comments
            .AsNoTracking()
            // LỌC XÓA MỀM: Chỉ lấy những comment chưa bị xóa (DeletedAt == null)
            .Where(c => c.IdeaId == ideaId && c.DeletedAt == null)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                IdeaId = c.IdeaId,

                // LOGIC ẨN DANH: Nếu IsAnonymous là true, che giấu UserId và Tên
                UserId = c.IsAnonymous ? string.Empty : c.UserId,
                AuthorName = c.IsAnonymous ? "Người dùng ẩn danh" : "Tên User", // (Sau này bạn Join với bảng User để lấy tên thật nhé)

                Text = c.Text,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                IsAnonymous = c.IsAnonymous
            })
            .ToListAsync();
    }

    public async Task<bool> CreateCommentAsync(CommentCreateDto dto, string authorId)
    {
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            IdeaId = dto.IdeaId,
            UserId = authorId, // Map đúng tên cột của bạn
            Text = dto.Text,
            IsAnonymous = dto.IsAnonymous,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Comments.AddAsync(comment);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateCommentAsync(Guid commentId, CommentUpdateDto dto, string authorId)
    {
        // Phải đảm bảo comment chưa bị xóa mềm mới cho sửa
        var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId && c.DeletedAt == null);
        if (comment == null) return false;

        // BẢO MẬT: Kiểm tra chính chủ
        if (comment.UserId != authorId) return false;

        // Cập nhật nội dung và thời gian
        comment.Text = dto.Text;
        comment.IsAnonymous = dto.IsAnonymous;
        comment.UpdatedAt = DateTime.UtcNow; // Cập nhật giờ sửa

        _context.Comments.Update(comment);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId, string authorId)
    {
        var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId && c.DeletedAt == null);
        if (comment == null) return false;

        // BẢO MẬT: Kiểm tra chính chủ
        if (comment.UserId != authorId) return false;

        // XÓA MỀM (Soft Delete): Không Remove() khỏi DB, chỉ gắn cờ thời gian xóa
        comment.DeletedAt = DateTime.UtcNow;

        _context.Comments.Update(comment);
        return await _context.SaveChangesAsync() > 0;
    }
}