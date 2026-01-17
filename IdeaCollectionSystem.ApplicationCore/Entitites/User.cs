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

		public Guid RoleId { get; set; }
		[ForeignKey("RoleId")]
		public Role? Role { get; set; }

		public Guid DepartmentId { get; set; }
		[ForeignKey("DepartmentId")]
		public Department? Department { get; set; }
		public DateTime CreatedAt { get; set; }	= DateTime.Now;
		public DateTime UpdatedAt { get; set; }	= DateTime.Now;
		public DateTime? DeletedAt { get; set; }
		public ICollection<Role> Roles { get; set; } = new List<Role>();
		public ICollection<Department> Departments { get; set; } = new List<Department>();

		public ICollection<Comment> Comments { get; set; } = new List<Comment>();
		public ICollection<Idea> Ideas { get; set; } = new List<Idea>();



	}
}
