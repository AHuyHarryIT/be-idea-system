namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class VoteRequestDto
	{
		public bool IsThumbsUp { get; set; }
	}

	public enum ThumbStatus
	{
		LIKE = 1,
		DISLIKE = 0,
		NONE = -1
	}
}