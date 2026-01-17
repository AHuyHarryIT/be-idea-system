using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class Category
	{
		[Key]
		public Guid Id { get; set; } 
		public string? Name { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime UpdateAt { get; set; }
		public DateTime? DeletedAt { get; set; }
	}
}
