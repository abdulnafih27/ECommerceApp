using AutoMapper;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _hasher;

    public UserService(AppDbContext context, ILogger<UserService> logger, IMapper mapper, IPasswordHasher hasher)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
        _hasher = hasher;
    }
    public async Task<ServiceResult<UserDetailsResponse>> GetUserDetails(int userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u=> u.UserId == userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Get UserDetails Failed: User doesn't exist {UserId}",userId);
            return ServiceResult<UserDetailsResponse>.Failed("User doesn't exist", ErrorType.NotFound);
        }

        return ServiceResult<UserDetailsResponse>.Success(_mapper.Map<UserDetailsResponse>(user));
    }
    public async Task<ServiceResult<UserDetailsResponse>> UpdateUser(int userId, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u=> u.UserId == userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Update Failed: User doesn't exist {UserId}",userId);
            return ServiceResult<UserDetailsResponse>.Failed("User doesn't exist", ErrorType.NotFound);
        }

        if (request.Email != null)
        {
            var emailExists = await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.UserId != userId && u.Email == request.Email.ToLowerInvariant(), cancellationToken);
            if (emailExists)
            {
                _logger.LogWarning("Update Failed: dublicate email {Email}", request.Email);
                return ServiceResult<UserDetailsResponse>.Failed("Email already exists", ErrorType.Conflict);
            }
            user.Email = request.Email;
        }

        if (request.Username != null)
        {
            var usernameExists = await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.UserId != userId && u.Username == request.Username.ToLowerInvariant(), cancellationToken);
            if (usernameExists)
            {
                _logger.LogWarning("Update Failed: dublicate username {Username}",request.Username);
                return ServiceResult<UserDetailsResponse>.Failed("Username already exists", ErrorType.Conflict);
            }

            user.Username = request.Username;
        }

        if (request.PhoneNumber != null)
        {
            user.PhoneNumber = request.PhoneNumber;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Update successful for user {UserId}", userId);
        return ServiceResult<UserDetailsResponse>.Success(_mapper.Map<UserDetailsResponse>(user));
    }
    public async Task<ServiceResult> DeleteUser(int userId, DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u=> u.UserId == userId, cancellationToken);


        if (user == null)
        {
            _logger.LogWarning("Delete Failed: User doesn't exist {UserId}",userId);
            return ServiceResult.Failed("User doesn't exist", ErrorType.NotFound);
        }

        if (user.Role == Roles.Admin)
        {
            _logger.LogWarning("Delete Failed: Admin cannot delete their account {UserId}", userId);
            return ServiceResult.Failed("Admin cannot delete their account", ErrorType.Forbidden);
        }

        if (!_hasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Delete user Failed: Invalid Credentials");
            return ServiceResult.Failed("Invalid Credentials", ErrorType.Unauthorized);
        }

        user.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Username} was deleted successfully", user.Username);
        return ServiceResult.Success();
    }
    public async Task<ServiceResult> ChangePassword(int userId, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u=> u.UserId == userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Change Password Failed: User doesn't exist {UserId}",userId);
            return ServiceResult.Failed("User doesn't exist", ErrorType.NotFound);
        }

        if (!_hasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Change Password Failed: Invalid Credentials");
            return ServiceResult.Failed("Invalid Credentials", ErrorType.Unauthorized);
        }

        user.PasswordHash = _hasher.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }
    public async Task<ServiceResult<PagedResult<UserDetailsResponse>>> GetAllUsers(Pagination pagination, CancellationToken cancellationToken)
    {
        var page = pagination.Page;
        var pageSize = pagination.PageSize;
        
        var usersCount = await _context.Users.CountAsync(cancellationToken);
        var users = await _context.Users
                                    .OrderByDescending(u => u.CreatedOn)
                                    .ThenBy(u => u.UserId)
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync(cancellationToken);

        PagedResult<UserDetailsResponse> res = PagedResult<UserDetailsResponse>.Create(_mapper.Map<List<UserDetailsResponse>>(users), pagination, usersCount);
        
        return ServiceResult<PagedResult<UserDetailsResponse>>.Success(res);

    }
}