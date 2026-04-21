using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.Application.DTOs.TaskDtos;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

public class TaskServiceTests
{
    #region Private Fields & Setup
    private readonly Mock<ITaskRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateTaskDto>> _validatorMock;
    private readonly Mock<IValidator<UpdateTaskDto>> _updateValidatorMock;
    private readonly Mock<ILogger<TaskService>> _loggerMock;
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _repositoryMock = new Mock<ITaskRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<CreateTaskDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateTaskDto>>();
        _loggerMock = new Mock<ILogger<TaskService>>();

        _service = new TaskService(
            _repositoryMock.Object,
            _uowMock.Object,
            _loggerMock.Object,
            _validatorMock.Object,
            _updateValidatorMock.Object,
            _mapperMock.Object
        );
    }
    #endregion

    #region Create Operation Tests
    [Fact]
    public async Task CreateAsync_ShouldReturnTaskResponse_WhenDataIsValid()
    {
        var dto = new CreateTaskDto
        {
            Title = "Teste Unitário",
            Status = TaskItemStatus.Pending
        };

        _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _uowMock.Setup(u => u.CommitAsync()).ReturnsAsync(true);

        _mapperMock.Setup(m => m.Map<TaskResponseDto>(It.IsAny<TaskItem>()))
            .Returns(new TaskResponseDto { Title = dto.Title, Status = "Pendente" });

        var result = await _service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Once);
        _uowMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenDataIsInvalid()
    {
        var dto = new CreateTaskDto { Title = "" };
        var failures = new List<FluentValidation.Results.ValidationFailure>
        {
            new ("Title", "O título é obrigatório.")
        };

        _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(failures));

        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(dto));

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Never);
    }
    #endregion

    #region Read Operation Tests
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTaskDoesNotExist()
    {
        var taskId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        var result = await _service.GetByIdAsync(taskId);

        Assert.Null(result);
    }
    #endregion

    #region Update Operation Tests
    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenTaskExistsAndIsValid()
    {
        var taskId = Guid.NewGuid();
        var dto = new UpdateTaskDto { Title = "Título Atualizado", Status = TaskItemStatus.Completed };
        var taskItem = new TaskItem("Título Antigo", "Desc", null, TaskItemStatus.Pending);

        _updateValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _repositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(taskItem);

        _uowMock.Setup(u => u.CommitAsync()).ReturnsAsync(true);

        var result = await _service.UpdateAsync(taskId, dto);

        Assert.True(result);
        Assert.Equal("Título Atualizado", taskItem.Title);
        _uowMock.Verify(u => u.CommitAsync(), Times.Once);
    }
    #endregion

    #region Delete Operation Tests
    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenTaskExists()
    {
        var taskId = Guid.NewGuid();
        var taskItem = new TaskItem("Tarefa para Deletar", "Desc", null, TaskItemStatus.Pending);

        _repositoryMock.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(taskItem);
        _uowMock.Setup(u => u.CommitAsync()).ReturnsAsync(true);

        var result = await _service.DeleteAsync(taskId);

        Assert.True(result);
        _repositoryMock.Verify(r => r.Delete(taskItem), Times.Once);
        _uowMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenTaskDoesNotExist()
    {
        var taskId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(taskId))
            .ReturnsAsync((TaskItem?)null);

        var result = await _service.DeleteAsync(taskId);

        Assert.False(result);
        _repositoryMock.Verify(r => r.Delete(It.IsAny<TaskItem>()), Times.Never);
        _uowMock.Verify(u => u.CommitAsync(), Times.Never);
    }
    #endregion
}
