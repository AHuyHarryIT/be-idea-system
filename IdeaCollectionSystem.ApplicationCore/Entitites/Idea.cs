using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class Idea
	{
		[Key]
		public Guid Id { get; set; }
		public string Text { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public DateTime UpdatedAt { get; set; } = DateTime.Now;
		public DateTime? DeletedAt { get; set; } 
		public bool IsAnonymous { get; set; }

		[ForeignKey("UserId")]
		public Guid UserId { get; set; }
		public User? User { get; set; }

		[ForeignKey("SubmissionId")]

		public Guid SubmissionId { get; set; }
		public Submission? Submission { get; set; }

		[ForeignKey("DepartmentId")]
		public Guid DepartmentId { get; set; }

		public Department? Department { get; set; }

		[ForeignKey("CategoryId")]
		public Guid CategoryId { get; set; }
		public Category? Category { get; set; }

		public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
		public ICollection<User> Users { get; set; } = new List<User>();
		public ICollection<Department> Departments { get; set; } = new List<Department>();
		public ICollection<Category> Categories { get; set; } = new List<Category>();
		public ICollection<Comment> Comments { get; set; } = new List<Comment>();


	}
}
