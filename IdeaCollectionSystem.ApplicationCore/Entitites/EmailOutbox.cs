using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class EmailOutBox
	{
		[Key]
		public Guid Id { get; set; }
		public string EmailTo { get; set; } = string.Empty;
		public string Subject { get; set; } = string.Empty;
		public string Body { get; set; } = string.Empty;

		public enum Status
		{
			PENDING = 0,
			SENT = 1,
			FAILED = 2
		}

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime SentAt { get; set; } = DateTime.UtcNow;

		public string Error { get; set; } = string.Empty;

		public Guid IdeaId { get; set; }
		[ForeignKey("IdeaId")]
		public Idea? Idea { get; set; }

		public Guid CommentId { get; set; }
		[ForeignKey("CommentId")]
		public Comment? Comment { get; set; }

	
	}
}