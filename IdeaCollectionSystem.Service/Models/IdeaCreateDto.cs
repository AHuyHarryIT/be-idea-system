namespace IdeaCollectionSystem.Service.Models.DTOs
{
    public class IdeaCreateDto
    {
        public string Text { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
      
        public bool IsAnonymous { get; set; }
        public List<string>? FilePaths { get; set; }
	
		public Guid CategoryId { get; set; }

	}
}