using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Entities;

public struct Image
{
    [Required]
    public int Id { get; set; }
    public string Url { get; set; }
    public byte[] Data { get; set; }
}