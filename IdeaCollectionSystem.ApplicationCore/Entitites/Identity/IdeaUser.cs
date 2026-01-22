using Microsoft.AspNetCore.Identity;

public class IdeaUser : IdentityUser
{
	
	public string Name { get; set; } = string.Empty;
	public string Avatar { get; set; } = string.Empty;
}
