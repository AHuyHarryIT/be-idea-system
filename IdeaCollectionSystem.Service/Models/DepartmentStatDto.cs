namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class DepartmentStatDto
	{
		public string DepartmentName { get; set; } = string.Empty;
		public int IdeaCount { get; set; }
		public double Percentage { get; set; }
		public int ContributorCount { get; set; }
	}
}