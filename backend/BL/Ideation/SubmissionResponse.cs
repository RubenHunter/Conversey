using Conversey.BL.Ai;
using Conversey.BL.Domain.Ideation;

namespace Conversey.BL.Ideation;

public abstract record SubmissionResponse
{
    public sealed record Approved(Idea Idea) : SubmissionResponse;

    public sealed record Pending(Idea Idea, ModerationDecision Decision) : SubmissionResponse;
}