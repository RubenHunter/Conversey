using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Domain.Survey;

namespace Conversey.DAL;

public static class DataSeeder
{
    public static void Seed(ConverseyDbContext context)
    {
        context.CreateDatabase(false);

        var now = new DateTime(2026, 4, 13, 0, 0, 0, DateTimeKind.Utc);

        // =====================================================
        // Seed case 1: AXA Bank / Mental Wellbeing 2026
        // =====================================================
        var axaBank = new Workspace
        {
            Id = Slug.FromName("AXA Bank"),
            Name = "AXA Bank",
            Projects = new List<Project>()
        };

        var mentalWellbeing = new Project
        {
            Id = Slug.FromName("Mental Wellbeing 2026"),
            Name = "Mental Wellbeing 2026",
            Description = "A survey about mental wellbeing in the organization.",
            ImageUrl = "https://images.unsplash.com/photo-1523240795612-9a054b0db644?auto=format&fit=crop&w=1600&q=80",
            Status = Status.Active,
            StartDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 7, 12, 0, 0, 0, DateTimeKind.Utc),
            InteractionForm = InteractionType.Chat,
            Workspace = axaBank,
            Topic = new List<Topic>(),
            Questions = new List<Question>(),
            Youth = new List<Youth>()
        };

        var axaTopics = new List<Topic>
        {
            CreateTopic(mentalWellbeing, "Study pressure and evaluation", "How can deadlines, exams and feedback be better aligned with manageable study pressure?"),
            CreateTopic(mentalWellbeing, "Accessible support", "What support do students expect from guidance staff, psychologists and lecturers?"),
            CreateTopic(mentalWellbeing, "Safe and connected campus", "How can we create more connection and a safer atmosphere on and around campus?"),
            CreateTopic(mentalWellbeing, "Study-work-life balance", "Which actions help students combine study with work, family and relaxation?"),
            CreateTopic(mentalWellbeing, "Digital and hybrid learning environment", "How do we make online and hybrid learning mentally manageable and socially supportive?")
        };

        var axaYouths = new List<Youth>
        {
            CreateYouth(Guid.Parse("11111111-1111-1111-1111-111111111111"), "amelie@student.nova.be", mentalWellbeing),
            CreateYouth(Guid.Parse("11111111-1111-1111-1111-111111111112"), "younes@student.nova.be", mentalWellbeing),
            CreateYouth(Guid.Parse("11111111-1111-1111-1111-111111111113"), "lotte@student.nova.be", mentalWellbeing),
            CreateYouth(Guid.Parse("11111111-1111-1111-1111-111111111114"), "milan@student.nova.be", mentalWellbeing),
            CreateYouth(Guid.Parse("11111111-1111-1111-1111-111111111115"), "sarah@student.nova.be", mentalWellbeing),
            CreateYouth(Guid.Parse("11111111-1111-1111-1111-111111111116"), "noah@student.nova.be", mentalWellbeing),
            CreateYouth(Guid.Parse("11111111-1111-1111-1111-111111111117"), "zineb@student.nova.be", mentalWellbeing),
            CreateYouth(Guid.Parse("11111111-1111-1111-1111-111111111118"), "ruben@student.nova.be", mentalWellbeing)
        };

        var axaQuestions = new List<Question>
        {
            CreateSingleChoiceQuestion(mentalWellbeing, "How do you feel about your mental wellbeing during a typical week?", true,
                "Very good", "Good", "Neutral", "Difficult", "Very difficult"),
            CreateSingleChoiceQuestion(mentalWellbeing, "What causes you the most stress at the organization?", true,
                "Deadlines and exams", "Study/work balance", "Uncertainty about results", "Social pressure", "Planning issues"),
            CreateMultipleChoiceQuestion(mentalWellbeing, "Which support channels would you use most often?", true,
                "Student coach", "Lecturer feedback", "Peer support", "Online wellbeing page", "Psychological support"),
            CreateScaleQuestion(mentalWellbeing, "How well do you know where to find help on campus?", true, 1, 5),
            CreateOpenQuestion(mentalWellbeing, "What concrete support from lecturers or supervisors would help you most?", true),
            CreateSingleChoiceQuestion(mentalWellbeing, "How connected do you feel to fellow students in your program?", true,
                "Very connected", "Connected", "Somewhat connected", "Hardly connected"),
            CreateOpenQuestion(mentalWellbeing, "Name one action the organization could start next semester.", false),
            CreateSingleChoiceQuestion(mentalWellbeing, "When do you most need wellbeing support?", true,
                "Start of semester", "During assignments", "Exam period", "All semester"),
            CreateOpenQuestion(mentalWellbeing, "How could digital or hybrid education be made more mentally manageable?", false)
        };

        var axaIdeas = new List<Idea>
        {
            CreateIdea(mentalWellbeing, axaTopics[0], axaYouths[0], "Plan one fixed deadline-free block every week so students always get at least one evening without school work.", "Weekly deadline-free block", now.AddDays(-12)),
            CreateIdea(mentalWellbeing, axaTopics[1], axaYouths[1], "Create a central wellbeing page with all support channels, opening hours and contact options.", "Central wellbeing page", now.AddDays(-11)),
            CreateIdea(mentalWellbeing, axaTopics[2], axaYouths[2], "Start small peer-support groups of eight students that meet every two weeks.", "Peer-support groups", now.AddDays(-10)),
            CreateIdea(mentalWellbeing, axaTopics[2], axaYouths[3], "Provide quiet relaxation spaces during exam weeks with water, fruit and short breathing exercises.", "Quiet relaxation spaces", now.AddDays(-9)),
            CreateIdea(mentalWellbeing, axaTopics[0], axaYouths[4], "Allow a flexible catch-up moment when students feel overloaded, without extra administrative barriers.", "Flexible catch-up moment", now.AddDays(-8)),
            CreateIdea(mentalWellbeing, axaTopics[1], axaYouths[5], "Organize a monthly lunch session on stress management with student support and experience experts.", "Monthly stress lunch", now.AddDays(-7)),
            CreateIdea(mentalWellbeing, axaTopics[0], axaYouths[6], "Spread large group assignments better across the semester so peak weeks are less heavy.", "Better spread of group work", now.AddDays(-6)),
            CreateIdea(mentalWellbeing, axaTopics[4], axaYouths[7], "Offer a quiet online study room with fixed moments and a moderator for students who get distracted at home.", "Online study room", now.AddDays(-5))
        };

        var axaResponses = new List<Response>
        {
            CreateResponse(axaIdeas[0], axaYouths[3], "Great idea. A no-deadline evening would really help to decompress.", now.AddDays(-11).AddHours(2)),
            CreateResponse(axaIdeas[1], axaYouths[0], "Yes please, right now you have to search across too many different pages.", now.AddDays(-10).AddHours(4)),
            CreateResponse(axaIdeas[2], axaYouths[5], "Peer support seems very strong, especially in the first semester.", now.AddDays(-9).AddHours(3)),
            CreateResponse(axaIdeas[3], axaYouths[2], "Quiet spaces would be valuable during exams.", now.AddDays(-8).AddHours(5)),
            CreateResponse(axaIdeas[4], axaYouths[7], "A simple form with a reason and preferred time would already help a lot.", now.AddDays(-7).AddHours(4)),
            CreateResponse(axaIdeas[5], axaYouths[4], "If there are also recordings or summaries for students who cannot attend, reach will be much better.", now.AddDays(-6).AddHours(2))
        };

        var axaIdeaReactions = new List<IdeaReaction>
        {
            CreateIdeaReaction(axaIdeas[0], axaYouths[2], "👍", now.AddDays(-11).AddHours(1)),
            CreateIdeaReaction(axaIdeas[1], axaYouths[4], "💡", now.AddDays(-10).AddHours(2)),
            CreateIdeaReaction(axaIdeas[2], axaYouths[6], "🔥", now.AddDays(-9).AddHours(1)),
            CreateIdeaReaction(axaIdeas[4], axaYouths[0], "🙌", now.AddDays(-8).AddHours(2))
        };

        var axaResponseReactions = new List<ResponseReaction>
        {
            CreateResponseReaction(axaResponses[0], axaYouths[1], "🔥", now.AddDays(-11).AddHours(3)),
            CreateResponseReaction(axaResponses[0], axaYouths[4], "💡", now.AddDays(-11).AddHours(4)),
            CreateResponseReaction(axaResponses[1], axaYouths[2], "❤️", now.AddDays(-10).AddHours(5)),
            CreateResponseReaction(axaResponses[2], axaYouths[6], "🙏", now.AddDays(-9).AddHours(5)),
            CreateResponseReaction(axaResponses[3], axaYouths[0], "🙌", now.AddDays(-8).AddHours(6)),
            CreateResponseReaction(axaResponses[4], axaYouths[3], "✅", now.AddDays(-7).AddHours(5)),
            CreateResponseReaction(axaResponses[5], axaYouths[6], "👏", now.AddDays(-6).AddHours(3))
        };

        // =====================================================
        // Seed case 2: Hogeschool Nova / Actieplan Mentaal Welzijn 2026-2027
        // =====================================================
        var hogeschool = new Workspace
        {
            Id = Slug.FromName("Hogeschool Nova"),
            Name = "Hogeschool Nova",
            Projects = new List<Project>()
        };

        var actieplan = new Project
        {
            Id = Slug.FromName("Actieplan Mentaal Welzijn 2026-2027"),
            Name = "Actieplan Mentaal Welzijn 2026-2027",
            Description = "Samen met studenten ontwikkelen we een actieplan dat mentaal welzijn versterkt op campus, in lessen en in begeleiding.",
            ImageUrl = "https://images.unsplash.com/photo-1523240795612-9a054b0db644?auto=format&fit=crop&w=1600&q=80",
            Status = Status.Active,
            StartDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 6, 30, 0, 0, 0, DateTimeKind.Utc),
            InteractionForm = InteractionType.Chat,
            Workspace = hogeschool,
            Topic = new List<Topic>(),
            Questions = new List<Question>(),
            Youth = new List<Youth>()
        };

        var schoolTopics = new List<Topic>
        {
            CreateTopic(actieplan, "Studiedruk en evaluatie", "Hoe kunnen deadlines, examens en feedback beter afgestemd worden op haalbare studiedruk?"),
            CreateTopic(actieplan, "Toegankelijke ondersteuning", "Welke ondersteuning verwachten studenten van trajectbegeleiding, studentenpsychologen en docenten?"),
            CreateTopic(actieplan, "Veilige en verbonden campus", "Hoe kunnen we meer verbondenheid en een veilige sfeer creëren op en rond de campus?"),
            CreateTopic(actieplan, "Balans studie-werk-prive", "Welke acties helpen studenten om studie te combineren met werk, familie en ontspanning?"),
            CreateTopic(actieplan, "Digitale en hybride leeromgeving", "Hoe maken we online en hybride leren mentaal draaglijker en sociaal ondersteunend?")
        };

        var schoolYouths = new List<Youth>
        {
            CreateYouth(Guid.Parse("22222222-2222-2222-2222-222222222221"), "student1@hogeschoolnova.be", actieplan),
            CreateYouth(Guid.Parse("22222222-2222-2222-2222-222222222222"), "student2@hogeschoolnova.be", actieplan),
            CreateYouth(Guid.Parse("22222222-2222-2222-2222-222222222223"), "student3@hogeschoolnova.be", actieplan),
            CreateYouth(Guid.Parse("22222222-2222-2222-2222-222222222224"), "student4@hogeschoolnova.be", actieplan),
            CreateYouth(Guid.Parse("22222222-2222-2222-2222-222222222225"), "student5@hogeschoolnova.be", actieplan),
            CreateYouth(Guid.Parse("22222222-2222-2222-2222-222222222226"), "student6@hogeschoolnova.be", actieplan)
        };

        var schoolQuestions = new List<Question>
        {
            CreateSingleChoiceQuestion(actieplan, "Hoe ervaar je je mentaal welzijn tijdens een gemiddelde lesweek?", true,
                "Zeer goed", "Goed", "Neutraal", "Moeilijk", "Zeer moeilijk"),
            CreateSingleChoiceQuestion(actieplan, "Wat veroorzaakt bij jou de meeste stress binnen de hogeschool?", true,
                "Deadlines en examens", "Combinatie studie met werk/prive", "Onzekerheid over resultaten", "Sociale druk", "Praktische organisatie"),
            CreateMultipleChoiceQuestion(actieplan, "Welke steunvormen gebruik je het liefst?", true,
                "Trajectbegeleiding", "Studiegenoten", "Docenten", "Online info", "Psycholoog"),
            CreateScaleQuestion(actieplan, "In welke mate weet je waar je hulp kan vinden op de campus?", true, 1, 3),
            CreateOpenQuestion(actieplan, "Welke concrete ondersteuning van docenten of begeleiders zou jou het meeste helpen?", true),
            CreateSingleChoiceQuestion(actieplan, "Hoe verbonden voel je je met medestudenten in je opleiding?", true,
                "Heel sterk", "Voldoende", "Beperkt", "Nauwelijks"),
            CreateOpenQuestion(actieplan, "Noem een actie die de hogeschool volgend semester meteen kan starten.", false)
        };

        var schoolIdeas = new List<Idea>
        {
            CreateIdea(actieplan, schoolTopics[0], schoolYouths[0], "Plan elke opleidingsweek een vast deadlinevrij blok zodat we minstens één avond zonder schoolwerk hebben.", "Wekelijks deadlinevrij blok", now.AddDays(-12)),
            CreateIdea(actieplan, schoolTopics[1], schoolYouths[1], "Maak een centrale welzijnspagina in Toledo met alle hulpkanalen, openingsuren en contactpersonen.", "Centrale welzijnspagina", now.AddDays(-11)),
            CreateIdea(actieplan, schoolTopics[2], schoolYouths[2], "Start per opleiding kleine peer-support groepen die tweewekelijks samenkomen.", "Peer-support groepen", now.AddDays(-10)),
            CreateIdea(actieplan, schoolTopics[2], schoolYouths[3], "Voorzie tijdens examenweken stille ontspanningsruimtes met water, fruit en ademhalingsoefeningen.", "Stille ontspanningsruimtes", now.AddDays(-9)),
            CreateIdea(actieplan, schoolTopics[0], schoolYouths[4], "Laat studenten een flexibel inhaalmoment kiezen wanneer ze overbelast zijn.", "Flexibel inhaalmoment", now.AddDays(-8)),
            CreateIdea(actieplan, schoolTopics[1], schoolYouths[5], "Organiseer maandelijks een lunchsessie over stressmanagement met studentenbegeleiding.", "Maandelijkse stress-lunch", now.AddDays(-7))
        };

        var schoolResponses = new List<Response>
        {
            CreateResponse(schoolIdeas[0], schoolYouths[3], "Topidee. Als we op dinsdagavond geen deadlines hebben, helpt dat echt om even op adem te komen.", now.AddDays(-11).AddHours(2)),
            CreateResponse(schoolIdeas[1], schoolYouths[0], "Ja graag, nu moet je info op drie verschillende pagina's zoeken.", now.AddDays(-10).AddHours(4)),
            CreateResponse(schoolIdeas[2], schoolYouths[5], "Peer-support lijkt me sterk, zeker in het eerste semester.", now.AddDays(-9).AddHours(3)),
            CreateResponse(schoolIdeas[3], schoolYouths[2], "Die stille ruimtes zouden tijdens blok echt waardevol zijn.", now.AddDays(-8).AddHours(5)),
            CreateResponse(schoolIdeas[4], schoolYouths[1], "Een eenvoudig formulier met reden en voorkeursmoment zou al genoeg zijn.", now.AddDays(-7).AddHours(4))
        };

        var schoolIdeaReactions = new List<IdeaReaction>
        {
            CreateIdeaReaction(schoolIdeas[0], schoolYouths[1], "👍", now.AddDays(-11).AddHours(1)),
            CreateIdeaReaction(schoolIdeas[1], schoolYouths[4], "💡", now.AddDays(-10).AddHours(1)),
            CreateIdeaReaction(schoolIdeas[2], schoolYouths[5], "🔥", now.AddDays(-9).AddHours(1))
        };

        var schoolResponseReactions = new List<ResponseReaction>
        {
            CreateResponseReaction(schoolResponses[0], schoolYouths[1], "🔥", now.AddDays(-11).AddHours(3)),
            CreateResponseReaction(schoolResponses[0], schoolYouths[4], "💡", now.AddDays(-11).AddHours(4)),
            CreateResponseReaction(schoolResponses[1], schoolYouths[2], "❤️", now.AddDays(-10).AddHours(5)),
            CreateResponseReaction(schoolResponses[2], schoolYouths[0], "🙌", now.AddDays(-9).AddHours(6)),
            CreateResponseReaction(schoolResponses[3], schoolYouths[3], "✅", now.AddDays(-8).AddHours(6))
        };

        // =====================================================
        // Persist
        // =====================================================
        context.Workspaces.AddRange(axaBank, hogeschool);
        context.Projects.AddRange(mentalWellbeing, actieplan);
        context.Topics.AddRange(axaTopics);
        context.Topics.AddRange(schoolTopics);
        context.Youths.AddRange(axaYouths);
        context.Youths.AddRange(schoolYouths);
        context.Questions.AddRange(axaQuestions);
        context.Questions.AddRange(schoolQuestions);
        context.Ideas.AddRange(axaIdeas);
        context.Ideas.AddRange(schoolIdeas);
        context.Responses.AddRange(axaResponses);
        context.Responses.AddRange(schoolResponses);
        context.IdeaReactions.AddRange(axaIdeaReactions);
        context.ResponseReactions.AddRange(axaResponseReactions);
        context.IdeaReactions.AddRange(schoolIdeaReactions);
        context.ResponseReactions.AddRange(schoolResponseReactions);

        context.SaveChanges();
    }

    private static Topic CreateTopic(Project project, string name, string context)
    {
        return new Topic
        {
            Name = name,
            Context = context,
            Project = project,
            Ideas = new List<Idea>()
        };
    }

    private static Youth CreateYouth(Guid token, string email, Project project)
    {
        return new Youth
        {
            Token = token,
            Email = email,
            Project = project,
            Ideas = new List<Idea>(),
            Reactions = new List<Reaction>(),
            Responses = new List<Response>(),
            Answers = new List<Answer>()
        };
    }

    private static OpenQuestion CreateOpenQuestion(Project project, string text, bool required)
    {
        return new OpenQuestion
        {
            Text = text,
            Required = required,
            Project = project
        };
    }

    private static ScaleQuestion CreateScaleQuestion(Project project, string text, bool required, int lowerBound, int upperBound)
    {
        return new ScaleQuestion
        {
            Text = text,
            Required = required,
            LowerBound = lowerBound,
            UpperBound = upperBound,
            Project = project
        };
    }

    private static ChoiceQuestion<SingleChoice> CreateSingleChoiceQuestion(Project project, string text, bool required,
        params string[] options)
    {
        var question = new ChoiceQuestion<SingleChoice>
        {
            Text = text,
            Required = required,
            Project = project,
            PossibleChoices = new List<SingleChoice>()
        };

        question.PossibleChoices = options
            .Select(option => new SingleChoice
            {
                Text = option,
                Question = question
            })
            .ToList();

        return question;
    }

    private static ChoiceQuestion<MultipleChoice> CreateMultipleChoiceQuestion(Project project, string text, bool required,
        params string[] options)
    {
        var question = new ChoiceQuestion<MultipleChoice>
        {
            Text = text,
            Required = required,
            Project = project,
            PossibleChoices = new List<MultipleChoice>()
        };

        question.PossibleChoices = options
            .Select(option => new MultipleChoice
            {
                Text = option,
                Question = question
            })
            .ToList();

        return question;
    }

    private static Idea CreateIdea(Project project, Topic topic, Youth youth, string content, string summary, DateTime submissionDate)
    {
        var idea = new Idea
        {
            Content = content,
            Summary = summary,
            SubmissionDate = submissionDate,
            Status = ModerationStatus.Approved,
            ModerationInfo = new ModerationInfo(),
            Project = project,
            Topic = topic,
            Youth = youth,
            Reactions = new List<IdeaReaction>(),
            Responses = new List<Response>()
        };

        if (topic.Ideas is ICollection<Idea> topicIdeas)
        {
            topicIdeas.Add(idea);
        }

        if (youth.Ideas is ICollection<Idea> youthIdeas)
        {
            youthIdeas.Add(idea);
        }

        return idea;
    }

    private static Response CreateResponse(Idea idea, Youth youth, string text, DateTime createdAt)
    {
        var response = new Response
        {
            Text = text,
            CreatedAt = createdAt,
            Status = ModerationStatus.Approved,
            ModerationInfo = new ModerationInfo(),
            Idea = idea,
            Youth = youth,
            Reactions = new List<ResponseReaction>()
        };

        if (idea.Responses is ICollection<Response> ideaResponses)
        {
            ideaResponses.Add(response);
        }

        if (youth.Responses is ICollection<Response> youthResponses)
        {
            youthResponses.Add(response);
        }

        return response;
    }

    private static IdeaReaction CreateIdeaReaction(Idea idea, Youth youth, string emoji, DateTime createdAt)
    {
        var reaction = new IdeaReaction
        {
            Emoji = emoji,
            CreatedAt = createdAt,
            Youth = youth,
            Idea = idea
        };

        if (idea.Reactions is ICollection<IdeaReaction> ideaReactions)
        {
            ideaReactions.Add(reaction);
        }

        if (youth.Reactions is ICollection<Reaction> youthReactions)
        {
            youthReactions.Add(reaction);
        }

        return reaction;
    }

    private static ResponseReaction CreateResponseReaction(Response response, Youth youth, string emoji, DateTime createdAt)
    {
        var reaction = new ResponseReaction
        {
            Emoji = emoji,
            CreatedAt = createdAt,
            Youth = youth,
            Response = response
        };

        if (response.Reactions is ICollection<ResponseReaction> responseReactions)
        {
            responseReactions.Add(reaction);
        }

        if (youth.Reactions is ICollection<Reaction> youthReactions)
        {
            youthReactions.Add(reaction);
        }

        return reaction;
    }
}








