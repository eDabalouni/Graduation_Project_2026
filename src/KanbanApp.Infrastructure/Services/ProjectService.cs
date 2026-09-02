using KanbanApp.Application.DTOs.Projects;
using KanbanApp.Application.Interfaces;
using KanbanApp.Domain.Entities;
using KanbanApp.Domain.Enums;
using KanbanApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KanbanApp.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;

    public ProjectService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(string userId)
    {
        return await _context.Projects
            .Where(p => p.Members.Any(m => m.UserId == userId))
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                OwnerId = p.OwnerId,
                OwnerName = p.Owner.FullName,
                MembersCount = p.Members.Count,
                TasksCount = p.Tasks.Count
            })
            .ToListAsync();
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(int projectId, string userId)
    {
        var project = await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project is null) return null;
        if (!project.Members.Any(m => m.UserId == userId)) return null;

        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            OwnerId = project.OwnerId,
            OwnerName = project.Owner.FullName,
            MembersCount = project.Members.Count,
            TasksCount = project.Tasks.Count
        };
    }

    public async Task<ProjectDto> CreateProjectAsync(ProjectCreateDto dto, string ownerId)
    {
        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            OwnerId = ownerId
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        _context.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = project.Id,
            UserId = ownerId,
            Role = ProjectRole.Owner
        });
        await _context.SaveChangesAsync();

        var owner = await _context.Users.FindAsync(ownerId);

        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            OwnerId = ownerId,
            OwnerName = owner?.FullName ?? string.Empty,
            MembersCount = 1,
            TasksCount = 0
        };
    }

    public async Task<bool> UpdateProjectAsync(int projectId, ProjectUpdateDto dto, string userId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project is null || project.OwnerId != userId) return false;

        project.Name = dto.Name;
        project.Description = dto.Description;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProjectAsync(int projectId, string userId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project is null || project.OwnerId != userId) return false;

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ProjectMemberDto>> GetMembersAsync(int projectId, string userId)
    {
        var isMember = await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
        if (!isMember) return Enumerable.Empty<ProjectMemberDto>();

        return await _context.ProjectMembers
            .Where(m => m.ProjectId == projectId)
            .Include(m => m.User)
            .Select(m => new ProjectMemberDto
            {
                UserId = m.UserId,
                Email = m.User.Email ?? string.Empty,
                FullName = m.User.FullName,
                Role = m.Role.ToString(),
                JoinedAt = m.JoinedAt
            })
            .ToListAsync();
    }

    public async Task<bool> AddMemberAsync(int projectId, string email, string requestingUserId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project is null || project.OwnerId != requestingUserId) return false;

        var userToAdd = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (userToAdd is null) return false;

        var alreadyMember = await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userToAdd.Id);
        if (alreadyMember) return false;

        _context.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = userToAdd.Id,
            Role = ProjectRole.Member
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveMemberAsync(int projectId, string memberUserId, string requestingUserId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project is null || project.OwnerId != requestingUserId) return false;
        if (memberUserId == project.OwnerId) return false; 

        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == memberUserId);
        if (member is null) return false;

        _context.ProjectMembers.Remove(member);
        await _context.SaveChangesAsync();
        return true;
    }
}