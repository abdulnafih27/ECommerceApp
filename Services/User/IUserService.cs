public interface IUserService
{
    // User Services change password and delete user can be used by all
    public Task<ServiceResult<GetUserResponse>> GetUserDetails(int userId, CancellationToken cancellationToken);
    public Task<ServiceResult<UpdateUserResponse>> UpdateUser(int userId, UpdateUserRequest user, CancellationToken cancellationToken);
    public Task<ServiceResult> ChangePassword(int userId, ChangePasswordRequest user, CancellationToken cancellationToken);
    public Task<ServiceResult> DeleteUser(int userId, DeleteUserRequest user, CancellationToken cancellationToken);

    // Owner Services
    //public Task<ServiceResult<GetOwnerResponse>> GetOwnerDetails(int ownerId, CancellationToken cancellationToken);
    //public Task<ServiceResult<UpdateOwnerResponse>> UpdateOwner(int ownerId, UpdateOwnerRequest user, CancellationToken cancellationToken);

    //// Admin Services
    //public Task<ServiceResult<List<GetUserResponse>>> GetAllUsers(int page, int pageSize, CancellationToken cancellationToken);
    //public Task<ServiceResult<List<GetOwnerResponse>>> GetAllOwners(int page, int pageSize, CancellationToken cancellationToken);
    //public Task<ServiceResult<List<GetUserResponse>>> GetAllOwnerRequests(int page, int pageSize, CancellationToken cancellationToken);
    //public Task<ServiceResult> OwnerApproval(int ownerId, OwnerApprovalRequest req, CancellationToken cancellationToken);
}