using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappings;
using TaskManager.Application.Services;
using TaskManager.Application.Validators;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Context;
using TaskManager.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Configura o Banco em Memória
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TaskManagerDb"));

// 2. Registra Repositório, Unit of Work e Service
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITaskService, TaskService>();

// 3. Configura o AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// 4. Configura o FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<TaskCreateValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<TaskUpdateValidator>();

// 5. Configura Controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
