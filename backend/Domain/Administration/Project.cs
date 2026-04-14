using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Survey;

namespace Conversey.BL.Domain.Administration;

public class Project
{
    [Required]
    public Slug Id { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public string ImageUrl { get; set; }
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

    public IEnumerable<Youth> Youth { get; set; }
}

public enum Status
{
    Draft,
    Active,
    Archived
}

public struct ProjectStyle
{
    public Color[] Theme { get; set; }
    public Image? Logo { get; set; }
    // To-Do public Font Font { get; set; }
}

public enum InteractionType
{
    Chat,
    VerticalScroll
}