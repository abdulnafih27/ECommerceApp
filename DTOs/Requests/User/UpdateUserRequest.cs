using System.ComponentModel.DataAnnotations;

public class UpdateUserRequest
{
    public string? Username { get; set; }
    [EmailAddress]
    public string? Email { get; set; } 
    [Phone]
    public string? PhoneNumber { get; set; }
}