using KanbanApp.Domain.Enums;

namespace KanbanApp.Domain.Entities;

//جدول كسر العلاقة بين projects ,ApplicationUser
public class ProjectMember
{
    public int Id { get; set; }
    public ProjectRole Role { get; set; } = ProjectRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}
