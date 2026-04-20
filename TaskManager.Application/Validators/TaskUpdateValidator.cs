using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using TaskManager.Application.DTOs.TaskDtos;

namespace TaskManager.Application.Validators
{
    public class TaskUpdateValidator : AbstractValidator<UpdateTaskDto>
    {
        public TaskUpdateValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("O título é obrigatório.")
                .MaximumLength(100).WithMessage("O título deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Description)
               .MaximumLength(500).WithMessage("A Descrição deve ter no máximo 500 caracteres.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Status inválido.");
        }
    }
}
