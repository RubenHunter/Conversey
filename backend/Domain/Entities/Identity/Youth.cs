using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Entities.Identity;

public class Youth
{
    [Required]
    public string Token { get; set; }
}