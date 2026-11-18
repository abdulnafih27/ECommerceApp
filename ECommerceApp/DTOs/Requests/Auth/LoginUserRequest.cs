using System.ComponentModel.DataAnnotations;


namespace ECommerceApp.DTOs.Requests.Auth
{
    public class LoginUserRequest
    {
        [Required]
        public string Identifier { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}