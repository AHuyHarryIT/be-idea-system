using System.ComponentModel.DataAnnotations;

namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class SubmissionDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public int AcademicYear { get; set; }
		public DateTime ClosureDate { get; set; } = DateTime.UtcNow;
		public DateTime FinalClosureDate { get; set; } = DateTime.UtcNow;
		public int IdeaCount { get; set; }
		public bool IsActive { get; set; }
	}

	public class SubmissionCreateDto
	{

		[Required(ErrorMessage = "The Submission Name is required.")]
		[StringLength(100, ErrorMessage = "The Name cannot exceed 100 characters.")]
		public string Name { get; set; } = string.Empty;

		public string? Description { get; set; }
		public int AcademicYear { get; set; }
		public DateTime ClosureDate { get; set; } = DateTime.UtcNow;
		public DateTime FinalClosureDate { get; set; } = DateTime.UtcNow;
	}
}