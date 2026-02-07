using Microsoft.AspNetCore.Identity;

public class IdeaUser : IdentityUser
{
	
	public string Name { get; set; } = string.Empty;
	public string Avatar { get; set; } = string.Empty;
	//public Guid? DepartmentId { get; set; }
	//public DateTime CreatedAt { get; set; } = DateTime.Now;
	//public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
