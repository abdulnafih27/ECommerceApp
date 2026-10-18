using Microsoft.AspNetCore.Mvc;

public static class ControllerBaseExtensions
{
    public static IActionResult ToActionResult<T>(this ControllerBase controller, ServiceResult<T> result)
    {
        if(result.IsSuccess) return controller.Ok(result.Data);
        
         return result.ErrorType switch
        {
            ErrorType.Conflict => controller.Conflict(result.Error),
            ErrorType.Unauthorized => controller.Unauthorized(result.Error),
            ErrorType.NotFound => controller.NotFound(result.Error),
            ErrorType.ValidationFailed => controller.BadRequest(result.Error),
            _ => controller.StatusCode(500, result.Error)
        };
    } 
    public static IActionResult ToActionResult(this ControllerBase controller, ServiceResult result)
    {
        if(result.IsSuccess) return controller.Ok();
        
         return result.ErrorType switch
        {
            ErrorType.Conflict => controller.Conflict(result.Error),
            ErrorType.Unauthorized => controller.Unauthorized(result.Error),
            ErrorType.NotFound => controller.NotFound(result.Error),
            ErrorType.ValidationFailed => controller.BadRequest(result.Error),
            _ => controller.StatusCode(500, result.Error)
        };
    } 
}