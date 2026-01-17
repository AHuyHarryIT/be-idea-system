using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class EmailOutBox
	{
		public Guid Id { get; set; } 
		public string EmailTo { get; set; } = string.Empty;
		public string Subject { get; set; } = string.Empty;
		public string Body { get; set; } = string.Empty;
		
		public string Status { get; set; } = "Pending";

		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public DateTime SentAt { get; set; } = DateTime.Now;
		public string Error { get; set; } = string.Empty;

		public Guid IdeaId { get; set; }
		public Idea? Idea { get; set; } 
		
		public Guid CommentId { get; set; }
		public Comment? Comment { get; set; }

		public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
		public ICollection<Comment> Comments { get; set; } = new List<Comment>();

	}
}
