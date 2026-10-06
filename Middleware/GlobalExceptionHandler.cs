using Microsoft.AspNetCore.Mvc;

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
            context.Response.StatusCode = 499;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occured");

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = ex switch
                {
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    NotImplementedException => StatusCodes.Status501NotImplemented,
                    // Default fallback for unhandled server errors
                    _ => StatusCodes.Status500InternalServerError  
                };
                
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                Status = context.Response.StatusCode,
                Title = "An Error occured",
                Detail = ex.Message
                }, context.RequestAborted);
            }

        }
        
    }
}