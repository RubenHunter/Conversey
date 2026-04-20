using System.ComponentModel.DataAnnotations;

namespace Conversey.UI_MVC.Models.Dto;

public class IdeaSubmitionDto
{
    [Required]
    public string Content { get; set; }
    public bool ForceSubmit { get; set; }
}