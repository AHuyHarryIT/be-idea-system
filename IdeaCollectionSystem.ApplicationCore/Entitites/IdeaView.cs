using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class IdeaView
	{

		public Guid IdeaId { get; set; }
		public string UserId { get; set; }
		public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

		public Idea Idea { get; set; }
		public IdeaUser User { get; set; }

	}
}
