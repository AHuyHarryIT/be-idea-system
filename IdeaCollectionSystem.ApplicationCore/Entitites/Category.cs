using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class Category
	{
		public Guid Id { get; set; } 
		public string? Name { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime UpdateAt { get; set; }
		public DateTime? DeletedAt { get; set; }
	}
}
