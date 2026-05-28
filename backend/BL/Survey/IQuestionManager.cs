using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Survey;

namespace Conversey.BL.Survey;

public interface IQuestionManager
{
    IEnumerable<Question> GetQuestions(Slug workspaceSlug, Slug projectSlug);
    void SubmitAnswers(
        Slug workspaceSlug,
        Slug projectSlug,
        Guid youthId,
        IEnumerable<(int QuestionId, int? SelectedOptionId, string OpenTextValue)> answers);


    Question AddQuestion(Question question);
    void RemoveQuestionsForProject(Slug workspaceId, Slug projectId);
}
