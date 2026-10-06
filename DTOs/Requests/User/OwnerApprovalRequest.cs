using System.ComponentModel.DataAnnotations;

public class OwnerApprovalRequest
{
    [Required]
    public bool IsApproved {get; set;}
}