using Granja.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Granja.Web.Middleware;

/// <summary>
/// Manejo centralizado de errores: traduce las excepciones a respuestas ProblemDetails
/// con el código HTTP correcto (400, 404, 409 o 500).
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetails = problemDetails;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            EntidadNoEncontradaException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            ReglaNegocioException => (StatusCodes.Status409Conflict, "Conflicto con una regla de negocio"),
            DbUpdateException => (StatusCodes.Status409Conflict, "Conflicto al guardar los datos"),
            DomainException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Error no controlado");
        else
            _logger.LogInformation("Solicitud rechazada ({Status}): {Message}", status, exception.Message);

        httpContext.Response.StatusCode = status;
        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == StatusCodes.Status500InternalServerError
                    ? "Ocurrió un error inesperado."
                    : exception is DbUpdateException ? "La operación viola una restricción de datos (posible registro duplicado)." : exception.Message,
                Type = $"https://httpstatuses.io/{status}",
                Instance = httpContext.Request.Path
            }
        });
    }
}
