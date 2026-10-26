using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.DTOs.Requests.User
{
    public class ChangePasswordRequest
    {
        [Required][MinLength(8)] public string Password { get; set; } = string.Empty;

        [Required][MinLength(8)] public string NewPassword { get; set; } = string.Empty;
    }
}