using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Survey;

namespace Conversey.DAL.Survey;

public interface IQuestionRepository
{
    Project ReadProjectBySlugWithWorkspaceAndQuestions(Slug projectSlug);

    Question ReadQuestionById(int questionId);
    Question ReadQuestionByIdWithProject(int questionId);

    IReadOnlyCollection<Question> ReadAllQuestions();
    IReadOnlyCollection<Question> ReadQuestionsByProjectIdWithChoices(Slug projectSlug);

    void CreateQuestion(Question question);

    Answer ReadAnswerById(int answerId);

    void CreateAnswer(Answer answer);
    void UpdateAnswer(Answer answer);
    bool DeleteAnswer(int answerId);

    Youth ReadYouthByTokenWithProject(Guid youthToken);
    Youth CreateYouth(Guid youthToken, Slug projectSlug);

    SingleChoice ReadSingleChoiceByIdForQuestion(int questionId, int optionId);
    MultipleChoice ReadMultipleChoiceByIdForQuestion(int questionId, int optionId);
}
