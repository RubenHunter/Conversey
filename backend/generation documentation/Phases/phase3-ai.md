```markdown
# 🤖 Fase 3: Mistral AI-Integratie (Bubbelgeneratie)
**Voorbeeldcode - Mistral Vibe bepaalt de optimale implementatie**

---

## 📌 Doel
In deze fase wordt de **Mistral AI-integratie** voor Magic Mode geïmplementeerd. Mistral Vibe zal:
1. Analyseren of **directe API calls** naar Mistral AI het beste passen, of dat een **service-laag** moet worden gebruikt.
2. Zorgen dat transcripties worden omgezet in **korte tekstfragmenten (bubbels)**.
3. De **AI-service** integreren met de bestaande STT-logica.
4. **Testen** of de bubbelgeneratie correct werkt.

---
## 📌 Belangrijke Richtlijnen voor Mistral Vibe
> **⚠️ BELANGRIJK**:
> Alle code in dit bestand is **voorbeeldimplementatie** en dient als **inspiratie**. Mistral Vibe zal:
> 1. De **bestaande Mistral AI-integratie** (indien aanwezig) analyseren.
> 2. Bepalen of:
>    - **Directe API calls** naar Mistral AI de beste oplossing zijn.
>    - Een **service-laag** (`IAiService`) moet worden geïmplementeerd.
>    - De **bestaande AI-logica** moet worden hergebruikt of aangepast.
> 3. De **optimalste integratie** selecteren gebaseerd op:
>    - Bestaande **folderstructuur** (bijv. `Services/`, `Models/`).
>    - **Theme variabelen** en **stijlrichtlijnen**.
>    - **Bestaande services** (bijv. `IAiService`).

### Richtlijnen voor Mistral Vibe:
| Aspect               | Richtlijn                                                                                   |
|----------------------|---------------------------------------------------------------------------------------------|
| **API vs Service**   | Als er al een `IAiService` bestaat, **hergebruik deze**. Anders gebruik **directe API calls**. |
| **Bestaande Logica** | Als de bestaande AI-logica een **key_phrases** endpoint gebruikt, **hergebruik deze**.    |
| **State Management** | Zorg dat de gegenereerde bubbels worden bijgehouden in het `MagicModeViewModel`.             |
| **Testbaarheid**     | Zorg dat de AI-functionaliteit **afzonderlijk testbaar** is (bijv. met mock API calls).      |

---
---

## 🛠 Stappenplan (Met Flexibiliteit voor Mistral Vibe)

---

### **Stap 1: Bepaal de Locatie van AI-Logica**
Mistral Vibe zal bepalen waar de AI-logica het beste past in de bestaande folderstructuur.

#### **Optie 1: In de MagicMode Service Map**
```
UI-MVC/
├── Services/
│   └── MagicMode/
│       ├── AiService.cs               # Voorbeeld: Service voor Mistral AI
│       └── IAiService.cs              # Voorbeeld: Interface voor AI-logica
└── ...
```

#### **Optie 2: Direct in de MagicModeController**
```csharp
// UI-MVC/Controllers/MagicModeController.cs
// Voorbeeld: AI-logica rechtstreeks in de controller
```

#### **Optie 3: In een Bestaande Service Map**
```
UI-MVC/
├── Services/
│   └── Ai/                           # Voorbeeld: Bestaande AI-service map
│       ├── MistralAiService.cs       # Voorbeeld: Implementatie voor Mistral AI
│       └── IAiService.cs
└── ...
```

> **Mistral Vibe beslist**:
> - Moet de AI-logica in een **aparte service-map** (bijv. `Services/MagicMode/`)?
> - Moet de AI-logica **direct in de controller** worden geplaatst?
> - Moet de AI-logica **in een bestaande service-map** (bijv. `Services/Ai/`)?

---
---

### **Stap 2: Implementeer de AI-Interface (IAiService.cs)**
Hier zijn **voorbeeldimplementaties** voor de AI-interface. Mistral Vibe zal de beste optie kiezen.

---

#### **Voorbeeld 1: AI-Interface**
```csharp
// UI-MVC/Services/MagicMode/IAiService.cs
using System.Threading.Tasks;

namespace UI_MVC.Services.MagicMode
{
    public interface IAiService
    {
        Task<List<string>> GenerateBubbles(string transcript);
    }
}
```

> **Mistral Vibe beslist**:
> - Moet de interface **aangepast** worden aan bestaande interfaces?
> - Moet de interface in een **bestaande map** (bijv. `Services/`) of in een **submap** (bijv. `Services/MagicMode/`)?

---
---

### **Stap 3: Implementeer de AI-Service**
Hier zijn **voorbeeldimplementaties** voor de AI-service. Mistral Vibe zal de beste optie kiezen.

---

#### **Voorbeeld 1: AI-Service met Directe API Calls**
```csharp
// UI-MVC/Services/MagicMode/AiService.cs
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace UI_MVC.Services.MagicMode
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;

        public AiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MistralApi");
        }

        public async Task<List<string>> GenerateBubbles(string transcript)
        {
            var request = new
            {
                input = transcript,
                max_phrases = 5
            };

            var response = await _httpClient.PostAsJsonAsync("/api/mistral/key-phrases", request);

            if (response.IsSuccessStatusCode)
            {
                var bubbles = await response.Content.ReadFromJsonAsync<List<string>>();
                return bubbles ?? new List<string>();
            }

            // Voorbeeld: Fallback als de API call mislukt
            return new List<string> { transcript }; // Toon de originele transcriptie als fallback
        }
    }
}
```

> **Mistral Vibe beslist**:
> - Moet de service **directe API calls** doen naar Mistral AI?
> - Moet er een **fallback** worden geïmplementeerd als de API call mislukt?

---
#### **Voorbeeld 2: AI-Service met Bestaande AI-Logica**
Als er al een `IAiService` of `MistralAiService` bestaat in de applicatie:

```csharp
// UI-MVC/Services/MagicMode/AiService.cs
using System.Threading.Tasks;

namespace UI_MVC.Services.MagicMode
{
    public class AiService : IAiService
    {
        private readonly IMistralAiService _mistralService;

        public AiService(IMistralAiService mistralService)
        {
            _mistralService = mistralService;
        }

        public async Task<List<string>> GenerateBubbles(string transcript)
        {
            // Hergebruik bestaande Mistral AI-logica
            return await _mistralService.GenerateKeyPhrases(transcript, maxPhrases: 5);
        }
    }
}
```

> **Mistral Vibe beslist**:
> - Moet de bestaande `IMistralAiService` worden **hergebruikt**?
> - Moet de service **aanpassingen** ondergaan om compatibel te zijn met Magic Mode?

---
---

### **Stap 4: Update de MagicModeController voor AI-Integratie**
Pas de `MagicModeController` aan om de AI-logica te integreren.

#### **Voorbeeld 1: Controller met Service-Injectie**
```csharp
// UI-MVC/Controllers/MagicModeController.cs
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UI_MVC.Services.MagicMode;

namespace UI_MVC.Controllers
{
    public class MagicModeController : Controller
    {
        private readonly ISttService _sttService;
        private readonly IAiService _aiService;

        public MagicModeController(ISttService sttService, IAiService aiService)
        {
            _sttService = sttService;
            _aiService = aiService;
        }

        public IActionResult Index()
        {
            var model = new MagicModeViewModel
            {
                QuestionText = "Vertel ons over je ervaring met onze dienst...",
                IsRecording = _sttService.IsRecording,
                Bubbles = new List<BubbleModel>()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> StartRecording()
        {
            var transcript = await _sttService.StartRecording();
            var bubbles = await _aiService.GenerateBubbles(transcript);

            // Voorbeeld: Voeg bubbels toe aan het model voor weergave
            var model = new MagicModeViewModel
            {
                QuestionText = "Vertel ons over je ervaring met onze dienst...",
                IsRecording = _sttService.IsRecording,
                Bubbles = bubbles.Select((text, index) => new BubbleModel { Id = index, Text = text }).ToList()
            };

            return PartialView("_BubblesPartial", model);
        }

        [HttpPost]
        public IActionResult StopRecording()
        {
            _sttService.StopRecording();
            return Ok();
        }
    }
}
```

> **Mistral Vibe beslist**:
> - Moet de controller **de AI-logica** aanroepen na STT-transcriptie?
> - Moet de controller **bubbels rechtstreeks renderen** of een model doorgeven?

---
---

### **Stap 5: Update het MagicModeViewModel voor Bubbels**
Voeg de `Bubbles` lijst toe aan het `MagicModeViewModel`:

```csharp
// UI-MVC/Models/MagicMode/MagicModeViewModel.cs
using System.Collections.Generic;

namespace UI_MVC.Models.MagicMode
{
    public class MagicModeViewModel
    {
        public string QuestionText { get; set; }
        public bool IsRecording { get; set; }
        public List<BubbleModel> Bubbles { get; set; } = new();
    }

    public class BubbleModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}
```

> **Mistral Vibe beslist**:
> - Moet het `MagicModeViewModel` worden **uitgebreid** met bubbels?
> - Moet de bubbels in een **bestaand ViewModel** worden geïntegreerd?

---
---

### **Stap 6: Update de Partial View voor Bubbels (_BubblesPartial.cshtml)**
Zorg dat de partial view de bubbels correct weergeeft:

```html
@* UI-MVC/Views/MagicMode/PartialViews/_BubblesPartial.cshtml *@
@model UI_MVC.Models.MagicMode.MagicModeViewModel

@if (Model.Bubbles.Any())
{
    foreach (var bubble in Model.Bubbles)
    {
        @Html.Partial("_BubblePartial", bubble)
    }
}
else
{
    <p class="placeholder">Geen bubbels gegenereerd. Spreek opnieuw.</p>
}
```

---
---

### **Stap 7: Configureer HttpClient voor Mistral AI in Program.cs**
Voeg de `HttpClient` configuratie toe voor Mistral AI-API calls:

```csharp
// UI-MVC/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Voorbeeld: Configureer HttpClient voor Mistral AI-API calls
// Mistral Vibe beslist: Is dit nodig of wordt een andere methode gebruikt?
builder.Services.AddHttpClient("MistralApi", client =>
{
    client.BaseAddress = new Uri("https://api.mistral.ai/"); // Vervang door de juiste Mistral API URL
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("Authorization", "Bearer JULLIE_MISTRAL_API_KEY");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

> **Mistral Vibe beslist**:
> - Moet een **aparte HttpClient** worden aangemaakt voor Mistral AI?
> - Moet de `BaseAddress` en `Authorization` worden aangepast aan de **juiste Mistral API URL**?

---
---

## 🧪 Testinstructies

### **1. Handmatige Test**
1. **Start de applicatie**:
    - Run de applicatie via Visual Studio (`F5`) of met `dotnet run`.
2. **Navigeer naar Magic Mode**:
    - Voeg een link toe in je survey-pagina (bijv. `<a asp-controller="MagicMode" asp-action="Index">Open Magic Mode</a>`).
3. **Test de AI-functionaliteit**:
    - Klik op de **microfoonknop** om de opname te starten.
    - Spreek een zin en wacht tot de **bubbels** verschijnen.
    - Controleer of:
        - De bubbels **correct worden gegenereerd**.
        - De bubbels **zichtbaar** zijn in de UI.
        - De bubbels **kort en betekenisvol** zijn.

### **2. Automatische Test (Optioneel)**
Voeg een **xUnit-test** toe om de AI-service te testen:

```csharp
// Tests/AiServiceTests.cs
using Xunit;
using UI_MVC.Services.MagicMode;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace UI_MVC.Tests
{
    public class AiServiceTests
    {
        [Fact]
        public async Task GenerateBubbles_ReturnsListOfBubbles()
        {
            // Arrange
            var mockHttp = new Mock<IHttpClientFactory>();
            var mockClient = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockClient.Object);
            mockHttp.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var service = new AiService(mockHttp.Object);

            // Act
            var bubbles = await service.GenerateBubbles("Dit is een test transcriptie.");

            // Assert
            Assert.NotNull(bubbles);
            Assert.NotEmpty(bubbles);
        }
    }
}
```

Voer de test uit met:
```bash
dotnet test
```

---
---

## ⚠ Veelvoorkomende Problemen en Oplossingen
Mistral Vibe zal deze problemen analyseren en oplossingen voorstellen gebaseerd op de bestaande codebase.

| Probleem                                      | Mogelijke Oplossing                                                                 |
|-----------------------------------------------|------------------------------------------------------------------------------------|
| **Bubbels worden niet gegenereerd**          | Controleer de **API endpoint URL** in `Program.cs` en de **authenticatie**.       |
| **Mistral AI retourneert lege bubbels**       | Mistral Vibe kan de **prompt** of **max_phrases** aanpassen.                     |
| **Bubbels renderen niet in de UI**            | Mistral Vibe kan de **partial view** of **model-data** aanpassen.                 |
| **API call naar Mistral AI mislukt**          | Mistral Vibe kan **foutafhandeling** toevoegen aan de AI-service.                 |
| **Bubbels zijn te lang of niet betekenisvol**  | Mistral Vibe kan de **Mistral AI endpoint** (bijv. `summarize` of `key_phrases`) aanpassen. |

---
---
## 📌 Changelog
| Datum       | Wijziging                                  | Door          |
|-------------|--------------------------------------------|---------------|
| 2026-04-26  | Initiële Mistral AI-integratie (voorbeeld)| Mistral Vibe  |
| 2026-04-26  | Voorbeelden voor API vs Service-laag       | Mistral Vibe  |
| 2026-04-26  | Client-side logica voor AI bijgewerkt     | Mistral Vibe  |
```