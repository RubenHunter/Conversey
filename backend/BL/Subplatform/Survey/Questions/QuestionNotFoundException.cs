namespace Conversey.BL.Subplatform.Survey.Questions;

public class QuestionNotFoundException(string questionIdentifier)
    : Exception($"Question with id {questionIdentifier} was not found.");

