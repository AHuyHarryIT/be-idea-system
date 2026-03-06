using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class Comment
	{
		[Key]
		public Guid Id { get; set; }
		public string? Text { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? DeletedAt { get; set; }
		public bool IsAnonymous { get; set; }
		public string UserId { get; set; } = string.Empty;

		[ForeignKey("IdeaId")]
		public Guid IdeaId { get; set; }
		public Idea? Idea { get; set; }
	}
}