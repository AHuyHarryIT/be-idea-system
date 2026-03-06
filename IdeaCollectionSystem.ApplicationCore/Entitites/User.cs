using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class User
	{
		[Key]
		public Guid  Id { get; set; }
		public string UserName { get; set; } = string.Empty;
		public string HashPassword { get; set; } = string.Empty;
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Avartar { get; set; } = string.Empty;

		[ForeignKey("RoleId")]
		public Guid RoleId { get; set; }
		[NotMapped]
		public Role? Role { get; set; }

		[ForeignKey("DepartmentId")]
		public Guid DepartmentId { get; set; }
		[NotMapped]
		public Department? Department { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; 
		public DateTime? DeletedAt { get; set; }
		[NotMapped]
		public ICollection<Role> Roles { get; set; } = new List<Role>();
		[NotMapped]
		public ICollection<Department> Departments { get; set; } = new List<Department>();
		[NotMapped]
		public ICollection<Comment> Comments { get; set; } = new List<Comment>();
		[NotMapped]
		public ICollection<Idea> Ideas { get; set; } = new List<Idea>();



	}
}
