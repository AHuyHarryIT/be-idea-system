namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class VoteRequestDto
	{
		public bool IsThumbsUp { get; set; }
	}

	public enum ThumbStatus
	{
		NONE = 0,    
		LIKE = 1,     
		DISLIKE = -1  
	}

}