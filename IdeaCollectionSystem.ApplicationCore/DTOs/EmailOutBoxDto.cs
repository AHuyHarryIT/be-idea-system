namespace IdeaCollectionSystem.ApplicationCore.DTOs
{
	public class EmailOutBoxDto
	{
		public Guid Id { get; set; }
		public string EmailTo { get; set; } = string.Empty;
		public string Subject { get; set; } = string.Empty;
		public string Body { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime SentAt { get; set; }
		public string Error { get; set; } = string.Empty;
		public Guid IdeaId { get; set; }
		public Guid CommentId { get; set; }
	}
}
