namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class SubmissionDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime AcademicYear { get; set; }
		public DateTime ClosureDate { get; set; }
		public DateTime FinalClosureDate { get; set; }
		public int IdeaCount { get; set; }
		public bool IsActive { get; set; }
	}

	public class SubmissionCreateDto
	{
		public string Name { get; set; } = string.Empty;
		public DateTime AcademicYear { get; set; }
		public DateTime ClosureDate { get; set; }
		public DateTime FinalClosureDate { get; set; }
	}
}