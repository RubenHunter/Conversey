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

    Question GetQuestionById(int questionId);
    IEnumerable<Question> GetAllQuestions();

    Question AddQuestion(Question question);

    Answer GetAnswerById(int answerId);

    Answer AddAnswer(Answer answer);
    Answer ChangeAnswer(Answer answer);
    void RemoveAnswer(int answerId);
}
