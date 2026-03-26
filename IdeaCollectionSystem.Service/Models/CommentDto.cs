using System.ComponentModel.DataAnnotations;

namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class CommentDto
	{
		public Guid Id { get; set; }
		public string Content { get; set; } = string.Empty;
		public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
		public bool IsAnonymous { get; set; }
		public string AuthorName { get; set; } = string.Empty; 
	}

	public class CommentCreateDto
	{
		[Required(ErrorMessage = "Vui lòng truyền IdeaId vào!")]
		public Guid IdeaId { get; set; }

		[Required(ErrorMessage = "Nội dung bình luận không được để trống!")]
		[MinLength(2, ErrorMessage = "Bình luận quá ngắn!")]
		public string Content { get; set; }

		public bool IsAnonymous { get; set; } = false;
	}
}