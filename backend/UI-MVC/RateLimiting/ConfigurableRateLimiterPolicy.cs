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
        return RateLimitPartition.GetFixedWindowLimiter(
            $"{_policyName}-v{config.Version}",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = config.PermitLimit,
                Window = TimeSpan.FromSeconds(config.WindowSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = config.QueueLimit
            });
    }

    public Func<OnRejectedContext, CancellationToken, ValueTask> OnRejected => null;
}
