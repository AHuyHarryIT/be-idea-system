namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class CategoryDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime UpdateAt { get; set; }
		public DateTime? DeletedAt { get; set; }
	}
}
