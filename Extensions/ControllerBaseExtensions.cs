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
        return result.ErrorType switch
        {
            ErrorType.Conflict => controller.Conflict(result.Error),
            ErrorType.Unauthorized => controller.Unauthorized(result.Error),
            ErrorType.NotFound => controller.NotFound(result.Error),
            ErrorType.ValidationFailed => controller.BadRequest(result.Error),
            ErrorType.Forbidden => controller.StatusCode(403, result.Error),
            _ => controller.StatusCode(500, result.Error)
        };
    }
}