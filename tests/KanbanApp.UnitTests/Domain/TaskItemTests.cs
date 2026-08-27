using KanbanApp.Domain.Entities;
using KanbanApp.Domain.Enums;
using Xunit;

namespace KanbanApp.UnitTests.Domain;

public class TaskItemTests
{
    [Fact]
    public void NewTaskItem_DefaultsToTodoStatusAndMediumPriority()
    {
        // Arrange & Act
        var task = new TaskItem
        {
            Title = "مهمة جديدة",
            ProjectId = 1
        };

        // Assert
        Assert.Equal(KanbanTaskStatus.Todo, task.Status);
        Assert.Equal(TaskPriority.Medium, task.Priority);
        Assert.Null(task.AssignedUserId); // لا يوجد إسناد افتراضياً
    }
}
