using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ToolShare.Api;

// WHY one handler: no try/catch in any action. Every failure, old and new,
// leaves the API in the same RFC 9457 problem+json shape.
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken ct)
    {
        // Map each exception type to a status code and a title.
        var (status, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            IdempotencyKeyReuseException => (StatusCodes.Status422UnprocessableEntity, "Idempotency key reused"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        // WHY log only unexpected errors: 404/409 are normal business outcomes,
        // not bugs. A 500 is a real problem someone needs to look at.
        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            // WHY hide details on 500: never leak internals to the caller.
            Detail = status == StatusCodes.Status500InternalServerError
                ? "Something went wrong on our side."
                : exception is ValidationException
                    ? "One or more validation errors occurred."
                    : exception.Message,
            Instance = context.Request.Path
        };

        // Validation errors get a per-field "errors" dictionary (RFC 9457 extension member).
        if (exception is ValidationException ve)
        {
            problem.Extensions["errors"] = ve.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        context.Response.StatusCode = status;

        // WHY IProblemDetailsService: it writes the application/problem+json content type for us.
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}