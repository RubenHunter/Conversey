using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform;
using Conversey.BL.Domain.Subplatform.Survey;
using Conversey.BL.Domain.Subplatform.Survey.Ideation;
using Conversey.BL.Domain.Subplatform.Survey.Questions;

namespace Conversey.DAL;

public static class DataSeeder
{
    public static void Seed(ConverseyDbContext context)
    {
        context.CreateDatabase(false);

        #region SeedWorkspaces

        var hogeschool = new Workspace
        {
            Name = "Hogeschool Nova"
        };
        hogeschool.Slug = Slug.FromName(hogeschool.Name);

        context.Workspaces.Add(hogeschool);

        #endregion

        #region SeedProjects

        var mentaalWelzijnActieplan = new Project
        {
            Title = "Actieplan Mentaal Welzijn 2026-2027",
            Description = "Samen met studenten ontwikkelen we een actieplan dat mentaal welzijn versterkt op campus, in lessen en in begeleiding.",
            ImageUrl = "https://images.unsplash.com/photo-1523240795612-9a054b0db644?auto=format&fit=crop&w=1600&q=80",
            Status = Status.Active,
            StartDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 6, 30, 0, 0, 0, DateTimeKind.Utc),
            InteractionForm = InteractionType.Chat,
            Workspace = hogeschool
        };
        mentaalWelzijnActieplan.Slug = Slug.FromName(mentaalWelzijnActieplan.Title);

        context.Projects.Add(mentaalWelzijnActieplan);

        #endregion

        #region SeedYouths

        var students = new List<Youth>
        {
            new() { Token = "st-amelie-01", Email = "amelie@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Token = "st-younes-02", Email = "younes@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Token = "st-lotte-03", Email = "lotte@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Token = "st-milan-04", Email = "milan@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Token = "st-sarah-05", Email = "sarah@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Token = "st-noah-06", Email = "noah@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Token = "st-zineb-07", Email = "zineb@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Token = "st-ruben-08", Email = "ruben@student.nova.be", Project = mentaalWelzijnActieplan }
        };

        context.Youths.AddRange(students);

        #endregion

        #region SeedTopics

        var topics = new List<Topic>
        {
            new()
            {
                Name = "Studiedruk en evaluatie",
                Context = "Hoe kunnen deadlines, examens en feedback beter afgestemd worden op haalbare studiedruk?",
                Project = mentaalWelzijnActieplan
            },
            new()
            {
                Name = "Toegankelijke ondersteuning",
                Context = "Welke ondersteuning verwachten studenten van trajectbegeleiding, studentenpsychologen en docenten?",
                Project = mentaalWelzijnActieplan
            },
            new()
            {
                Name = "Veilige en verbonden campus",
                Context = "Hoe kunnen we meer verbondenheid en een veilige sfeer creëren op en rond de campus?",
                Project = mentaalWelzijnActieplan
            },
            new()
            {
                Name = "Balans studie-werk-prive",
                Context = "Welke acties helpen studenten om studie te combineren met werk, familie en ontspanning?",
                Project = mentaalWelzijnActieplan
            },
            new()
            {
                Name = "Digitale en hybride leeromgeving",
                Context = "Hoe maken we online en hybride leren mentaal haalbaar en sociaal ondersteunend?",
                Project = mentaalWelzijnActieplan
            }
        };

        context.Topics.AddRange(topics);

        #endregion

        #region SeedQuestions

        context.Questions.AddRange(
            CreateSingleChoiceQuestion(
                mentaalWelzijnActieplan,
                1,
                "Hoe ervaar je je mentaal welzijn tijdens een gemiddelde lesweek?",
                true,
                "Zeer goed",
                "Goed",
                "Neutraal",
                "Moeilijk",
                "Zeer moeilijk"
            ),
            CreateSingleChoiceQuestion(
                mentaalWelzijnActieplan,
                2,
                "Wat veroorzaakt bij jou de meeste stress binnen de hogeschool?",
                true,
                "Deadlines en examens",
                "Combinatie studie met werk/prive",
                "Onzekerheid over resultaten",
                "Sociale druk",
                "Praktische organisatie"
            ),
            CreateSingleChoiceQuestion(
                mentaalWelzijnActieplan,
                3,
                "In welke mate weet je waar je hulp kan vinden op de campus?",
                true,
                "Ik weet exact waar en bij wie",
                "Ik heb een idee, maar niet volledig",
                "Ik weet het niet"
            ),
            CreateOpenQuestion(
                mentaalWelzijnActieplan,
                4,
                "Welke concrete ondersteuning van docenten of begeleiders zou jou het meeste helpen?",
                true
            ),
            CreateSingleChoiceQuestion(
                mentaalWelzijnActieplan,
                5,
                "Hoe verbonden voel je je met medestudenten in je opleiding?",
                true,
                "Heel sterk",
                "Voldoende",
                "Beperkt",
                "Nauwelijks"
            ),
            CreateOpenQuestion(
                mentaalWelzijnActieplan,
                6,
                "Noem een actie die de hogeschool volgend semester meteen kan starten.",
                false
            ),
            CreateSingleChoiceQuestion(
                mentaalWelzijnActieplan,
                7,
                "Wanneer heb je het meeste nood aan welzijnsondersteuning?",
                true,
                "Begin semester",
                "Tijdens tussentijdse opdrachten",
                "Examenperiode",
                "Doorheen het hele semester"
            ),
            CreateOpenQuestion(
                mentaalWelzijnActieplan,
                8,
                "Hoe kan digitaal of hybride onderwijs mentaal draaglijker gemaakt worden?",
                false
            )
        );

        #endregion

        #region SeedIdeas

        var now = DateTime.UtcNow;
        var ideas = new List<Idea>
        {
            new()
            {
                Content = "Plan elke opleidingsweek een vast 'deadlinevrij blok' zodat we minstens 1 avond zonder schoolwerk hebben.",
                Summary = "Wekelijks deadlinevrij blok",
                SubmissionDate = now.AddDays(-12),
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[0]
            },
            new()
            {
                Content = "Maak een centrale welzijnspagina in Toledo met alle hulpkanalen, openingsuren en wie je waarvoor kan contacteren.",
                Summary = "Centrale welzijnspagina",
                SubmissionDate = now.AddDays(-11),
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[1],
                Youth = students[1]
            },
            new()
            {
                Content = "Start per opleiding met kleine peer-support groepen van 8 studenten die tweewekelijks samenkomen.",
                Summary = "Peer-support groepen",
                SubmissionDate = now.AddDays(-10),
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[2],
                Youth = students[2]
            },
            new()
            {
                Content = "Voorzie tijdens examenweken stille ontspanningsruimtes met water, fruit en korte ademhalingsoefeningen.",
                Summary = "Stille ontspanningsruimtes",
                SubmissionDate = now.AddDays(-9),
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[2],
                Youth = students[3]
            },
            new()
            {
                Content = "Laat studenten een flexibel inhaalmoment kiezen wanneer ze overbelast zijn, zonder extra administratieve drempels.",
                Summary = "Flexibel inhaalmoment",
                SubmissionDate = now.AddDays(-8),
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[4]
            },
            new()
            {
                Content = "Organiseer elke maand een lunchsessie over stressmanagement met studentenbegeleiding en ervaringsstudenten.",
                Summary = "Maandelijkse stress-lunch",
                SubmissionDate = now.AddDays(-7),
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[1],
                Youth = students[5]
            },
            new()
            {
                Content = "Spreid grote groepsopdrachten beter over het semester zodat piekweken minder zwaar zijn.",
                Summary = "Betere spreiding groepsopdrachten",
                SubmissionDate = now.AddDays(-6),
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[6]
            },
            new()
            {
                Content = "Bied een stille online studie-room aan met vaste momenten en een moderator, voor wie thuis snel afgeleid is.",
                Summary = "Online studie-room",
                SubmissionDate = now.AddDays(-5),
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[4],
                Youth = students[7]
            },
            new()
            {
                Content = "Voor werkstudenten zou een rooster met minstens drie weken voorspelbaarheid veel mentale rust geven.",
                Summary = "Voorspelbaar rooster",
                SubmissionDate = now.AddDays(-4),
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[3],
                Youth = students[1]
            }
        };

        context.Ideas.AddRange(ideas);

        #endregion

        #region SeedResponses

        var responses = new List<Response>
        {
            new()
            {
                Idea = ideas[0],
                Text = "Topidee. Als we op dinsdagavond geen deadlines hebben, helpt dat echt om even op adem te komen.",
                CreatedAt = now.AddDays(-11).AddHours(2),
                Youth = students[3],
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[0],
                Text = "Misschien ook aanduiden in het lessenrooster welke week rustiger is, dat maakt plannen makkelijker.",
                CreatedAt = now.AddDays(-11).AddHours(6),
                Youth = students[6],
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[1],
                Text = "Ja graag, nu moet je info op drie verschillende pagina's zoeken.",
                CreatedAt = now.AddDays(-10).AddHours(4),
                Youth = students[0],
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[2],
                Text = "Peer-support lijkt me sterk, zeker in het eerste semester wanneer iedereen nog zoekt naar ritme.",
                CreatedAt = now.AddDays(-9).AddHours(3),
                Youth = students[5],
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[3],
                Text = "Die stille ruimtes zouden tijdens blok echt waardevol zijn. Misschien ook korte stretch-momenten tonen op scherm.",
                CreatedAt = now.AddDays(-8).AddHours(5),
                Youth = students[2],
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[4],
                Text = "Een eenvoudig formulier met reden en voorkeursmoment zou al genoeg zijn.",
                CreatedAt = now.AddDays(-7).AddHours(4),
                Youth = students[7],
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[5],
                Text = "Als er ook opnames of samenvattingen zijn voor wie niet kan komen, bereik je meer studenten.",
                CreatedAt = now.AddDays(-6).AddHours(2),
                Youth = students[4],
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[6],
                Text = "Helemaal akkoord, in sommige weken hebben we drie grote deadlines op twee dagen.",
                CreatedAt = now.AddDays(-5).AddHours(7),
                Youth = students[1],
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[7],
                Text = "Online study room met camera optioneel zou fijn zijn, dan is de drempel lager.",
                CreatedAt = now.AddDays(-4).AddHours(1),
                Youth = students[6],
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[8],
                Text = "Voor werkstudenten is voorspelbaarheid echt het verschil tussen haalbaar en niet haalbaar.",
                CreatedAt = now.AddDays(-3).AddHours(3),
                Youth = students[0],
                Status = IdeaStatus.Approved,
                ModerationInfo = new ModerationInfo()
            }
        };

        context.Responses.AddRange(responses);

        #endregion

        #region SeedResponseReactions

        // Raw unicode works well with Postgres (UTF-8) and keeps demo data close to UI behavior.
        var reactions = new List<ResponseReaction>
        {
            new() { Response = responses[0], Emoji = "🔥", CreatedAt = now.AddDays(-11).AddHours(3), Youth = students[1], YouthToken = students[1].Token },
            new() { Response = responses[0], Emoji = "💡", CreatedAt = now.AddDays(-11).AddHours(4), Youth = students[4], YouthToken = students[4].Token },
            new() { Response = responses[1], Emoji = "❤️", CreatedAt = now.AddDays(-11).AddHours(7), Youth = students[2], YouthToken = students[2].Token },
            new() { Response = responses[2], Emoji = "🙏", CreatedAt = now.AddDays(-10).AddHours(5), Youth = students[5], YouthToken = students[5].Token },
            new() { Response = responses[2], Emoji = "😂", CreatedAt = now.AddDays(-10).AddHours(6), Youth = students[7], YouthToken = students[7].Token },
            new() { Response = responses[3], Emoji = "🙌", CreatedAt = now.AddDays(-9).AddHours(6), Youth = students[0], YouthToken = students[0].Token },
            new() { Response = responses[4], Emoji = "😢", CreatedAt = now.AddDays(-8).AddHours(6), Youth = students[3], YouthToken = students[3].Token },
            new() { Response = responses[4], Emoji = "💚", CreatedAt = now.AddDays(-8).AddHours(7), Youth = students[1], YouthToken = students[1].Token },
            new() { Response = responses[5], Emoji = "👏", CreatedAt = now.AddDays(-7).AddHours(5), Youth = students[6], YouthToken = students[6].Token },
            new() { Response = responses[6], Emoji = "🎯", CreatedAt = now.AddDays(-6).AddHours(3), Youth = students[2], YouthToken = students[2].Token },
            new() { Response = responses[6], Emoji = "👍", CreatedAt = now.AddDays(-6).AddHours(4), Youth = students[0], YouthToken = students[0].Token },
            new() { Response = responses[7], Emoji = "💯", CreatedAt = now.AddDays(-5).AddHours(8), Youth = students[4], YouthToken = students[4].Token },
            new() { Response = responses[8], Emoji = "🧠", CreatedAt = now.AddDays(-4).AddHours(2), Youth = students[5], YouthToken = students[5].Token },
            new() { Response = responses[9], Emoji = "✅", CreatedAt = now.AddDays(-3).AddHours(4), Youth = students[3], YouthToken = students[3].Token }
        };

        context.ResponseReactions.AddRange(reactions);

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
                    Text = optionText
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
            IsRequired = isRequired
        };
    }
}
