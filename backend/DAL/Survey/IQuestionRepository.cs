using Conversey.BL.Domain.Survey;

namespace Conversey.DAL.Survey;

public interface IQuestionRepository
{
    Question ReadQuestionById(int questionId);
    Question ReadQuestionByIdWithProject(int questionId);

    IReadOnlyCollection<Question> ReadAllQuestions();
    IReadOnlyCollection<Question> ReadAllQuestionsWithProject();

    IReadOnlyCollection<Question> ReadQuestionsByProjectId(int projectId);
    IReadOnlyCollection<Question> ReadQuestionsByProjectIdWithProject(int projectId);

    void CreateQuestion(Question question);
    void UpdateQuestion(Question question);
    bool DeleteQuestion(int questionId);

    TextAnswer ReadTextAnswerById(int answerId);
    TextAnswer ReadTextAnswerByIdWithYouth(int answerId);
    TextAnswer ReadTextAnswerByIdWithQuestion(int answerId);
    TextAnswer ReadTextAnswerByIdWithYouthAndQuestion(int answerId);

    IReadOnlyCollection<TextAnswer> ReadTextAnswersByQuestionId(int questionId);
    IReadOnlyCollection<TextAnswer> ReadTextAnswersByQuestionIdWithYouth(int questionId);
    IReadOnlyCollection<TextAnswer> ReadTextAnswersByQuestionIdWithQuestion(int questionId);
    IReadOnlyCollection<TextAnswer> ReadTextAnswersByQuestionIdWithYouthAndQuestion(int questionId);

    void CreateTextAnswer(TextAnswer answer);
    void UpdateTextAnswer(TextAnswer answer);
    bool DeleteTextAnswer(int answerId);

    IntegerAnswer ReadIntegerAnswerById(int answerId);
    IntegerAnswer ReadIntegerAnswerByIdWithYouth(int answerId);
    IntegerAnswer ReadIntegerAnswerByIdWithQuestion(int answerId);
    IntegerAnswer ReadIntegerAnswerByIdWithYouthAndQuestion(int answerId);

    IReadOnlyCollection<IntegerAnswer> ReadIntegerAnswersByQuestionId(int questionId);
    IReadOnlyCollection<IntegerAnswer> ReadIntegerAnswersByQuestionIdWithYouth(int questionId);
    IReadOnlyCollection<IntegerAnswer> ReadIntegerAnswersByQuestionIdWithQuestion(int questionId);
    IReadOnlyCollection<IntegerAnswer> ReadIntegerAnswersByQuestionIdWithYouthAndQuestion(int questionId);

    void CreateIntegerAnswer(IntegerAnswer answer);
    void UpdateIntegerAnswer(IntegerAnswer answer);
    bool DeleteIntegerAnswer(int answerId);
}
