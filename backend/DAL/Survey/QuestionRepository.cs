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

    public Question ReadQuestionById(int questionId)
    {
        return _dbContext.Questions
            .SingleOrDefault(q => q.Id == questionId);
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
            .Where(question => question is ChoiceQuestion<SingleChoice> || question is ChoiceQuestion<MultipleChoice>)
            .Select(question => question.Id)
            .ToHashSet();

        if (choiceQuestionIds.Count == 0)
        {
            return questions.AsReadOnly();
        }

        var singleChoices = _dbContext.Set<SingleChoice>()
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
                group => (IList<SingleChoice>)group.Select(entry => entry.Choice).ToList());

        var multipleChoices = _dbContext.Set<MultipleChoice>()
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
                group => (IList<MultipleChoice>)group.Select(entry => entry.Choice).ToList());

        foreach (var question in questions)
        {
            if (question is ChoiceQuestion<SingleChoice> singleChoiceQuestion)
            {
                singleChoiceQuestion.PossibleChoices = singleChoices.TryGetValue(question.Id, out var values)
                    ? values
                    : new List<SingleChoice>();
            }

            if (question is ChoiceQuestion<MultipleChoice> multipleChoiceQuestion)
            {
                multipleChoiceQuestion.PossibleChoices = multipleChoices.TryGetValue(question.Id, out var values)
                    ? values
                    : new List<MultipleChoice>();
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

    public SingleChoice ReadSingleChoiceByIdForQuestion(int questionId, int optionId)
    {
        return _dbContext.Set<SingleChoice>()
            .SingleOrDefault(choice =>
                choice.Id == optionId &&
                choice.Question.Id == questionId);
    }

    public MultipleChoice ReadMultipleChoiceByIdForQuestion(int questionId, int optionId)
    {
        return _dbContext.Set<MultipleChoice>()
            .SingleOrDefault(choice =>
                choice.Id == optionId &&
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

        builder.HasMany(q => q.AnswerSubmissions)
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
        builder.HasMany(q => q.AnswerSubmissions)
            .WithOne(a => a.Question as ScaleQuestion)
            .IsRequired();

        #endregion
    }
}

// Base configuration for ChoiceQuestion
public abstract class ChoiceQuestionConfig<TChoice, TQuestion> : IEntityTypeConfiguration<TQuestion>
    where TChoice : Choice<TChoice>
    where TQuestion : ChoiceQuestion<TChoice>
{
    public virtual void Configure(EntityTypeBuilder<TQuestion> builder)
    {
        #region Relations

        // ChoiceQuestion -> Choices
        builder.HasMany(q => q.PossibleChoices)
            .WithOne(c => c.Question as TQuestion)
            .IsRequired();

        // ChoiceQuestion -> Answer<TChoice> submissions
        builder.HasMany(q => q.AnswerSubmissions)
            .WithOne(a => a.Question as TQuestion)
            .IsRequired();

        #endregion
    }
}

// Concrete configurations for each choice question type
public class SingleChoiceQuestionConfig : ChoiceQuestionConfig<SingleChoice, ChoiceQuestion<SingleChoice>>
{
}

public class MultipleChoiceQuestionConfig : ChoiceQuestionConfig<MultipleChoice, ChoiceQuestion<MultipleChoice>>
{
}

// Choice configurations
public class SingleChoiceConfig : IEntityTypeConfiguration<SingleChoice>
{
    public void Configure(EntityTypeBuilder<SingleChoice> builder)
    {
        //builder.HasKey(c => new { c.Question.Id, c.Text }); // Composite key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Text)
            .HasMaxLength(250)
            .IsRequired();
    }
}

public class MultipleChoiceConfig : IEntityTypeConfiguration<MultipleChoice>
{
    public void Configure(EntityTypeBuilder<MultipleChoice> builder)
    {
        //builder.HasKey(c => new { c.Question.Id, c.Text }); // Composite key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Text)
            .HasMaxLength(250)
            .IsRequired();
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

public class AnswerSingleChoiceConfig : IEntityTypeConfiguration<Answer<SingleChoice>>
{
    public void Configure(EntityTypeBuilder<Answer<SingleChoice>> builder)
    {
        // Answer<SingleChoice> -> SingleChoice reference
        builder.HasOne(a => a.Value)
            .WithMany()
            .IsRequired();
    }
}

public class AnswerMultipleChoiceConfig : IEntityTypeConfiguration<Answer<MultipleChoice>>
{
    public void Configure(EntityTypeBuilder<Answer<MultipleChoice>> builder)
    {
        // Answer<MultipleChoice> -> MultipleChoice reference
        builder.HasOne(a => a.Value)
            .WithMany()
            .IsRequired();
    }
}

