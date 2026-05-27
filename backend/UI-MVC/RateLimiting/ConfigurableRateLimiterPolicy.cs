using System.Security.Claims;
using System.Threading.RateLimiting;
using Conversey.BL.Ai;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace Conversey.UI_MVC.RateLimiting;

public class AiUserRateLimiterPolicy : ConfigurableRateLimiterPolicy
{
    public AiUserRateLimiterPolicy(RateLimitConfigCache cache) : base(cache, "AiFixedPolicy") { }
}

public class AiAdminRateLimiterPolicy : ConfigurableRateLimiterPolicy
{
    public AiAdminRateLimiterPolicy(RateLimitConfigCache cache) : base(cache, "AiAdminPolicy") { }
}

public abstract class ConfigurableRateLimiterPolicy : IRateLimiterPolicy<string>
{
    private readonly RateLimitConfigCache _cache;
    private readonly string _policyName;

    protected ConfigurableRateLimiterPolicy(RateLimitConfigCache cache, string policyName)
    {
        _cache = cache;
        _policyName = policyName;
    }

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var config = _cache.Get(_policyName);

        var partitionKey = config.PartitionType == "user"
            ? $"{_policyName}:{GetUserKey(httpContext)}"
            : $"{_policyName}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = config.PermitLimit,
                Window = TimeSpan.FromSeconds(config.WindowSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = config.QueueLimit
            });
    }

    private static string GetUserKey(HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? httpContext.User.Identity?.Name;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            return userId;
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    public Func<OnRejectedContext, CancellationToken, ValueTask> OnRejected => null;
}