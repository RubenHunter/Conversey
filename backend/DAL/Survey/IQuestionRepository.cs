using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Survey;

namespace Conversey.DAL.Survey;

public interface IQuestionRepository
{

    Question ReadQuestionByIdWithProject(int questionId);

    IReadOnlyCollection<Question> ReadQuestionsByProjectIdWithChoices(Slug projectSlug);

    void CreateQuestion(Question question);
    void DeleteAllQuestionsForProject(Slug projectId);
    
    void CreateAnswer(Answer answer);
    
    Choice ReadChoiceByIdForQuestion(int questionId, int choiceId);
}
