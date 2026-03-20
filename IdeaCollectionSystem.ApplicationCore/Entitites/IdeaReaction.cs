using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class IdeaReaction
	{
		[Key]
		public Guid Id { get; set; }

		[ForeignKey("IdeaId")]
		public Guid IdeaId { get; set; }
		public Idea? Idea { get; set; }

		public string UserId { get; set; } = string.Empty;

		public string Reaction { get; set; } = string.Empty;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	}
}