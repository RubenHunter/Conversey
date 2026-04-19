using Conversey.BL.Domain.Common;

namespace Conversey.BL.Survey;

public class AnswerNotFoundException(string answerIdentifier)
    : NotFoundException($"Answer with id {answerIdentifier}");

public class IntegerAnswerNotFoundException(string answerIdentifier)
    : NotFoundException($"IntegerAnswer with id {answerIdentifier}");

public class QuestionNotFoundException(string questionIdentifier)
    : NotFoundException($"Question with id {questionIdentifier}");
    
public class TextAnswerNotFoundException(string answerIdentifier)
    : NotFoundException($"TextAnswer with id {answerIdentifier}");
