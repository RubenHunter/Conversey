using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Subplatform.Survey.Ideation;

namespace Conversey.BL.Domain.Subplatform.Survey;

public class Youth
{
    [Key]
    [Required]
    public string Token { get; set; }

    public Project Project { get; set; }

    public string Email { get; set; } = "";

    public ICollection<Idea> Ideas { get; set; } = new List<Idea>();

    public ICollection<Response> Responses { get; set; } = new List<Response>();

    public ICollection<ResponseReaction> ResponseReactions { get; set; } = new List<ResponseReaction>();
}