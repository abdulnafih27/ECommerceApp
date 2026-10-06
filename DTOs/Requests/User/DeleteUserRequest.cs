using System.ComponentModel.DataAnnotations;

public class DeleteUserRequest
{
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}