using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conversey.BL.Domain.Workspace.Project;

public class Project
{
    [Required]
    public int Id { get; set; }

    [StringLength(100)]
    public string Title { get; set; }
    
    [StringLength(4000)]
    public string Description { get; set; }
    public Status Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public InteractionType InteractionForm { get; set; }
    
    public IEnumerable<Topic> Topic { get; set; }
    [NotMapped]
    public ProjectStyle Style { get; set; }
    
    public IEnumerable<Question.Question> Questions { get; set; }

    [Required]
    public Workspace Workspace { get; set; }

    public IEnumerable<Youth> Youths { get; set; }
}