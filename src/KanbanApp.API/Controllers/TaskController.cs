using System.IdentityModel.Tokens.Jwt;
using KanbanApp.Application.DTOs.Tasks;
using KanbanApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KanbanApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId}/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    private string CurrentUserId =>
        User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks(
        int projectId,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? assignedUserId = null)
    {
        var tasks = await _taskService.GetProjectTasksAsync(
            projectId, CurrentUserId, status, priority, assignedUserId);
        return Ok(tasks);
    }

    [HttpGet("/api/tasks/{taskId}")]
    public async Task<ActionResult<TaskDto>> GetTask(int taskId)
    {
        var task = await _taskService.GetTaskByIdAsync(taskId, CurrentUserId);
        if (task is null) return NotFound();
        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask(int projectId, TaskCreateDto dto)
    {
        var task = await _taskService.CreateTaskAsync(projectId, dto, CurrentUserId);
        if (task is null)
        {
            return BadRequest(new { message = "تعذر إنشاء المهمة — تحقق أنك عضو في المشروع وأن الشخص المُسند إليه عضو فيه أيضاً" });
        }
        return CreatedAtAction(nameof(GetTask), new { taskId = task.Id }, task);
    }

    [HttpPut("/api/tasks/{taskId}")]
    public async Task<IActionResult> UpdateTask(int taskId, TaskUpdateDto dto)
    {
        var success = await _taskService.UpdateTaskAsync(taskId, dto, CurrentUserId);
        if (!success) return BadRequest();
        return NoContent();
    }

    [HttpPatch("/api/tasks/{taskId}/status")]
    public async Task<IActionResult> UpdateTaskStatus(int taskId, TaskStatusUpdateDto dto)
    {
        var success = await _taskService.UpdateTaskStatusAsync(taskId, dto.Status, CurrentUserId);
        if (!success) return BadRequest(new { message = "حالة غير صالحة أو ليس لديك صلاحية" });
        return NoContent();
    }

    [HttpDelete("/api/tasks/{taskId}")]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        var success = await _taskService.DeleteTaskAsync(taskId, CurrentUserId);
        if (!success) return BadRequest();
        return NoContent();
    }
}