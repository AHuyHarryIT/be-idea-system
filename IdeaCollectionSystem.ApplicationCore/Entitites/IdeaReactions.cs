using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class IdeaReactions
	{
		public Guid Id { get; set; }
		public Guid IdeaId { get; set; }
		public Idea? Idea { get; set; }
		public Guid UserId { get; set; }
		public User? User { get; set; }

		public string Reaction { get; set; } = string.Empty;

		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public DateTime UpdatedAt { get; set; } = DateTime.Now;

		public ICollection<User> Users { get; set; } = new List<User>();
		public ICollection<Idea> Ideas { get; set; } = new List<Idea>();

	}
}
