using IdeaCollectionIdea.Common.Constants;
using System.ComponentModel.DataAnnotations;
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