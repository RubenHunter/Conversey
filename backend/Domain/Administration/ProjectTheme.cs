using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Domain.Administration;

public class ProjectTheme
{
    public Slug ProjectId { get; set; }
    public Project Project { get; set; }

    [StringLength(7)]
    public string Primary { get; set; } = "#6c5ce7";

    [StringLength(7)]
    public string Secondary { get; set; } = "#db99c8";

    [StringLength(7)]
    public string Accent { get; set; } = "#cd6f88";

    [StringLength(32)]
    public string Preset { get; set; } = "default";

    [StringLength(64)]
    public string Font { get; set; } = "Helvetica";

    public static ProjectTheme Default => new()
    {
        Primary = "#6c5ce7",
        Secondary = "#db99c8",
        Accent = "#cd6f88",
        Preset = "default",
        Font = "Helvetica"
    };
}
