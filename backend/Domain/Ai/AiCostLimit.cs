using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Domain.Ai;

public class AiCostLimit
{
    public int Id { get; set; }

    public decimal LimitAmount { get; set; }

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Slug? WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }

    public Slug? ProjectId { get; set; }
    public Project? Project { get; set; }

    public bool IsWorkspaceLimit => WorkspaceId.HasValue && !ProjectId.HasValue;
}
