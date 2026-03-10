using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain;

public class ConverseyAdmin
{
    [Required] public int Id { get; set; }
}