namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class Idea
	{
		public Guid Id { get; set; }
		public string Text { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public DateTime UpdatedAt { get; set; } = DateTime.Now;
		public DateTime? DeletedAt { get; set; } 
		public bool IsAnonymous { get; set; }

		public Guid UserId { get; set; }
		public User? User { get; set; }

		public Guid SubmissionId { get; set; }
		public Submission? Submission { get; set; }

		public Guid DepartmentId { get; set; }

		public Department? Department { get; set; }

		public Guid CategoryId { get; set; }
		public Category? Category { get; set; }

		public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
		public ICollection<User> Users { get; set; } = new List<User>();
		public ICollection<Department> Departments { get; set; } = new List<Department>();
		public ICollection<Category> Categories { get; set; } = new List<Category>();


	}
}
