using Microsoft.AspNetCore.Identity;

public class IdeaUser : IdentityUser
{
	public string Name { get; set; } = string.Empty;

	// Chỉ lưu DepartmentId (Guid?) — KHÔNG có navigation property Department
	// Vì Departments nằm ở IdeaCollectionDbContext khác, không thể FK cross-context
	public Guid? DepartmentId { get; set; }
}