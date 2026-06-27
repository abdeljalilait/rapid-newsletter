using System.Net;
using System.Text.Json;
using NewsletterPlatform.Application.Common;
using NewsletterPlatform.Application.Exceptions;

namespace NewsletterPlatform.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
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
        try { await _next(context); }
        catch (Exception ex) { await HandleAsync(context, ex); }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, code, payload) = ex switch
        {
            ValidationException ve => (HttpStatusCode.BadRequest, ErrorCodes.Validation, (object)new { code = ErrorCodes.Validation, errors = ve.Errors }),
            UnauthorizedException => (HttpStatusCode.Unauthorized, ErrorCodes.Unauthorized, new { code = ErrorCodes.Unauthorized, error = ex.Message }),
            ForbiddenException => (HttpStatusCode.Forbidden, ErrorCodes.Forbidden, new { code = ErrorCodes.Forbidden, error = ex.Message }),
            NotFoundException => (HttpStatusCode.NotFound, ErrorCodes.NotFound, new { code = ErrorCodes.NotFound, error = ex.Message }),
            ConflictException => (HttpStatusCode.Conflict, ErrorCodes.Conflict, new { code = ErrorCodes.Conflict, error = ex.Message }),
            Domain.Exceptions.DomainException => (HttpStatusCode.BadRequest, ErrorCodes.Unprocessable, new { code = ErrorCodes.Unprocessable, error = ex.Message }),
            _ => (HttpStatusCode.InternalServerError, ErrorCodes.Unprocessable, (object)new { code = ErrorCodes.Unprocessable, error = "An unexpected error occurred." }),
        };

        if (status == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Unhandled exception");
        else
            _logger.LogWarning(ex, "Handled domain exception: {Code}", code);

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";
        await JsonSerializer.SerializeAsync(context.Response.Body, payload);
    }
}