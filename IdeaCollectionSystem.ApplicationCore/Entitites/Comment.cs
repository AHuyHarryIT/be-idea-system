using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class Comment
	{
		[Key]
		public Guid Id { get; set; }
		public string? Content { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? DeletedAt { get; set; } = DateTime.UtcNow;
		public bool IsAnonymous { get; set; }

		public string UserId { get; set; } = string.Empty;

		[ForeignKey("UserId")]
		public IdeaUser? User { get; set; }


		[ForeignKey("IdeaId")]
		public Guid IdeaId { get; set; }
		public Idea? Idea { get; set; }


	}
}
