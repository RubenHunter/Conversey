using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using Conversey.BL.Domain.Ai;
using Conversey.DAL.Subplatform.Ai;

namespace Conversey.BL.Ai;

public class AiPricingService : IAiPricingService
{
    private readonly IModelPricingRepository _pricingRepository;
    private readonly HttpClient _httpClient;
    private decimal _eurRate = 0.92m;
    private DateTime _rateUpdatedAt = DateTime.MinValue;

    private static readonly Dictionary<string, (decimal input, decimal output)> DefaultPrices = new(StringComparer.OrdinalIgnoreCase)
    {
        ["mistral-small-latest"] = (0.10m, 0.30m),
        ["mistral-medium-latest"] = (2.70m, 8.10m),
        ["mistral-large-latest"] = (4.00m, 12.00m),
        ["mistral-moderation-latest"] = (0.10m, 0m),
        ["gpt-4o"] = (2.50m, 10.00m),
        ["gpt-4o-mini"] = (0.15m, 0.60m),
        ["gpt-3.5-turbo"] = (0.50m, 1.50m),
        ["gemini-2.5-flash"] = (0.15m, 0.60m),
        ["gemini-2.5-pro"] = (1.25m, 10.00m),
        ["gemini-2.0-flash"] = (0.10m, 0.40m),
        ["gemini-1.5-flash"] = (0.075m, 0.30m),
        ["gemini-1.5-pro"] = (1.25m, 5.00m),
        ["claude-3-haiku"] = (0.25m, 1.25m),
        ["claude-3.5-sonnet"] = (3.00m, 15.00m),
        ["claude-3-opus"] = (15.00m, 75.00m),
        ["llama-3-70b"] = (0.59m, 0.79m),
        ["llama-3-8b"] = (0.05m, 0.08m),
        ["llama-4-maverick"] = (0.20m, 0.90m),
        ["llama-4-scout"] = (0.10m, 0.40m),
    };

    private static readonly Dictionary<string, (decimal input, decimal output)> FuzzyPrices = new(StringComparer.OrdinalIgnoreCase)
    {
        ["llama"] = (0.20m, 0.90m),
        ["mistral"] = (0.10m, 0.30m),
        ["gemini"] = (0.15m, 0.60m),
        ["gpt"] = (0.50m, 1.50m),
        ["claude"] = (0.25m, 1.25m),
        ["command-r"] = (0.50m, 1.50m),
    };

    public AiPricingService(IModelPricingRepository pricingRepository)
    {
        _pricingRepository = pricingRepository;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Conversey/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<decimal> GetEurExchangeRateAsync()
    {
        if (_rateUpdatedAt > DateTime.UtcNow.AddHours(-6))
        {
            return _eurRate;
        }

        try
        {
            var response = await _httpClient.GetAsync("https://api.exchangerate.host/latest?base=USD&symbols=EUR");
            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                if (doc.RootElement.TryGetProperty("rates", out var rates) &&
                    rates.TryGetProperty("EUR", out var eur))
                {
                    _eurRate = eur.GetDecimal();
                    _rateUpdatedAt = DateTime.UtcNow;
                }
            }
        }
        catch
        {
            // Keep previous rate
        }

        return _eurRate;
    }

    public async Task<decimal> CalculateCostAsync(string modelName, int inputTokens, int outputTokens)
    {
        var pricing = await GetPriceAsync(modelName);
        if (pricing == null)
        {
            return 0m;
        }

        var eurRate = await GetEurExchangeRateAsync();
        var inputCost = (inputTokens / 1_000_000m) * pricing.InputPricePerMillionTokens * eurRate;
        var outputCost = (outputTokens / 1_000_000m) * pricing.OutputPricePerMillionTokens * eurRate;

        return Math.Round(inputCost + outputCost, 6);
    }

    public async Task RefreshPricingAsync()
    {
        var fetched = await TryFetchOpenRouterPricingAsync();
        if (fetched.Count > 0)
        {
            await _pricingRepository.SavePricingBatchAsync(fetched);
        }

        foreach (var (modelName, (input, output)) in DefaultPrices)
        {
            var existing = await _pricingRepository.GetPricingAsync(modelName);
            if (existing != null && fetched.Any(f => f.ModelName.Equals(modelName, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }
            var pricing = new AiModelPricing
            {
                ModelName = modelName,
                ProviderName = modelName.Split('-')[0],
                InputPricePerMillionTokens = input,
                OutputPricePerMillionTokens = output
            };
            await _pricingRepository.SavePricingAsync(pricing);
        }
    }

    private async Task<AiModelPricing?> GetPriceAsync(string modelName)
    {
        var cached = await _pricingRepository.GetPricingAsync(modelName);
        if (cached != null)
        {
            return cached;
        }

        var normalized = modelName.Trim().ToLowerInvariant();
        if (DefaultPrices.TryGetValue(normalized, out var defaults))
        {
            return new AiModelPricing
            {
                ModelName = normalized,
                InputPricePerMillionTokens = defaults.input,
                OutputPricePerMillionTokens = defaults.output
            };
        }

        foreach (var (key, (input, output)) in FuzzyPrices)
        {
            if (normalized.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                return new AiModelPricing
                {
                    ModelName = normalized,
                    InputPricePerMillionTokens = input,
                    OutputPricePerMillionTokens = output
                };
            }
        }

        return null;
    }

    private async Task<IReadOnlyList<AiModelPricing>> TryFetchOpenRouterPricingAsync()
    {
        var result = new List<AiModelPricing>();

        try
        {
            var response = await _httpClient.GetAsync("https://openrouter.ai/api/v1/models");
            if (!response.IsSuccessStatusCode)
            {
                return result.AsReadOnly();
            }

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
            {
                return result.AsReadOnly();
            }

            foreach (var model in data.EnumerateArray())
            {
                var id = model.TryGetProperty("id", out var idEl) ? idEl.GetString() ?? "" : "";
                var name = model.TryGetProperty("name", out var nameEl) ? nameEl.GetString() ?? "" : "";

                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                var inputPrice = 0m;
                var outputPrice = 0m;

                if (model.TryGetProperty("pricing", out var pricing))
                {
                    if (pricing.TryGetProperty("prompt", out var prompt))
                    {
                        inputPrice = decimal.TryParse(prompt.GetString() ?? "0", out var p) ? p * 1_000_000m : 0m;
                    }
                    if (pricing.TryGetProperty("completion", out var completion))
                    {
                        outputPrice = decimal.TryParse(completion.GetString() ?? "0", out var p) ? p * 1_000_000m : 0m;
                    }
                }

                result.Add(new AiModelPricing
                {
                    ModelName = id,
                    ProviderName = id.Split('/').FirstOrDefault() ?? name,
                    InputPricePerMillionTokens = inputPrice,
                    OutputPricePerMillionTokens = outputPrice
                });
            }
        }
        catch
        {
            // Fall back to DB or defaults
        }

        return result.AsReadOnly();
    }

    public async Task<IReadOnlyList<AiModelPricing>> GetAllPricingAsync()
    {
        return await _pricingRepository.GetAllPricingAsync();
    }
}
