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

        var now = DateTime.UtcNow;

        // =====================================================
        // Case 1: Hogeschool Nova / Actieplan Mentaal Welzijn
        // =====================================================
        if (!context.Workspaces.Any(w => w.Id == Slug.FromName("hogeschool-nova")))
        {
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
        }

        // =====================================================
        // Case 2: Stad Linden / Jong in een Groene Stad
        // =====================================================
        if (!context.Workspaces.Any(w => w.Id == Slug.FromName("stad-linden")))
        {
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
        }

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
            Workspace = collegeNova
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

        var collegeSupportQuestion = new ChoiceQuestion<SingleChoice>
        {
            Text = "How do you rate the accessibility of mental support on campus?",
            Required = true,
            Project = mentalWellbeingActionPlan,
            PossibleChoices = new List<SingleChoice>()
        };
        var collegeSupportChoices = new List<SingleChoice>
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

        var collegeFlexQuestion = new ChoiceQuestion<SingleChoice>
        {
            Text = "Would you use flexible catch-up moments when overloaded?",
            Required = true,
            Project = mentalWellbeingActionPlan,
            PossibleChoices = new List<SingleChoice>()
        };
        var collegeFlexChoices = new List<SingleChoice>
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
            new Answer<SingleChoice> { Question = collegeSupportQuestion, Youth = collegeStudents[0], Value = collegeSupportChoices[2] },
            new Answer<int> { Question = collegeStressScaleQuestion, Youth = collegeStudents[0], Value = 7 },
            new Answer<SingleChoice> { Question = collegeFlexQuestion, Youth = collegeStudents[0], Value = collegeFlexChoices[0] },
            new Answer<string> { Question = collegeOpenQuestion, Youth = collegeStudents[0], Value = "A weekly deadline-free evening per program would immediately reduce stress." },

            new Answer<SingleChoice> { Question = collegeSupportQuestion, Youth = collegeStudents[3], Value = collegeSupportChoices[1] },
            new Answer<int> { Question = collegeStressScaleQuestion, Youth = collegeStudents[3], Value = 8 },
            new Answer<SingleChoice> { Question = collegeFlexQuestion, Youth = collegeStudents[3], Value = collegeFlexChoices[1] },
            new Answer<string> { Question = collegeOpenQuestion, Youth = collegeStudents[3], Value = "Make guidance more visible on one central well-being page." },

            new Answer<SingleChoice> { Question = collegeSupportQuestion, Youth = collegeStudents[6], Value = collegeSupportChoices[3] },
            new Answer<int> { Question = collegeStressScaleQuestion, Youth = collegeStudents[6], Value = 6 },
            new Answer<SingleChoice> { Question = collegeFlexQuestion, Youth = collegeStudents[6], Value = collegeFlexChoices[0] },
            new Answer<string> { Question = collegeOpenQuestion, Youth = collegeStudents[6], Value = "Offer more quiet spaces with short relaxation exercises during peak weeks." }
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

        context.SaveChanges();
    }
}
