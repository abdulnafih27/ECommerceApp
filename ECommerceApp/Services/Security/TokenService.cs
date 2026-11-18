using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerceApp.Models;
using Microsoft.IdentityModel.Tokens;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user)
    {
         // Get all the details from teh config
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing from configuration.");
        var jwtIssuer = _config["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing from configuration.");
        var jwtAudience = _config["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is missing from configuration.");
        var jwtDuration = _config["Jwt:ExpiryInMinutes"] ?? throw new InvalidOperationException("JWT ExpiryInMinutes is missing from configuration.");
       
       
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {   
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtDuration)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);    
    }
}