using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Entities.Identity;

public class ConverseyAdmin : Admin
{
    [Required] public int Id { get; set; }
    [Required]
    public Conversey Conversey { get; set; }
}