namespace KanbanApp.Application.DTOs.Tasks;

public class TaskUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public DateTime? DueDate { get; set; }
    public string? AssignedUserId { get; set; }
}