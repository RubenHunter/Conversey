using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Domain.Survey;

namespace Conversey.BL.Domain.Administration;

public class Project
{
    [Required]
    public Slug Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; }
    
    [StringLength(4000)]
    public string Description { get; set; }

    [StringLength(2048)]
    public string ImageUrl { get; set; }
    public Status Status { get; set; }
    
    [Range(0, 150)]
    public int? MinAge { get; set; }
    
    [Range(0, 150)]
    public int? MaxAge { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public InteractionType InteractionForm { get; set; }
    
    [Range(1, 5)]
    public int NudgingStrength { get; set; } = 3;
    
    public IEnumerable<Topic> Topic { get; set; }

    public ProjectTheme Theme { get; set; }

    public IEnumerable<Question> Questions { get; set; }

    [Required]
    public Workspace Workspace { get; set; }

    public IEnumerable<Youth> Youth { get; set; }

    public IEnumerable<Idea> ProjectIdeas { get; set; }
}

public enum Status
{
    Draft,
    Active,
    Archived
}

public enum InteractionType
{
    Chat,
    VerticalScroll,
    UserDefined
}
