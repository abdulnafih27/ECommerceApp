using AutoMapper;
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
    public async Task<ServiceResult<GetUserResponse>> GetUserDetails(int userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u=> u.UserId == userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Get UserDetails Failed: User doesn't exist {UserId}",userId);
            return ServiceResult<GetUserResponse>.Failed("User doesn't exist", ErrorType.NotFound);
        }

        return ServiceResult<GetUserResponse>.Success(_mapper.Map<GetUserResponse>(user));
    }
    public async Task<ServiceResult<UpdateUserResponse>> UpdateUser(int userId, UpdateUserRequest req, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u=> u.UserId == userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Update Failed: User doesn't exist {UserId}",userId);
            return ServiceResult<UpdateUserResponse>.Failed("User doesn't exist", ErrorType.NotFound);
        }

        if (req.Email != null)
        {
            var emailExists = await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.UserId != userId && u.Email == req.Email.ToLowerInvariant(), cancellationToken);
            if (emailExists)
            {
                _logger.LogWarning("Update Failed: dublicate email {Email}", req.Email);
                return ServiceResult<UpdateUserResponse>.Failed("Email already exists", ErrorType.Conflict);
            }
            user.Email = req.Email;
        }

        if (req.Username != null)
        {
            var usernameExists = await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.UserId != userId && u.Username == req.Username.ToLowerInvariant(), cancellationToken);
            if (usernameExists)
            {
                _logger.LogWarning("Update Failed: dublicate username {Username}",req.Username);
                return ServiceResult<UpdateUserResponse>.Failed("Username already exists", ErrorType.Conflict);
            }

            user.Username = req.Username;
        }

        if (req.PhoneNumber != null)
        {
            user.PhoneNumber = req.PhoneNumber;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Update successful for user {UserId}", userId);
        return ServiceResult<UpdateUserResponse>.Success(_mapper.Map<UpdateUserResponse>(user));
    }
    public async Task<ServiceResult> DeleteUser(int userId, DeleteUserRequest req, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u=> u.UserId == userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Delete Failed: User doesn't exist {UserId}",userId);
            return ServiceResult.Failed("User doesn't exist", ErrorType.NotFound);
        }

        if (!_hasher.VerifyPassword(req.Password, user.PasswordHash))
        {
            _logger.LogWarning("Delete user Failed: Invalid Credentials");
            return ServiceResult.Failed("Invalid Credentials", ErrorType.Unauthorized);
        }

        user.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Username} was deleted successfully", user.Username);
        return ServiceResult.Success();
    }
    public async Task<ServiceResult> ChangePassword(int userId, ChangePasswordRequest req, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u=> u.UserId == userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Change Password Failed: User doesn't exist {UserId}",userId);
            return ServiceResult.Failed("User doesn't exist", ErrorType.NotFound);
        }

        if (!_hasher.VerifyPassword(req.Password, user.PasswordHash))
        {
            _logger.LogWarning("Change Password Failed: Invalid Credentials");
            return ServiceResult.Failed("Invalid Credentials", ErrorType.Unauthorized);
        }

        user.PasswordHash = _hasher.HashPassword(req.NewPassword);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

}