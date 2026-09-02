using KanbanApp.Application.DTOs.Projects;

namespace KanbanApp.Application.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(string userId);
    Task<ProjectDto?> GetProjectByIdAsync(int projectId, string userId);
    Task<ProjectDto> CreateProjectAsync(ProjectCreateDto dto, string ownerId);
    Task<bool> UpdateProjectAsync(int projectId, ProjectUpdateDto dto, string userId);
    Task<bool> DeleteProjectAsync(int projectId, string userId);
    Task<IEnumerable<ProjectMemberDto>> GetMembersAsync(int projectId, string userId);
    Task<bool> AddMemberAsync(int projectId, string email, string requestingUserId);
    Task<bool> RemoveMemberAsync(int projectId, string memberUserId, string requestingUserId);
}