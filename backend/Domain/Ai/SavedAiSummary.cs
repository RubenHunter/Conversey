using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Domain.Ai;

public class SavedAiSummary
{
    public int Id { get; set; }

    public Slug? WorkspaceId { get; set; }
    public Workspace Workspace { get; set; }

    public Slug? ProjectId { get; set; }
    public Project Project { get; set; }

    [MaxLength(200)]
    public string Focus { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Language { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Overview { get; set; } = string.Empty;

    public string TrendsJson { get; set; } = "[]";

    public string MinorityViewsJson { get; set; } = "[]";

    public string NotableQuotesJson { get; set; } = "[]";

    public string SuggestedActionsJson { get; set; } = "[]";

    public DateTime GeneratedAt { get; set; }
}
