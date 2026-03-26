using IdeaCollectionSystem.ApplicationCore.Entitites;
using System.ComponentModel.DataAnnotations;

public class Idea
{
	[Key]
	public Guid Id { get; set; }

	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? DeletedAt { get; set; }

	public bool IsAnonymous { get; set; }

	public int ViewCount { get; set; } = 0;
	public string UserId { get; set; } = string.Empty;

	public bool IsApproved { get; set; } = false;
	public Guid SubmissionId { get; set; }
	public Submission? Submission { get; set; }

	public Guid DepartmentId { get; set; }
	public Department? Department { get; set; }

	public Guid CategoryId { get; set; }
	public Category? Category { get; set; }

	public ICollection<Comment> Comments { get; set; } = new List<Comment>();
	public ICollection<IdeaReaction> IdeaReactions { get; set; } = new List<IdeaReaction>();
	public ICollection<IdeaDocument> IdeaDocuments { get; set; } = new List<IdeaDocument>();
}