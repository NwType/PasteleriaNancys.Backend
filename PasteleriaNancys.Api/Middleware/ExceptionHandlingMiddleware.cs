using System.Net;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Common.Exceptions;

namespace PasteleriaNancys.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var (status, title) = ex switch
                {
                    NoEncontradoException => (HttpStatusCode.NotFound, "Recurso no encontrado"),
                    ConflictoException => (HttpStatusCode.Conflict, "Conflicto"),
                    ReglaNegocioException => (HttpStatusCode.BadRequest, "Solicitud inválida"),
                    CredencialesInvalidasException => (HttpStatusCode.Unauthorized, "No autorizado"),
                    CuentaBloqueadaException => (HttpStatusCode.Forbidden, "Cuenta bloqueada"),
                    _ => (HttpStatusCode.InternalServerError, "Error interno del servidor")
                };

                if (status == HttpStatusCode.InternalServerError)
                {
                    _logger.LogError(ex, "Error no controlado procesando {Path}", context.Request.Path);
                }

                var problemDetails = new ProblemDetails
                {
                    Status = (int)status,
                    Title = title,
                    Detail = status == HttpStatusCode.InternalServerError ? "Ocurrió un error inesperado." : ex.Message
                };

                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = problemDetails.Status.Value;
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}
