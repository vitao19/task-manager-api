using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs.TaskDtos
{
    public class CreateTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskItemStatus Status { get; set; }
    }
}