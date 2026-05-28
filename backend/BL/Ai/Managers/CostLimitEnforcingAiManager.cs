using Conversey.BL.Ai.Dto;
using Conversey.BL.Domain.Ideation;
using Conversey.DAL.Subplatform.Ai;

using Conversey.BL.Domain.Common;

namespace Conversey.BL.Ai;

public sealed class CostLimitEnforcingAiManager : IAiManager
{
    private readonly IAiManager _inner;
    private readonly ICostLimitRepository _costLimitRepo;
    private readonly IAuditRepository _auditRepo;
    private readonly NoopAiManager _noop;

    public CostLimitEnforcingAiManager(
        IAiManager inner,
        ICostLimitRepository costLimitRepo,
        IAuditRepository auditRepo,
        IModerationKeywordRepository keywordRepo)
    {
        _inner = inner;
        _costLimitRepo = costLimitRepo;
        _auditRepo = auditRepo;
        _noop = new NoopAiManager(keywordRepo);
    }

    private async Task<bool> IsWorkspaceOverLimitAsync(string workspaceId)
    {
        var limit = await _costLimitRepo.GetWorkspaceLimitAsync(workspaceId);
        if (limit == null || !limit.IsActive) return false;
        var totalCost = await _auditRepo.GetTotalCostForWorkspaceAsync(workspaceId, limit.PeriodStart, limit.PeriodEnd);
        return totalCost >= limit.LimitAmount;
    }

    private async Task<bool> IsProjectOverLimitAsync(string projectId)
    {
        var limit = await _costLimitRepo.GetProjectLimitAsync(projectId);
        if (limit == null || !limit.IsActive) return false;
        var totalCost = await _auditRepo.GetTotalCostForProjectAsync(projectId, limit.PeriodStart, limit.PeriodEnd);
        return totalCost >= limit.LimitAmount;
    }

    private async Task<bool> IsOverLimit(string? workspaceId, string? projectId)
    {
        if (!string.IsNullOrWhiteSpace(projectId))
        {
            if (await IsProjectOverLimitAsync(projectId))
                return true;
        }

        if (!string.IsNullOrWhiteSpace(workspaceId))
        {
            if (await IsWorkspaceOverLimitAsync(workspaceId))
                return true;
        }

        return false;
    }

    public async Task<string> GenerateAlternativeAsync(string content, ModerationDecision decision = null, string? workspaceId = null, string? projectId = null)
    {
        if (await IsOverLimit(workspaceId, projectId))
            return await _noop.GenerateAlternativeAsync(content, decision, workspaceId, projectId);
        return await _inner.GenerateAlternativeAsync(content, decision, workspaceId, projectId);
    }

    public async Task<ModerationDecision> ModerateContentAsync(string content, string? workspaceId = null, string? projectId = null)
    {
        if (await IsOverLimit(workspaceId, projectId))
            return await _noop.ModerateContentAsync(content, workspaceId, projectId);
        return await _inner.ModerateContentAsync(content, workspaceId, projectId);
    }

    public async Task<IdeaNudgeDecision> AssessIdeaNudgeAsync(IdeaNudgeAssessmentRequest request, string? workspaceId = null, string? projectId = null)
    {
        if (await IsOverLimit(workspaceId, projectId))
            return await _noop.AssessIdeaNudgeAsync(request, workspaceId, projectId);
        return await _inner.AssessIdeaNudgeAsync(request, workspaceId, projectId);
    }

    public async Task<IEnumerable<int>> RankIdeasByRelationAsync(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit, string? workspaceId = null, string? projectId = null)
    {
        if (await IsOverLimit(workspaceId, projectId))
            return await _noop.RankIdeasByRelationAsync(referenceIdea, candidateIdeas, preferDifferent, limit, workspaceId, projectId);
        return await _inner.RankIdeasByRelationAsync(referenceIdea, candidateIdeas, preferDifferent, limit, workspaceId, projectId);
    }

    public async Task<IReadOnlyDictionary<int, IReadOnlyList<string>>> CategorizeIdeasAsync(IReadOnlyList<string> ideas, IReadOnlyList<string> existingCategories, int maxCategoriesPerIdea, string? workspaceId = null, string? projectId = null)
    {
        if (await IsOverLimit(workspaceId, projectId))
            return await _noop.CategorizeIdeasAsync(ideas, existingCategories, maxCategoriesPerIdea, workspaceId, projectId);
        return await _inner.CategorizeIdeasAsync(ideas, existingCategories, maxCategoriesPerIdea, workspaceId, projectId);
    }

    public Task<ExtractKeyPhrasesResponse> ExtractKeyPhrases(
        string transcript, Language language, int maxPhrases,
        IReadOnlyList<string> existingPhrases = null, IReadOnlyList<string> rejectedPhrases = null)
    {
        return _inner.ExtractKeyPhrases(transcript, language, maxPhrases, existingPhrases, rejectedPhrases);
    }

    public Task<string> GenerateTextFromBubbles(
        string transcript, IReadOnlyList<string> bubbles, Language language,
        IReadOnlyList<string> rejectedPhrases = null)
    {
        return _inner.GenerateTextFromBubbles(transcript, bubbles, language, rejectedPhrases);
    }

    public async Task<string> CompletePlainTextAsync(
        string systemPrompt, string userPrompt,
        string? workspaceId = null, string? projectId = null, string? displayPromptName = null)
    {
        if (await IsOverLimit(workspaceId, projectId))
            return await _noop.CompletePlainTextAsync(systemPrompt, userPrompt, workspaceId, projectId, displayPromptName);
        return await _inner.CompletePlainTextAsync(systemPrompt, userPrompt, workspaceId, projectId, displayPromptName);
    }
}
