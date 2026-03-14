using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform.Survey.Questions;

namespace Conversey.BL.Domain.Subplatform.Survey;

public class Project
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public Slug Slug { get; set; }

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
    
    public IEnumerable<Question> Questions { get; set; }

    [Required]
    public Workspace Workspace { get; set; }

    public IEnumerable<Youth> Youths { get; set; }
    
    
}