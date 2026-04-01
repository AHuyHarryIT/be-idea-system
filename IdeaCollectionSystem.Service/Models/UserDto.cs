namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class UserDto
	{
		public string Id { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public Guid? DepartmentId { get; set; }
		public string DepartmentName { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}

	public class CreateUserRequest
	{
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public Guid? DepartmentId { get; set; }
		public string Role { get; set; } = string.Empty;
	}

	public class UpdateUserRequest
	{
		public string Name { get; set; } = string.Empty;
		public Guid? DepartmentId { get; set; }
		public string Role { get; set; } = string.Empty;
	}

	public class LoginRequestDto
	{
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
	}

	public class RegisterRequestDto
	{
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
	}
}