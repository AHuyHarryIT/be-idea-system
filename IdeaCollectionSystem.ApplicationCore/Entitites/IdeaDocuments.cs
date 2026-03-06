using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class IdeaDocuments
	{
		[Key]
		public Guid Id { get; set; }
		public string StoredPath { get; set; } = string.Empty;
		public string OriginalFileName { get; set; } = string.Empty;
		public string MimeType { get; set; } = string.Empty;
		public long FizeSize { get; set; }
		public DateTime UploadtedAt { get; set; } = DateTime.UtcNow;
		public DateTime? DeletedAt { get; set; }

		[ForeignKey("IdeaId")]
		public Guid IdeaId { get; set; }
		public Idea? Idea { get; set; }

	}
}