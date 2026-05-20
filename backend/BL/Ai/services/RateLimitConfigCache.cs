using System.Collections.Concurrent;
using Conversey.DAL.Subplatform.Ai;
using Microsoft.Extensions.DependencyInjection;

namespace Conversey.BL.Ai;

public sealed class RateLimitConfigEntry
{
    public int PermitLimit { get; init; }
    public int WindowSeconds { get; init; }
    public int QueueLimit { get; init; }
    public int Version { get; init; }
    public string PartitionType { get; init; } = "global";
}

public sealed class RateLimitConfigCache
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentDictionary<string, RateLimitConfigEntry> _entries = new();
    private int _generation;

    private static readonly RateLimitConfigEntry DefaultEntry = new()
    {
        PermitLimit = 30,
        WindowSeconds = 60,
        QueueLimit = 0,
        Version = 0,
        PartitionType = "global"
    };

    public RateLimitConfigCache(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public RateLimitConfigEntry Get(string policyName)
    {
        return _entries.TryGetValue(policyName, out var entry) ? entry : DefaultEntry;
    }

    public async Task InitializeAsync()
    {
        await ReloadAsync();
    }

    public async Task ReloadAsync()
    {
        var generation = Interlocked.Increment(ref _generation);

        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRateLimitConfigRepository>();
        var configs = await repo.GetAllConfigsAsync();

        foreach (var config in configs)
        {
            if (string.IsNullOrWhiteSpace(config.PolicyName))
            {
                continue;
            }

            _entries[config.PolicyName] = new RateLimitConfigEntry
            {
                PermitLimit = config.PermitLimit,
                WindowSeconds = config.WindowSeconds,
                QueueLimit = config.QueueLimit,
                Version = generation,
                PartitionType = config.PartitionType
            };
        }
    }
}
