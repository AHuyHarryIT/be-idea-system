namespace IdeaCollectionSystem.ApplicationCore.DTOs
{
	public class SubmissionDto
	{
		public Guid Id { get; set; }
		public DateTime AcademicYear { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime ClousureDate { get; set; }
		public DateTime FinaleClosureDate { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }
	}
}
