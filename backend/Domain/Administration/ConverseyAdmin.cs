using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Administration;

public class ConverseyAdmin
{
    [Required] 
    public int Id { get; set; }
}