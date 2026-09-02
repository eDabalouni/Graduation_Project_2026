using KanbanApp.Application.DTOs.Tasks;

namespace KanbanApp.Application.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetProjectTasksAsync(
        int projectId,
        string userId,
        string? statusFilter = null,
        string? priorityFilter = null,
        string? assignedUserIdFilter = null);

    Task<TaskDto?> GetTaskByIdAsync(int taskId, string userId);
    Task<TaskDto?> CreateTaskAsync(int projectId, TaskCreateDto dto, string userId);
    Task<bool> UpdateTaskAsync(int taskId, TaskUpdateDto dto, string userId);
    Task<bool> UpdateTaskStatusAsync(int taskId, string newStatus, string userId);
    Task<bool> DeleteTaskAsync(int taskId, string userId);
}