namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class CommentDto
	{
		public Guid Id { get; set; }
		public string? Text { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }
		public bool IsAnonymous { get; set; }
		public Guid UserId { get; set; }
		public Guid IdeaId { get; set; }
	}
}
