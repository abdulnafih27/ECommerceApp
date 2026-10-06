public interface IAuthService
{
    public Task<ServiceResult<AuthUserResponse>> CreateUser(CreateUserRequest user, CancellationToken cancellationToken);
    public Task<ServiceResult<AuthUserResponse>> LoginUser(LoginUserRequest user, CancellationToken cancellationToken);
    public Task<ServiceResult<OwnerRequestResponse>> RegisterOwner(RegisterOwnerRequest user, CancellationToken cancellationToken);
}