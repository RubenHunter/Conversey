namespace Conversey.BL.Subplatform.Survey.Questions;

public class IntegerAnswerNotFoundException(string answerIdentifier)
    : Exception($"IntegerAnswer with id {answerIdentifier} was not found.");

