using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Subplatform.Survey;

public class Topic
{
    [Required] public int Id { get; set; }
    public Subplatform.Survey.Project Project { get; set; }
    public string Name { get; set; }
    public string Context { get; set; }
}