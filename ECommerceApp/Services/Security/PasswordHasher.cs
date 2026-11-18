public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password) => BCrypt.Net.BCrypt.EnhancedHashPassword(password, 12);

    public bool VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
}