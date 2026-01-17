using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class IdeaReactions
	{
		[Key]
		public Guid Id { get; set; }

		public Guid IdeaId { get; set; }
		[ForeignKey("IdeaId")]
		public Idea? Idea { get; set; }

		public Guid UserId { get; set; }
		[ForeignKey("UserId")]
		public User? User { get; set; }

		public string Reaction { get; set; } = string.Empty;

		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public DateTime UpdatedAt { get; set; } = DateTime.Now;

		public ICollection<User> Users { get; set; } = new List<User>();
		public ICollection<Idea> Ideas { get; set; } = new List<Idea>();

	}
}
