using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Filters;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappings;
using TaskManager.Application.Services;
using TaskManager.Application.Validators;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Context;
using TaskManager.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

#region 1. Configuração de Serviços (DI)

// Banco de Dados em Memória
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TaskManagerDb"));

// Repositórios e Unit of Work (Padrão Híbrido)
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Serviço de Negócio
builder.Services.AddScoped<ITaskService, TaskService>();

// AutoMapper (Configurado para ler o perfil de mapeamento)
builder.Services.AddAutoMapper(typeof(MappingProfile));

// FluentValidation (Registra todos os validators do Assembly da Application)
builder.Services.AddValidatorsFromAssemblyContaining<TaskCreateValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<TaskUpdateValidator>();

// Controllers com customização de erro de validação (FluentValidation)
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => {
                    if (e.Exception is System.Text.Json.JsonException || e.ErrorMessage.Contains("could not be converted"))
                        return "Formato de dado inválido. Verifique se o Status ou Data estão corretos.";

                    return e.ErrorMessage;
                });

            return new BadRequestObjectResult(new { Errors = errors });
        };
    });

// Configuração do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

#endregion

var app = builder.Build();

#region 2. Pipeline de Requisição (Middlewares)

app.UseCors("DefaultPolicy");

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Ativa Swagger apenas no ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskManager API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Mapeia os endpoints dos Controllers para as rotas da API
app.MapControllers();

app.MapGet("/", context => {
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.Run();

#endregion
