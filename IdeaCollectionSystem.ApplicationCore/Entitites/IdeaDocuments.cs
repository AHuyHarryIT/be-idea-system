using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public DateTime UploadtedAt { get; set; } = DateTime.Now;
		public DateTime DeletedAt { get; set; }

		[ForeignKey("IdeaId")]
		public Guid IdeaId { get; set; }
		public Idea? Idea { get; set; }

		public ICollection<Idea> Ideas { get; set; } = new List<Idea>();

	}
}
