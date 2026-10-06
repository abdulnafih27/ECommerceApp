using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]/me")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserDetails(CancellationToken cancellationToken)
    {
        var userId = User.GetCurrentUser();
        var res = await _userService.GetUserDetails(userId, cancellationToken);
        return this.ToActionResult(res);
    }

    [HttpPatch("update")]
    [Authorize(policy: "ActiveStatus")]
    public async Task<IActionResult> UpdateUser(UpdateUserRequest req, CancellationToken cancellationToken)
    {
        var userId = User.GetCurrentUser();
        var res = await _userService.UpdateUser(userId, req, cancellationToken);
        return this.ToActionResult(res);
    }

    [HttpDelete("delete")]
    [Authorize(policy: "ActiveStatus")]
    public async Task<IActionResult> DeleteUser(DeleteUserRequest req, CancellationToken cancellationToken)
    {
        var userId = User.GetCurrentUser();
        var res = await _userService.DeleteUser(userId, req, cancellationToken);
        return this.ToActionResult(res);
    }

    [HttpPatch("password")]
    [Authorize(policy: "ActiveStatus")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest req, CancellationToken cancellationToken)
    {
        var userId = User.GetCurrentUser();
        var res = await _userService.ChangePassword(userId, req, cancellationToken);
        return this.ToActionResult(res);
    }
}