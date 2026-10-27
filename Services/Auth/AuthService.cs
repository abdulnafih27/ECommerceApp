using AutoMapper;
using ECommerceApp.DTOs.Responses.Auth;
using ECommerceApp.DTOs.Requests.Auth;
using Microsoft.EntityFrameworkCore;
using ECommerceApp.Models;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthService> _logger;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _token;

    public AuthService(AppDbContext context, ILogger<AuthService> logger, IMapper mapper, IPasswordHasher hasher, ITokenService token)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
        _hasher = hasher;
        _token = token;
    }

    public async Task<ServiceResult<AuthUserResponse>> CreateUser(CreateUserRequest user, CancellationToken cancellationToken)
    {
        var emailExists = await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == user.Email.ToLowerInvariant(), cancellationToken);
        if (emailExists)
        {
            _logger.LogWarning("Registration Failed: dublicate email {Email}", user.Email);
            return ServiceResult<AuthUserResponse>.Failed("Email already exists", ErrorType.Conflict);
        }

        var usernameExists = await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Username == user.Username.ToLowerInvariant(), cancellationToken);
        if (usernameExists)
        {
            _logger.LogWarning("Registration Failed: dublicate username {Username}",user.Username);
            return ServiceResult<AuthUserResponse>.Failed("Username already exists", ErrorType.Conflict);
        }

        // map the user request to the User table
        var newUser = _mapper.Map<User>(user);
        newUser.PasswordHash = _hasher.HashPassword(user.Password);

        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync(cancellationToken);

        // Generate Token and Login the user
        var res = _mapper.Map<AuthUserResponse>(newUser);
        res.Token = _token.GenerateToken(newUser);

        _logger.LogInformation("Registration Successfull: username {Username}", res.Username);
        return ServiceResult<AuthUserResponse>.Success(res);
    } 
    public async Task<ServiceResult<AuthUserResponse>> LoginUser(LoginUserRequest req, CancellationToken cancellationToken)
    {
        // Check if user exists, also verify the password
        var Identifier = req.Identifier.ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Identifier || u.Username == Identifier, cancellationToken);
        
        if (user is null || !_hasher.VerifyPassword(req.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login Failed: Invalid Credentials");
            return ServiceResult<AuthUserResponse>.Failed("Invalid Credentials", ErrorType.Unauthorized);
        }

        // return the user details with the token
        var res = _mapper.Map<AuthUserResponse>(user);
        res.Token = _token.GenerateToken(user);
        

        _logger.LogInformation("Login Successfull: username {Username}", res.Username);
        return ServiceResult<AuthUserResponse>.Success(res);
    }

}