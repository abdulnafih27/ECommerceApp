using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.DTOs.Requests.User
{
    public class DeleteUserRequest
    {
        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}