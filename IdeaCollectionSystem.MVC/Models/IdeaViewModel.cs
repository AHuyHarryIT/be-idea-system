using System.ComponentModel.DataAnnotations;

namespace IdeaCollectionSystem.Models
{
	public class IdeaViewModel
	{
		[Required(ErrorMessage = "Title is required")]
		[StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
		public string Title { get; set; }

		[Required(ErrorMessage = "Description is required")]
		[StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
		public string Description { get; set; }

		[Required(ErrorMessage = "Please select a category")]
		public int CategoryId { get; set; }

		public bool IsAnonymous { get; set; }

		// Không cần thuộc tính này trong form nhưng có thể thêm để binding
		// public List<IFormFile>? SupportingFiles { get; set; }
	}
}