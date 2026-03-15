using Conversey.BL.Domain.Subplatform.Survey.Ideation;

namespace Conversey.BL.Subplatform.Survey.Ideation;

public abstract record SubmissionResponse
{
    public sealed record Approved(Idea idea) : SubmissionResponse;

    public sealed record Pending(Idea idea, string suggestion) : SubmissionResponse;
}