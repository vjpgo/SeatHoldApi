namespace SeatHold.Api.Middleware;

using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SeatHold.Core.Exceptions;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (SeatAlreadyHeldException ex)
        {
            await WriteProblemDetailsAsync(
                context,
                statusCode: StatusCodes.Status409Conflict,
                title: "Seat already held",
                detail: ex.Message).ConfigureAwait(false);
        }
        catch (InvalidHoldRequestException ex)
        {
            await WriteProblemDetailsAsync(
                context,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid request",
                detail: ex.Message).ConfigureAwait(false);
        }
        catch (Exception)
        {
            await WriteProblemDetailsAsync(
                context,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unexpected error",
                detail: "An unexpected error occurred.").ConfigureAwait(false);
        }
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        await context.Response.WriteAsJsonAsync(problem).ConfigureAwait(false);
    }
}
