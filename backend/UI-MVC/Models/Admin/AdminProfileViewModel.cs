using System.ComponentModel.DataAnnotations;

namespace Conversey.UI_MVC.Models.Admin;

public class AdminProfileViewModel
{
    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; }

    [Display(Name = "Workspace name")]
    [MaxLength(49)]
    public string? WorkspaceName { get; set; }

    [Display(Name = "Workspace logo URL")]
    public string? WorkspaceLogo { get; set; }
}
