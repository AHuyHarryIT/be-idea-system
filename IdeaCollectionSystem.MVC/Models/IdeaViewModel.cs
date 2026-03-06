using System.ComponentModel.DataAnnotations;

namespace IdeaCollectionSystem.Models
{
	public class IdeaViewModel
	{
		[Required(ErrorMessage = "Text is required")]
		[StringLength(200, MinimumLength = 5, ErrorMessage = "Text must be between 5 and 200 characters")]
		public string Text { get; set; }

		[Required(ErrorMessage = "Description is required")]
		[StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
		public string Description { get; set; }

		[Required(ErrorMessage = "Please select a category")]
		public Guid CategoryId { get; set; } 

		public Guid DepartmentId { get; set; }

		public bool IsAnonymous { get; set; }


	}
}