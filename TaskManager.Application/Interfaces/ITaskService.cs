using TaskManager.Application.DTOs.TaskDtos;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskResponseDto>> GetAllAsync(TaskItemStatus? status, DateTime? dueDate);
        Task<TaskResponseDto?> GetByIdAsync(Guid id);
        Task<TaskResponseDto> CreateAsync(CreateTaskDto dto);
        Task<bool> UpdateAsync(Guid id, UpdateTaskDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
