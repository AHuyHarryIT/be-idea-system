namespace IdeaCollectionSystem.ApplicationCore.DTOs
{
	public class TermVersionDto
	{
		public Guid Id { get; set; }
		public string Version { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
