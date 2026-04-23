using System.ComponentModel.DataAnnotations;

namespace Conversey.UI_MVC.Models.Dto;

public sealed class YouthContactEmailDto
{
	[Required]
	[EmailAddress]
	public string Email { get; set; } = string.Empty;
}



