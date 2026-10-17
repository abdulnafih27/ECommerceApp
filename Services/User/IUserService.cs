public interface IUserService
{
    // User Services change password and delete user can be used by all
    public Task<ServiceResult<UserDetailsResponse>> GetUserDetails(int userId, CancellationToken cancellationToken);
    public Task<ServiceResult<UserDetailsResponse>> UpdateUser(int userId, UpdateUserRequest request, CancellationToken cancellationToken);
    public Task<ServiceResult> ChangePassword(int userId, ChangePasswordRequest request, CancellationToken cancellationToken);
    public Task<ServiceResult> DeleteUser(int userId, DeleteUserRequest request, CancellationToken cancellationToken);

    // Admin Services
    public Task<ServiceResult<PagedResult<UserDetailsResponse>>> GetAllUsers(Pagination pagination, CancellationToken cancellationToken);
}