namespace IdeaCollectionSystem.ApplicationCore.DTOs
{
	public class CategoryDto
	{
		public Guid Id { get; set; }
		public string? Name { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdateAt { get; set; }
		public DateTime? DeletedAt { get; set; }
	}
}
