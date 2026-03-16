using IdeaResponse = Conversey.BL.Domain.Subplatform.Survey.Ideation.Response;

namespace Conversey.BL.Subplatform.Survey.Ideation;

public abstract record ResponseSubmissionResponse
{
    public sealed record Approved(IdeaResponse Response) : ResponseSubmissionResponse;

    public sealed record Pending(IdeaResponse Response, string Suggestion) : ResponseSubmissionResponse;
}

