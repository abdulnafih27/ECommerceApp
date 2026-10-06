using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> RegisterUser(CreateUserRequest user, CancellationToken cancellationToken)
    {
        var res = await _authService.CreateUser(user, cancellationToken);
        return this.ToActionResult(res);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser(LoginUserRequest user, CancellationToken cancellationToken)
    {
        var res = await _authService.LoginUser(user, cancellationToken);
        return this.ToActionResult(res);
    }

    [HttpPost("owner/register")]
    public async Task<IActionResult> RegisterOwner(RegisterOwnerRequest user, CancellationToken cancellationToken)
    {
        var res = await _authService.RegisterOwner(user, cancellationToken);
        return this.ToActionResult(res);
    }
}