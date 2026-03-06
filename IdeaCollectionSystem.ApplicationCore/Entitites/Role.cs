using System.ComponentModel.DataAnnotations;

namespace IdeaCollectionSystem.ApplicationCore.Entitites
{
	public class Role
	{
		[Key]
		public Guid Id { get; set; }
		public string? Name { get; set; }
		public string Description { get; set; } = string.Empty;

		public virtual ICollection<User> Users { get; set; } = new List<User>();
	}
}