using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Survey;

namespace Conversey.BL.Subplatform.Survey.Questions;

public interface IQuestionManager
{
    Question GetQuestionById(int questionId);
    Question GetQuestionByIdWithProject(int questionId);

    IReadOnlyCollection<Question> GetAllQuestions();
    IReadOnlyCollection<Question> GetAllQuestionsWithProject();

    IReadOnlyCollection<Question> GetQuestionsByProjectId(Slug projectSlug);
    IReadOnlyCollection<Question> GetQuestionsByProjectIdWithProject(Slug projectSlug);

    Question AddQuestion(Question question);
    Question ChangeQuestion(Question question);
    void RemoveQuestion(int questionId);

    Answer GetAnswerById(int answerId);
    IReadOnlyCollection<Answer> GetAnswersByQuestionId(int questionId);

    Answer AddAnswer(Answer answer);
    Answer ChangeAnswer(Answer answer);
    void RemoveAnswer(int answerId);
}
