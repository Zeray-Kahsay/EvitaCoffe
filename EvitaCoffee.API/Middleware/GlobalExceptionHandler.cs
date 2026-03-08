using System;
using EvitaCoffee.Contracts;
using EvitaCoffee.Contracts.Common;
using EvitaCoffee.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace EvitaCoffee.API.Middleware;

public class GlobalExceptionHandler (ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString()
                ?? context.TraceIdentifier;
        
        context.Response.ContentType = "application/json";

        switch(exception)
        {
            case DomainException domainEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(
                    CreateResponse("DomainError", domainEx.Message, correlationId), cancellationToken
                );
            return true;

            case UnauthorizedAccessException: 
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(
                CreateResponse("Unauthorized", "Unauthorized.", correlationId),
                cancellationToken
            );
            return true;

            default: 
                _logger.LogError(exception, "Unhandled exception");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(
                    CreateResponse("ServerError", "Unexpected error occured", correlationId)
                );
                return true;
                
        }
    }

    private static ApiResponse<object> CreateResponse(
        string code,
        string message,
        string correlationId) => new ()
        {
            Success = false,
            Error = new ApiErrorResponse
            {
                Code = code,
                Message = message
            },
            CorrelationId = correlationId
        };
}
