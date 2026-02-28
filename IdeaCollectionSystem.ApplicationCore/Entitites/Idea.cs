using IdeaCollectionSystem.ApplicationCore.Entitites;
using System.ComponentModel.DataAnnotations;

public class Idea
{
	[Key]
	public Guid Id { get; set; }

	public string Text { get; set; } = string.Empty;

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? DeletedAt { get; set; }

	public bool IsAnonymous { get; set; }

	// USER (1 Idea - 1 User)
	public Guid UserId { get; set; }
	public User? User { get; set; }


	public Guid SubmissionId { get; set; }
	public Submission? Submission { get; set; }


	public Guid DepartmentId { get; set; }
	public Department? Department { get; set; }


	public Guid CategoryId { get; set; }
	public Category? Category { get; set; }


	public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}