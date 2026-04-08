using IdeaCollectionSystem.ApplicationCore.Entitites;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

public class IdeaUser : IdentityUser
{
	public string Name { get; set; } = string.Empty;

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	// Chỉ lưu DepartmentId (Guid?) — KHÔNG có navigation property Department
	// Vì Departments nằm ở IdeaCollectionDbContext khác, không thể FK cross-context
	public Guid? DepartmentId { get; set; }

	[ForeignKey("DepartmentId")]
	public virtual Department? Department { get; set; }
}