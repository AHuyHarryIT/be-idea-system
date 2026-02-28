public class IdeaCreateDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public Guid CategoryId { get; set; }
    public bool IsAnonymous { get; set; }
    public List<string>? FilePaths { get; set; }
    public int DepartmentId { get; set; } 
}