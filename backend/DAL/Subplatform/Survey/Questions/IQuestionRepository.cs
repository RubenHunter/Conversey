using Conversey.BL.Domain.Subplatform.Survey.Questions;
using Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;

namespace Conversey.DAL.Subplatform.Survey.Questions;

public interface IQuestionRepository
{
    Question ReadQuestionById(int questionId);
    IReadOnlyCollection<Question> ReadAllQuestions();
    IReadOnlyCollection<Question> ReadQuestionsByProjectId(int projectId);
    void CreateQuestion(Question question);
    void UpdateQuestion(Question question);
    void DeleteQuestion(int questionId);
    TextAnswer ReadTextAnswerById(int answerId);
    IReadOnlyCollection<TextAnswer> ReadTextAnswersByQuestionId(int questionId);
    void CreateTextAnswer(TextAnswer answer);
    void UpdateTextAnswer(TextAnswer answer);
    void DeleteTextAnswer(int answerId);
    IntegerAnswer ReadIntegerAnswerById(int answerId);
    IReadOnlyCollection<IntegerAnswer> ReadIntegerAnswersByQuestionId(int questionId);
    void CreateIntegerAnswer(IntegerAnswer answer);
    void UpdateIntegerAnswer(IntegerAnswer answer);
    void DeleteIntegerAnswer(int answerId);
}
