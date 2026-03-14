using Conversey.BL.Domain.Subplatform.Survey.Questions;
using Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;

namespace Conversey.BL.Subplatform.Survey.Questions;

public interface IQuestionManager
{
    Question GetQuestionById(int questionId);
    Question GetQuestionByIdWithProject(int questionId);

    IReadOnlyCollection<Question> GetAllQuestions();
    IReadOnlyCollection<Question> GetAllQuestionsWithProject();

    IReadOnlyCollection<Question> GetQuestionsByProjectId(int projectId);
    IReadOnlyCollection<Question> GetQuestionsByProjectIdWithProject(int projectId);

    Question AddQuestion(Question question);
    Question ChangeQuestion(Question question);
    void RemoveQuestion(int questionId);

    TextAnswer GetTextAnswerById(int answerId);
    TextAnswer GetTextAnswerByIdWithYouth(int answerId);
    TextAnswer GetTextAnswerByIdWithQuestion(int answerId);
    TextAnswer GetTextAnswerByIdWithYouthAndQuestion(int answerId);

    IReadOnlyCollection<TextAnswer> GetTextAnswersByQuestionId(int questionId);
    IReadOnlyCollection<TextAnswer> GetTextAnswersByQuestionIdWithYouth(int questionId);
    IReadOnlyCollection<TextAnswer> GetTextAnswersByQuestionIdWithQuestion(int questionId);
    IReadOnlyCollection<TextAnswer> GetTextAnswersByQuestionIdWithYouthAndQuestion(int questionId);

    TextAnswer AddTextAnswer(TextAnswer answer);
    TextAnswer ChangeTextAnswer(TextAnswer answer);
    void RemoveTextAnswer(int answerId);

    IntegerAnswer GetIntegerAnswerById(int answerId);
    IntegerAnswer GetIntegerAnswerByIdWithYouth(int answerId);
    IntegerAnswer GetIntegerAnswerByIdWithQuestion(int answerId);
    IntegerAnswer GetIntegerAnswerByIdWithYouthAndQuestion(int answerId);

    IReadOnlyCollection<IntegerAnswer> GetIntegerAnswersByQuestionId(int questionId);
    IReadOnlyCollection<IntegerAnswer> GetIntegerAnswersByQuestionIdWithYouth(int questionId);
    IReadOnlyCollection<IntegerAnswer> GetIntegerAnswersByQuestionIdWithQuestion(int questionId);
    IReadOnlyCollection<IntegerAnswer> GetIntegerAnswersByQuestionIdWithYouthAndQuestion(int questionId);

    IntegerAnswer AddIntegerAnswer(IntegerAnswer answer);
    IntegerAnswer ChangeIntegerAnswer(IntegerAnswer answer);
    void RemoveIntegerAnswer(int answerId);
}
