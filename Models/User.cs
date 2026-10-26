namespace ECommerceApp.Models
{
    public class User : IAuditable
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public Roles Role { get; set; } = Roles.User;
        public bool IsDeleted { get; set; } = false;
        public UserStatus Status { get; set; } = UserStatus.Active;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;
    }
}