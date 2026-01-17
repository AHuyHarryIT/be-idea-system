using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class UserTermAcceptance
	{
		public Guid Id { get; set; } 
		public Guid UserId { get; set; }
		public User? User { get; set; } 
		public Guid TermId { get; set; }
		public TermVersion? Term { get; set; }
		public DateTime AcceptedAt { get; set; } = DateTime.Now;
		public string  IpAddress { get; set; } = string.Empty;
		public string UserAgent { get; set; } = string.Empty; 
		public ICollection<User> Users { get; set; } = new List<User>();
		public ICollection<TermVersion> Terms { get; set; } = new List<TermVersion>();
	}
}
