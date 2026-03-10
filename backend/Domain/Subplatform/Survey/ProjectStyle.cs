using System.Drawing;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Domain.Subplatform.Survey;

public struct ProjectStyle
{
    public Color[] Theme { get; set; }
    public Image? Logo { get; set; }
    // To-Do public Font Font { get; set; }
}