namespace Conversey.BL.Subplatform.Survey.Questions;

public class TextAnswerNotFoundException(string answerIdentifier)
    : Exception($"TextAnswer with id {answerIdentifier} was not found.");

