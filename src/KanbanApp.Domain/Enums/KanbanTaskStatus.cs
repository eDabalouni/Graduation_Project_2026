namespace KanbanApp.Domain.Enums;

// اسم مختلف عمداً عن TaskStatus لتجنب التعارض مع System.Threading.Tasks.TaskStatus
public enum KanbanTaskStatus
{
    Todo = 0,
    InProgress = 1,
    Done = 2
}
