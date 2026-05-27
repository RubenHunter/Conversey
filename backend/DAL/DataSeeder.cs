using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Domain.Survey;
using Microsoft.Extensions.Configuration;

namespace Conversey.DAL;

public static class DataSeeder
{
    public static void Seed(ConverseyDbContext context, IConfiguration configuration = null)
    {
        context.CreateDatabase(false);

        var now = DateTime.UtcNow;

        // =====================================================
        // Case 1: Hogeschool Nova / Actieplan Mentaal Welzijn
        // =====================================================
        var hogeschool = new Workspace
        {
            Name = "Hogeschool Nova"
        };
        hogeschool.Id = Slug.FromName(hogeschool.Name);

        var mentaalWelzijnActieplan = new Project
        {
            Name = "Actieplan Mentaal Welzijn 2026-2027",
            Description = "Samen met studenten ontwikkelen we een actieplan dat mentaal welzijn versterkt op campus, in lessen en in begeleiding.",
            ImageUrl = "https://images.unsplash.com/photo-1523240795612-9a054b0db644?auto=format&fit=crop&w=1600&q=80",
            Status = Status.Active,
            StartDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 6, 30, 0, 0, 0, DateTimeKind.Utc),
            NudgingStrength = 3,
            InteractionForm = InteractionType.UserDefined,
            Workspace = hogeschool,
            MinAge = 18,
            MaxAge = 26
        };
        mentaalWelzijnActieplan.Id = Slug.FromName(mentaalWelzijnActieplan.Name);

        context.Workspaces.Add(hogeschool);
        context.Projects.Add(mentaalWelzijnActieplan);

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
                Context = "Hoe maken we online en hybride leren mentaal draaglijker en sociaal ondersteunend?",
                Project = mentaalWelzijnActieplan
            }
        };

        context.Topics.AddRange(topics);

        var students = new List<Youth>
        {
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444441"), Email = "amelie@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444442"), Email = "younes@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444443"), Email = "lotte@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Email = "milan@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444445"), Email = "sarah@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444446"), Email = "noah@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444447"), Email = "zineb@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444448"), Email = "ruben@student.nova.be", Project = mentaalWelzijnActieplan }
        };

        context.Youths.AddRange(students);
        // Survey-only youth (no ideas) — for realistic conversion rates
        var surveyOnlyStudents1 = new List<Youth>
        {
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444420"), Email = "karim@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444421"), Email = null, Project = mentaalWelzijnActieplan },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444422"), Email = "dirk@student.nova.be", Project = mentaalWelzijnActieplan }
        };
        context.Youths.AddRange(surveyOnlyStudents1);

        var mentaalSupportQuestion = new SingleChoiceQuestion
        {
            Text = "Hoe beoordeel je de toegankelijkheid van mentale ondersteuning op de campus?",
            Required = true,
            Project = mentaalWelzijnActieplan,
        };
        var mentaalSupportChoices = new List<Choice>
        {
            new() { Text = "Helemaal onvoldoende", Question = mentaalSupportQuestion },
            new() { Text = "Eerder onvoldoende", Question = mentaalSupportQuestion },
            new() { Text = "Voldoende", Question = mentaalSupportQuestion },
            new() { Text = "Goed", Question = mentaalSupportQuestion },
            new() { Text = "Uitstekend", Question = mentaalSupportQuestion }
        };
        mentaalSupportQuestion.PossibleChoices = mentaalSupportChoices;

        var mentaalStressScaleQuestion = new ScaleQuestion
        {
            Text = "Hoeveel stress ervaar je gemiddeld tijdens een opleidingsweek? (1 = zeer laag, 10 = zeer hoog)",
            Required = true,
            LowerBound = 1,
            UpperBound = 10,
            Project = mentaalWelzijnActieplan
        };

        var mentaalFlexQuestion = new SingleChoiceQuestion
        {
            Text = "Zou je gebruik maken van flexibele inhaalmomenten bij overbelasting?",
            Required = true,
            Project = mentaalWelzijnActieplan,
        };
        var mentaalFlexChoices = new List<Choice>
        {
            new() { Text = "Ja, zeker", Question = mentaalFlexQuestion },
            new() { Text = "Misschien, afhankelijk van het vak", Question = mentaalFlexQuestion },
            new() { Text = "Nee", Question = mentaalFlexQuestion }
        };
        mentaalFlexQuestion.PossibleChoices = mentaalFlexChoices;

        var mentaalOpenQuestion = new OpenQuestion
        {
            Text = "Welke concrete actie zou volgens jou het meeste verschil maken voor mentaal welzijn op school?",
            Required = false,
            Project = mentaalWelzijnActieplan
        };

        var mentaalQuestions = new List<Question>
        {
            mentaalSupportQuestion,
            mentaalFlexQuestion,
        };
        context.Questions.AddRange(mentaalQuestions);

        var mentaalAnswers = new List<Answer>
        {
            new SingleChoiceAnswer { Question = mentaalSupportQuestion, Youth = students[0], Value = mentaalSupportChoices[2] },
            new Answer<int> { Question = mentaalStressScaleQuestion, Youth = students[0], Value = 7 },
            new SingleChoiceAnswer { Question = mentaalFlexQuestion, Youth = students[0], Value = mentaalFlexChoices[0] },
            new Answer<string> { Question = mentaalOpenQuestion, Youth = students[0], Value = "Een wekelijkse deadlinevrije avond per opleiding zou direct stress verlagen." },

            new SingleChoiceAnswer { Question = mentaalSupportQuestion, Youth = students[3], Value = mentaalSupportChoices[1] },
            new Answer<int> { Question = mentaalStressScaleQuestion, Youth = students[3], Value = 8 },
            new SingleChoiceAnswer { Question = mentaalFlexQuestion, Youth = students[3], Value = mentaalFlexChoices[1] },
            new Answer<string> { Question = mentaalOpenQuestion, Youth = students[3], Value = "Maak begeleiding zichtbaarder in één centrale welzijnspagina." },

            new SingleChoiceAnswer { Question = mentaalSupportQuestion, Youth = students[6], Value = mentaalSupportChoices[3] },
            new Answer<int> { Question = mentaalStressScaleQuestion, Youth = students[6], Value = 6 },
            new SingleChoiceAnswer { Question = mentaalFlexQuestion, Youth = students[6], Value = mentaalFlexChoices[0] },
            new Answer<string> { Question = mentaalOpenQuestion, Youth = students[6], Value = "Bied meer stille ruimtes met korte ontspanningsoefeningen in piekweken." },
            
            new SingleChoiceAnswer { Question = mentaalSupportQuestion, Youth = students[1], Value = mentaalSupportChoices[1] },
            new Answer<int> { Question = mentaalStressScaleQuestion, Youth = students[1], Value = 9 },
            new SingleChoiceAnswer { Question = mentaalFlexQuestion, Youth = students[1], Value = mentaalFlexChoices[2] },
            new Answer<string> { Question = mentaalOpenQuestion, Youth = students[1], Value = "Peer-coaching tussen studenten van verschillende jaren werkt beter dan een anonieme psycholoog." },

            new SingleChoiceAnswer { Question = mentaalSupportQuestion, Youth = students[2], Value = mentaalSupportChoices[4] },
            new Answer<int> { Question = mentaalStressScaleQuestion, Youth = students[2], Value = 5 },
            new SingleChoiceAnswer { Question = mentaalFlexQuestion, Youth = students[2], Value = mentaalFlexChoices[0] },
            new Answer<string> { Question = mentaalOpenQuestion, Youth = students[2], Value = "Zet in op vroegtijdige signalering: docenten moeten getraind worden om stresssignalen te herkennen." },

            new SingleChoiceAnswer { Question = mentaalSupportQuestion, Youth = students[4], Value = mentaalSupportChoices[0] },
            new Answer<int> { Question = mentaalStressScaleQuestion, Youth = students[4], Value = 10 },
            new SingleChoiceAnswer { Question = mentaalFlexQuestion, Youth = students[4], Value = mentaalFlexChoices[1] },
            new Answer<string> { Question = mentaalOpenQuestion, Youth = students[4], Value = "Examenspreiding over het hele semester in plaats van alles in twee weken." },

            new SingleChoiceAnswer { Question = mentaalSupportQuestion, Youth = students[5], Value = mentaalSupportChoices[3] },
            new Answer<int> { Question = mentaalStressScaleQuestion, Youth = students[5], Value = 7 },
            new SingleChoiceAnswer { Question = mentaalFlexQuestion, Youth = students[5], Value = mentaalFlexChoices[2] },
            new Answer<string> { Question = mentaalOpenQuestion, Youth = students[5], Value = "Een buddy-systeem waarbij elke eerstejaars gekoppeld wordt aan een ouderejaars voor mentale steun." },

            new SingleChoiceAnswer { Question = mentaalSupportQuestion, Youth = students[7], Value = mentaalSupportChoices[2] },
            new Answer<int> { Question = mentaalStressScaleQuestion, Youth = students[7], Value = 8 },
            new SingleChoiceAnswer { Question = mentaalFlexQuestion, Youth = students[7], Value = mentaalFlexChoices[0] },
            new Answer<string> { Question = mentaalOpenQuestion, Youth = students[7], Value = "Creëer een online portaal waar studenten anoniem mentale gezondheidsvragen kunnen stellen." },

            new SingleChoiceAnswer { Question = mentaalSupportQuestion, Youth = surveyOnlyStudents1[0], Value = mentaalSupportChoices[3] },
            new Answer<int> { Question = mentaalStressScaleQuestion, Youth = surveyOnlyStudents1[0], Value = 4 },
            new SingleChoiceAnswer { Question = mentaalFlexQuestion, Youth = surveyOnlyStudents1[0], Value = mentaalFlexChoices[0] },
            new Answer<string> { Question = mentaalOpenQuestion, Youth = surveyOnlyStudents1[0], Value = "Meer begeleiding bij studiekeuze en loopbaanoriëntatie vermindert stress." },

            new SingleChoiceAnswer { Question = mentaalSupportQuestion, Youth = surveyOnlyStudents1[1], Value = mentaalSupportChoices[1] },
            new Answer<int> { Question = mentaalStressScaleQuestion, Youth = surveyOnlyStudents1[1], Value = 3 },
            new SingleChoiceAnswer { Question = mentaalFlexQuestion, Youth = surveyOnlyStudents1[1], Value = mentaalFlexChoices[1] },
            new Answer<string> { Question = mentaalOpenQuestion, Youth = surveyOnlyStudents1[1], Value = "Mindfulness-sessies integreren in het lesrooster zou helpen." },

            new SingleChoiceAnswer { Question = mentaalSupportQuestion, Youth = surveyOnlyStudents1[2], Value = mentaalSupportChoices[2] },
            new Answer<int> { Question = mentaalStressScaleQuestion, Youth = surveyOnlyStudents1[2], Value = 6 },
            new SingleChoiceAnswer { Question = mentaalFlexQuestion, Youth = surveyOnlyStudents1[2], Value = mentaalFlexChoices[2] },
            new Answer<string> { Question = mentaalOpenQuestion, Youth = surveyOnlyStudents1[2], Value = "Betere sportfaciliteiten op campus helpen stress te verminderen." }
        
        };
        context.Answers.AddRange(mentaalAnswers);

        var ideas = new List<Idea>
{
    new()
    {
        Content = "Plan elke opleidingsweek een vast 'deadlinevrij blok' zodat we minstens 1 avond zonder schoolwerk hebben.",
        Summary = "Wekelijks deadlinevrij blok",
        SubmissionDate = now.AddDays(-12),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[0],
        Youth = students[0],
        SemanticCategories = new[] { "study-load", "work-life-balance", "stress-reduction" }
    },
    new()
    {
        Content = "Maak een centrale welzijnspagina in Toledo met alle hulpkanalen, openingsuren en wie je waarvoor kan contacteren.",
        Summary = "Centrale welzijnspagina",
        SubmissionDate = now.AddDays(-11),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[1],
        Youth = students[1],
        SemanticCategories = new[] { "support-services", "campus-community", "digital-learning" }
    },
    new()
    {
        Content = "Start per opleiding met kleine peer-support groepen van 8 studenten die tweewekelijks samenkomen.",
        Summary = "Peer-support groepen",
        SubmissionDate = now.AddDays(-10),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[2],
        Youth = students[2],
        SemanticCategories = new[] { "campus-community", "mental-health", "support-services" }
    },
    new()
    {
        Content = "Voorzie tijdens examenweken stille ontspanningsruimtes met water, fruit en korte ademhalingsoefeningen.",
        Summary = "Stille ontspanningsruimtes",
        SubmissionDate = now.AddDays(-9),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[2],
        Youth = students[3],
        SemanticCategories = new[] { "mental-health", "stress-reduction", "campus-community" }
    },
    new()
    {
        Content = "Laat studenten een flexibel inhaalmoment kiezen wanneer ze overbelast zijn, zonder extra administratieve drempels.",
        Summary = "Flexibel inhaalmoment",
        SubmissionDate = now.AddDays(-8),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[0],
        Youth = students[4],
        SemanticCategories = new[] { "flexible-learning", "study-load", "work-life-balance" }
    },
    new()
    {
        Content = "Organiseer elke maand een lunchsessie over stressmanagement met studentenbegeleiding en ervaringsstudenten.",
        Summary = "Maandelijkse stress-lunch",
        SubmissionDate = now.AddDays(-7),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[1],
        Youth = students[5],
        SemanticCategories = new[] { "mental-health", "support-services", "campus-community" }
    },
    new()
    {
        Content = "Spreid grote groepsopdrachten beter over het semester zodat piekweken minder zwaar zijn.",
        Summary = "Betere spreiding groepsopdrachten",
        SubmissionDate = now.AddDays(-6),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[0],
        Youth = students[6],
        SemanticCategories = new[] { "study-load", "curriculum-design" }
    },
    new()
    {
        Content = "Bied een stille online studie-room aan met vaste momenten en een moderator, voor wie thuis snel afgeleid is.",
        Summary = "Online studie-room",
        SubmissionDate = now.AddDays(-5),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[4],
        Youth = students[7],
        SemanticCategories = new[] { "digital-learning", "focus-productivity", "campus-community" }
    },
    new()
    {
        Content = "Voor werkstudenten zou een rooster met minstens drie weken voorspelbaarheid veel mentale rust geven.",
        Summary = "Voorspelbaar rooster",
        SubmissionDate = now.AddDays(-4),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[3],
        Youth = students[1],
        SemanticCategories = new[] { "work-life-balance", "study-load" }
    },
    new()
    {
        Content = "Publiceer per vak een duidelijke weekplanning met geschatte studietijd, zodat we beter kunnen inschatten wat haalbaar is.",
        Summary = "Weekplanning met studietijd",
        SubmissionDate = now.AddDays(-3),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[0],
        Youth = students[2],
        SemanticCategories = new[] { "curriculum-design", "study-load", "transparency" }
    },
    new()
    {
        Content = "Introduceer een 'mentaal welzijn buddy' systeem waarbij eerstejaars gekoppeld worden aan ervaren studenten.",
        Summary = "Buddy systeem mentaal welzijn",
        SubmissionDate = now.AddDays(-2),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[0],
        Youth = students[3],
        SemanticCategories = new[] { "study-load", "campus-community", "support-services" }
    },
    new()
    {
        Content = "Zorg voor meer fysieke activiteit op school door werkstations met loopbanden op strategische plekken.",
        Summary = "Loopbanden werkstations",
        SubmissionDate = now.AddDays(-1),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[0],
        Youth = students[0],
        SemanticCategories = new[] { "mental-health", "campus-community", "focus-productivity" }
    },
    new()
    {
        Content = "Maak meditatie en mindfulness apps beschikbaar met korte sessies die passen in lunchmomenten.",
        Summary = "Mindfulness apps beschikbaar",
        SubmissionDate = now.AddDays(1),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[0],
        Youth = students[4],
        SemanticCategories = new[] { "digital-learning", "mental-health", "stress-reduction" }
    },
    new()
    {
        Content = "Organiseer 'stress-free lijstjes' waar studenten hun learning goals kunnen uploaden en feedback krijgen van docenten.",
        Summary = "Stress-free lijstjes feedback",
        SubmissionDate = now.AddDays(2),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[0],
        Youth = students[5],
        SemanticCategories = new[] { "curriculum-design", "study-load", "support-services" }
    },
    new()
    {
        Content = "Creëer anonieme 'stress-o-meter' waar studenten kunnen aangeven hoe ze zich voelen zonder gegevens in te vullen.",
        Summary = "Anonieme stress-o-meter",
        SubmissionDate = now.AddDays(3),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[0],
        Youth = students[6],
        SemanticCategories = new[] { "mental-health", "campus-community", "transparency" }
    },
    new()
    {
        Content = "Bied financiële ondersteuning of kortingen voor mentale gezondheidsapps en wellnessprogramma's voor studenten.",
        Summary = "Financiële ondersteuning wellness",
        SubmissionDate = now.AddDays(4),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[0],
        Youth = students[7],
        SemanticCategories = new[] { "support-services", "mental-health", "work-life-balance" }
    },
    new()
    {
        Content = "Start een studentenresearchproject waarbij jongeren zelf onderzoeken wat hun welzijn verbetert.",
        Summary = "Studentenresearch welzijn",
        SubmissionDate = now.AddDays(5),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[0],
        Youth = students[1],
        SemanticCategories = new[] { "curriculum-design", "campus-community", "transparency" }
    },
    new()
    {
        Content = "Maak doelgerichte workshops beschikbaar over slaaphygiëne, voeding en andere factoren die mentaal welzijn beïnvloeden.",
        Summary = "Workshops slaap en voeding",
        SubmissionDate = now.AddDays(6),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[0],
        Youth = students[2],
        SemanticCategories = new[] { "mental-health", "support-services", "work-life-balance" }
    },
    new()
    {
        Content = "Introduceer een 'mentale gezondheidsweek' elk semester met diverse activiteiten en laagdrempelige hulpaanbiedingen.",
        Summary = "Jaarlijkse mentale gezondheidsweek",
        SubmissionDate = now.AddDays(7),
        Status = ModerationStatus.Approved,
        ModerationInfo = new ModerationInfo(),
        Project = mentaalWelzijnActieplan,
        Topic = topics[0],
        Youth = students[3],
        SemanticCategories = new[] { "mental-health", "campus-community", "support-services" }
    }
};

        context.Ideas.AddRange(ideas);

        var responses = new List<IdeaResponse>
        {
            new()
            {
                Idea = ideas[0],
                Text = "Topidee. Als we op dinsdagavond geen deadlines hebben, helpt dat echt om even op adem te komen.",
                CreatedAt = now.AddDays(-11).AddHours(2),
                Youth = students[3],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[0],
                Text = "Misschien ook aanduiden in het lessenrooster welke week rustiger is, dat maakt plannen makkelijker.",
                CreatedAt = now.AddDays(-11).AddHours(6),
                Youth = students[6],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[1],
                Text = "Ja graag, nu moet je info op drie verschillende pagina's zoeken.",
                CreatedAt = now.AddDays(-10).AddHours(4),
                Youth = students[0],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[2],
                Text = "Peer-support lijkt me sterk, zeker in het eerste semester wanneer iedereen nog zoekt naar ritme.",
                CreatedAt = now.AddDays(-9).AddHours(3),
                Youth = students[5],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[3],
                Text = "Ik haat jullie allemaal, dit is de stomste school ooit. Niemand luistert hier.",
                CreatedAt = now.AddDays(-8).AddHours(1),
                Youth = students[1],
                Status = ModerationStatus.Rejected,
                ModerationInfo = new ModerationInfo { HateAndDiscrimination = true, ViolenceAndThreats = true }
            },
            new()
            {
                Idea = ideas[4],
                Text = "Hou gewoon je bek, niemand zit te wachten op nog meer saaie workshops over mentale gezondheid.",
                CreatedAt = now.AddDays(-7).AddHours(9),
                Youth = students[4],
                Status = ModerationStatus.Rejected,
                ModerationInfo = new ModerationInfo { HateAndDiscrimination = true }
            },
            new()
            {
                Idea = ideas[5],
                Text = "Als ik jou was zou ik mezelf iets aandoen, echt waar. Niemand vindt je aardig.",
                CreatedAt = now.AddDays(-6).AddHours(3),
                Youth = students[7],
                Status = ModerationStatus.Rejected,
                ModerationInfo = new ModerationInfo { SelfHarm = true, HateAndDiscrimination = true }
            },
            new()
            {
                Idea = ideas[6],
                Text = "Stuur me je nummer dan stuur ik je wat leuks 😉 0471 123 456",
                CreatedAt = now.AddDays(-5).AddHours(7),
                Youth = students[2],
                Status = ModerationStatus.Rejected,
                ModerationInfo = new ModerationInfo { Pii = true, Sexual = true }
            },
            new()
            {
                Idea = ideas[3],
                Text = "Die stille ruimtes zouden tijdens blok echt waardevol zijn. Misschien ook korte stretch-momenten tonen op scherm.",
                CreatedAt = now.AddDays(-8).AddHours(5),
                Youth = students[2],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[4],
                Text = "Een eenvoudig formulier met reden en voorkeursmoment zou al genoeg zijn.",
                CreatedAt = now.AddDays(-7).AddHours(4),
                Youth = students[7],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[5],
                Text = "Als er ook opnames of samenvattingen zijn voor wie niet kan komen, bereik je meer studenten.",
                CreatedAt = now.AddDays(-6).AddHours(2),
                Youth = students[4],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[6],
                Text = "Helemaal akkoord, in sommige weken hebben we drie grote deadlines op twee dagen.",
                CreatedAt = now.AddDays(-5).AddHours(7),
                Youth = students[1],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[7],
                Text = "Online study room met camera optioneel zou fijn zijn, dan is de drempel lager.",
                CreatedAt = now.AddDays(-4).AddHours(1),
                Youth = students[6],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = ideas[8],
                Text = "Voor werkstudenten is voorspelbaarheid echt het verschil tussen haalbaar en niet haalbaar.",
                CreatedAt = now.AddDays(-3).AddHours(3),
                Youth = students[0],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            }
        };

        context.Responses.AddRange(responses);

        var reactions = new List<ResponseReaction>
        {
            new() { IdeaResponse = responses[0], Emoji = "🔥", CreatedAt = now.AddDays(-11).AddHours(3), Youth = students[1] },
            new() { IdeaResponse = responses[0], Emoji = "💡", CreatedAt = now.AddDays(-11).AddHours(4), Youth = students[4] },
            new() { IdeaResponse = responses[1], Emoji = "❤️", CreatedAt = now.AddDays(-11).AddHours(7), Youth = students[2] },
            new() { IdeaResponse = responses[2], Emoji = "🙏", CreatedAt = now.AddDays(-10).AddHours(5), Youth = students[5] },
            new() { IdeaResponse = responses[2], Emoji = "😂", CreatedAt = now.AddDays(-10).AddHours(6), Youth = students[7] },
            new() { IdeaResponse = responses[3], Emoji = "🙌", CreatedAt = now.AddDays(-9).AddHours(6), Youth = students[0] },
            new() { IdeaResponse = responses[4], Emoji = "😢", CreatedAt = now.AddDays(-8).AddHours(6), Youth = students[3] },
            new() { IdeaResponse = responses[4], Emoji = "💚", CreatedAt = now.AddDays(-8).AddHours(7), Youth = students[1] },
            new() { IdeaResponse = responses[5], Emoji = "👏", CreatedAt = now.AddDays(-7).AddHours(5), Youth = students[6] },
            new() { IdeaResponse = responses[6], Emoji = "🎯", CreatedAt = now.AddDays(-6).AddHours(3), Youth = students[2] },
            new() { IdeaResponse = responses[6], Emoji = "👍", CreatedAt = now.AddDays(-6).AddHours(4), Youth = students[0] },
            new() { IdeaResponse = responses[7], Emoji = "💯", CreatedAt = now.AddDays(-5).AddHours(8), Youth = students[4] },
            new() { IdeaResponse = responses[8], Emoji = "🧠", CreatedAt = now.AddDays(-4).AddHours(2), Youth = students[5] },
            new() { IdeaResponse = responses[9], Emoji = "✅", CreatedAt = now.AddDays(-3).AddHours(4), Youth = students[3] }
        };

        context.ResponseReactions.AddRange(reactions);

        // ---------------------------------------------------------
        // Case 1b: Hogeschool Nova / Digitale Campus Ervaring
        // ---------------------------------------------------------
        var digitaleCampusProject = new Project
        {
            Name = "Digitale Campus Ervaring 2026",
            Description = "Studenten denken mee over de digitale leeromgeving: van online colleges tot hybride werkvormen en digitale samenwerking.",
            ImageUrl = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=1600&q=80",
            Status = Status.Active,
            StartDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            NudgingStrength = 2,
            InteractionForm = InteractionType.Chat,
            Workspace = hogeschool,
            MinAge = 22,
            MaxAge = 30
        };
        digitaleCampusProject.Id = Slug.FromName(digitaleCampusProject.Name);

        context.Projects.Add(digitaleCampusProject);

        var dcTopics = new List<Topic>
        {
            new() { Name = "Online leerplatforms", Context = "Hoe kunnen we Canvas en andere platforms beter laten aansluiten op studentbehoeften?", Project = digitaleCampusProject },
            new() { Name = "Hybride werkvormen", Context = "Welke mix van online en fysiek onderwijs werkt het beste voor jouw leerstijl?", Project = digitaleCampusProject }
        };

        context.Topics.AddRange(dcTopics);

        var dcStudents = new List<Youth>
        {
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444410"), Email = "lisa@student.nova.be", Project = digitaleCampusProject },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444411"), Email = "tom@student.nova.be", Project = digitaleCampusProject },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444412"), Email = "fatima@student.nova.be", Project = digitaleCampusProject },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444413"), Email = "jens@student.nova.be", Project = digitaleCampusProject }
        };

        context.Youths.AddRange(dcStudents);

        var dcSurveyOnly = new List<Youth>
        {
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444430"), Email = "emma@student.nova.be", Project = digitaleCampusProject },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444431"), Email = "thijs@student.nova.be", Project = digitaleCampusProject },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444432"), Email = "nadia@student.nova.be", Project = digitaleCampusProject }
        };
        context.Youths.AddRange(dcSurveyOnly);

        var dcPlatformQuestion = new SingleChoiceQuestion
        {
            Text = "Hoe tevreden ben je met de huidige digitale leeromgeving?",
            Required = true,
            Project = digitaleCampusProject,
        };
        var dcPlatformSingleChoicePossibleAnswers = new List<Choice>
        {
            new() { Text = "Zeer ontevreden" },
            new() { Text = "Ontevreden" },
            new() { Text = "Neutraal" },
            new() { Text = "Tevreden" },
            new() { Text = "Zeer tevreden" }
        };
        dcPlatformQuestion.PossibleChoices = dcPlatformSingleChoicePossibleAnswers;

        var dcHybridQuestion = new ScaleQuestion
        {
            Text = "Hoe belangrijk vind je de mogelijkheid tot hybride onderwijs? (1 = totaal niet, 10 = extreem belangrijk)",
            Required = true,
            LowerBound = 1,
            UpperBound = 10,
            Project = digitaleCampusProject
        };

        var dcToolQuestion = new SingleChoiceQuestion
        {
            Text = "Welk digitaal hulpmiddel zou je het liefst zien op campus?",
            Required = true,
            Project = digitaleCampusProject,
        };
        var dcToolSingleChoicePossibleAnswers = new List<Choice>
        {
            new() { Text = "Interactieve schermen in leslokalen" },
            new() { Text = "VR/AR leeromgevingen" },
            new() { Text = "AI-gestuurde studie-assistent" },
            new() { Text = "Betere opname- en streamingapparatuur" }
        };
        dcToolQuestion.PossibleChoices = dcToolSingleChoicePossibleAnswers;

        var dcOpenQuestion = new OpenQuestion
        {
            Text = "Wat is jouw grootste frustratie met de huidige digitale tools op school?",
            Required = false,
            Project = digitaleCampusProject
        };

        var dcMultiChoiceQuestion = new MultipleChoiceQuestion
        {
            Text = "Welke extra ondersteuning zou jij willen van de digitale leeromgeving? (meerdere antwoorden mogelijk)",
            Required = false,
            Project = digitaleCampusProject,
        };
        var dcMultiChoicePossibleAnswers = new List<Choice>
        {
            new() { Text = "24/7 online tutoring" },
            new() { Text = "Automatische ondertiteling bij opnames" },
            new() { Text = "Persoonlijk studie-dashboard met voortgang" },
            new() { Text = "Integratie met externe tools (Notion, Google Drive)" },
            new() { Text = "Meertalige interface" }
        };
        dcMultiChoiceQuestion.PossibleChoices = dcMultiChoicePossibleAnswers;

        context.Questions.Add(dcPlatformQuestion);
        context.Questions.Add(dcHybridQuestion);
        context.Questions.Add(dcToolQuestion);
        context.Questions.Add(dcOpenQuestion);
        context.Questions.Add(dcMultiChoiceQuestion);

        context.SaveChanges();

        var dcChoices = new List<Answer>
        {
            new SingleChoiceAnswer { Question = dcPlatformQuestion, Youth = dcStudents[0], Value = dcPlatformSingleChoicePossibleAnswers[2] },
            new Answer<int> { Question = dcHybridQuestion, Youth = dcStudents[0], Value = 8 },
            new SingleChoiceAnswer { Question = dcToolQuestion, Youth = dcStudents[0], Value = dcToolSingleChoicePossibleAnswers[3] },
            new Answer<string> { Question = dcOpenQuestion, Youth = dcStudents[0], Value = "De constante notificaties van Canvas zijn overweldigend. Graag meer controle over wat ik wel en niet ontvang." },

            new SingleChoiceAnswer { Question = dcPlatformQuestion, Youth = dcStudents[1], Value = dcPlatformSingleChoicePossibleAnswers[1] },
            new Answer<int> { Question = dcHybridQuestion, Youth = dcStudents[1], Value = 9 },
            new SingleChoiceAnswer { Question = dcToolQuestion, Youth = dcStudents[1], Value = dcToolSingleChoicePossibleAnswers[2] },
            new Answer<string> { Question = dcOpenQuestion, Youth = dcStudents[1], Value = "Opnamekwaliteit van colleges is ondermaats. Slecht geluid en wazig beeld bij veel opgenomen lessen." },

            new SingleChoiceAnswer { Question = dcPlatformQuestion, Youth = dcStudents[2], Value = dcPlatformSingleChoicePossibleAnswers[3] },
            new Answer<int> { Question = dcHybridQuestion, Youth = dcStudents[2], Value = 6 },
            new SingleChoiceAnswer { Question = dcToolQuestion, Youth = dcStudents[2], Value = dcToolSingleChoicePossibleAnswers[1] },
            new Answer<string> { Question = dcOpenQuestion, Youth = dcStudents[2], Value = "Ik mis een centrale plek waar alle deadlines en taken van verschillende vakken samenkomen. Nu moet ik overal apart kijken." },

            new SingleChoiceAnswer { Question = dcPlatformQuestion, Youth = dcStudents[3], Value = dcPlatformSingleChoicePossibleAnswers[2] },
            new Answer<int> { Question = dcHybridQuestion, Youth = dcStudents[3], Value = 7 },
            new SingleChoiceAnswer { Question = dcToolQuestion, Youth = dcStudents[3], Value = dcToolSingleChoicePossibleAnswers[0] },
            new Answer<string> { Question = dcOpenQuestion, Youth = dcStudents[3], Value = "De wifi op campus is te traag voor grote bestanden. Vooral bij online toetsen is dit stressvol." },

            new SingleChoiceAnswer { Question = dcPlatformQuestion, Youth = dcSurveyOnly[0], Value = dcPlatformSingleChoicePossibleAnswers[4] },
            new Answer<int> { Question = dcHybridQuestion, Youth = dcSurveyOnly[0], Value = 9 },
            new SingleChoiceAnswer { Question = dcToolQuestion, Youth = dcSurveyOnly[0], Value = dcToolSingleChoicePossibleAnswers[2] },
            new Answer<string> { Question = dcOpenQuestion, Youth = dcSurveyOnly[0], Value = "Live ondertiteling bij colleges zou enorm helpen voor internationale studenten." },

            new SingleChoiceAnswer { Question = dcPlatformQuestion, Youth = dcSurveyOnly[1], Value = dcPlatformSingleChoicePossibleAnswers[1] },
            new Answer<int> { Question = dcHybridQuestion, Youth = dcSurveyOnly[1], Value = 5 },
            new SingleChoiceAnswer { Question = dcToolQuestion, Youth = dcSurveyOnly[1], Value = dcToolSingleChoicePossibleAnswers[3] },
            new Answer<string> { Question = dcOpenQuestion, Youth = dcSurveyOnly[1], Value = "Group project tools zijn slecht. Geen goede manier om samen aan documenten te werken op afstand." },

            new SingleChoiceAnswer { Question = dcPlatformQuestion, Youth = dcSurveyOnly[2], Value = dcPlatformSingleChoicePossibleAnswers[3] },
            new Answer<int> { Question = dcHybridQuestion, Youth = dcSurveyOnly[2], Value = 8 },
            new SingleChoiceAnswer { Question = dcToolQuestion, Youth = dcSurveyOnly[2], Value = dcToolSingleChoicePossibleAnswers[0] },
            new Answer<string> { Question = dcOpenQuestion, Youth = dcSurveyOnly[2], Value = "Meer stopcontacten en oplaadpunten in leslokalen. Batterijstress is een ding." },

            new MultipleChoiceAnswer { Question = dcMultiChoiceQuestion, Youth = dcStudents[0], Value = new List<Choice> { dcMultiChoicePossibleAnswers[0] } },
            new MultipleChoiceAnswer { Question = dcMultiChoiceQuestion, Youth = dcStudents[0], Value = new List<Choice> { dcMultiChoicePossibleAnswers[2] } },
            new MultipleChoiceAnswer { Question = dcMultiChoiceQuestion, Youth = dcStudents[1], Value = new List<Choice> { dcMultiChoicePossibleAnswers[1] } },
            new MultipleChoiceAnswer { Question = dcMultiChoiceQuestion, Youth = dcStudents[1], Value = new List<Choice> { dcMultiChoicePossibleAnswers[3] } },
            new MultipleChoiceAnswer { Question = dcMultiChoiceQuestion, Youth = dcStudents[2], Value = new List<Choice> { dcMultiChoicePossibleAnswers[0] } },
            new MultipleChoiceAnswer { Question = dcMultiChoiceQuestion, Youth = dcStudents[2], Value = new List<Choice> { dcMultiChoicePossibleAnswers[1] } },
            new MultipleChoiceAnswer { Question = dcMultiChoiceQuestion, Youth = dcStudents[2], Value = new List<Choice> { dcMultiChoicePossibleAnswers[4] } },
            new MultipleChoiceAnswer { Question = dcMultiChoiceQuestion, Youth = dcStudents[3], Value = new List<Choice> { dcMultiChoicePossibleAnswers[2] } },
            new MultipleChoiceAnswer { Question = dcMultiChoiceQuestion, Youth = dcSurveyOnly[0], Value = new List<Choice> { dcMultiChoicePossibleAnswers[0] } },
            new MultipleChoiceAnswer { Question = dcMultiChoiceQuestion, Youth = dcSurveyOnly[0], Value = new List<Choice> { dcMultiChoicePossibleAnswers[1] } },
            new MultipleChoiceAnswer { Question = dcMultiChoiceQuestion, Youth = dcSurveyOnly[0], Value = new List<Choice> { dcMultiChoicePossibleAnswers[3] } },
            new MultipleChoiceAnswer { Question = dcMultiChoiceQuestion, Youth = dcSurveyOnly[1], Value = new List<Choice> { dcMultiChoicePossibleAnswers[2] } },
            new MultipleChoiceAnswer { Question = dcMultiChoiceQuestion, Youth = dcSurveyOnly[1], Value = new List<Choice> { dcMultiChoicePossibleAnswers[4] } },
            new MultipleChoiceAnswer { Question = dcMultiChoiceQuestion, Youth = dcSurveyOnly[2], Value = new List<Choice> { dcMultiChoicePossibleAnswers[0] } },
            new MultipleChoiceAnswer { Question = dcMultiChoiceQuestion, Youth = dcSurveyOnly[2], Value = new List<Choice> { dcMultiChoicePossibleAnswers[2] } },
            new MultipleChoiceAnswer { Question = dcMultiChoiceQuestion, Youth = dcSurveyOnly[2], Value = new List<Choice> { dcMultiChoicePossibleAnswers[3] } }
        };

        context.Answers.AddRange(dcChoices);
        context.SaveChanges();

        var dcIdeas = new List<Idea>
        {
            new() { Content = "Een app die alle roosters, deadlines en cijfers per vak in één overzicht toont. Geintegreerd met Canvas en Outlook.", Summary = "Gecentraliseerde studentenapp", SubmissionDate = now.AddDays(-20), Status = ModerationStatus.Approved, Project = digitaleCampusProject, Topic = dcTopics[0], Youth = dcStudents[0], SemanticCategories = new[] { "digital-tools", "ux" } },
            new() { Content = "Verplichte opname van ALLE colleges, niet alleen hoorcolleges. Ook werkcolleges moeten beschikbaar zijn voor terugkijken.", Summary = "Verplichte college-opnames", SubmissionDate = now.AddDays(-18), Status = ModerationStatus.Approved, Project = digitaleCampusProject, Topic = dcTopics[0], Youth = dcStudents[1], SemanticCategories = new[] { "accessibility", "online-learning" } },
            new() { Content = "Optionele online deelname aan alle lessen. Studenten kunnen kiezen: fysiek of via livestream. Dit helpt bij ziekte en reistijd.", Summary = "Optie hybride aanwezigheid", SubmissionDate = now.AddDays(-15), Status = ModerationStatus.Approved, Project = digitaleCampusProject, Topic = dcTopics[1], Youth = dcStudents[2], SemanticCategories = new[] { "hybrid", "flexibility" } },
            new() { Content = "AI chatbot voor studentenvragen: 24/7 vragen stellen over roosters, deadlines, en veelgestelde vragen. Ontlast de docenten.", Summary = "AI chatbot support", SubmissionDate = now.AddDays(-14), Status = ModerationStatus.Approved, Project = digitaleCampusProject, Topic = dcTopics[0], Youth = dcStudents[3], SemanticCategories = new[] { "ai", "student-support" } },
            new() { Content = "Deze school is kut en iedereen hier is dom.", Summary = "Toxische comment", SubmissionDate = now.AddDays(-10), Status = ModerationStatus.Rejected, Project = digitaleCampusProject, Topic = dcTopics[0], Youth = dcStudents[0], SemanticCategories = Array.Empty<string>(), ModerationInfo = new ModerationInfo { HateAndDiscrimination = true } },
            new() { Content = "Verhoogde beveiliging moet gewoon echt, zoveel messen op school.", Summary = "Gewelddadige suggestie", SubmissionDate = now.AddDays(-8), Status = ModerationStatus.Rejected, Project = digitaleCampusProject, Topic = dcTopics[1], Youth = dcStudents[1], SemanticCategories = Array.Empty<string>(), ModerationInfo = new ModerationInfo { ViolenceAndThreats = true } },
            new() { Content = "Online samenwerkingsruimtes per vakgroep waar studenten samen aan opdrachten kunnen werken met gedeelde whiteboards en documenten.", Summary = "Online samenwerkingsruimtes", SubmissionDate = now.AddDays(-12), Status = ModerationStatus.Approved, Project = digitaleCampusProject, Topic = dcTopics[1], Youth = dcStudents[2], SemanticCategories = new[] { "collaboration", "digital-tools" } },
            new() { Content = "Een peer-review systeem waarbij studenten elkaars werk beoordelen met rubrics. Dit bespaart docententijd en leert studenten kritisch kijken.", Summary = "Peer-review platform", SubmissionDate = now.AddDays(-7), Status = ModerationStatus.Approved, Project = digitaleCampusProject, Topic = dcTopics[0], Youth = dcStudents[3], SemanticCategories = new[] { "assessment", "peer-learning" } }
        };

        context.Ideas.AddRange(dcIdeas);

        // =====================================================
        // Case 2: Stad Linden / Jong in een Groene Stad
        // =====================================================
        var stadLinden = new Workspace
        {
            Name = "Stad Linden"
        };
        stadLinden.Id = Slug.FromName(stadLinden.Name);

        var vergroeningEnRecreatiePlan = new Project
        {
            Name = "Jong in een Groene Stad 2026-2028",
            Description = "Stad Linden betrekt jongeren van 18 tot 30 actief bij keuzes rond vergroening, klimaatmaatregelen en verdeling van stedelijke recreatie.",
            ImageUrl = "https://images.unsplash.com/photo-1473448912268-2022ce9509d8?auto=format&fit=crop&w=1600&q=80",
            Status = Status.Active,
            StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            NudgingStrength = 4,
            InteractionForm = InteractionType.UserDefined,
            Workspace = stadLinden,
            MinAge = 18,
            MaxAge = 30
        };
        vergroeningEnRecreatiePlan.Id = Slug.FromName(vergroeningEnRecreatiePlan.Name);

        context.Workspaces.Add(stadLinden);
        context.Projects.Add(vergroeningEnRecreatiePlan);

        var cityTopics = new List<Topic>
        {
            new()
            {
                Name = "Vergroening van buurten",
                Context = "Welke straten en wijken hebben volgens jongeren dringend meer groen, schaduw en klimaatbestendige inrichting nodig?",
                Project = vergroeningEnRecreatiePlan
            },
            new()
            {
                Name = "Recreatie en publieke ruimte",
                Context = "Hoe kunnen pleinen, parken en ontmoetingsplekken beter verdeeld en toegankelijk gemaakt worden voor jongeren?",
                Project = vergroeningEnRecreatiePlan
            },
            new()
            {
                Name = "Verkeersveiligheid en leefbaarheid",
                Context = "Welke ingrepen maken buurten veiliger, rustiger en aantrekkelijker voor jongeren die zich te voet of met de fiets verplaatsen?",
                Project = vergroeningEnRecreatiePlan
            },
            new()
            {
                Name = "Jongerenparticipatie in beleid",
                Context = "Hoe wil de doelgroep betrokken worden bij keuzes, prioriteiten en communicatie rond klimaat en leefomgeving?",
                Project = vergroeningEnRecreatiePlan
            },
            new()
            {
                Name = "Lokale klimaatacties",
                Context = "Welke concrete maatregelen kunnen stad en jongeren samen opnemen om de stad duurzamer te maken?",
                Project = vergroeningEnRecreatiePlan
            }
        };

        context.Topics.AddRange(cityTopics);

        var cityYouths = new List<Youth>
        {
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555551"), Email = "juna@stadlinden.be", Project = vergroeningEnRecreatiePlan },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555552"), Email = "faris@stadlinden.be", Project = vergroeningEnRecreatiePlan },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555553"), Email = "nora@stadlinden.be", Project = vergroeningEnRecreatiePlan },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555554"), Email = "daan@stadlinden.be", Project = vergroeningEnRecreatiePlan },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Email = "ayla@stadlinden.be", Project = vergroeningEnRecreatiePlan },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555556"), Email = "bram@stadlinden.be", Project = vergroeningEnRecreatiePlan },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555557"), Email = "ines@stadlinden.be", Project = vergroeningEnRecreatiePlan },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555558"), Email = "yara@stadlinden.be", Project = vergroeningEnRecreatiePlan }
        };

        context.Youths.AddRange(cityYouths);
        
        var citySurveyOnly = new List<Youth>
        {
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555520"), Email = null, Project = vergroeningEnRecreatiePlan },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555521"), Email = "femke@stadlinden.be", Project = vergroeningEnRecreatiePlan },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555522"), Email = null, Project = vergroeningEnRecreatiePlan },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555523"), Email = "lies@stadlinden.be", Project = vergroeningEnRecreatiePlan }
        };
        context.Youths.AddRange(citySurveyOnly);
        
        var cityGreenPriorityQuestion = new SingleChoiceQuestion
        {
            Text = "Welke prioriteit moet de stad eerst aanpakken?",
            Required = true,
            Project = vergroeningEnRecreatiePlan,
        };
        var cityGreenPriorityChoices = new List<Choice>
        {
            new() { Text = "Meer bomen en schaduw", Question = cityGreenPriorityQuestion },
            new() { Text = "Veiligere fiets- en wandelroutes", Question = cityGreenPriorityQuestion },
            new() { Text = "Meer jeugdvriendelijke ontmoetingsplekken", Question = cityGreenPriorityQuestion },
            new() { Text = "Meer inspraak voor jongeren", Question = cityGreenPriorityQuestion }
        };
        cityGreenPriorityQuestion.PossibleChoices = cityGreenPriorityChoices;

        var cityLeefbaarheidScaleQuestion = new ScaleQuestion
        {
            Text = "Hoe leefbaar ervaar je je buurt vandaag? (1 = zeer slecht, 10 = zeer goed)",
            Required = true,
            LowerBound = 1,
            UpperBound = 10,
            Project = vergroeningEnRecreatiePlan
        };

        var cityParticipationQuestion = new SingleChoiceQuestion
        {
            Text = "Op welke manier wil je het liefst betrokken worden bij stadsbeleid?",
            Required = true,
            Project = vergroeningEnRecreatiePlan,
        };
        var cityParticipationChoices = new List<Choice>
        {
            new() { Text = "Online bevragingen", Question = cityParticipationQuestion },
            new() { Text = "Kwartaalpanel met stadsbestuur", Question = cityParticipationQuestion },
            new() { Text = "Workshops in de buurt", Question = cityParticipationQuestion },
            new() { Text = "Ik wil niet actief deelnemen", Question = cityParticipationQuestion }
        };
        cityParticipationQuestion.PossibleChoices = cityParticipationChoices;

        var cityOpenQuestion = new OpenQuestion
        {
            Text = "Welke plek in Stad Linden zou jij als eerste vergroenen en waarom?",
            Required = false,
            Project = vergroeningEnRecreatiePlan
        };

        var cityQuestions = new List<Question>
        {
            cityGreenPriorityQuestion,
            cityParticipationQuestion,
        };
        context.Questions.AddRange(cityQuestions);

        var citySurveyAnswers = new List<Answer>
        {
            new SingleChoiceAnswer { Question = cityGreenPriorityQuestion, Youth = cityYouths[0], Value = cityGreenPriorityChoices[0] },
            new Answer<int> { Question = cityLeefbaarheidScaleQuestion, Youth = cityYouths[0], Value = 5 },
            new SingleChoiceAnswer { Question = cityParticipationQuestion, Youth = cityYouths[0], Value = cityParticipationChoices[1] },
            new Answer<string> { Question = cityOpenQuestion, Youth = cityYouths[0], Value = "Het Stationsplein: daar is te weinig schaduw en bijna geen groene zitruimte." },

            new SingleChoiceAnswer { Question = cityGreenPriorityQuestion, Youth = cityYouths[3], Value = cityGreenPriorityChoices[1] },
            new Answer<int> { Question = cityLeefbaarheidScaleQuestion, Youth = cityYouths[3], Value = 6 },
            new SingleChoiceAnswer { Question = cityParticipationQuestion, Youth = cityYouths[3], Value = cityParticipationChoices[2] },
            new Answer<string> { Question = cityOpenQuestion, Youth = cityYouths[3], Value = "Rond de campusroute: vooral voor veiligere, groene fietsverbindingen." },

            new SingleChoiceAnswer { Question = cityGreenPriorityQuestion, Youth = cityYouths[6], Value = cityGreenPriorityChoices[2] },
            new Answer<int> { Question = cityLeefbaarheidScaleQuestion, Youth = cityYouths[6], Value = 4 },
            new SingleChoiceAnswer { Question = cityParticipationQuestion, Youth = cityYouths[6], Value = cityParticipationChoices[0] },
            new Answer<string> { Question = cityOpenQuestion, Youth = cityYouths[6], Value = "Een braakliggend terrein in Noordwijk, als tijdelijke pop-up groene ontmoetingsplek." },
            new SingleChoiceAnswer { Question = cityParticipationQuestion, Youth = cityYouths[6], Value = cityParticipationChoices[0] },
            new Answer<string> { Question = cityOpenQuestion, Youth = cityYouths[6], Value = "Een braakliggend terrein in Noordwijk, als tijdelijke pop-up groene ontmoetingsplek." },

            new SingleChoiceAnswer { Question = cityGreenPriorityQuestion, Youth = cityYouths[1], Value = cityGreenPriorityChoices[3] },
            new Answer<int> { Question = cityLeefbaarheidScaleQuestion, Youth = cityYouths[1], Value = 7 },
            new SingleChoiceAnswer { Question = cityParticipationQuestion, Youth = cityYouths[1], Value = cityParticipationChoices[0] },
            new Answer<string> { Question = cityOpenQuestion, Youth = cityYouths[1], Value = "De oude spoorzone: ideaal voor een groen stadspark met skate- en chillplekken." },

            new SingleChoiceAnswer { Question = cityGreenPriorityQuestion, Youth = cityYouths[2], Value = cityGreenPriorityChoices[0] },
            new Answer<int> { Question = cityLeefbaarheidScaleQuestion, Youth = cityYouths[2], Value = 3 },
            new SingleChoiceAnswer { Question = cityParticipationQuestion, Youth = cityYouths[2], Value = cityParticipationChoices[1] },
            new Answer<string> { Question = cityOpenQuestion, Youth = cityYouths[2], Value = "Buurtmoestuinen waar jongeren groenten leren kweken en verkopen op de markt." },

            new SingleChoiceAnswer { Question = cityGreenPriorityQuestion, Youth = cityYouths[4], Value = cityGreenPriorityChoices[2] },
            new Answer<int> { Question = cityLeefbaarheidScaleQuestion, Youth = cityYouths[4], Value = 8 },
            new SingleChoiceAnswer { Question = cityParticipationQuestion, Youth = cityYouths[4], Value = cityParticipationChoices[0] },
            new Answer<string> { Question = cityOpenQuestion, Youth = cityYouths[4], Value = "Meer openbare watertappunten en schaduwplekken bij sportvelden en basketbalveldjes." },

            new SingleChoiceAnswer { Question = cityGreenPriorityQuestion, Youth = cityYouths[5], Value = cityGreenPriorityChoices[1] },
            new Answer<int> { Question = cityLeefbaarheidScaleQuestion, Youth = cityYouths[5], Value = 6 },
            new SingleChoiceAnswer { Question = cityParticipationQuestion, Youth = cityYouths[5], Value = cityParticipationChoices[2] },
            new Answer<string> { Question = cityOpenQuestion, Youth = cityYouths[5], Value = "Veiligere oversteekplaatsen rond scholen met groene middenbermen." },

            new SingleChoiceAnswer { Question = cityGreenPriorityQuestion, Youth = cityYouths[7], Value = cityGreenPriorityChoices[0] },
            new Answer<int> { Question = cityLeefbaarheidScaleQuestion, Youth = cityYouths[7], Value = 5 },
            new SingleChoiceAnswer { Question = cityParticipationQuestion, Youth = cityYouths[7], Value = cityParticipationChoices[1] },
            new Answer<string> { Question = cityOpenQuestion, Youth = cityYouths[7], Value = "Zet leegstaande panden tijdelijk om in jongerenhuiskamers met groen dakterras." },

            new SingleChoiceAnswer { Question = cityGreenPriorityQuestion, Youth = citySurveyOnly[0], Value = cityGreenPriorityChoices[1] },
            new Answer<int> { Question = cityLeefbaarheidScaleQuestion, Youth = citySurveyOnly[0], Value = 6 },
            new SingleChoiceAnswer { Question = cityParticipationQuestion, Youth = citySurveyOnly[0], Value = cityParticipationChoices[0] },
            new Answer<string> { Question = cityOpenQuestion, Youth = citySurveyOnly[0], Value = "Meer zitbanken langs wandelroutes voor ouderen en minder mobiele mensen." },

            new SingleChoiceAnswer { Question = cityGreenPriorityQuestion, Youth = citySurveyOnly[1], Value = cityGreenPriorityChoices[3] },
            new Answer<int> { Question = cityLeefbaarheidScaleQuestion, Youth = citySurveyOnly[1], Value = 4 },
            new SingleChoiceAnswer { Question = cityParticipationQuestion, Youth = citySurveyOnly[1], Value = cityParticipationChoices[1] },
            new Answer<string> { Question = cityOpenQuestion, Youth = citySurveyOnly[1], Value = "Een vast jongerenpanel dat de gemeente adviseert over groenprojecten." },

            new SingleChoiceAnswer { Question = cityGreenPriorityQuestion, Youth = citySurveyOnly[2], Value = cityGreenPriorityChoices[0] },
            new Answer<int> { Question = cityLeefbaarheidScaleQuestion, Youth = citySurveyOnly[2], Value = 7 },
            new SingleChoiceAnswer { Question = cityParticipationQuestion, Youth = citySurveyOnly[2], Value = cityParticipationChoices[2] },
            new Answer<string> { Question = cityOpenQuestion, Youth = citySurveyOnly[2], Value = "Bomen planten langs alle hoofdfietsroutes voor schaduw en koelte." },

            new SingleChoiceAnswer { Question = cityGreenPriorityQuestion, Youth = citySurveyOnly[3], Value = cityGreenPriorityChoices[2] },
            new Answer<int> { Question = cityLeefbaarheidScaleQuestion, Youth = citySurveyOnly[3], Value = 5 },
            new SingleChoiceAnswer { Question = cityParticipationQuestion, Youth = citySurveyOnly[3], Value = cityParticipationChoices[0] },
            new Answer<string> { Question = cityOpenQuestion, Youth = citySurveyOnly[3], Value = "Wijkbudgetten voor jongeren zodat ze zelf groenprojecten kunnen starten." }
        };
        context.Answers.AddRange(citySurveyAnswers);

        var cityIdeas = new List<Idea>
        {
            new()
            {
                Content = "Maak van het Stationsplein een hittebestendig plein met extra bomen, zitbanken in de schaduw en een gratis drinkwaterpunt.",
                Summary = "Stationsplein vergroenen en verkoelen",
                SubmissionDate = now.AddDays(-9),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = vergroeningEnRecreatiePlan,
                Topic = cityTopics[0],
                Youth = cityYouths[0],
                SemanticCategories = new[] { "urban-greenery", "public-space", "climate-action" }
            },
            new()
            {
                Content = "Voorzie in elk stadsdeel minstens een jeugdvriendelijke ontmoetingszone waar je veilig kan chillen zonder iets te moeten kopen.",
                Summary = "Jeugdvriendelijke ontmoetingszones",
                SubmissionDate = now.AddDays(-8),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = vergroeningEnRecreatiePlan,
                Topic = cityTopics[1],
                Youth = cityYouths[1],
                SemanticCategories = new[] { "public-space", "recreation", "youth-participation" }
            },
            new()
            {
                Content = "Voer autoluwe school- en campusstraten in tijdens piekuren zodat fietsen en stappen veiliger wordt voor jongeren.",
                Summary = "Autoluwe piekuren rond scholen",
                SubmissionDate = now.AddDays(-7),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = vergroeningEnRecreatiePlan,
                Topic = cityTopics[2],
                Youth = cityYouths[2],
                SemanticCategories = new[] { "mobility-safety", "climate-action", "public-space" }
            },
            new()
            {
                Content = "Start een jongerenklimaatpanel dat elk kwartaal samenkomt met schepenen om prioriteiten in het klimaatplan te bespreken.",
                Summary = "Jongerenklimaatpanel",
                SubmissionDate = now.AddDays(-6),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = vergroeningEnRecreatiePlan,
                Topic = cityTopics[3],
                Youth = cityYouths[3],
                SemanticCategories = new[] { "youth-participation", "governance", "climate-action" }
            },
            new()
            {
                Content = "Lanceer een subsidie voor geveltuintjes en mini-buurttuinen zodat jongeren zelf hun straat mee groener kunnen maken.",
                Summary = "Subsidie voor geveltuintjes",
                SubmissionDate = now.AddDays(-5),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = vergroeningEnRecreatiePlan,
                Topic = cityTopics[4],
                Youth = cityYouths[4],
                SemanticCategories = new[] { "urban-greenery", "climate-action", "youth-participation" }
            },
            new()
            {
                Content = "Gebruik braakliggende terreinen tijdelijk als pop-up sport en recreatiezones met veel groen, vooral in drukke wijken.",
                Summary = "Pop-up sport en groen",
                SubmissionDate = now.AddDays(-4),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = vergroeningEnRecreatiePlan,
                Topic = cityTopics[1],
                Youth = cityYouths[5],
                SemanticCategories = new[] { "recreation", "public-space", "urban-greenery" }
            },
            new()
            {
                Content = "Maak een interactieve kaart waarop jongeren gevaarlijke verkeerspunten en kansen voor vergroening kunnen pinnen.",
                Summary = "Interactieve knelpuntenkaart",
                SubmissionDate = now.AddDays(-3),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = vergroeningEnRecreatiePlan,
                Topic = cityTopics[3],
                Youth = cityYouths[6],
                SemanticCategories = new[] { "digital-tools", "mobility-safety", "youth-participation" }
            },
            new()
            {
                Content = "Plant meer avondvriendelijke groene routes met betere verlichting, zodat parken en verbindingen ook na zonsondergang veilig aanvoelen.",
                Summary = "Veilige groene avondroutes",
                SubmissionDate = now.AddDays(-2),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = vergroeningEnRecreatiePlan,
                Topic = cityTopics[2],
                Youth = cityYouths[7],
                SemanticCategories = new[] { "mobility-safety", "urban-greenery", "public-space" }
            }
        };

        context.Ideas.AddRange(cityIdeas);

        var cityResponses = new List<IdeaResponse>
        {
            new()
            {
                Idea = cityIdeas[0],
                Text = "Helemaal mee eens, op warme dagen is dat plein nu bijna niet bruikbaar in de namiddag.",
                CreatedAt = now.AddDays(-8).AddHours(2),
                Youth = cityYouths[2],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = cityIdeas[1],
                Text = "Zeker in het centrum missen we plekken waar je gewoon kan zitten zonder verplicht iets te bestellen.",
                CreatedAt = now.AddDays(-7).AddHours(5),
                Youth = cityYouths[0],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = cityIdeas[2],
                Text = "Autoluwe momenten rond de campus zouden echt een groot verschil maken voor fietsveiligheid.",
                CreatedAt = now.AddDays(-6).AddHours(1),
                Youth = cityYouths[5],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = cityIdeas[3],
                Text = "Als de stad de feedback van zo'n panel ook zichtbaar terugkoppelt, gaan meer jongeren deelnemen.",
                CreatedAt = now.AddDays(-5).AddHours(4),
                Youth = cityYouths[4],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = cityIdeas[4],
                Text = "Top. Met een eenvoudig stappenplan kunnen veel jongeren meteen starten in hun straat.",
                CreatedAt = now.AddDays(-4).AddHours(2),
                Youth = cityYouths[7],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = cityIdeas[5],
                Text = "Pop-up zones zijn ideaal, zeker als er ook rustige groene hoekjes zijn naast sportruimte.",
                CreatedAt = now.AddDays(-3).AddHours(6),
                Youth = cityYouths[1],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = cityIdeas[6],
                Text = "Die kaart zou ook handig zijn voor meldingen van kapotte bankjes of te weinig schaduwplekken.",
                CreatedAt = now.AddDays(-2).AddHours(7),
                Youth = cityYouths[3],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = cityIdeas[7],
                Text = "Veiligere avondroutes zouden het voor veel jongeren makkelijker maken om zich duurzaam te verplaatsen.",
                CreatedAt = now.AddDays(-1).AddHours(3),
                Youth = cityYouths[6],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            }
        };

        context.Responses.AddRange(cityResponses);

        var cityReactions = new List<ResponseReaction>
        {
            new() { IdeaResponse = cityResponses[0], Emoji = "🔥", CreatedAt = now.AddDays(-8).AddHours(3), Youth = cityYouths[1] },
            new() { IdeaResponse = cityResponses[0], Emoji = "💡", CreatedAt = now.AddDays(-8).AddHours(4), Youth = cityYouths[4] },
            new() { IdeaResponse = cityResponses[1], Emoji = "❤️", CreatedAt = now.AddDays(-7).AddHours(6), Youth = cityYouths[2] },
            new() { IdeaResponse = cityResponses[2], Emoji = "🙏", CreatedAt = now.AddDays(-6).AddHours(2), Youth = cityYouths[0] },
            new() { IdeaResponse = cityResponses[2], Emoji = "👍", CreatedAt = now.AddDays(-6).AddHours(3), Youth = cityYouths[7] },
            new() { IdeaResponse = cityResponses[3], Emoji = "🙌", CreatedAt = now.AddDays(-5).AddHours(5), Youth = cityYouths[5] },
            new() { IdeaResponse = cityResponses[4], Emoji = "✅", CreatedAt = now.AddDays(-4).AddHours(3), Youth = cityYouths[3] },
            new() { IdeaResponse = cityResponses[5], Emoji = "👏", CreatedAt = now.AddDays(-3).AddHours(7), Youth = cityYouths[6] },
            new() { IdeaResponse = cityResponses[6], Emoji = "🧠", CreatedAt = now.AddDays(-2).AddHours(8), Youth = cityYouths[4] },
            new() { IdeaResponse = cityResponses[7], Emoji = "🎯", CreatedAt = now.AddDays(-1).AddHours(4), Youth = cityYouths[2] }
        };

        context.ResponseReactions.AddRange(cityReactions);

        // =====================================================
        // Case 3: College Nova / Mental Well-being Action Plan
        // =====================================================
        var collegeNova = new Workspace
        {
            Name = "College Nova"
        };
        collegeNova.Id = Slug.FromName(collegeNova.Name);

        var mentalWellbeingActionPlan = new Project
        {
            Name = "Mental Well-being Action Plan 2026-2027",
            Description = "Together with students, we are developing an action plan that strengthens mental well-being on campus, in classes, and in guidance.",
            ImageUrl = "https://images.unsplash.com/photo-1523240795612-9a054b0db644?auto=format&fit=crop&w=1600&q=80",
            Status = Status.Active,
            StartDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 6, 30, 0, 0, 0, DateTimeKind.Utc),
            NudgingStrength = 3,
            InteractionForm = InteractionType.UserDefined,
            Workspace = collegeNova,
            MinAge = 18,
            MaxAge = 26
        };
        mentalWellbeingActionPlan.Id = Slug.FromName(mentalWellbeingActionPlan.Name);

        context.Workspaces.Add(collegeNova);
        context.Projects.Add(mentalWellbeingActionPlan);

        var collegeTopics = new List<Topic>
        {
            new()
            {
                Name = "Study pressure and evaluation",
                Context = "How can deadlines, exams, and feedback be better aligned with manageable study pressure?",
                Project = mentalWellbeingActionPlan
            },
            new()
            {
                Name = "Accessible support",
                Context = "What support do students expect from academic guidance, student psychologists, and teachers?",
                Project = mentalWellbeingActionPlan
            },
            new()
            {
                Name = "Safe and connected campus",
                Context = "How can we create more connectedness and a safe atmosphere on and around campus?",
                Project = mentalWellbeingActionPlan
            },
            new()
            {
                Name = "Study-work-life balance",
                Context = "What actions help students combine study with work, family, and relaxation?",
                Project = mentalWellbeingActionPlan
            },
            new()
            {
                Name = "Digital and hybrid learning environment",
                Context = "How can we make online and hybrid learning more mentally manageable and socially supportive?",
                Project = mentalWellbeingActionPlan
            }
        };

        context.Topics.AddRange(collegeTopics);

        var collegeStudents = new List<Youth>
        {
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666661"), Email = "amelie@student.collegenova.edu", Project = mentalWellbeingActionPlan },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666662"), Email = "younes@student.collegenova.edu", Project = mentalWellbeingActionPlan },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666663"), Email = "lotte@student.collegenova.edu", Project = mentalWellbeingActionPlan },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666664"), Email = "milan@student.collegenova.edu", Project = mentalWellbeingActionPlan },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666665"), Email = "sarah@student.collegenova.edu", Project = mentalWellbeingActionPlan },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Email = "noah@student.collegenova.edu", Project = mentalWellbeingActionPlan },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666667"), Email = "zineb@student.collegenova.edu", Project = mentalWellbeingActionPlan },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666668"), Email = "ruben@student.collegenova.edu", Project = mentalWellbeingActionPlan }
        };

        context.Youths.AddRange(collegeStudents);
        
        var collegeSurveyOnly = new List<Youth>
        {
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666620"), Email = null, Project = mentalWellbeingActionPlan },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666621"), Email = null, Project = mentalWellbeingActionPlan },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666622"), Email = "priya@student.collegenova.edu", Project = mentalWellbeingActionPlan }
        };
        context.Youths.AddRange(collegeSurveyOnly);
        
        var collegeSupportQuestion = new SingleChoiceQuestion
        {
            Text = "How do you rate the accessibility of mental support on campus?",
            Required = true,
            Project = mentalWellbeingActionPlan,
        };
        var collegeSupportChoices = new List<Choice>
        {
            new() { Text = "Completely insufficient", Question = collegeSupportQuestion },
            new() { Text = "Rather insufficient", Question = collegeSupportQuestion },
            new() { Text = "Sufficient", Question = collegeSupportQuestion },
            new() { Text = "Good", Question = collegeSupportQuestion },
            new() { Text = "Excellent", Question = collegeSupportQuestion }
        };
        collegeSupportQuestion.PossibleChoices = collegeSupportChoices;

        var collegeStressScaleQuestion = new ScaleQuestion
        {
            Text = "How much stress do you experience on average during a study week? (1 = very low, 10 = very high)",
            Required = true,
            LowerBound = 1,
            UpperBound = 10,
            Project = mentalWellbeingActionPlan
        };

        var collegeFlexQuestion = new SingleChoiceQuestion
        {
            Text = "Would you use flexible catch-up moments when overloaded?",
            Required = true,
            Project = mentalWellbeingActionPlan,
        };
        var collegeFlexChoices = new List<Choice>
        {
            new() { Text = "Yes, definitely", Question = collegeFlexQuestion },
            new() { Text = "Maybe, depending on the subject", Question = collegeFlexQuestion },
            new() { Text = "No", Question = collegeFlexQuestion }
        };
        collegeFlexQuestion.PossibleChoices = collegeFlexChoices;

        var collegeOpenQuestion = new OpenQuestion
        {
            Text = "What concrete action would make the biggest difference for mental well-being at school?",
            Required = false,
            Project = mentalWellbeingActionPlan
        };

        var collegeQuestions = new List<Question>
        {
            collegeSupportQuestion,
            collegeStressScaleQuestion,
            collegeFlexQuestion,
            collegeOpenQuestion
        };
        context.Questions.AddRange(collegeQuestions);

        var collegeAnswers = new List<Answer>
        {
            new SingleChoiceAnswer { Question = collegeSupportQuestion, Youth = collegeStudents[0], Value = collegeSupportChoices[2] },
            new Answer<int> { Question = collegeStressScaleQuestion, Youth = collegeStudents[0], Value = 7 },
            new SingleChoiceAnswer { Question = collegeFlexQuestion, Youth = collegeStudents[0], Value = collegeFlexChoices[0] },
            new Answer<string> { Question = collegeOpenQuestion, Youth = collegeStudents[0], Value = "A weekly deadline-free evening per program would immediately reduce stress." },

            new SingleChoiceAnswer { Question = collegeSupportQuestion, Youth = collegeStudents[3], Value = collegeSupportChoices[1] },
            new Answer<int> { Question = collegeStressScaleQuestion, Youth = collegeStudents[3], Value = 8 },
            new SingleChoiceAnswer { Question = collegeFlexQuestion, Youth = collegeStudents[3], Value = collegeFlexChoices[1] },
            new Answer<string> { Question = collegeOpenQuestion, Youth = collegeStudents[3], Value = "Make guidance more visible on one central well-being page." },

            new SingleChoiceAnswer { Question = collegeSupportQuestion, Youth = collegeStudents[6], Value = collegeSupportChoices[3] },
            new Answer<int> { Question = collegeStressScaleQuestion, Youth = collegeStudents[6], Value = 6 },
            new SingleChoiceAnswer { Question = collegeFlexQuestion, Youth = collegeStudents[6], Value = collegeFlexChoices[0] },
            new Answer<string> { Question = collegeOpenQuestion, Youth = collegeStudents[6], Value = "Offer more quiet spaces with short relaxation exercises during peak weeks." },
            
            new SingleChoiceAnswer { Question = collegeSupportQuestion, Youth = collegeStudents[1], Value = collegeSupportChoices[3] },
            new Answer<int> { Question = collegeStressScaleQuestion, Youth = collegeStudents[1], Value = 8 },
            new SingleChoiceAnswer { Question = collegeFlexQuestion, Youth = collegeStudents[1], Value = collegeFlexChoices[1] },
            new Answer<string> { Question = collegeOpenQuestion, Youth = collegeStudents[1], Value = "Mandatory mental health check-ins with a counselor at least once per semester." },

            new SingleChoiceAnswer { Question = collegeSupportQuestion, Youth = collegeStudents[2], Value = collegeSupportChoices[1] },
            new Answer<int> { Question = collegeStressScaleQuestion, Youth = collegeStudents[2], Value = 5 },
            new SingleChoiceAnswer { Question = collegeFlexQuestion, Youth = collegeStudents[2], Value = collegeFlexChoices[2] },
            new Answer<string> { Question = collegeOpenQuestion, Youth = collegeStudents[2], Value = "Train professors to recognize burnout signs in students before exams." },

            new SingleChoiceAnswer { Question = collegeSupportQuestion, Youth = collegeStudents[4], Value = collegeSupportChoices[2] },
            new Answer<int> { Question = collegeStressScaleQuestion, Youth = collegeStudents[4], Value = 7 },
            new SingleChoiceAnswer { Question = collegeFlexQuestion, Youth = collegeStudents[4], Value = collegeFlexChoices[0] },
            new Answer<string> { Question = collegeOpenQuestion, Youth = collegeStudents[4], Value = "Spread exams across the entire semester instead of cramming everything into two weeks." },

            new SingleChoiceAnswer { Question = collegeSupportQuestion, Youth = collegeStudents[5], Value = collegeSupportChoices[4] },
            new Answer<int> { Question = collegeStressScaleQuestion, Youth = collegeStudents[5], Value = 9 },
            new SingleChoiceAnswer { Question = collegeFlexQuestion, Youth = collegeStudents[5], Value = collegeFlexChoices[1] },
            new Answer<string> { Question = collegeOpenQuestion, Youth = collegeStudents[5], Value = "Create a buddy system pairing first-year students with seniors for peer support." },

            new SingleChoiceAnswer { Question = collegeSupportQuestion, Youth = collegeStudents[7], Value = collegeSupportChoices[0] },
            new Answer<int> { Question = collegeStressScaleQuestion, Youth = collegeStudents[7], Value = 4 },
            new SingleChoiceAnswer { Question = collegeFlexQuestion, Youth = collegeStudents[7], Value = collegeFlexChoices[0] },
            new Answer<string> { Question = collegeOpenQuestion, Youth = collegeStudents[7], Value = "An anonymous portal for students to ask mental health questions without fear of judgment." },

            new SingleChoiceAnswer { Question = collegeSupportQuestion, Youth = collegeSurveyOnly[0], Value = collegeSupportChoices[2] },
            new Answer<int> { Question = collegeStressScaleQuestion, Youth = collegeSurveyOnly[0], Value = 6 },
            new SingleChoiceAnswer { Question = collegeFlexQuestion, Youth = collegeSurveyOnly[0], Value = collegeFlexChoices[1] },
            new Answer<string> { Question = collegeOpenQuestion, Youth = collegeSurveyOnly[0], Value = "Better career counseling would reduce long-term stress about job prospects." },

            new SingleChoiceAnswer { Question = collegeSupportQuestion, Youth = collegeSurveyOnly[1], Value = collegeSupportChoices[3] },
            new Answer<int> { Question = collegeStressScaleQuestion, Youth = collegeSurveyOnly[1], Value = 3 },
            new SingleChoiceAnswer { Question = collegeFlexQuestion, Youth = collegeSurveyOnly[1], Value = collegeFlexChoices[0] },
            new Answer<string> { Question = collegeOpenQuestion, Youth = collegeSurveyOnly[1], Value = "Integrate mindfulness sessions into the class schedule." },

            new SingleChoiceAnswer { Question = collegeSupportQuestion, Youth = collegeSurveyOnly[2], Value = collegeSupportChoices[1] },
            new Answer<int> { Question = collegeStressScaleQuestion, Youth = collegeSurveyOnly[2], Value = 7 },
            new SingleChoiceAnswer { Question = collegeFlexQuestion, Youth = collegeSurveyOnly[2], Value = collegeFlexChoices[2] },
            new Answer<string> { Question = collegeOpenQuestion, Youth = collegeSurveyOnly[2], Value = "Better sports facilities on campus help reduce stress levels significantly." }
        
        };
        context.Answers.AddRange(collegeAnswers);

        var collegeIdeas = new List<Idea>
        {
            new()
            {
                Content = "Schedule a fixed 'deadline-free block' each study week so we have at least 1 evening without schoolwork.",
                Summary = "Weekly deadline-free block",
                SubmissionDate = now.AddDays(-12),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[0],
                Youth = collegeStudents[0],
                SemanticCategories = new[] { "study-load", "work-life-balance", "stress-reduction" }
            },
            new()
            {
                Content = "Create a central well-being page on Toledo with all support channels, opening hours, and who to contact for what.",
                Summary = "Central well-being page",
                SubmissionDate = now.AddDays(-11),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[1],
                Youth = collegeStudents[1],
                SemanticCategories = new[] { "support-services", "campus-community", "digital-learning" }
            },
            new()
            {
                Content = "Start small peer-support groups of 8 students per program who meet biweekly.",
                Summary = "Peer-support groups",
                SubmissionDate = now.AddDays(-10),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[2],
                Youth = collegeStudents[2],
                SemanticCategories = new[] { "campus-community", "mental-health", "support-services" }
            },
            new()
            {
                Content = "Provide quiet relaxation spaces during exam weeks with water, fruit, and short breathing exercises.",
                Summary = "Quiet relaxation spaces",
                SubmissionDate = now.AddDays(-9),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[2],
                Youth = collegeStudents[3],
                SemanticCategories = new[] { "mental-health", "stress-reduction", "campus-community" }
            },
            new()
            {
                Content = "Let students choose a flexible catch-up moment when they are overloaded, without extra administrative barriers.",
                Summary = "Flexible catch-up moment",
                SubmissionDate = now.AddDays(-8),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[0],
                Youth = collegeStudents[4],
                SemanticCategories = new[] { "flexible-learning", "study-load", "work-life-balance" }
            },
            new()
            {
                Content = "Organize a monthly lunch session about stress management with student guidance and experienced students.",
                Summary = "Monthly stress lunch",
                SubmissionDate = now.AddDays(-7),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[1],
                Youth = collegeStudents[5],
                SemanticCategories = new[] { "mental-health", "support-services", "campus-community" }
            },
            new()
            {
                Content = "Spread large group assignments better across the semester so peak weeks are less heavy.",
                Summary = "Better distribution of group assignments",
                SubmissionDate = now.AddDays(-6),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[0],
                Youth = collegeStudents[6],
                SemanticCategories = new[] { "study-load", "curriculum-design" }
            },
            new()
            {
                Content = "Offer a quiet online study room with fixed times and a moderator, for those who are easily distracted at home.",
                Summary = "Online study room",
                SubmissionDate = now.AddDays(-5),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[4],
                Youth = collegeStudents[7],
                SemanticCategories = new[] { "digital-learning", "focus-productivity", "campus-community" }
            },
            new()
            {
                Content = "For working students, a schedule with at least three weeks of predictability would bring a lot of mental peace.",
                Summary = "Predictable schedule",
                SubmissionDate = now.AddDays(-4),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[3],
                Youth = collegeStudents[1],
                SemanticCategories = new[] { "work-life-balance", "study-load" }
            },
            new()
            {
                Content = "Publish a clear weekly schedule per subject with estimated study time, so we can better assess what is feasible.",
                Summary = "Weekly schedule with study time",
                SubmissionDate = now.AddDays(-3),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[0],
                Youth = collegeStudents[2],
                SemanticCategories = new[] { "curriculum-design", "study-load", "transparency" }
            },
            new()
            {
                Content = "Introduce a 'mental well-being buddy' system where first-years are paired with experienced students.",
                Summary = "Mental well-being buddy system",
                SubmissionDate = now.AddDays(-2),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[0],
                Youth = collegeStudents[3],
                SemanticCategories = new[] { "study-load", "campus-community", "support-services" }
            },
            new()
            {
                Content = "Provide more physical activity at school with treadmill workstations in strategic locations.",
                Summary = "Treadmill workstations",
                SubmissionDate = now.AddDays(-1),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[0],
                Youth = collegeStudents[0],
                SemanticCategories = new[] { "mental-health", "campus-community", "focus-productivity" }
            },
            new()
            {
                Content = "Make meditation and mindfulness apps available with short sessions that fit into lunch breaks.",
                Summary = "Mindfulness apps available",
                SubmissionDate = now.AddDays(1),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[0],
                Youth = collegeStudents[4],
                SemanticCategories = new[] { "digital-learning", "mental-health", "stress-reduction" }
            },
            new()
            {
                Content = "Organize 'stress-free lists' where students can upload their learning goals and receive feedback from teachers.",
                Summary = "Stress-free list feedback",
                SubmissionDate = now.AddDays(2),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[0],
                Youth = collegeStudents[5],
                SemanticCategories = new[] { "curriculum-design", "study-load", "support-services" }
            },
            new()
            {
                Content = "Create an anonymous 'stress-o-meter' where students can indicate how they feel without filling in data.",
                Summary = "Anonymous stress-o-meter",
                SubmissionDate = now.AddDays(3),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[0],
                Youth = collegeStudents[6],
                SemanticCategories = new[] { "mental-health", "campus-community", "transparency" }
            },
            new()
            {
                Content = "Offer financial support or discounts for mental health apps and wellness programs for students.",
                Summary = "Financial support wellness",
                SubmissionDate = now.AddDays(4),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[0],
                Youth = collegeStudents[7],
                SemanticCategories = new[] { "support-services", "mental-health", "work-life-balance" }
            },
            new()
            {
                Content = "Start a student research project where young people themselves investigate what improves their well-being.",
                Summary = "Student research well-being",
                SubmissionDate = now.AddDays(5),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[0],
                Youth = collegeStudents[1],
                SemanticCategories = new[] { "curriculum-design", "campus-community", "transparency" }
            },
            new()
            {
                Content = "Make targeted workshops available about sleep hygiene, nutrition, and other factors that influence mental well-being.",
                Summary = "Workshops sleep and nutrition",
                SubmissionDate = now.AddDays(6),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[0],
                Youth = collegeStudents[2],
                SemanticCategories = new[] { "mental-health", "support-services", "work-life-balance" }
            },
            new()
            {
                Content = "Introduce a 'mental health week' each semester with diverse activities and accessible support offerings.",
                Summary = "Annual mental health week",
                SubmissionDate = now.AddDays(7),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentalWellbeingActionPlan,
                Topic = collegeTopics[0],
                Youth = collegeStudents[3],
                SemanticCategories = new[] { "mental-health", "campus-community", "support-services" }
            }
        };

        context.Ideas.AddRange(collegeIdeas);

        var collegeResponses = new List<IdeaResponse>
        {
            new()
            {
                Idea = collegeIdeas[0],
                Text = "Great idea. If we have no deadlines on Tuesday evening, it really helps to catch our breath.",
                CreatedAt = now.AddDays(-11).AddHours(2),
                Youth = collegeStudents[3],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = collegeIdeas[0],
                Text = "Maybe also indicate in the class schedule which week is quieter, that makes planning easier.",
                CreatedAt = now.AddDays(-11).AddHours(6),
                Youth = collegeStudents[6],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = collegeIdeas[1],
                Text = "Yes please, now you have to search for info on three different pages.",
                CreatedAt = now.AddDays(-10).AddHours(4),
                Youth = collegeStudents[0],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = collegeIdeas[2],
                Text = "Peer-support seems strong to me, especially in the first semester when everyone is still finding their rhythm.",
                CreatedAt = now.AddDays(-9).AddHours(3),
                Youth = collegeStudents[5],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = collegeIdeas[3],
                Text = "Those quiet spaces would be really valuable during exam period. Maybe also show short stretch moments on screen.",
                CreatedAt = now.AddDays(-8).AddHours(5),
                Youth = collegeStudents[2],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = collegeIdeas[4],
                Text = "A simple form with reason and preferred time would already be enough.",
                CreatedAt = now.AddDays(-7).AddHours(4),
                Youth = collegeStudents[7],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = collegeIdeas[5],
                Text = "If there are also recordings or summaries for those who can't come, you reach more students.",
                CreatedAt = now.AddDays(-6).AddHours(2),
                Youth = collegeStudents[4],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = collegeIdeas[6],
                Text = "Totally agree, in some weeks we have three major deadlines on two days.",
                CreatedAt = now.AddDays(-5).AddHours(7),
                Youth = collegeStudents[1],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = collegeIdeas[7],
                Text = "Online study room with optional camera would be nice, then the threshold is lower.",
                CreatedAt = now.AddDays(-4).AddHours(1),
                Youth = collegeStudents[6],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            },
            new()
            {
                Idea = collegeIdeas[8],
                Text = "For working students, predictability really makes the difference between feasible and not feasible.",
                CreatedAt = now.AddDays(-3).AddHours(3),
                Youth = collegeStudents[0],
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo()
            }
        };

        context.Responses.AddRange(collegeResponses);

        var collegeReactions = new List<ResponseReaction>
        {
            new() { IdeaResponse = collegeResponses[0], Emoji = "🔥", CreatedAt = now.AddDays(-11).AddHours(3), Youth = collegeStudents[1] },
            new() { IdeaResponse = collegeResponses[0], Emoji = "💡", CreatedAt = now.AddDays(-11).AddHours(4), Youth = collegeStudents[4] },
            new() { IdeaResponse = collegeResponses[1], Emoji = "❤️", CreatedAt = now.AddDays(-11).AddHours(7), Youth = collegeStudents[2] },
            new() { IdeaResponse = collegeResponses[2], Emoji = "🙏", CreatedAt = now.AddDays(-10).AddHours(5), Youth = collegeStudents[5] },
            new() { IdeaResponse = collegeResponses[2], Emoji = "😂", CreatedAt = now.AddDays(-10).AddHours(6), Youth = collegeStudents[7] },
            new() { IdeaResponse = collegeResponses[3], Emoji = "🙌", CreatedAt = now.AddDays(-9).AddHours(6), Youth = collegeStudents[0] },
            new() { IdeaResponse = collegeResponses[4], Emoji = "😢", CreatedAt = now.AddDays(-8).AddHours(6), Youth = collegeStudents[3] },
            new() { IdeaResponse = collegeResponses[4], Emoji = "💚", CreatedAt = now.AddDays(-8).AddHours(7), Youth = collegeStudents[1] },
            new() { IdeaResponse = collegeResponses[5], Emoji = "👏", CreatedAt = now.AddDays(-7).AddHours(5), Youth = collegeStudents[6] },
            new() { IdeaResponse = collegeResponses[6], Emoji = "🎯", CreatedAt = now.AddDays(-6).AddHours(3), Youth = collegeStudents[2] },
            new() { IdeaResponse = collegeResponses[6], Emoji = "👍", CreatedAt = now.AddDays(-6).AddHours(4), Youth = collegeStudents[0] },
            new() { IdeaResponse = collegeResponses[7], Emoji = "💯", CreatedAt = now.AddDays(-5).AddHours(8), Youth = collegeStudents[4] },
            new() { IdeaResponse = collegeResponses[8], Emoji = "🧠", CreatedAt = now.AddDays(-4).AddHours(2), Youth = collegeStudents[5] },
            new() { IdeaResponse = collegeResponses[9], Emoji = "✅", CreatedAt = now.AddDays(-3).AddHours(4), Youth = collegeStudents[3] }
        };

        context.ResponseReactions.AddRange(collegeReactions);

        SeedAiPrompts(context, now);
        SeedAiRateLimits(context, now);
        SeedModerationKeywords(context, now);
        SeedAiDefaultProvider(context, now, configuration);
        SeedAiAuditLogs(context, now, hogeschool, mentaalWelzijnActieplan, stadLinden, vergroeningEnRecreatiePlan, collegeNova, mentalWellbeingActionPlan);

        context.SaveChanges();
    }

    private static void SeedAiPrompts(ConverseyDbContext context, DateTime now)
    {
        if (context.AiPrompts.Any())
        {
            return;
        }

        var prompts = new List<AiPrompt>
        {
            new()
            {
                Name = "ModerationGenerateAlternative",
                SystemPrompt = "You rewrite unsafe user feedback into respectful, constructive feedback while preserving intent. Return only the rewritten text.",
                UserPromptTemplate = "{{IdeaText}}",
                Description = "System prompt for generating a respectful alternative when content is flagged by moderation.",
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "ModerationPrompt",
                SystemPrompt = "You are a strict content safety classifier for a youth platform. Your task is to flag ANY harmful, toxic, or unsafe content.\n\nAnalyze the text against these categories:\n- sexual: sexually explicit content, sexual harassment, or sexualized language\n- hate_and_discrimination: slurs, hate speech, racism, homophobia, transphobia, bigotry, or discrimination based on identity\n- violence_and_threats: threats of violence, encouragement of violence, or glorification of harm\n- dangerous_and_criminal_content: illegal activity, self-harm instructions, or dangerous pranks\n- self_harm: content promoting or encouraging self-harm or suicide\n- pii: personal identifiable information like phone numbers, addresses, or full names\n\nAlso mark hate_and_discrimination as true for: personal insults involving slurs, name-calling with protected characteristics, profanity-laced harassment, hostile derogatory language, or general offensive/crude language targeting others.\n\nCRITICAL: Be conservative. If you are unsure whether content violates a category, mark it as violating. False positives are safer than false negatives.\n\nReturn ONLY a JSON object with this exact schema:\n{\"flagged\":true,\"categories\":{\"sexual\":false,\"hate_and_discrimination\":true,\"violence_and_threats\":false,\"dangerous_and_criminal_content\":false,\"self_harm\":false,\"pii\":false}}\n\nNo markdown, no code blocks, no explanation — just the raw JSON.",
                UserPromptTemplate = "",
                Description = "Prompt-based content moderation fallback for providers without a dedicated moderation endpoint (non-Mistral). Sends content as user message, expects structured JSON response.",
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "IdeaNudgingSystem",
                SystemPrompt = "You help youth improve the quality of their idea before publishing. Ask exactly one concrete follow-up question when the idea is too shallow, vague, or underspecified. If the idea is already acceptable for the configured nudging strength, approve it. Never invent multiple questions. Return strict JSON only with the shape {\"isApproved\":true} or {\"isApproved\":false,\"question\":\"...\"}. Nudging strength: {{NudgingModeDescription}}.",
                UserPromptTemplate = "",
                Description = "System prompt for the idea quality nudging assessment. NudgingModeDescription is injected based on the project's nudging strength setting.",
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "IdeaNudgingUser",
                SystemPrompt = "",
                UserPromptTemplate = "Project title: {{ProjectTitle}}\nProject description: {{ProjectDescription}}\nTopic title: {{TopicTitle}}\nTopic prompt/question: {{TopicPrompt}}\n\nCurrent idea draft:\n{{IdeaText}}\n\nConversation so far:\n{{Conversation}}\n\nDecide whether the draft is ready. If not, ask one follow-up question that is specific to this idea and helps deepen it using the project and topic context.",
                Description = "User prompt template for idea nudging. Contains the idea draft, project/topic context, and previous conversation turns.",
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "IdeaRankingSystem",
                SystemPrompt = "You compare youth ideas by meaning. Return only strict JSON with field rankedIndexes as an array of integer indexes. For similarity tasks, return clearly similar ideas. For difference tasks, return ideas with a noticeably different focus or approach; be inclusive rather than restrictive.",
                UserPromptTemplate = "",
                Description = "System prompt for ranking ideas by semantic similarity or difference.",
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "IdeaRankingUser",
                SystemPrompt = "",
                UserPromptTemplate = "Reference idea:\n{{ReferenceIdea}}\n\nCandidate ideas (use only these indexes):\n{{Candidates}}\n\nTask:\n- {{RelationGoal}}\n- Return up to {{Limit}} indexes, ordered from best to least fitting for this relation.\n- Do not invent indexes.\n- Return strict JSON only with this schema:\n{\"rankedIndexes\":[0,1,2]}",
                Description = "User prompt template for ranking ideas. Contains reference idea, candidates with indexes, relation goal, and limit.",
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "IdeaCategorizationSystem",
                SystemPrompt = "You assign semantic categories to youth ideas. Return only strict JSON.",
                UserPromptTemplate = "",
                Description = "System prompt for assigning semantic category labels to ideas.",
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "IdeaCategorizationUser",
                SystemPrompt = "",
                UserPromptTemplate = "Categorize each idea semantically. One idea may belong to multiple categories.\n\nThese are the existing categories already used in this topic. Reuse these exact labels whenever possible and only invent a new label if nothing fits:\n{{ExistingCategories}}\n\nIdeas:\n{{Ideas}}\n\nRules:\n- Use short, human-readable category names.\n- Max {{MaxCategoriesPerIdea}} categories per idea.\n- Prefer reusing an existing category label when it is semantically close enough.\n- Avoid near-duplicate labels when an existing category already covers the same meaning.\n- Do not invent idea indexes.\n- Avoid creating near-duplicate labels if an existing category already fits.\n- Return strict JSON only in this shape:\n{\"items\":[{\"index\":0,\"categories\":[\"Category A\",\"Category B\"]}]}",
                Description = "User prompt template for idea categorization. Contains index-labeled ideas, existing category labels, and max categories per idea.",
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "ExtractKeyPhrasesSystem",
                SystemPrompt = "You are a professional note-taking assistant. You ALWAYS return valid JSON. Extract concise, meaningful key phrases from spoken language in {{Language}} as if taking meeting notes. Be precise, remove all fluff, focus on actionable content, and never include filler words or greetings.",
                UserPromptTemplate = "",
                Description = "System prompt for extracting key phrases from speech transcripts. Language variable injected at runtime.",
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "ExtractKeyPhrasesUser",
                SystemPrompt = "",
                UserPromptTemplate = "",
                Description = "User prompt template for key phrase extraction. Uses hardcoded fallback when empty.",
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "GenerateTextFromBubblesSystem",
                SystemPrompt = "You rewrite text from the user's first-person perspective. Always use first-person pronouns matching the language (Dutch: ik/mijn/wij/onze, English: I/my/we/our, French: je/mon/nous/notre). Always respond in {{Language}}.",
                UserPromptTemplate = "",
                Description = "System prompt for generating first-person text from key phrase bubbles. Language variable injected at runtime.",
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "GenerateTextFromBubblesUser",
                SystemPrompt = "",
                UserPromptTemplate = "",
                Description = "User prompt template for text generation from bubbles. Uses hardcoded fallback when empty.",
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "AnalyticsIdeaSummarySystem",
                SystemPrompt = "You are an insightful data analyst for a youth participation platform. Your task is to analyze a collection of youth-contributed ideas and produce a clear, structured summary.\n\nReturn ONLY a JSON object with this exact schema:\n{\"overview\":\"A 2-3 sentence overview of the main themes across all ideas.\",\"trends\":[\"trend 1\",\"trend 2\",\"trend 3\"],\"minorityViews\":[\"niche or less common perspective 1\",\"niche or less common perspective 2\"],\"notableQuotes\":[\"direct quote or close paraphrase 1\",\"direct quote or close paraphrase 2\"],\"suggestedActions\":[\"actionable recommendation 1\",\"actionable recommendation 2\"]}\n\nRules:\n- overview: concise, covers breadth of all ideas seen\n- trends: 2-4 recurring patterns or dominant themes\n- minorityViews: 1-3 ideas that stand out from the mainstream (unique, dissenting, or niche)\n- notableQuotes: 2-3 exact or near-exact quotes from the ideas that are particularly insightful\n- suggestedActions: 2-3 concrete recommendations based on what youth are saying\n- Write in {{Language}}.\n- If focus instruction provided, prioritize that angle while still covering general patterns.",
                UserPromptTemplate = "",
                Description = "System prompt for AI-generated summary of youth ideas in the analytics dashboard. Language variable injected at runtime.",
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "AnalyticsIdeaSummaryUser",
                SystemPrompt = "",
                UserPromptTemplate = "",
                Description = "User prompt template for AI-generated idea summaries. Uses hardcoded fallback when empty.",
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        context.AiPrompts.AddRange(prompts);
    }

    private static void SeedAiRateLimits(ConverseyDbContext context, DateTime now)
    {
        if (context.RateLimitConfigs.Any())
        {
            return;
        }

        var configs = new List<RateLimitConfig>
        {
            new()
            {
                PolicyName = "AiFixedPolicy",
                PermitLimit = 30,
                WindowSeconds = 60,
                QueueLimit = 0,
                PartitionType = "user",
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                PolicyName = "AiAdminPolicy",
                PermitLimit = 60,
                WindowSeconds = 60,
                QueueLimit = 0,
                PartitionType = "user",
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        context.RateLimitConfigs.AddRange(configs);
    }

    private static void SeedModerationKeywords(ConverseyDbContext context, DateTime now)
    {
        if (context.ModerationKeywords.Any())
        {
            return;
        }

        var keywords = new List<ModerationKeyword>
        {
            new() { Keyword = "retarded", CreatedAt = now },
            new() { Keyword = "moron", CreatedAt = now },
            new() { Keyword = "dumbass", CreatedAt = now },
            new() { Keyword = "dumb ass", CreatedAt = now },
            new() { Keyword = "fucking", CreatedAt = now },
            new() { Keyword = "faggot", CreatedAt = now },
            new() { Keyword = "fag", CreatedAt = now },
            new() { Keyword = "nigger", CreatedAt = now },
            new() { Keyword = "nigga", CreatedAt = now },
        };

        context.ModerationKeywords.AddRange(keywords);
    }

    private static void SeedAiDefaultProvider(ConverseyDbContext context, DateTime now, IConfiguration configuration)
    {
        if (context.AiProviderConfigs.Any())
        {
            return;
        }

        var apiKey = configuration?["AI:Mistral:ApiKey"] ?? string.Empty;

        context.AiProviderConfigs.Add(new AiProviderConfig
        {
            ProviderName = "Mistral",
            BaseUrl = "https://api.mistral.ai/v1/",
            ApiKey = apiKey,
            CompletionsModel = configuration?["AI:Mistral:CompletionsModel"] ?? "mistral-small-latest",
            ModerationModel = configuration?["AI:Mistral:ModerationModel"] ?? "mistral-moderation-latest",
            ApiVersion = "",
            Temperature = 0.2m,
            IsEnabled = true,
            IsDefault = true,
            CreatedAt = now,
            UpdatedAt = now
        });
    }

    private static void SeedAiAuditLogs(ConverseyDbContext context, DateTime now,
        Workspace hogeschool, Project mentaalWelzijnActieplan,
        Workspace stadLinden, Project vergroeningEnRecreatiePlan,
        Workspace collegeNova, Project mentalWellbeingActionPlan)
    {
        if (context.AiAuditLogs.Any())
        {
            return;
        }

        var rnd = new Random(42);
        var models = new[] { "mistral-small-latest", "mistral-large-latest", "mistral-moderation-latest" };
        var types = new[] { "Completions", "Moderation" };
        var prompts = new[] { "IdeaNudgingSystem", "IdeaNudgingUser", "IdeaRankingSystem", "IdeaRankingUser", "IdeaCategorizationSystem", "IdeaCategorizationUser", "ModerationPrompt", "ModerationGenerateAlternative", "ExtractKeyPhrasesSystem", "ExtractKeyPhrasesUser", "GenerateTextFromBubblesSystem", "GenerateTextFromBubblesUser" };
        var providers = new[] { "Mistral" };

        var workspaces = new[] { hogeschool, stadLinden, collegeNova };
        var projects = new[] { mentaalWelzijnActieplan, vergroeningEnRecreatiePlan, mentalWellbeingActionPlan };

        var logs = new List<AiAuditLog>();

        for (int i = 0; i < 150; i++)
        {
            var wsIndex = rnd.Next(0, workspaces.Length);
            var workspace = workspaces[wsIndex];
            var project = projects[wsIndex];

            var daysAgo = rnd.Next(2, 60);
            var startTime = now.AddDays(-daysAgo).AddHours(rnd.Next(0, 24)).AddMinutes(rnd.Next(0, 60));
            var durationMs = rnd.Next(100, 5000);
            var modelType = types[rnd.Next(0, types.Length)];
            var modelName = modelType == "Moderation" ? "mistral-moderation-latest" : models[rnd.Next(0, 2)];
            var promptName = prompts[rnd.Next(0, prompts.Length)];
            var inputTokens = rnd.Next(200, 4000);
            var outputTokens = modelType == "Moderation" ? rnd.Next(50, 200) : rnd.Next(100, 1500);
            var cost = modelType == "Moderation"
                ? inputTokens * 0.00000015m + outputTokens * 0.00000015m
                : inputTokens * 0.000002m + outputTokens * 0.000006m;

            logs.Add(new AiAuditLog
            {
                ModelName = modelName,
                ModelType = modelType,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                Cost = Math.Round(cost, 6),
                ProviderName = providers[rnd.Next(0, providers.Length)],
                PromptName = promptName,
                StartTime = startTime,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                CreatedAt = startTime,
                WorkspaceId = workspace.Id,
                ProjectId = project.Id
            });
        }

        context.AiAuditLogs.AddRange(logs.OrderByDescending(l => l.StartTime));
    }
}
