using AutoMapper;
using FluentValidation;
using TaskManager.Application.DTOs.TaskDtos;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateTaskDto> _validator;
        private readonly IValidator<UpdateTaskDto> _updateValidator;

        public TaskService(
            ITaskRepository repository, IUnitOfWork uow, IMapper mapper, 
            IValidator<CreateTaskDto> validator, IValidator<UpdateTaskDto> updateValidator)
        {
            _repository = repository;
            _uow = uow;
            _mapper = mapper;
            _validator = validator;
            _updateValidator = updateValidator;
        }

        #region Read Operations 
        public async Task<IEnumerable<TaskResponseDto>> GetAllAsync(
            TaskItemStatus? status, DateTime? dueDate)
        {
            var tasks = await _repository.GetAllAsync(status, dueDate);
            return _mapper.Map<IEnumerable<TaskResponseDto>>(tasks);
        }

        public async Task<TaskResponseDto?> GetByIdAsync(Guid id)
        {
            var task = await _repository.GetByIdAsync(id);
            return _mapper.Map<TaskResponseDto>(task);
        }
        #endregion

        #region Write Operations
        public async Task<TaskResponseDto> CreateAsync(CreateTaskDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description;
                        
            var task = new TaskItem(
                dto.Title,
                description,
                dto.DueDate,
                dto.Status!.Value 
            );

            await _repository.AddAsync(task);
            await _uow.CommitAsync();

            return _mapper.Map<TaskResponseDto>(task);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateTaskDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var task = await _repository.GetByIdAsync(id);
            if (task == null)
                return false;

            var description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description;

            task.Update(
                dto.Title,
                description,
                dto.DueDate,
                dto.Status!.Value
            );

            _repository.Update(task);
            await _uow.CommitAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var task = await _repository.GetByIdAsync(id);

            if (task == null)
                return false;

            _repository.Delete(task);

            return await _uow.CommitAsync();
        }
        #endregion
    }
}

