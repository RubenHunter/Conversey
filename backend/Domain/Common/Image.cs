using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Common;

public struct Image
{
    [Required]
    public int Id { get; set; }
    public byte[] Data { get; set; }
}