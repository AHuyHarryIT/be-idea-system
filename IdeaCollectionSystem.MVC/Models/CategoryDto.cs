using System.ComponentModel.DataAnnotations;
using IdeaCollectionIdea.Common.Constants;

namespace IdeaCollectionSystem.Models
{
	public class CategoryDto
	{
		public Guid? Id { get; set; }

		[Required]
		[MaxLength(MaxLengths.NAME)]
		public string Name { get; set; } = string.Empty;
	}
}
