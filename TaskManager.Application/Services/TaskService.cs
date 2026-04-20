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

        #region Write Operations (Usa Repository + UoW)
        public async Task<TaskResponseDto> CreateAsync(CreateTaskDto dto)
        {
            // 1. Validação
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors); 

            // 2. Mapeamento
            var task = new TaskItem(dto.Title, dto.Description, dto.DueDate, dto.Status);

            // 3. Persistência
            await _repository.AddAsync(task);
            await _uow.CommitAsync();

            // 4. Retorno mapeado
            return _mapper.Map<TaskResponseDto>(task);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateTaskDto dto)
        {
            // 1. Validação dos dados de entrada
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            // 2. Busca a tarefa existente
            var task = await _repository.GetByIdAsync(id);
            if (task == null) return false;

            // 3. Atualiza o estado da entidade (Lógica de Domínio)
            task.Update(dto.Title, dto.Description, dto.DueDate, dto.Status);

            // 4. Persistência
            _repository.Update(task);
            await _uow.CommitAsync();

            return true;
        }
        #endregion
    }
}

