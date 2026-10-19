using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation("Client aborted the request: {Path}", context.Request.Path);
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 499;
            }
        }
        catch(Exception ex)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            _logger.LogError(ex, "Unhandled exception occurred {TraceId}", traceId);

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = ex switch
                {
                    NotImplementedException => StatusCodes.Status501NotImplemented,
                    // Default fallback for unhandled server errors
                    _ => StatusCodes.Status500InternalServerError  
                };
                
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = context.Response.StatusCode,
                    Title = ReasonPhrases.GetReasonPhrase(context.Response.StatusCode),
                    Detail = "An unexpected error occurred.",
                    Instance = context.Request.Path,
                    Extensions = new Dictionary<string, object?>
                    {
                        {"traceId", traceId}
                    }

                }, context.RequestAborted);
            }

        }
        
    }
}