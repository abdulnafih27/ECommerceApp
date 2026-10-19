using Microsoft.AspNetCore.Mvc;

public static class ControllerBaseExtensions
{
    public static IActionResult ToActionResult<T>(this ControllerBase controller, ServiceResult<T> result)
    {
        if(result.IsSuccess) return controller.Ok(result.Data);
        
         return ToErrorResult(controller, result);
    } 
    public static IActionResult ToActionResult(this ControllerBase controller, ServiceResult result)
    {
        if(result.IsSuccess) return controller.Ok();

        return ToErrorResult(controller, result);
    } 

    private static ObjectResult ToErrorResult(ControllerBase controller, ServiceResult result)
    {
        return controller.Problem(detail: result.Error, statusCode: ToStatusCode(result.ErrorType));
            
    }

    private static int ToStatusCode(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.ValidationFailed => StatusCodes.Status400BadRequest,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => throw new ArgumentOutOfRangeException(nameof(errorType), errorType, "No HTTP status mapped for this ErrorType")
        };
    }
}