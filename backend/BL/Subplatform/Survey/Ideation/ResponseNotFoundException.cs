namespace Conversey.BL.Subplatform.Survey.Ideation;

public class ResponseNotFoundException(string responseIdentifier)
    : Exception($"Response with id {responseIdentifier} was not found.");

