using Conversey.BL.Ai;

namespace Conversey.UI_MVC.Models.AiAdmin;

public class AiDashboardViewModel
{
    public AiCostsSummary CostsSummary { get; set; } = new();
    public int TotalProviders { get; set; }
    public int TotalPrompts { get; set; }
    public AiHealthCheckResult HealthCheck { get; set; } = new();
}
