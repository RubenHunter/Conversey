using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Enums;

namespace Conversey.BL.Domain.Entities.Project;

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
    public ProjectStyle Style { get; set; }
    
    public IEnumerable<Question.Question> Questions { get; set; }
}