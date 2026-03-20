namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class CommentDto
	{
		public Guid Id { get; set; }
		public string Text { get; set; } = string.Empty;
		public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
		public bool IsAnonymous { get; set; }
		public string AuthorName { get; set; } = string.Empty; 
	}
}