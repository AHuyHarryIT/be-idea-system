namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class SubmissionDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime AcademicYear { get; set; } = DateTime.UtcNow;
		public DateTime ClosureDate { get; set; } = DateTime.UtcNow;
		public DateTime FinalClosureDate { get; set; } = DateTime.UtcNow;
		public int IdeaCount { get; set; }
		public bool IsActive { get; set; }
	}

	public class SubmissionCreateDto
	{
		public string Name { get; set; } = string.Empty;
		public DateTime AcademicYear { get; set; } = DateTime.UtcNow;
		public DateTime ClosureDate { get; set; } = DateTime.UtcNow;
		public DateTime FinalClosureDate { get; set; } = DateTime.UtcNow;
	}
}