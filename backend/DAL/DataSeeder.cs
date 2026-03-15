using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform;
using Conversey.BL.Domain.Subplatform.Survey;
using Conversey.BL.Domain.Subplatform.Survey.Questions;

namespace Conversey.DAL;

public static class DataSeeder
{
    public static void Seed(ConverseyDbContext context)
    {
        context.CreateDatabase(false);

        #region SeedWorkspaces

        var gemeente = new Workspace
        {
            Name = "Gemeente"
        };
        gemeente.Slug = Slug.FromName(gemeente.Name);

        var axaBank = new Workspace
        {
            Name = "Axa Bank"
        };
        axaBank.Slug = Slug.FromName(axaBank.Name);

        var school = new Workspace
        {
            Name = "School",
        };
        school.Slug = Slug.FromName(school.Name);

        context.Workspaces.Add(gemeente);
        context.Workspaces.Add(axaBank);
        context.Workspaces.Add(school);

        #endregion

        #region SeedProjects

        var openbaarVervoer = new Project
        {
            Title = "Openbaar Vervoer",
            Description = "Denk mee over betere bereikbaarheid, comfortabeler reizen en duidelijkere communicatie voor het openbaar vervoer in jouw buurt.",
            ImageUrl = "https://images.unsplash.com/photo-1517142089942-ba376ce32a2e?auto=format&fit=crop&w=1600&q=80",
            Status = Status.Active,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddMonths(2),
            InteractionForm = InteractionType.Chat,
            Workspace = gemeente,
        };
        openbaarVervoer.Slug = Slug.FromName(openbaarVervoer.Title);

        var mentalWellbeing2026 = new Project
        {
            Title = "mental wellbeing 2026",
            Description = "Help us understand how young people experience stress, wellbeing, and support in their daily lives. Your answers are anonymous and will shape future initiatives.",
            ImageUrl = "https://images.unsplash.com/photo-1544027993-37dbfe43562a?auto=format&fit=crop&w=1648&h=3660&q=90&dpr=2",
            Status = Status.Active,
            StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            InteractionForm = InteractionType.Chat,
            Workspace = axaBank,
        };
        mentalWellbeing2026.Slug = Slug.FromName(mentalWellbeing2026.Title);

        var mentaal = new Project
        {
            Title = "Mentale gezondheid",
            Description = "Vertel hoe jouw school mentale gezondheid beter kan ondersteunen.",
            ImageUrl = "https://images.unsplash.com/photo-1491841550275-ad7854e35ca6?auto=format&fit=crop&w=1600&q=80",
            Status = Status.Active,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddMonths(3),
            InteractionForm = InteractionType.Chat,
            Workspace = school,
        };
        mentaal.Slug = Slug.FromName(mentaal.Title);

        context.Projects.Add(openbaarVervoer);
        context.Projects.Add(mentalWellbeing2026);
        context.Projects.Add(mentaal);

        #endregion

        #region SeedQuestions

        context.Questions.AddRange(
            CreateSingleChoiceQuestion(
                mentalWellbeing2026,
                1,
                "What is your main source of stress?",
                true,
                "Exams",
                "Financial situation",
                "Family",
                "Social pressure"
            ),
            CreateSingleChoiceQuestion(
                mentalWellbeing2026,
                2,
                "How often do you feel overwhelmed during a typical week?",
                true,
                "Never",
                "1-2 times",
                "3-4 times",
                "Almost every day"
            ),
            CreateSingleChoiceQuestion(
                mentalWellbeing2026,
                3,
                "What is your preferred way to relax after a stressful day?",
                true,
                "Spending time with friends",
                "Physical exercise",
                "Creative activities",
                "Sleeping or resting"
            ),
            CreateSingleChoiceQuestion(
                mentalWellbeing2026,
                4,
                "How do you rate your current mental health on a scale of 1-10?",
                true,
                "1-3 (Poor)",
                "4-6 (Fair)",
                "7-8 (Good)",
                "9-10 (Excellent)"
            ),
            CreateOpenQuestion(
                mentalWellbeing2026,
                5,
                "Describe a situation where you felt supported by someone around you.",
                true
            ),
            CreateOpenQuestion(
                mentalWellbeing2026,
                6,
                "What would help you manage stress better? Share your ideas.",
                false
            ),
            CreateSingleChoiceQuestion(
                mentalWellbeing2026,
                7,
                "Do you have access to mental health resources or counseling?",
                true,
                "Yes, easily accessible",
                "Yes, but difficult to access",
                "No, not available",
                "Not sure"
            ),
            CreateOpenQuestion(
                mentalWellbeing2026,
                8,
                "What changes would you like to see in your school or workplace to better support mental health?",
                false
            )
        );

        #endregion

        context.SaveChanges();
    }

    private static SingleChoiceQuestion CreateSingleChoiceQuestion(Project project, int order, string text, bool isRequired, params string[] options)
    {
        return new SingleChoiceQuestion
        {
            Project = project,
            Order = order,
            Text = text,
            IsRequired = isRequired,
            Options = options
                .Select((optionText, index) => new QuestionOption
                {
                    Order = index + 1,
                    Text = optionText,
                })
                .ToList()
        };
    }

    private static OpenQuestion CreateOpenQuestion(Project project, int order, string text, bool isRequired)
    {
        return new OpenQuestion
        {
            Project = project,
            Order = order,
            Text = text,
            IsRequired = isRequired,
        };
    }
}