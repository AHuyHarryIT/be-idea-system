using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class Submission
	{
		public Guid Id { get; set; } 
		public DateTime AcademicYear { get; set; }
		public string Name { get; set; } = string.Empty;

		public DateTime ClousureDate { get; set; }
		public DateTime FinaleClosureDate	{ get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public DateTime UpdatedAt { get; set; } = DateTime.Now;
		public DateTime? DeletedAt { get; set; }
	}
}
