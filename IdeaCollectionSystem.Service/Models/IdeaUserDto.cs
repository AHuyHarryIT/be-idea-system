namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class IdeaUserDto
	{
		public string Id { get; set; } = string.Empty;
		public string? UserName { get; set; }
		public string? Email { get; set; }
		public string? PhoneNumber { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Avatar { get; set; } = string.Empty;
	}
}
