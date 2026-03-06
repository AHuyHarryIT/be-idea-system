using System.ComponentModel.DataAnnotations;

namespace IdeaCollectionSystem.Models
{
	public class IdeaViewModel
	{
		[Required(ErrorMessage = "Title is required")]
		[StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
		public string Text { get; set; } = string.Empty;

		[Required(ErrorMessage = "Description is required")]
		[StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
		public string Description { get; set; } = string.Empty;

		// Guid? để [Required] bắt được Guid.Empty / null
		[Required(ErrorMessage = "Please select a category")]
		public Guid? CategoryId { get; set; }

		// Bắt buộc chọn Submission
		[Required(ErrorMessage = "Please select a submission period")]
		public Guid? SubmissionId { get; set; }

		public Guid DepartmentId { get; set; }

		public bool IsAnonymous { get; set; }
	}
}