using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Domain.Survey;

namespace Conversey.BL.Domain.Administration;

//TODO make this extend IdentityUser
public class Youth
{
    [Required]
    public Guid Id { get; set; }
    
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public Project Project { get; set; }

    public IEnumerable<Idea> Ideas { get; set; }

    public IEnumerable<Reaction> Reactions { get; set; }

    public IEnumerable<Response> Responses { get; set; }
    
    public IEnumerable<Answer> Answers { get; set; }
}
