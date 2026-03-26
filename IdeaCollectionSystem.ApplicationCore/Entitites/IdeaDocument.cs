using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class IdeaDocument
	{
		[Key]
		public Guid Id { get; set; }
		public string StoredPath { get; set; } = string.Empty;
		public string OriginalFileName { get; set; } = string.Empty;
		public string MimeType { get; set; } = string.Empty;
		public long FileSize { get; set; }
		public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
		public DateTime? DeletedAt { get; set; } 
		[ForeignKey("IdeaId")]
		public Guid IdeaId { get; set; }
		public Idea? Idea { get; set; }

	
	}
}