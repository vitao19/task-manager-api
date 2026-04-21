using AutoMapper;
using FluentValidation;
using TaskManager.Application.DTOs.TaskDtos;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace TaskManager.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILogger<TaskService> _logger;
        private readonly IValidator<CreateTaskDto> _validator;
        private readonly IValidator<UpdateTaskDto> _updateValidator;

        public TaskService(
            ITaskRepository repository,
            IUnitOfWork uow,
            ILogger<TaskService> logger,
            IValidator<CreateTaskDto> validator,
            IValidator<UpdateTaskDto> updateValidator,
            IMapper mapper)
        {
            _repository = repository;
            _uow = uow;
            _logger = logger;
            _validator = validator;
            _updateValidator = updateValidator;
            _mapper = mapper;
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
            try
            {
                _logger.LogInformation("Iniciando criação da tarefa: {Title}", dto.Title);

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

                _logger.LogInformation("Tarefa criada com sucesso!");

                return _mapper.Map<TaskResponseDto>(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar tarefa: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateTaskDto dto)
        {
            try
            {
                _logger.LogInformation("Iniciando atualização da tarefa: {Title}", dto.Title);

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

                _logger.LogInformation("Tarefa atualizada com sucesso!");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar tarefa: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                _logger.LogWarning("Tentando excluir a tarefa com ID: {Id}", id);

                var task = await _repository.GetByIdAsync(id);

                if (task == null)
                    return false;

                _repository.Delete(task);

                _logger.LogInformation("Tarefa deletada com sucesso!");

                return await _uow.CommitAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar tarefa: {Message}", ex.Message);
                throw;
            }
        }
        #endregion
    }
}

