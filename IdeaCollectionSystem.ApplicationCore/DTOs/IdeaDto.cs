namespace IdeaCollectionSystem.ApplicationCore.DTOs
{
	public class IdeaDto
	{
		public Guid Id { get; set; }
		public string Text { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }
		public bool IsAnonymous { get; set; }
		public Guid UserId { get; set; }
		public Guid SubmissionId { get; set; }
		public Guid DepartmentId { get; set; }
		public Guid CategoryId { get; set; }
	}
}
