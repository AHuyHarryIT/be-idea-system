namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class IdeaRoleDto
	{
		public string Id { get; set; } = string.Empty;
		public string? Name { get; set; }
		public string? NormalizedName { get; set; }
		public string Description { get; set; } = string.Empty;
	}
}
