using System.IdentityModel.Tokens.Jwt;
using KanbanApp.Application.DTOs.Projects;
using KanbanApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KanbanApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    private string CurrentUserId =>
        User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetMyProjects()
    {
        var projects = await _projectService.GetUserProjectsAsync(CurrentUserId);
        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id, CurrentUserId);
        if (project is null) return NotFound();
        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject(ProjectCreateDto dto)
    {
        var project = await _projectService.CreateProjectAsync(dto, CurrentUserId);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(int id, ProjectUpdateDto dto)
    {
        var success = await _projectService.UpdateProjectAsync(id, dto, CurrentUserId);
        if (!success) return Forbid();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var success = await _projectService.DeleteProjectAsync(id, CurrentUserId);
        if (!success) return Forbid();
        return NoContent();
    }

    [HttpGet("{id}/members")]
    public async Task<ActionResult<IEnumerable<ProjectMemberDto>>> GetMembers(int id)
    {
        var members = await _projectService.GetMembersAsync(id, CurrentUserId);
        return Ok(members);
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMember(int id, AddProjectMemberDto dto)
    {
        var success = await _projectService.AddMemberAsync(id, dto.Email, CurrentUserId);
        if (!success)
        {
            return BadRequest(new { message = " Can't add the member" });
        }
        return NoContent();
    }

[HttpDelete("{id}/members/{memberUserId}")]
public async Task<IActionResult> RemoveMember(int id, string memberUserId)
{
    if (memberUserId == CurrentUserId)
    {
        return BadRequest(new { message = " Can't Delete the owner " });
    }

    var success = await _projectService.RemoveMemberAsync(id, memberUserId, CurrentUserId);
    if (!success)
    {
        return BadRequest(new { message = " The member doesn't exist " });
    }
    return NoContent();
}
}