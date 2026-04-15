namespace Conversey.BL.Survey;

public class AnswerNotFoundException(string answerIdentifier)
    : Exception($"Answer with id {answerIdentifier} was not found.");

public class IntegerAnswerNotFoundException(string answerIdentifier)
    : Exception($"IntegerAnswer with id {answerIdentifier} was not found.");

public class QuestionNotFoundException(string questionIdentifier)
    : Exception($"Question with id {questionIdentifier} was not found.");
    
public class TextAnswerNotFoundException(string answerIdentifier)
    : Exception($"TextAnswer with id {answerIdentifier} was not found.");