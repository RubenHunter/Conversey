using Conversey.BL.Domain.Subplatform.Survey.Questions;
using Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;

namespace Conversey.BL.Subplatform.Survey.Questions;

public interface IQuestionManager
{
    Question GetQuestionById(int questionId);
    IReadOnlyCollection<Question> GetAllQuestions();
    IReadOnlyCollection<Question> GetQuestionsByProjectId(int projectId);
    Question AddQuestion(Question question);
    Question EditQuestion(Question question);
    void RemoveQuestion(int questionId);
    TextAnswer GetTextAnswerById(int answerId);
    IReadOnlyCollection<TextAnswer> GetTextAnswersByQuestionId(int questionId);
    TextAnswer AddTextAnswer(TextAnswer answer);
    TextAnswer EditTextAnswer(TextAnswer answer);
    void RemoveTextAnswer(int answerId);
    IntegerAnswer GetIntegerAnswerById(int answerId);
    IReadOnlyCollection<IntegerAnswer> GetIntegerAnswersByQuestionId(int questionId);
    IntegerAnswer AddIntegerAnswer(IntegerAnswer answer);
    IntegerAnswer EditIntegerAnswer(IntegerAnswer answer);
    void RemoveIntegerAnswer(int answerId);
}

