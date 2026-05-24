using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Survey;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conversey.DAL.Survey;

public class QuestionRepository : IQuestionRepository
{
    private readonly ConverseyDbContext _dbContext;

    public QuestionRepository(ConverseyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Project ReadProjectBySlugWithWorkspaceAndQuestions(Slug projectSlug)
    {
        return _dbContext.Projects
            .Include(project => project.Workspace)
            .Include(project => project.Questions)
            .SingleOrDefault(project => project.Id == projectSlug);
    }

    public Question ReadQuestionByIdWithProject(int questionId)
    {
        return _dbContext.Questions
            .Include(q => q.Project)
            .SingleOrDefault(q => q.Id == questionId);
    }

    public IReadOnlyCollection<Question> ReadAllQuestions()
    {
        return _dbContext.Questions.ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Question> ReadQuestionsByProjectIdWithChoices(Slug projectSlug)
    {
        var questions = _dbContext.Questions
            .Where(question => question.Project.Id == projectSlug)
            .ToList();

        if (questions.Count == 0)
        {
            return questions.AsReadOnly();
        }

        var choiceQuestionIds = questions
            .Where(question => question is ChoiceQuestion)
            .Select(question => question.Id)
            .ToHashSet();

        if (choiceQuestionIds.Count == 0)
        {
            return questions.AsReadOnly();
        }

        var choices = _dbContext.Set<Choice>()
            .Where(choice => choiceQuestionIds.Contains(choice.Question.Id))
            .Select(choice => new
            {
                QuestionId = choice.Question.Id,
                Choice = choice
            })
            .ToList()
            .GroupBy(entry => entry.QuestionId)
            .ToDictionary(
                group => group.Key,
                group => (IList<Choice>)group.Select(entry => entry.Choice).ToList());

        foreach (var question in questions)
        {
            switch (question)
            {
                case SingleChoiceQuestion singleChoiceQuestion:
                {
                    singleChoiceQuestion.PossibleChoices = choices.TryGetValue(question.Id, out var values)
                        ? values
                        : new List<Choice>();
                    break;
                }
                case MultipleChoiceQuestion multipleChoiceQuestion:
                {
                    multipleChoiceQuestion.PossibleChoices = choices.TryGetValue(question.Id, out var values)
                        ? values
                        : new List<Choice>();
                    break;
                }
            }
        }

        return questions.AsReadOnly();
    }

    public void CreateQuestion(Question question)
    {
        _dbContext.Questions.Add(question);
        _dbContext.SaveChanges();
    }

    public Answer ReadAnswerById(int answerId)
    {
        return _dbContext.Answers
            .SingleOrDefault(a => a.Id == answerId);
    }

    public void CreateAnswer(Answer answer)
    {
        _dbContext.Answers.Add(answer);
        _dbContext.SaveChanges();
    }

    public void UpdateAnswer(Answer answer)
    {
        _dbContext.Answers.Update(answer);
        _dbContext.SaveChanges();
    }

    public bool DeleteAnswer(int answerId)
    {
        var answer = _dbContext.Answers
            .SingleOrDefault(a => a.Id == answerId);
        if (answer == null) return false;

        _dbContext.Answers.Remove(answer);
        _dbContext.SaveChanges();
        return true;
    }

    public Youth ReadYouthByTokenWithProject(Guid youthToken)
    {
        return _dbContext.Youths
            .Include(youth => youth.Project)
            .SingleOrDefault(youth => youth.Id == youthToken);
    }

    public Youth CreateYouth(Guid youthToken, Slug projectSlug)
    {
        var project = _dbContext.Projects
            .Single(project => project.Id == projectSlug);

        var youth = new Youth
        {
            Id = youthToken,
            Email = $"{youthToken:N}@local.invalid",
            Project = project
        };

        _dbContext.Youths.Add(youth);
        _dbContext.SaveChanges();

        return _dbContext.Youths
            .Include(createdYouth => createdYouth.Project)
            .Single(createdYouth => createdYouth.Id == youthToken);
    }

    public Choice ReadChoiceByIdForQuestion(int questionId, int choiceId)
    {
        return _dbContext.Set<Choice>()
            .SingleOrDefault(choice =>
                choice.Id == choiceId &&
                choice.Question.Id == questionId);
    }
}

public class QuestionConfig : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        #region Properties

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Text)
            .HasMaxLength(500)
            .IsRequired();

        #endregion
    }
}

public class OpenQuestionConfig : IEntityTypeConfiguration<OpenQuestion>
{
    public void Configure(EntityTypeBuilder<OpenQuestion> builder)
    {
        #region Relations

        builder.HasMany(q => q.AnsweredAnswers)
            .WithOne(a => a.Question as OpenQuestion)
            .IsRequired();

        #endregion

    }
}

public class ScaleQuestionConfig : IEntityTypeConfiguration<ScaleQuestion>
{
    public void Configure(EntityTypeBuilder<ScaleQuestion> builder)
    {
        #region Properties
        
        builder.Property(q => q.LowerBound)
            .IsRequired();
        
        builder.Property(q => q.UpperBound)
            .IsRequired();

        #endregion

        #region Relations

        // ScaleQuestion has Answer<int> submissions
        builder.HasMany(q => q.AnsweredAnswers)
            .WithOne(a => a.Question as ScaleQuestion)
            .IsRequired();

        #endregion
    }
}

// Concrete configurations for each choice question type
public class SingleChoiceQuestionConfig : IEntityTypeConfiguration<SingleChoiceQuestion>
{
    public virtual void Configure(EntityTypeBuilder<SingleChoiceQuestion> builder)
    {
        #region Relations

        // SingleChoiceQuestion -> Answer<TChoice> submissions
        builder.HasMany(q => q.AnsweredAnswers)
            .WithOne(a => a.Question as SingleChoiceQuestion)
            .IsRequired();

        #endregion
    }
}

public class MultipleChoiceQuestionConfig : IEntityTypeConfiguration<MultipleChoiceQuestion>
{
    public virtual void Configure(EntityTypeBuilder<MultipleChoiceQuestion> builder)
    {
        #region Relations

        // MultipleChoiceQuestion -> Answer<TChoice> submissions
        builder.HasMany(q => q.AnsweredAnswers)
            .WithOne(a => a.Question as MultipleChoiceQuestion)
            .IsRequired();

        #endregion
    }
}

// Choice configurations
public class ChoiceConfig : IEntityTypeConfiguration<Choice>
{
    public void Configure(EntityTypeBuilder<Choice> builder)
    {
        //builder.HasKey(c => new { c.Question.Id, c.Text }); // Composite key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Text)
            .HasMaxLength(250)
            .IsRequired();

        builder.HasOne(c => c.Question)
            .WithMany(cq => cq.PossibleChoices);
    }
}

// Answer configurations
public class AnswerStringConfig : IEntityTypeConfiguration<Answer<string>>
{
    public void Configure(EntityTypeBuilder<Answer<string>> builder)
    {
        builder.Property(a => a.Value)
            .HasMaxLength(4000);
    }
}

public class AnswerIntConfig : IEntityTypeConfiguration<Answer<int>>
{
    public void Configure(EntityTypeBuilder<Answer<int>> builder)
    {
        builder.Property(a => a.Value)
            .IsRequired();
    }
}

public class AnswerChoiceConfig : IEntityTypeConfiguration<Answer<Choice>>
{
    public void Configure(EntityTypeBuilder<Answer<Choice>> builder)
    {
        // Answer<Choice> -> Choice reference
        builder.HasOne(a => a.Value)
            .WithMany()
            .IsRequired();
    }
}

