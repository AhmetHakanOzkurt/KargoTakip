using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KargoTakip.ServiceDefaults
{
    /// <summary>
    /// Bu middleware onceden yalnizca AuthService ve OrderService'te vardi;
    /// diger dort serviste yakalanmamis hatalar ham stack trace ile
    /// donebiliyordu. Artik tum servisler ayni davranisi paylasir.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Beklenmeyen hata: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                statusCode = context.Response.StatusCode,
                message = "Beklenmeyen bir hata oluştu.",
                // Istisna metni ic yapiyi ifsa edebilir; yalnizca gelistirmede.
                detail = _environment.IsDevelopment() ? exception.Message : null
            };

            // Istemci baglantiyi kesmisse yazma islemi de iptal edilebilmeli.
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response), context.RequestAborted);
        }
    }
}
