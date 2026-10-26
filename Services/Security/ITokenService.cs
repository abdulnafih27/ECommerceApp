using ECommerceApp.Models;

public interface ITokenService
{
    string GenerateToken(User user);
}