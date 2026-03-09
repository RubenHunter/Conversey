using System.Drawing;
using Conversey.BL.Domain.Entities;

namespace Conversey.BL.Domain.Workspace.Project;

public struct ProjectStyle
{
    public Color[] Theme { get; set; }
    public Image? Logo { get; set; }
    // To-Do public Font Font { get; set; }
}