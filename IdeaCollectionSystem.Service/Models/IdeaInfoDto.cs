using IdeaCollectionSystem.Service.Models;
using IdeaCollectionSystem.Service.Models.DTOs;

public class IdeaInfoDto
{
	public Guid Id { get; set; } 
	public string Title { get; set; } = string.Empty;

	public string Description { get; set; }
	public string CategoryName { get; set; } = string.Empty;

	public string DepartmentName { get; set; } = string.Empty;
	public string AuthorName { get; set; } = string.Empty;
	public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
	public bool IsAnonymous { get; set; }

	public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.PENDING;
	public ThumbStatus ThumbStatus { get; set; } = ThumbStatus.NONE;
	public int ThumbsUpCount { get; set; }
	public int ThumbsDownCount { get; set; }
	public int CommentCount { get; set; }
	public bool CanComment { get; set; }

	public int ViewCount { get; set; }

	public List<CommentDto> Comments { get; set; } = new List<CommentDto>();


}

