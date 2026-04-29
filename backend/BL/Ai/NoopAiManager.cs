using Microsoft.Extensions.AI;

namespace Conversey.BL.Ai;

public sealed class NoopAiManager : IAiManager
{
    private static readonly string[] UnsafeTerms =
    {
        "retarded",
        "moron",
        "dumbass",
        "dumb ass",
        "fucking"
    };

    public void Dispose()
    {
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null,
        CancellationToken cancellationToken = default)
    {
        var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, "AI integration is currently disabled."));
        return Task.FromResult(response);
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages,
        ChatOptions options = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        yield break;
    }

    public object GetService(Type serviceType, object serviceKey = null)
    {
        return null;
    }

    public Task<string> GenerateAiAlternative(string prompt, ModerationDecision decision = null)
    {
        return Task.FromResult("Please rephrase your message in a respectful way.");
    }

    public Task<ModerationDecision> ModerateContent(string content)
    {
        var normalized = (content ?? string.Empty).Trim().ToLowerInvariant();
        var containsUnsafeLanguage = UnsafeTerms.Any(term => normalized.Contains(term));

        return Task.FromResult(new ModerationDecision
        {
            IsAllowed = !containsUnsafeLanguage,
            Suggestion = containsUnsafeLanguage ? "Please rephrase your idea in a respectful and constructive way." : null
        });
    }

    public Task<IdeaNudgeDecision> AssessIdeaNudge(IdeaNudgeAssessmentRequest request)
    {
        var ideaText = (request.IdeaText ?? string.Empty).Trim();
        var mode = (request.NudgingMode ?? string.Empty).Trim().ToLowerInvariant();
        var wordCount = ideaText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
        var isPlaceholder = string.IsNullOrWhiteSpace(ideaText)
            || ideaText.Equals("idk", StringComparison.OrdinalIgnoreCase)
            || ideaText.Equals("test123", StringComparison.OrdinalIgnoreCase);
        var hasConcreteSubject = wordCount >= 4;
        var hasWhatAndWhy = wordCount >= 8;
        var hasContextOrImpact = wordCount >= 10;
        var hasDeepElaboration = wordCount >= 14;

        var minimalApproval = mode is "minimal" or "lenient" or "verylenient" or "acceptable";
        if (!isPlaceholder && minimalApproval)
        {
            return Task.FromResult(new IdeaNudgeDecision { IsApproved = true });
        }

        if (!isPlaceholder && mode is "light" or "gentle")
        {
            return Task.FromResult(new IdeaNudgeDecision { IsApproved = hasConcreteSubject });
        }

        if (!isPlaceholder && mode is "medium" or "balanced" or "guided")
        {
            return Task.FromResult(new IdeaNudgeDecision { IsApproved = hasWhatAndWhy });
        }

        if (!isPlaceholder && mode is "strong" or "strict" or "thorough")
        {
            return Task.FromResult(new IdeaNudgeDecision { IsApproved = hasContextOrImpact });
        }

        if (!isPlaceholder && mode is "deep" or "relentless")
        {
            return Task.FromResult(new IdeaNudgeDecision { IsApproved = hasDeepElaboration });
        }

        var topicTitle = string.IsNullOrWhiteSpace(request.TopicTitle) ? "this topic" : request.TopicTitle.Trim();
        var projectTitle = string.IsNullOrWhiteSpace(request.ProjectTitle) ? "this project" : request.ProjectTitle.Trim();
        var question = mode switch
        {
            "deep" or "relentless" => $"What evidence, example, or concrete scenario shows your idea would improve {topicTitle} in {projectTitle}?",
            "strong" or "strict" or "thorough" => $"What concrete impact do you expect from this idea for {topicTitle}, and for whom?",
            "medium" or "balanced" or "guided" => $"Can you explain what exactly should change and why that would help for {topicTitle}?",
            "light" or "gentle" => $"What concrete part of {topicTitle} are you proposing to improve?",
            _ => $"Could you make this idea a bit more specific for {topicTitle}?"
        };

        return Task.FromResult(new IdeaNudgeDecision
        {
            IsApproved = false,
            Question = question,
        });
    }

    public Task<IEnumerable<int>> RankIdeasByRelation(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit)
    {
        if (candidateIdeas.Count == 0 || limit <= 0)
        {
            return Task.FromResult<IEnumerable<int>>(Array.Empty<int>());
        }

        var ordered = Enumerable.Range(0, candidateIdeas.Count);
        if (preferDifferent)
        {
            ordered = ordered.Reverse();
        }

        return Task.FromResult(ordered.Take(limit));
    }

    public Task<IReadOnlyDictionary<int, IReadOnlyList<string>>> CategorizeIdeas(IReadOnlyList<string> ideas, IReadOnlyList<string> existingCategories, int maxCategoriesPerIdea)
    {
        var result = new Dictionary<int, IReadOnlyList<string>>();
        var canonicalExisting = existingCategories
            .Select(category => (category ?? string.Empty).Trim())
            .Where(category => category.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        for (int index = 0; index < ideas.Count; index++)
        {
            var text = (ideas[index] ?? string.Empty).ToLowerInvariant();
            var categories = new List<string>();

            if (text.Contains("stress") || text.Contains("druk") || text.Contains("exam") || text.Contains("deadline"))
                categories.Add("Study pressure");
            if (text.Contains("coach") || text.Contains("support") || text.Contains("psych") || text.Contains("help"))
                categories.Add("Support services");
            if (text.Contains("campus") || text.Contains("group") || text.Contains("peer") || text.Contains("connected"))
                categories.Add("Community & belonging");
            if (text.Contains("online") || text.Contains("digital") || text.Contains("hybrid"))
                categories.Add("Digital learning");

            if (categories.Count == 0)
            {
                if (canonicalExisting.Count > 0)
                {
                    categories.AddRange(canonicalExisting.Take(Math.Max(1, maxCategoriesPerIdea)));
                }
                else
                {
                    categories.Add("General ideas");
                }
            }

            if (canonicalExisting.Count > 0)
            {
                var reused = canonicalExisting
                    .Where(existing => categories.Any(category => NormalizeCategoryKey(category) == NormalizeCategoryKey(existing)))
                    .Take(Math.Max(1, maxCategoriesPerIdea))
                    .ToList();

                if (reused.Count > 0)
                {
                    result[index] = reused.AsReadOnly();
                    continue;
                }
            }

            result[index] = categories
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(Math.Max(1, maxCategoriesPerIdea))
                .ToList()
                .AsReadOnly();
        }

        return Task.FromResult<IReadOnlyDictionary<int, IReadOnlyList<string>>>(result);
    }

    private static string NormalizeCategoryKey(string value)
    {
        return new string((value ?? string.Empty)
            .ToLowerInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());
    }
}

