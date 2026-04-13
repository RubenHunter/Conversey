using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Survey;

namespace Conversey.DAL.Survey;

public interface IQuestionRepository
{
    Question ReadQuestionById(int questionId);
    Question ReadQuestionByIdWithProject(int questionId);

    IReadOnlyCollection<Question> ReadAllQuestions();
    IReadOnlyCollection<Question> ReadAllQuestionsWithProject();

    IReadOnlyCollection<Question> ReadQuestionsByProjectId(Slug projectSlug);
    IReadOnlyCollection<Question> ReadQuestionsByProjectIdWithProject(Slug projectSlug);

    void CreateQuestion(Question question);
    void UpdateQuestion(Question question);
    bool DeleteQuestion(int questionId);

    Answer ReadAnswerById(int answerId);
    IReadOnlyCollection<Answer> ReadAnswersByQuestionId(int questionId);

    void CreateAnswer(Answer answer);
    void UpdateAnswer(Answer answer);
    bool DeleteAnswer(int answerId);
}
