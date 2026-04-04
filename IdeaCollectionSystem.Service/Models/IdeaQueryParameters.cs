namespace IdeaCollectionSystem.Service.Models.DTOs
{

	public class IdeaQueryParameters : PaginationFilter
	{
		public Guid? SubmissionId { get; set; }
		public Guid? CategoryId { get; set; }
		public Guid? DepartmentId { get; set; }
		public ReviewStatus? ReviewStatus { get; set; }
		public string? SortBy { get; set; }
	}
}