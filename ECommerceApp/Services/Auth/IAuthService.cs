using ECommerceApp.DTOs.Responses.Auth;
using ECommerceApp.DTOs.Requests.Auth;

public interface IAuthService
{
    public Task<ServiceResult<AuthUserResponse>> CreateUser(CreateUserRequest user, CancellationToken cancellationToken);
    public Task<ServiceResult<AuthUserResponse>> LoginUser(LoginUserRequest user, CancellationToken cancellationToken);
}