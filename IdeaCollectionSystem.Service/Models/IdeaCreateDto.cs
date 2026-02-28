namespace IdeaCollectionSystem.Service.Models.DTOs
{
    public class IdeaCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public bool IsAnonymous { get; set; }
        public List<string>? FilePaths { get; set; }
		public int DepartmentId { get; set; }

	}
}