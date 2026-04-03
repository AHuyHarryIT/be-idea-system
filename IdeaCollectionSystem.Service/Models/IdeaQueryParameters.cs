namespace IdeaCollectionSystem.Service.Models.DTOs
{

	public class IdeaQueryParameters : PaginationFilter
	{
		public Guid? IdeaId { get; set; }
		public Guid? CategoryId { get; set; }
		public Guid? SubmissionId { get; set; }
		public Guid? DepartmentId { get; set; }
		public string? IdeaKeyword { get; set; }
		public string? CategoryKeyword { get; set; }
		public string? DepartmentKeyword { get; set; }
		public string? SubmissionKeyword { get; set; }
		public ReviewStatus? ReviewStatus { get; set; }
		public string? SortBy { get; set; }
	}
}