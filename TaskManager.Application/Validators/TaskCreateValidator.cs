using FluentValidation;
using TaskManager.Application.DTOs.TaskDtos;

namespace TaskManager.Application.Validators
{
    public class TaskCreateValidator : AbstractValidator<CreateTaskDto>
    {
        public TaskCreateValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("O título é obrigatório.")
                .MaximumLength(150).WithMessage("O título deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("A Descrição deve ter no máximo 500 caracteres.");

            RuleFor(x => x.Status)
                .NotNull().WithMessage("O status é obrigatório.")
                .IsInEnum().WithMessage("Status inválido. Escolha: 1 (Pendente), 2 (Em progresso) ou 3 (Concluída).");

            RuleFor(x => x.DueDate)
                .Must(date => !date.HasValue || date.Value.Date >= DateTime.Today)
                .WithMessage("A data de vencimento não pode ser anterior a hoje.");
        }
    }
}
