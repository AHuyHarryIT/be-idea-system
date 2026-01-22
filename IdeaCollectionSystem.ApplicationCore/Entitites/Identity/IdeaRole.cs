using Microsoft.AspNetCore.Identity;

namespace IdeaCollectionSystem.ApplicationCore.Entitites.Identity
{
	public class IdeaRole : IdentityRole
	{
		public string Description { get; set; } = string.Empty;
	}
}

