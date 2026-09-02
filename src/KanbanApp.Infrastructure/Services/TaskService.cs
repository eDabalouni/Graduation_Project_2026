using KanbanApp.Application.DTOs.Tasks;
using KanbanApp.Application.Interfaces;
using KanbanApp.Domain.Enums;
using KanbanApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TaskEntity = KanbanApp.Domain.Entities.TaskItem;

namespace KanbanApp.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;

    public TaskService(ApplicationDbContext context)
    {
        _context = context;
    }

    private async Task<bool> IsProjectMemberAsync(int projectId, string userId)
    {
        return await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
    }

    public async Task<IEnumerable<TaskDto>> GetProjectTasksAsync(
        int projectId,
        string userId,
        string? statusFilter = null,
        string? priorityFilter = null,
        string? assignedUserIdFilter = null)
    {
        if (!await IsProjectMemberAsync(projectId, userId))
        {
            return Enumerable.Empty<TaskDto>();
        }

        var query = _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.AssignedUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusFilter) &&
            Enum.TryParse<KanbanTaskStatus>(statusFilter, true, out var statusEnum))
        {
            query = query.Where(t => t.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(priorityFilter) &&
            Enum.TryParse<TaskPriority>(priorityFilter, true, out var priorityEnum))
        {
            query = query.Where(t => t.Priority == priorityEnum);
        }

        if (!string.IsNullOrWhiteSpace(assignedUserIdFilter))
        {
            query = query.Where(t => t.AssignedUserId == assignedUserIdFilter);
        }

        return await query
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                DueDate = t.DueDate,
                CreatedAt = t.CreatedAt,
                ProjectId = t.ProjectId,
                AssignedUserId = t.AssignedUserId,
                AssignedUserName = t.AssignedUser != null ? t.AssignedUser.FullName : null
            })
            .ToListAsync();
    }

    public async Task<TaskDto?> GetTaskByIdAsync(int taskId, string userId)
    {
        var task = await _context.Tasks
            .Include(t => t.AssignedUser)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task is null) return null;
        if (!await IsProjectMemberAsync(task.ProjectId, userId)) return null;

        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            Priority = task.Priority.ToString(),
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            ProjectId = task.ProjectId,
            AssignedUserId = task.AssignedUserId,
            AssignedUserName = task.AssignedUser?.FullName
        };
    }

    public async Task<TaskDto?> CreateTaskAsync(int projectId, TaskCreateDto dto, string userId)
    {
        if (!await IsProjectMemberAsync(projectId, userId)) return null;

        if (!Enum.TryParse<TaskPriority>(dto.Priority, true, out var priority))
        {
            priority = TaskPriority.Medium;
        }

        if (!string.IsNullOrWhiteSpace(dto.AssignedUserId) &&
            !await IsProjectMemberAsync(projectId, dto.AssignedUserId))
        {
            return null; // لا يمكن إسناد مهمة لشخص ليس عضواً في المشروع
        }

        var task = new TaskEntity
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = priority,
            DueDate = dto.DueDate,
            ProjectId = projectId,
            AssignedUserId = dto.AssignedUserId,
            Status = KanbanTaskStatus.Todo
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return await GetTaskByIdAsync(task.Id, userId);
    }

    public async Task<bool> UpdateTaskAsync(int taskId, TaskUpdateDto dto, string userId)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task is null) return false;
        if (!await IsProjectMemberAsync(task.ProjectId, userId)) return false;

        if (!string.IsNullOrWhiteSpace(dto.AssignedUserId) &&
            !await IsProjectMemberAsync(task.ProjectId, dto.AssignedUserId))
        {
            return false;
        }

        if (!Enum.TryParse<TaskPriority>(dto.Priority, true, out var priority))
        {
            priority = task.Priority;
        }

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Priority = priority;
        task.DueDate = dto.DueDate;
        task.AssignedUserId = dto.AssignedUserId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateTaskStatusAsync(int taskId, string newStatus, string userId)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task is null) return false;
        if (!await IsProjectMemberAsync(task.ProjectId, userId)) return false;

        if (!Enum.TryParse<KanbanTaskStatus>(newStatus, true, out var statusEnum))
        {
            return false;
        }

        task.Status = statusEnum;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTaskAsync(int taskId, string userId)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task is null) return false;
        if (!await IsProjectMemberAsync(task.ProjectId, userId)) return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }
}