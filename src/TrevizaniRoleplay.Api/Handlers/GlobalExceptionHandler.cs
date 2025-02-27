using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;
using TrevizaniRoleplay.Core.Models.Responses;

namespace TrevizaniRoleplay.Api.Handlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var code = HttpStatusCode.InternalServerError;

        var errorsResponse = new ErrorsResponse
        {
            Errors = ["Internal Server Error"]
        };

        if (exception is ArgumentException)
        {
            errorsResponse.Errors = [exception.Message];
            code = HttpStatusCode.BadRequest;
        }
        else if (exception is AggregateException aggregateException)
        {
            errorsResponse.Errors = aggregateException.InnerExceptions.Select(x => x.Message).ToList();
            code = HttpStatusCode.BadRequest;
        }
        else
        {
            var errorMessage = exception.InnerException?.Message ?? exception.Message;
            logger.LogError(exception, "Exception occurred: {Message}", errorMessage);
        }

        httpContext.Response.StatusCode = (int)code;
        await httpContext.Response.WriteAsJsonAsync(
            errorsResponse,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase },
            cancellationToken
        );

        return true;
    }
}