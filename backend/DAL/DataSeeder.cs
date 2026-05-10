using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Domain.Survey;

namespace Conversey.DAL;

public static class DataSeeder
{
    public static void Seed(ConverseyDbContext context)
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
            Workspace = hogeschool
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

        var mentaalSupportQuestion = new ChoiceQuestion<SingleChoice>
        {
            Text = "Hoe beoordeel je de toegankelijkheid van mentale ondersteuning op de campus?",
            Required = true,
            Project = mentaalWelzijnActieplan,
            PossibleChoices = new List<SingleChoice>()
        };
        var mentaalSupportChoices = new List<SingleChoice>
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

        var mentaalFlexQuestion = new ChoiceQuestion<SingleChoice>
        {
            Text = "Zou je gebruik maken van flexibele inhaalmomenten bij overbelasting?",
            Required = true,
            Project = mentaalWelzijnActieplan,
            PossibleChoices = new List<SingleChoice>()
        };
        var mentaalFlexChoices = new List<SingleChoice>
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
            mentaalStressScaleQuestion,
            mentaalFlexQuestion,
            mentaalOpenQuestion
        };
        context.Questions.AddRange(mentaalQuestions);

        var mentaalAnswers = new List<Answer>
        {
            new Answer<SingleChoice> { Question = mentaalSupportQuestion, Youth = students[0], Value = mentaalSupportChoices[2] },
            new Answer<int> { Question = mentaalStressScaleQuestion, Youth = students[0], Value = 7 },
            new Answer<SingleChoice> { Question = mentaalFlexQuestion, Youth = students[0], Value = mentaalFlexChoices[0] },
            new Answer<string> { Question = mentaalOpenQuestion, Youth = students[0], Value = "Een wekelijkse deadlinevrije avond per opleiding zou direct stress verlagen." },

            new Answer<SingleChoice> { Question = mentaalSupportQuestion, Youth = students[3], Value = mentaalSupportChoices[1] },
            new Answer<int> { Question = mentaalStressScaleQuestion, Youth = students[3], Value = 8 },
            new Answer<SingleChoice> { Question = mentaalFlexQuestion, Youth = students[3], Value = mentaalFlexChoices[1] },
            new Answer<string> { Question = mentaalOpenQuestion, Youth = students[3], Value = "Maak begeleiding zichtbaarder in één centrale welzijnspagina." },

            new Answer<SingleChoice> { Question = mentaalSupportQuestion, Youth = students[6], Value = mentaalSupportChoices[3] },
            new Answer<int> { Question = mentaalStressScaleQuestion, Youth = students[6], Value = 6 },
            new Answer<SingleChoice> { Question = mentaalFlexQuestion, Youth = students[6], Value = mentaalFlexChoices[0] },
            new Answer<string> { Question = mentaalOpenQuestion, Youth = students[6], Value = "Bied meer stille ruimtes met korte ontspanningsoefeningen in piekweken." }
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
            Workspace = stadLinden
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

        var cityGreenPriorityQuestion = new ChoiceQuestion<SingleChoice>
        {
            Text = "Welke prioriteit moet de stad eerst aanpakken?",
            Required = true,
            Project = vergroeningEnRecreatiePlan,
            PossibleChoices = new List<SingleChoice>()
        };
        var cityGreenPriorityChoices = new List<SingleChoice>
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

        var cityParticipationQuestion = new ChoiceQuestion<SingleChoice>
        {
            Text = "Op welke manier wil je het liefst betrokken worden bij stadsbeleid?",
            Required = true,
            Project = vergroeningEnRecreatiePlan,
            PossibleChoices = new List<SingleChoice>()
        };
        var cityParticipationChoices = new List<SingleChoice>
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
            cityLeefbaarheidScaleQuestion,
            cityParticipationQuestion,
            cityOpenQuestion
        };
        context.Questions.AddRange(cityQuestions);

        var citySurveyAnswers = new List<Answer>
        {
            new Answer<SingleChoice> { Question = cityGreenPriorityQuestion, Youth = cityYouths[0], Value = cityGreenPriorityChoices[0] },
            new Answer<int> { Question = cityLeefbaarheidScaleQuestion, Youth = cityYouths[0], Value = 5 },
            new Answer<SingleChoice> { Question = cityParticipationQuestion, Youth = cityYouths[0], Value = cityParticipationChoices[1] },
            new Answer<string> { Question = cityOpenQuestion, Youth = cityYouths[0], Value = "Het Stationsplein: daar is te weinig schaduw en bijna geen groene zitruimte." },

            new Answer<SingleChoice> { Question = cityGreenPriorityQuestion, Youth = cityYouths[3], Value = cityGreenPriorityChoices[1] },
            new Answer<int> { Question = cityLeefbaarheidScaleQuestion, Youth = cityYouths[3], Value = 6 },
            new Answer<SingleChoice> { Question = cityParticipationQuestion, Youth = cityYouths[3], Value = cityParticipationChoices[2] },
            new Answer<string> { Question = cityOpenQuestion, Youth = cityYouths[3], Value = "Rond de campusroute: vooral voor veiligere, groene fietsverbindingen." },

            new Answer<SingleChoice> { Question = cityGreenPriorityQuestion, Youth = cityYouths[6], Value = cityGreenPriorityChoices[2] },
            new Answer<int> { Question = cityLeefbaarheidScaleQuestion, Youth = cityYouths[6], Value = 4 },
            new Answer<SingleChoice> { Question = cityParticipationQuestion, Youth = cityYouths[6], Value = cityParticipationChoices[0] },
            new Answer<string> { Question = cityOpenQuestion, Youth = cityYouths[6], Value = "Een braakliggend terrein in Noordwijk, als tijdelijke pop-up groene ontmoetingsplek." }
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

        SeedAiPrompts(context, now);

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
            }
        };

        context.AiPrompts.AddRange(prompts);
    }
}
