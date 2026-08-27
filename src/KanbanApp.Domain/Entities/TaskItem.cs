using KanbanApp.Domain.Enums;

namespace KanbanApp.Domain.Entities;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public KanbanTaskStatus Status { get; set; } = KanbanTaskStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    // قد لا تكون المهمة مُسندة لأحد بعد
    public string? AssignedUserId { get; set; }
    public ApplicationUser? AssignedUser { get; set; }
}
