namespace TaskManager.API.Filters
{
    using System.Net;
    using System.Text.Json;
    using FluentValidation;

    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError; // 500 por padrão
            object errors = new { message = "Ocorreu um erro interno no servidor." };

            // Se o erro for de validação do FluentValidation
            if (exception is ValidationException validationException)
            {
                code = HttpStatusCode.BadRequest; // 400
                errors = new { errors = validationException.Errors.Select(e => e.ErrorMessage) };
            }
            else if (exception is JsonException)
            {
                code = HttpStatusCode.BadRequest;
                errors = new { errors = new[] { "Formato de dados inválido. Verifique os campos de Status ou Data." } };
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            var result = JsonSerializer.Serialize(errors);
            return context.Response.WriteAsync(result);
        }
    }
}
