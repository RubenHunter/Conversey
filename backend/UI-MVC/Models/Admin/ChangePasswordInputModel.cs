using System.ComponentModel.DataAnnotations;

namespace Conversey.UI_MVC.Models.Admin;

public class ChangePasswordInputModel
{
    [DataType(DataType.Password)]
    public string? CurrentPassword { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6,
        ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
