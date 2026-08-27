using KanbanApp.Domain.Entities;
using KanbanApp.Domain.Enums;
using KanbanApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KanbanApp.UnitTests.Infrastructure;

// ملاحظة: نستخدم InMemory provider هنا لأن اختبارات الوحدة يجب أن تكون سريعة
// ولا تعتمد على SQL Server فعلي. اختبارات SQL Server الحقيقية (Integration Tests)
// تُضاف لاحقاً في مشروع منفصل عند الحاجة.
public class ApplicationDbContextTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // قاعدة بيانات جديدة معزولة لكل اختبار
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CanAddProjectWithOwnerAndTask()
    {
        // Arrange
        await using var context = CreateContext();

        var owner = new ApplicationUser
        {
            Id = "user-1",
            UserName = "owner@test.com",
            Email = "owner@test.com",
            FullName = "مالك تجريبي"
        };

        var project = new Project
        {
            Name = "مشروع تجريبي",
            OwnerId = owner.Id,
            Owner = owner
        };

        var task = new TaskItem
        {
            Title = "أول مهمة",
            Project = project,
            Status = KanbanTaskStatus.Todo,
            Priority = TaskPriority.High
        };

        // Act
        context.Users.Add(owner);
        context.Projects.Add(project);
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var savedProject = await context.Projects
            .Include(p => p.Tasks)
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Name == "مشروع تجريبي");

        // Assert
        Assert.NotNull(savedProject);
        Assert.Single(savedProject!.Tasks);
        Assert.Equal("owner@test.com", savedProject.Owner.Email);
    }

    [Fact]
    public async Task AddingProjectMember_PersistsWithCorrectRole()
    {
        // Arrange
        await using var context = CreateContext();

        var owner = new ApplicationUser { Id = "u1", UserName = "a@test.com", Email = "a@test.com" };
        var member = new ApplicationUser { Id = "u2", UserName = "b@test.com", Email = "b@test.com" };
        var project = new Project { Name = "مشروع 2", OwnerId = owner.Id, Owner = owner };

        context.Users.AddRange(owner, member);
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        // Act
        context.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = project.Id,
            UserId = member.Id,
            Role = ProjectRole.Member
        });
        await context.SaveChangesAsync();

        // Assert
        var savedMember = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == project.Id && pm.UserId == member.Id);

        Assert.NotNull(savedMember);
        Assert.Equal(ProjectRole.Member, savedMember!.Role);
    }

    [Fact]
    public async Task DeletingProject_CascadesToItsTasks()
    {
        // Arrange
        await using var context = CreateContext();

        var owner = new ApplicationUser { Id = "u3", UserName = "c@test.com", Email = "c@test.com" };
        var project = new Project { Name = "مشروع للحذف", OwnerId = owner.Id, Owner = owner };
        var task = new TaskItem { Title = "مهمة ستُحذف", Project = project };

        context.Users.Add(owner);
        context.Projects.Add(project);
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        // Act
        context.Projects.Remove(project);
        await context.SaveChangesAsync();

        // Assert
        var remainingTasks = await context.Tasks.CountAsync(t => t.ProjectId == project.Id);
        Assert.Equal(0, remainingTasks);
    }
}
