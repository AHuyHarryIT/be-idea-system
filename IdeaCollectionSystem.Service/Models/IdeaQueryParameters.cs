namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class IdeaQueryParameters
	{
	
		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 5;

	
		public Guid? SubmissionId { get; set; }


		public string SortBy { get; set; } = "latest";
		public Guid? DepartmentId { get; set; }

	}
}