using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class Submission
	{
		[Key]
		public Guid Id { get; set; } 
		public DateTime AcademicYear { get; set; }
		public string Name { get; set; } = string.Empty;

		public DateTime ClousureDate { get; set; }
		public DateTime FinalClousureDate { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? DeletedAt { get; set; }

		public virtual ICollection<Idea> Ideas { get; set; } = new List<Idea>();
	}
}
