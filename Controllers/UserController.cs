using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetUserDetails(CancellationToken cancellationToken)
    {
        var userId = User.GetCurrentUser();
        var res = await _userService.GetUserDetails(userId, cancellationToken);
        return this.ToActionResult(res);
    }

    [HttpPatch("me/update")]
    [Authorize(policy: "ActiveStatus")]
    public async Task<IActionResult> UpdateUser(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetCurrentUser();
        var res = await _userService.UpdateUser(userId, request, cancellationToken);
        return this.ToActionResult(res);
    }

    [HttpDelete("me/delete")]
    [Authorize(policy: "ActiveStatus")]
    public async Task<IActionResult> DeleteUser(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetCurrentUser();
        var res = await _userService.DeleteUser(userId, request, cancellationToken);
        return this.ToActionResult(res);
    }

    [HttpPatch("me/password")]
    [Authorize(policy: "ActiveStatus")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetCurrentUser();
        var res = await _userService.ChangePassword(userId, request, cancellationToken);
        return this.ToActionResult(res);
    }

    [HttpGet("/api/admin/users")]
    [Authorize(Roles = "Admin"), Authorize(policy: "ActiveStatus")]
    public async Task<IActionResult> GetAllUsers([FromQuery] Pagination pagination, CancellationToken cancellationToken)
    {
        var res = await _userService.GetAllUsers(pagination, cancellationToken);
        return this.ToActionResult(res);
    }
}