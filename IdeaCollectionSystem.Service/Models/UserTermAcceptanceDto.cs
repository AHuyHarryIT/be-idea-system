namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class UserTermAcceptanceDto
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public Guid TermId { get; set; }
		public DateTime AcceptedAt { get; set; }
		public string IpAddress { get; set; } = string.Empty;
		public string UserAgent { get; set; } = string.Empty;
	}
}
