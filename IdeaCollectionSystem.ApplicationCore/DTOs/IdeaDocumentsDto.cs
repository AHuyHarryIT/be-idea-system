namespace IdeaCollectionSystem.ApplicationCore.DTOs
{
	public class IdeaDocumentsDto
	{
		public Guid Id { get; set; }
		public string StoredPath { get; set; } = string.Empty;
		public string OriginalFileName { get; set; } = string.Empty;
		public string MimeType { get; set; } = string.Empty;
		public long FizeSize { get; set; }
		public DateTime UploadtedAt { get; set; }
		public DateTime DeletedAt { get; set; }
		public Guid IdeaId { get; set; }
	}
}
