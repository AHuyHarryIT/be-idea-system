namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class QaDashboardDto
	{
		public int TotalIdeas { get; set; }
		public int TotalCategories { get; set; }
		public int TotalDepartments { get; set; }
		public int TotalUsers { get; set; }
		public int IdeasWithoutComments { get; set; }
		public int IdeasThisMonth { get; set; }
	}
}