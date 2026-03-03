using System.Drawing;

namespace Conversey.BL.Domain.Entities.Project;

public struct ProjectStyle
{
    public Color[] Theme { get; set; }
    public Image? Logo { get; set; }
    // To-Do public Font Font { get; set; }
}