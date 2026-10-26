using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.DTOs.Responses.Auth
{
    public class AuthUserResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public Roles Role { get; set; }
        public UserStatus Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}