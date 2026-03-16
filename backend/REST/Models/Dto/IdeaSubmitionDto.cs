using System.ComponentModel.DataAnnotations;

namespace Conversey.REST.Models.Dto;

public class IdeaSubmitionDto
{
    [Required]
    public string Content { get; set; }
    public bool ForceSubmit { get; set; }
}