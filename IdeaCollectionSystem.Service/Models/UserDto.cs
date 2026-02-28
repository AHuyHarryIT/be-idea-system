namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class UserDto
	{
		public Guid Id { get; set; }
		public string UserName { get; set; } = string.Empty;
		public string HashPassword { get; set; } = string.Empty;
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Avartar { get; set; } = string.Empty;
		public Guid RoleId { get; set; }
		public Guid DepartmentId { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }
	}
}
