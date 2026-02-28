namespace IdeaCollectionSystem.ApplicationCore.DTOs
{
	public class IdeaReactionsDto
	{
		public Guid Id { get; set; }
		public Guid IdeaId { get; set; }
		public Guid UserId { get; set; }
		public string Reaction { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
