using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Conversey.UI_MVC.Models.Dto;

public sealed class YouthContactEmailDto
{
	[Required]
	[EmailAddress]
	[JsonPropertyName("email")]
	public string Email { get; set; } = string.Empty;
}



