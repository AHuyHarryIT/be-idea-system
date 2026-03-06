namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class IdeaInfoDto
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string CategoryName { get; set; } = string.Empty;
		public string DepartmentName { get; set; } = string.Empty;
		public string AuthorName { get; set; } = string.Empty;
		public DateTime CreatedDate { get; set; }
		public bool IsAnonymous { get; set; }
		public int ThumbsUpCount { get; set; }
		public int ThumbsDownCount { get; set; }
		public int CommentCount { get; set; }
		public int ViewCount { get; set; }
	}
}