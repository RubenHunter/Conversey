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

        #region SeedWorkspaces

        var hogeschool = new Workspace
        {
            Name = "Hogeschool Nova"
        };
        hogeschool.Id = Slug.FromName(hogeschool.Name);

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
        mentaalWelzijnActieplan.Id = Slug.FromName(mentaalWelzijnActieplan.Title);

        context.Projects.Add(mentaalWelzijnActieplan);

        #endregion

        #region SeedYouths

        var students = new List<Youth>
        {
            new() { Token = Guid.NewGuid(), Email = "amelie@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Token = Guid.NewGuid(), Email = "younes@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Token = Guid.NewGuid(), Email = "lotte@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Token = Guid.NewGuid(), Email = "milan@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Token = Guid.NewGuid(), Email = "sarah@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Token = Guid.NewGuid(), Email = "noah@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Token = Guid.NewGuid(), Email = "zineb@student.nova.be", Project = mentaalWelzijnActieplan },
            new() { Token = Guid.NewGuid(), Email = "ruben@student.nova.be", Project = mentaalWelzijnActieplan }
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
                "In welke mate weet je waar je hulp kan vinden op de campus?",
                true,
                "Ik weet exact waar en bij wie",
                "Ik heb een idee, maar niet volledig",
                "Ik weet het niet"
            ),
            CreateOpenQuestion(
                mentaalWelzijnActieplan,
                "Welke concrete ondersteuning van docenten of begeleiders zou jou het meeste helpen?",
                true
            ),
            CreateSingleChoiceQuestion(
                mentaalWelzijnActieplan,
                "Hoe verbonden voel je je met medestudenten in je opleiding?",
                true,
                "Heel sterk",
                "Voldoende",
                "Beperkt",
                "Nauwelijks"
            ),
            CreateOpenQuestion(
                mentaalWelzijnActieplan,
                "Noem een actie die de hogeschool volgend semester meteen kan starten.",
                false
            ),
            CreateSingleChoiceQuestion(
                mentaalWelzijnActieplan,
                "Wanneer heb je het meeste nood aan welzijnsondersteuning?",
                true,
                "Begin semester",
                "Tijdens tussentijdse opdrachten",
                "Examenperiode",
                "Doorheen het hele semester"
            ),
            CreateOpenQuestion(
                mentaalWelzijnActieplan,
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
                Status = ModerationStatus.Approved,
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
                Status = ModerationStatus.Approved,
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
                Status = ModerationStatus.Approved,
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
                Status = ModerationStatus.Approved,
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
                Status = ModerationStatus.Approved,
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
                Status = ModerationStatus.Approved,
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
                Status = ModerationStatus.Approved,
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
                Status = ModerationStatus.Approved,
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
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[3],
                Youth = students[1]
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
                Youth = students[2]
            },
            new()
            {
                Content = "Voorkom dat meerdere grote deadlines op dezelfde dag vallen door vakoverschrijdende afstemming binnen de opleiding.",
                Summary = "Deadlines beter afstemmen",
                SubmissionDate = now.AddDays(-3).AddHours(2),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[3]
            },
            new()
            {
                Content = "Geef per semester minstens twee welzijnsmomenten zonder evaluatie, met ruimte voor vragen en planning.",
                Summary = "Welzijnsmomenten zonder evaluatie",
                SubmissionDate = now.AddDays(-2),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[5]
            },
            new()
            {
                Content = "Voorzie een korte check-in in elke lesweek waarin docenten aangeven wat echt prioritair is voor de volgende deadline.",
                Summary = "Wekelijkse prioriteiten-check",
                SubmissionDate = now.AddDays(-2).AddHours(1),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[7]
            },
            new()
            {
                Content = "Werk met een centraal dashboard per klasgroep waar alle deadlines chronologisch en kleurgecodeerd staan.",
                Summary = "Centraal deadline-dashboard",
                SubmissionDate = now.AddDays(-2).AddHours(3),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[0]
            },
            new()
            {
                Content = "Laat studenten bij uitzonderlijke overbelasting een korte aanvraag doen voor een eenmalige deadlineverschuiving.",
                Summary = "Eenmalige deadlineverschuiving",
                SubmissionDate = now.AddDays(-1).AddHours(2),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[4]
            },
            new()
            {
                Content = "Plan in drukkere weken geen onverwachte tussentoetsen, zodat studenten hun planning realistisch kunnen houden.",
                Summary = "Geen onverwachte toetsen in piekweken",
                SubmissionDate = now.AddDays(-1).AddHours(4),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[6]
            },
            new()
            {
                Content = "Voor groepswerken: verplicht een tussentijdse feedbackronde zodat last-minute stress en misverstanden verminderen.",
                Summary = "Tussentijdse feedback bij groepswerk",
                SubmissionDate = now.AddDays(-1).AddHours(6),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[1]
            },
            new()
            {
                Content = "Maak een richtlijn dat grote opdrachten minimum drie weken op voorhand volledig gecommuniceerd moeten worden.",
                Summary = "Opdrachten vroeger communiceren",
                SubmissionDate = now.AddDays(-1).AddHours(8),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[3]
            },
            new()
            {
                Content = "Voorzie in elke module een korte workshop over studieplanning en energiemanagement, afgestemd op de examenperiode.",
                Summary = "Workshop studieplanning",
                SubmissionDate = now.AddHours(-10),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[5]
            },
            new()
            {
                Content = "Organiseer een jaarlijkse welzijnsbeurs waar studenten alle ondersteuningsmogelijkheden kunnen ontdekken en uitproberen.",
                Summary = "Jaarlijkse welzijnsbeurs",
                SubmissionDate = now.AddHours(-9),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[7]
            },
            new()
            {
                Content = "Introduceer een buddy-systeem waarbij eerstejaars studenten gekoppeld worden aan ervaren studenten voor ondersteuning.",
                Summary = "Buddy-systeem voor eerstejaars",
                SubmissionDate = now.AddHours(-8),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[6]
            },
            new()
            {
                Content = "Zorg voor een centrale online kalender waar alle belangrijke data, deadlines en evenementen per opleiding in staan.",
                Summary = "Centrale online kalender",
                SubmissionDate = now.AddHours(-7),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[5]
            },
            new()
            {
                Content = "Voorzie meer ruimte voor informele ontmoetingen tussen studenten en docenten, zowel fysiek als digitaal.",
                Summary = "Meer informele ontmoetingen",
                SubmissionDate = now.AddHours(-6),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[4]
            },
            new()
            {
                Content = "Stimuleer opleidingen om gezamenlijke evenementen te organiseren, zodat studenten uit verschillende jaren elkaar leren kennen.",
                Summary = "Gezamenlijke evenementen tussen jaren",
                SubmissionDate = now.AddHours(-5),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[3]
            },
            new()
            {
                Content = "Bied een online cursus aan over timemanagement en studievaardigheden, specifiek gericht op hogeschoolstudenten.",
                Summary = "Online cursus timemanagement",
                SubmissionDate = now.AddHours(-4),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[2]
            },
            new()
            {
                Content = "Maak het mogelijk om in plaats van een examen een extra opdracht te doen voor wie meer tijd nodig heeft om te studeren.",
                Summary = "Extra opdracht in plaats van examen",
                SubmissionDate = now.AddHours(-3),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[1]
            },
            new()
            {
                Content = "Voorzie tijdens de blokperiode extra ondersteuning door studentenpsychologen, met een focus op stress- en tijdsmanagement.",
                Summary = "Extra ondersteuning tijdens blok",
                SubmissionDate = now.AddHours(-2),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[0]
            },
            new()
            {
                Content = "Zorg voor een anonieme feedbacktool waar studenten op elk moment van het semester feedback kunnen geven over hun werload en welzijn.",
                Summary = "Anonieme feedbacktool voor studenten",
                SubmissionDate = now.AddHours(-1),
                Status = ModerationStatus.Approved,
                ModerationInfo = new ModerationInfo(),
                Project = mentaalWelzijnActieplan,
                Topic = topics[0],
                Youth = students[7]
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

        #endregion

        #region SeedResponseReactions

        // Raw unicode works well with Postgres (UTF-8) and keeps demo data close to UI behavior.
        var reactions = new List<ResponseReaction>
        {
            new() { Response = responses[0], Emoji = "🔥", CreatedAt = now.AddDays(-11).AddHours(3), Youth = students[1] },
            new() { Response = responses[0], Emoji = "💡", CreatedAt = now.AddDays(-11).AddHours(4), Youth = students[4] },
            new() { Response = responses[1], Emoji = "❤️", CreatedAt = now.AddDays(-11).AddHours(7), Youth = students[2] },
            new() { Response = responses[2], Emoji = "🙏", CreatedAt = now.AddDays(-10).AddHours(5), Youth = students[5] },
            new() { Response = responses[2], Emoji = "😂", CreatedAt = now.AddDays(-10).AddHours(6), Youth = students[7] },
            new() { Response = responses[3], Emoji = "🙌", CreatedAt = now.AddDays(-9).AddHours(6), Youth = students[0] },
            new() { Response = responses[4], Emoji = "😢", CreatedAt = now.AddDays(-8).AddHours(6), Youth = students[3] },
            new() { Response = responses[4], Emoji = "💚", CreatedAt = now.AddDays(-8).AddHours(7), Youth = students[1] },
            new() { Response = responses[5], Emoji = "👏", CreatedAt = now.AddDays(-7).AddHours(5), Youth = students[6] },
            new() { Response = responses[6], Emoji = "🎯", CreatedAt = now.AddDays(-6).AddHours(3), Youth = students[2] },
            new() { Response = responses[6], Emoji = "👍", CreatedAt = now.AddDays(-6).AddHours(4), Youth = students[0] },
            new() { Response = responses[7], Emoji = "💯", CreatedAt = now.AddDays(-5).AddHours(8), Youth = students[4] },
            new() { Response = responses[8], Emoji = "🧠", CreatedAt = now.AddDays(-4).AddHours(2), Youth = students[5] },
            new() { Response = responses[9], Emoji = "✅", CreatedAt = now.AddDays(-3).AddHours(4), Youth = students[3] }
        };

        context.ResponseReactions.AddRange(reactions);

        #endregion

        #region Case2_SeedWorkspaces

        var stadLinden = new Workspace
        {
            Name = "Stad Linden"
        };
        stadLinden.Id = Slug.FromName(stadLinden.Name);

        context.Workspaces.Add(stadLinden);

        #endregion

        #region Case2_SeedProjects

        var vergroeningEnRecreatiePlan = new Project
        {
            Title = "Jong in een Groene Stad 2026-2028",
            Description = "Stad Linden betrekt jongeren van 18 tot 30 actief bij keuzes rond vergroening, klimaatmaatregelen en verdeling van stedelijke recreatie.",
            ImageUrl = "https://images.unsplash.com/photo-1473448912268-2022ce9509d8?auto=format&fit=crop&w=1600&q=80",
            Status = Status.Active,
            StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2028, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            InteractionForm = InteractionType.Chat,
            Workspace = stadLinden
        };
        vergroeningEnRecreatiePlan.Id = Slug.FromName(vergroeningEnRecreatiePlan.Title);

        context.Projects.Add(vergroeningEnRecreatiePlan);

        #endregion

        #region Case2_SeedYouths

        var cityYouths = new List<Youth>
        {
            new() { Token = Guid.NewGuid(), Email = "juna@stadlinden.be", Project = vergroeningEnRecreatiePlan },
            new() { Token = Guid.NewGuid(), Email = "faris@stadlinden.be", Project = vergroeningEnRecreatiePlan },
            new() { Token = Guid.NewGuid(), Email = "nora@stadlinden.be", Project = vergroeningEnRecreatiePlan },
            new() { Token = Guid.NewGuid(), Email = "daan@stadlinden.be", Project = vergroeningEnRecreatiePlan },
            new() { Token = Guid.NewGuid(), Email = "ayla@stadlinden.be", Project = vergroeningEnRecreatiePlan },
            new() { Token = Guid.NewGuid(), Email = "bram@stadlinden.be", Project = vergroeningEnRecreatiePlan },
            new() { Token = Guid.NewGuid(), Email = "ines@stadlinden.be", Project = vergroeningEnRecreatiePlan },
            new() { Token = Guid.NewGuid(), Email = "yara@stadlinden.be", Project = vergroeningEnRecreatiePlan }
        };

        context.Youths.AddRange(cityYouths);

        #endregion

        #region Case2_SeedTopics

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

        #endregion

        #region Case2_SeedQuestions

        context.Questions.AddRange(
            CreateSingleChoiceQuestion(
                vergroeningEnRecreatiePlan,
                "Hoe ervaar je de leefbaarheid van je stad of gemeente vandaag?",
                true,
                "Heel positief",
                "Eerder positief",
                "Neutraal",
                "Eerder negatief",
                "Heel negatief"
            ),
            CreateSingleChoiceQuestion(
                vergroeningEnRecreatiePlan,
                "Vind je je stad voldoende groen en klimaatvriendelijk?",
                true,
                "Ja, zeker",
                "Grotendeels",
                "Eerder niet",
                "Nee, duidelijk te weinig"
            ),
            CreateSingleChoiceQuestion(
                vergroeningEnRecreatiePlan,
                "Waar ervaar je de grootste tekorten in je buurt?",
                true,
                "Te weinig groen en schaduw",
                "Te druk of onveilig verkeer",
                "Te weinig toegankelijke ontmoetingsplekken",
                "Onvoldoende onderhoud van publieke ruimte"
            ),
            CreateSingleChoiceQuestion(
                vergroeningEnRecreatiePlan,
                "In welke mate voel je je vandaag betrokken bij stedelijke beslissingen rond klimaat en leefomgeving?",
                true,
                "Sterk betrokken",
                "Soms betrokken",
                "Nauwelijks betrokken",
                "Helemaal niet betrokken"
            ),
            CreateOpenQuestion(
                vergroeningEnRecreatiePlan,
                "Welke plek in de stad verdient volgens jou als eerste extra aandacht op vlak van vergroening?",
                true
            ),
            CreateOpenQuestion(
                vergroeningEnRecreatiePlan,
                "Welke concrete maatregel moet de stad volgend jaar prioritair uitvoeren?",
                true
            ),
            CreateOpenQuestion(
                vergroeningEnRecreatiePlan,
                "Hoe zie jij je eigen rol in een duurzamere en groenere stad?",
                false
            ),
            CreateSingleChoiceQuestion(
                vergroeningEnRecreatiePlan,
                "Hoe wil je in de toekomst het liefst participeren aan stadsbeleid?",
                true,
                "Online bevragingen",
                "Workshops of burgerpanels",
                "Korte ideesessies op locatie",
                "Combinatie van bovenstaande"
            )
        );

        #endregion

        #region Case2_SeedIdeas

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
                Youth = cityYouths[0]
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
                Youth = cityYouths[1]
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
                Youth = cityYouths[2]
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
                Youth = cityYouths[3]
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
                Youth = cityYouths[4]
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
                Youth = cityYouths[5]
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
                Youth = cityYouths[6]
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
                Youth = cityYouths[7]
            }
        };

        context.Ideas.AddRange(cityIdeas);

        #endregion

        #region Case2_SeedResponses

        var cityResponses = new List<Response>
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

        #endregion

        #region Case2_SeedResponseReactions

        var cityReactions = new List<ResponseReaction>
        {
            new() { Response = cityResponses[0], Emoji = "🔥", CreatedAt = now.AddDays(-8).AddHours(3), Youth = cityYouths[1] },
            new() { Response = cityResponses[0], Emoji = "💡", CreatedAt = now.AddDays(-8).AddHours(4), Youth = cityYouths[4] },
            new() { Response = cityResponses[1], Emoji = "❤️", CreatedAt = now.AddDays(-7).AddHours(6), Youth = cityYouths[2] },
            new() { Response = cityResponses[2], Emoji = "🙏", CreatedAt = now.AddDays(-6).AddHours(2), Youth = cityYouths[0] },
            new() { Response = cityResponses[2], Emoji = "👍", CreatedAt = now.AddDays(-6).AddHours(3), Youth = cityYouths[7] },
            new() { Response = cityResponses[3], Emoji = "🙌", CreatedAt = now.AddDays(-5).AddHours(5), Youth = cityYouths[5] },
            new() { Response = cityResponses[4], Emoji = "✅", CreatedAt = now.AddDays(-4).AddHours(3), Youth = cityYouths[3] },
            new() { Response = cityResponses[5], Emoji = "👏", CreatedAt = now.AddDays(-3).AddHours(7), Youth = cityYouths[6] },
            new() { Response = cityResponses[6], Emoji = "🧠", CreatedAt = now.AddDays(-2).AddHours(8), Youth = cityYouths[4] },
            new() { Response = cityResponses[7], Emoji = "🎯", CreatedAt = now.AddDays(-1).AddHours(4), Youth = cityYouths[2] }
        };

        context.ResponseReactions.AddRange(cityReactions);

        #endregion

        context.SaveChanges();
    }

    private static SingleChoiceQuestion CreateSingleChoiceQuestion(Project project, string text, bool isRequired, params string[] options)
    {
        var question = new SingleChoiceQuestion
        {
            Project = project,
            Text = text,
            Required = isRequired,
        };
        
        var answers = options
            .Select((option, index) => new { option, index })
            .ToList();

        question.PossibleAnswers = answers
            .Select(item => new TextAnswer
            {
                Question = question,
                Value = item.option
            })
            .OrderBy(answer => answers.First(item => item.option == answer.Value).index);

        return question;
    }

    private static OpenQuestion CreateOpenQuestion(Project project, string text, bool isRequired)
    {
        return new OpenQuestion
        {
            Project = project,
            Text = text,
            Required = isRequired
        };
    }
}
