using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IdeaCollectionSystem.Service.Models.DTOs
{
	public class IdeaCreateDto
	{
		[Required(ErrorMessage = "The idea title cannot be blank.")]
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;

		public bool IsAnonymous { get; set; }
		public bool HasAcceptedTerms { get; set; }

		public List<IFormFile>? UploadedFiles { get; set; }

		public Guid CategoryId { get; set; }
		public Guid DepartmentId { get; set; }

		public Guid SubmissionId { get; set; }
	}

	public class IdeaView
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid IdeaId { get; set; }
		public string UserId { get; set; }
		public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
	}

	public class IdeaUpdateDto
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public Guid CategoryId { get; set; }
		public bool IsAnonymous { get; set; }
		public List<IFormFile>? UploadedFiles { get; set; } // Hỗ trợ up thêm file lúc edit
	}
}
