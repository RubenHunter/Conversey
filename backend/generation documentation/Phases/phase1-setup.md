```markdown
# 🚀 Fase 1: Projectstructuur en Afhankelijkheden
**Voorbeeldcode - Mistral Vibe bepaalt de optimale implementatie**

---

## 📌 Doel
Aanmaken van de **projectstructuur** voor Magic Mode **binnen de bestaande ASP.NET Core MVC folderstructuur**, zonder aparte `MagicMode` map. We integreren Magic Mode als een **feature** met:
- **Geen aparte NuGet packages** (alles via API calls of bestaande services).
- **Keuze tussen directe API calls of service-implementatie** (Mistral Vibe analyseert bestaande code en beslist).
- **Hergebruik** van bestaande mappen (`Controllers/`, `Services/`, `Models/`, `wwwroot/`).
- **Consistente theming** met bestaande CSS variabelen.

---

## 📌 Belangrijke Richtlijnen voor Mistral Vibe
> **⚠️ BELANGRIJK**:
> Alle code in dit bestand is **voorbeeldimplementatie** en dient als **inspiratie**. Mistral Vibe zal:
> 1. De **bestaande codebase** analyseren.
> 2. Bepalen of:
>    - Directe **API calls** de beste oplossing zijn.
>    - Een **service-laag** (`IMagicModeApiService`) moet worden geïmplementeerd.
>    - **Bestaande services** (bijv. `IVoxtrallSttService`) moeten worden hergebruikt.
>    - **Partial views** of **Razor components** het meest geschikt zijn.
> 3. De **optimalste structuur en implementatie** selecteren gebaseerd op:
>    - Bestaande **folderstructuur**.
>    - **Theme variabelen** en **stijlrichtlijnen**.
>    - **Bestaande services** en **infrastructuur**.

### Richtlijnen voor Mistral Vibe:
| Aspect               | Richtlijn                                                                                   |
|----------------------|---------------------------------------------------------------------------------------------|
| **API vs Service**   | Als er al een `HttpClient` is geconfigureerd in `Program.cs`, gebruik **directe API calls**. |
| **Service Laag**     | Als er al een service-laag bestaat (bijv. `IVoxtrallSttService`), **hergebruik deze**.       |
| **UI Componenten**   | Gebruik **Razor Views** als er veel `.cshtml` bestanden zijn. Voor moderne apps: **Razor Components**. |
| **Styling**          | Gebruik **bestaande CSS variabelen** (bijv. `--theme-secondary`) uit `wwwroot/css/site.css`.|
| **State Management** | Gebruik **bestaande ViewModels** of pas ze aan voor Magic Mode.                             |
| **Testbaarheid**     | Zorg dat elke component **afzonderlijk testbaar** is (unit tests voor logica, UI tests voor views). |

---

## 🛠 Stappenplan (Met Flexibiliteit voor Mistral Vibe)

---

### **Stap 1: Bepaal de Folderstructuur**
Bepaal waar Magic Mode het beste past in de **bestaande folderstructuur**. Mogelijke opties:

#### **Optie 1: Direct in bestaande folders**
```
UI-MVC/
├── Controllers/
│   └── MagicModeController.cs          # Voorbeeld: Controller met API calls of service-injectie
├── Models/
│   └── MagicMode/                      # Voorbeeld: ViewModels voor Magic Mode
│       ├── MagicModeViewModel.cs
│       └── BubbleModel.cs
├── Services/                          # Voorbeeld: Service-laag of API-client
│   └── MagicMode/
│       ├── IMagicModeApiService.cs     # Voorbeeld: Interface voor API calls
│       └── MagicModeApiService.cs      # Voorbeeld: Implementatie met HttpClient
├── Views/
│   └── MagicMode/                      # Voorbeeld: Razor Views
│       ├── Index.cshtml
│       └── PartialViews/
│           └── _BubblePartial.cshtml
└── wwwroot/
├── css/magic-mode/                 # Voorbeeld: Styling voor Magic Mode
│   └── styles.css
└── js/magic-mode/                  # Voorbeeld: Client-side scripts
└── magicMode.js
```

#### **Optie 2: In een bestaande feature map**
Als er al een feature-map bestaat (bijv. `Features/`), plaats Magic Mode daar:
```
UI-MVC/
├── Features/
│   └── MagicMode/                      # Voorbeeld: Als er een Features-map bestaat
│       ├── Controllers/
│       ├── Models/
│       ├── Services/
│       ├── Views/
│       └── wwwroot/
└── ...
```

> **Mistral Vibe beslist**:
> - Moet Magic Mode in een **submap** (bijv. `Features/MagicMode/`) of **direct in bestaande folders**?
> - Moeten er **nieuwe mappen** worden aangemaakt of worden bestaande mappen hergebruikt?

---

### **Stap 2: Maak de Basisbestanden aan**
Hier zijn **voorbeeldimplementaties** voor de basisbestanden. Mistral Vibe zal de bestaande code analyseren en beslissen welke implementatie het beste past.

---

#### **Voorbeeld 1: MagicModeController.cs**
Kies **één van de volgende implementaties** gebaseerd op bestaande code:

##### **Optie A: Controller met Directe API Calls**
```csharp
// UI-MVC/Controllers/MagicModeController.cs
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace UI_MVC.Controllers
{
    public class MagicModeController : Controller
    {
        private readonly HttpClient _httpClient;

        public MagicModeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MagicModeApi");
        }

        public IActionResult Index()
        {
            var model = new MagicModeViewModel
            {
                QuestionText = "Vertel ons over je ervaring met onze dienst...",
                IsRecording = false,
                Bubbles = new List<BubbleModel>()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> StartRecording()
        {
            // Voorbeeld: Call naar Voxtrall API (gebruik bestaande API endpoint)
            var transcript = await _httpClient.GetStringAsync("/api/voxtrall/start-recording");

            // Voorbeeld: Call naar Mistral AI API
            var bubblesResponse = await _httpClient.PostAsJsonAsync("/api/mistral/key-phrases", new
            {
                input = transcript,
                max_phrases = 5
            });

            var bubbles = await bubblesResponse.Content.ReadFromJsonAsync<List<string>>();
            return PartialView("_BubblesPartial", bubbles);
        }

        [HttpPost]
        public IActionResult StopRecording()
        {
            _httpClient.PostAsync("/api/voxtrall/stop-recording", null);
            return Ok();
        }
    }
}
```

##### **Optie B: Controller met Service-Injectie**
```csharp
// UI-MVC/Controllers/MagicModeController.cs
using Microsoft.AspNetCore.Mvc;
using UI_MVC.Services.MagicMode;

namespace UI_MVC.Controllers
{
    public class MagicModeController : Controller
    {
        private readonly IMagicModeApiService _magicModeService;

        public MagicModeController(IMagicModeApiService magicModeService)
        {
            _magicModeService = magicModeService;
        }

        public IActionResult Index()
        {
            var model = new MagicModeViewModel
            {
                QuestionText = "Vertel ons over je ervaring met onze dienst...",
                IsRecording = false,
                Bubbles = new List<BubbleModel>()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> StartRecording()
        {
            var transcript = await _magicModeService.StartRecording();
            var bubbles = await _magicModeService.GenerateBubbles(transcript);
            return PartialView("_BubblesPartial", bubbles);
        }

        [HttpPost]
        public IActionResult StopRecording()
        {
            _magicModeService.StopRecording();
            return Ok();
        }
    }
}
```

> **Mistral Vibe beslist**:
> - Gebruikt de applicatie al een **service-laag**? Zo ja, kies **Optie B**.
> - Wordt er **directe API communicatie** gebruikt? Kies **Optie A**.

---

#### **Voorbeeld 2: IMagicModeApiService.cs en MagicModeApiService.cs**
Alleen implementeren als **Optie B** (service-laag) wordt gekozen.

##### **IMagicModeApiService.cs**
```csharp
// UI-MVC/Services/MagicMode/IMagicModeApiService.cs
using System.Threading.Tasks;

namespace UI_MVC.Services.MagicMode
{
    public interface IMagicModeApiService
    {
        Task<string> StartRecording();
        void StopRecording();
        Task<List<string>> GenerateBubbles(string transcript);
    }
}
```

##### **MagicModeApiService.cs**
```csharp
// UI-MVC/Services/MagicMode/MagicModeApiService.cs
using System.Net.Http;
using System.Threading.Tasks;

namespace UI_MVC.Services.MagicMode
{
    public class MagicModeApiService : IMagicModeApiService
    {
        private readonly HttpClient _httpClient;

        public MagicModeApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MagicModeApi");
        }

        public async Task<string> StartRecording()
        {
            return await _httpClient.GetStringAsync("/api/voxtrall/start-recording");
        }

        public void StopRecording()
        {
            _httpClient.PostAsync("/api/voxtrall/stop-recording", null);
        }

        public async Task<List<string>> GenerateBubbles(string transcript)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/mistral/key-phrases", new
            {
                input = transcript,
                max_phrases = 5
            });
            return await response.Content.ReadFromJsonAsync<List<string>>();
        }
    }
}
```

> **Mistral Vibe beslist**:
> - Bestaat er al een `HttpClient` configuratie in `Program.cs`? Gebruik deze.
> - Moet de service **directe API calls** doen of **bestaande services** (bijv. `IVoxtrallSttService`) hergebruiken?

---

#### **Voorbeeld 3: ViewModels (MagicModeViewModel.cs en BubbleModel.cs)**
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
> - Moeten de ViewModels **aangepast** worden aan bestaande ViewModels?
> - Moeten de ViewModels in een **bestaande map** (bijv. `Models/`) of in een **submap** (bijv. `Models/MagicMode/`)?

---

#### **Voorbeeld 4: Razor Views**
Kies **één van de volgende opties** gebaseerd op bestaande UI-praktijken:

##### **Optie A: Traditionele Razor Views**
```html
@* UI-MVC/Views/MagicMode/Index.cshtml *@
@model MagicModeViewModel

<div class="magic-mode-container">
    <h2>@Model.QuestionText</h2>

    <div class="bubble-container" id="bubbles">
        @if (Model.Bubbles.Any())
        {
            foreach (var bubble in Model.Bubbles)
            {
                @Html.Partial("PartialViews/_BubblePartial", bubble)
            }
        }
        else
        {
            <p class="placeholder">Klik op de microfoon om te beginnen met spreken...</p>
        }
    </div>

    <button id="microphoneButton" class="btn-microphone" onclick="toggleRecording()">
        @(Model.IsRecording ? "⏸ Pauzeer" : "🎤")
        <span>@(Model.IsRecording ? " Pauzeer" : " Opnemen")</span>
    </button>

    <button id="closeButton" class="btn-close" onclick="closeMagicMode()">
        Sluiten
    </button>
</div>

@section Styles {
    <link rel="stylesheet" href="~/css/magic-mode/styles.css" />
}

@section Scripts {
    <script src="~/js/magic-mode/magicMode.js"></script>
}
```

##### **Optie B: Razor Components (Blazor)**
Als de applicatie Blazor ondersteunt:
```html
@* UI-MVC/Components/MagicMode.razor *@
@using UI_MVC.Models.MagicMode

<MagicModeContainer QuestionText="@Model.QuestionText">
    @if (Model.Bubbles.Any())
    {
        @foreach (var bubble in Model.Bubbles)
        {
            <Bubble Text="@bubble.Text" OnRemove="() => RemoveBubble(bubble.Id)" />
        }
    }
    else
    {
        <p class="placeholder">Klik op de microfoon om te beginnen met spreken...</p>
    }
</MagicModeContainer>

<button @onclick="ToggleRecording" class="btn-microphone">
    @(Model.IsRecording ? "⏸ Pauzeer" : "🎤")
    <span>@(Model.IsRecording ? " Pauzeer" : " Opnemen")</span>
</button>

<button @onclick="CloseMagicMode" class="btn-close">
    Sluiten
</button>

@code {
    [Parameter]
    public MagicModeViewModel Model { get; set; }

    private void ToggleRecording() { /* ... */ }
    private void CloseMagicMode() { /* ... */ }
}
```

> **Mistral Vibe beslist**:
> - Wordt er gebruikgemaakt van **Razor Views** of **Razor Components**?
> - Moeten **partial views** worden gebruikt voor bubbels?

---

#### **Voorbeeld 5: _BubblePartial.cshtml**
```html
@* UI-MVC/Views/MagicMode/PartialViews/_BubblePartial.cshtml *@
@model BubbleModel

<div class="bubble" id="bubble-@Model.Id">
    <p>@Model.Text</p>
    <button onclick="removeBubble(@Model.Id)" class="bubble-close">X</button>
</div>
```

---

#### **Voorbeeld 6: magicMode.js**
```javascript
// UI-MVC/wwwroot/js/magic-mode/magicMode.js
function toggleRecording() {
    const button = document.getElementById('microphoneButton');
    const isRecording = button.classList.contains('recording');

    if (isRecording) {
        fetch('/MagicMode/StopRecording', { method: 'POST' })
            .then(() => {
                button.classList.remove('recording');
                button.querySelector('span').textContent = ' Opnemen';
            });
    } else {
        fetch('/MagicMode/StartRecording', { method: 'POST' })
            .then(response => response.text())
            .then(bubblesHtml => {
                document.getElementById('bubbles').innerHTML = bubblesHtml;
                button.classList.add('recording');
                button.querySelector('span').textContent = ' Pauzeer';
            });
    }
}

function removeBubble(bubbleId) {
    const bubble = document.getElementById(`bubble-${bubbleId}`);
    bubble.style.opacity = '0';
    setTimeout(() => bubble.remove(), 300);
}

function closeMagicMode() {
    alert("Magic Mode afgesloten!");
}
```

> **Mistral Vibe beslist**:
> - Moeten er **bestaande JavaScript libraries** (bijv. jQuery, Axios) worden gebruikt?
> - Moet de logica in een **moderne ES6 module** worden geschreven?

---
#### **Voorbeeld 7: styles.css**
```css
/* UI-MVC/wwwroot/css/magic-mode/styles.css */
.magic-mode-container {
    max-width: 800px;
    margin: 0 auto;
    padding: 20px;
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.bubble-container {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
    gap: 15px;
    max-height: 500px;
    overflow-y: auto;
    padding: 10px;
    margin: 20px 0;
    border: 1px solid var(--theme-border-color);
    border-radius: var(--theme-border-radius);
    background-color: var(--theme-surface);
}

.bubble {
    background-color: var(--theme-secondary);
    color: var(--theme-text);
    padding: 12px 15px;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    animation: fadeIn 0.3s ease-out;
}

.bubble-close {
    background: none;
    border: none;
    color: var(--theme-error);
    cursor: pointer;
    float: right;
}

@keyframes fadeIn {
    from { opacity: 0; transform: translateY(10px); }
    to { opacity: 1; transform: translateY(0); }
}

.btn-microphone, .btn-close {
    padding: 10px 20px;
    border: none;
    border-radius: 8px;
    font-size: 16px;
    cursor: pointer;
    margin: 5px;
}

.btn-microphone {
    background-color: var(--theme-primary);
    color: white;
}

.btn-microphone.recording {
    background-color: var(--theme-error);
    animation: pulse 1.5s infinite;
}

.btn-close {
    background-color: var(--theme-secondary);
    color: var(--theme-text);
}

.placeholder {
    color: var(--theme-text-secondary);
    font-style: italic;
    text-align: center;
    margin-top: 50px;
}
```

> **Mistral Vibe beslist**:
> - Moet de styling **inline** of in een **afzonderlijk CSS-bestand**?
> - Moeten **bestaande animaties** of **custom CSS** worden gebruikt?

---

### **Stap 3: Configureer HttpClient in Program.cs**
**Voorbeeldimplementatie**. Mistral Vibe zal bepalen of deze nodig is:

```csharp
// UI-MVC/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Voorbeeld: Configureer HttpClient voor externe API calls
// Mistral Vibe beslist: Is dit nodig of wordt er een andere methode gebruikt?
builder.Services.AddHttpClient("MagicModeApi", client =>
{
    client.BaseAddress = new Uri("https://api.yourdomain.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

> **Mistral Vibe beslist**:
> - Wordt `HttpClient` al gebruikt in de applicatie? Zo ja, **hergebruik de bestaande configuratie**.
> - Moet een **aparte HttpClient** worden aangemaakt voor Magic Mode?

---

## 🧪 Testinstructies

### **1. Handmatige Test**
1. **Start de applicatie**:
   - Run de applicatie via Visual Studio (`F5`) of met `dotnet run`.
2. **Navigeer naar Magic Mode**:
   - Voeg een link toe in je survey-pagina (bijv. `<a asp-controller="MagicMode" asp-action="Index">Open Magic Mode</a>`).
3. **Test de functionaliteit**:
   - Klik op de **microfoonknop** en spreek een zin.
   - Controleer of:
      - De knop pulseert tijdens opname.
      - Er **bubbels** verschijnen in de container.
      - Je bubbels kunt **verwijderen** met de "X"-knop.
      - De **sluitknop** werkt.

### **2. Automatische Test (Optioneel)**
Voeg een **xUnit-test** toe om de controller te testen:

```csharp
// Tests/MagicModeControllerTests.cs
using Xunit;
using UI_MVC.Controllers;
using UI_MVC.Models.MagicMode;
using Microsoft.AspNetCore.Mvc;

namespace UI_MVC.Tests
{
    public class MagicModeControllerTests
    {
        [Fact]
        public void Index_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var controller = new MagicModeController(null);

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MagicModeViewModel>(viewResult.Model);
            Assert.Equal("Vertel ons over je ervaring met onze dienst...", model.QuestionText);
            Assert.Empty(model.Bubbles);
        }
    }
}
```

Voer de test uit met:
```bash
dotnet test
```

---

## ⚠ Veelvoorkomende Problemen en Oplossingen
Mistral Vibe zal deze problemen analyseren en oplossingen voorstellen gebaseerd op de bestaande codebase.

| Probleem                                      | Mogelijke Oplossing                                                                 |
|-----------------------------------------------|------------------------------------------------------------------------------------|
| **CSS wordt niet toegepast**                  | Controleer of het bestand is gelinked in de view. Mistral Vibe kan een **bestaande link** suggereeren. |
| **JavaScript werkt niet**                     | Mistral Vibe kan bepalen of de script is geladen via **bestaande bundeling** (bijv. Webpack). |
| **API call retourneert geen transcriptie**   | Mistral Vibe kan de **API endpoint URL** aanpassen of **authenticatie** toevoegen. |
| **Bubbels renderen niet**                     | Mistral Vibe kan de **partial view** of **model-data** aanpassen.                  |
| **Animaties werken niet**                     | Mistral Vibe kan bestaande **CSS klassen** of **animaties** hergebruiken.         |

---
## 📌 Changelog
| Datum       | Wijziging                                  | Door          |
|-------------|--------------------------------------------|---------------|
| 2026-04-26  | Initiële projectstructuur (voorbeeld)      | Mistral Vibe  |
| 2026-04-26  | Voorbeelden voor API vs Service-laag       | Mistral Vibe  |
| 2026-04-26  | Voorbeelden voor Razor Views/Blazor       | Mistral Vibe  |

---
## 🔹 Samenvatting voor Mistral Vibe
> **✅ Wat Mistral Vibe moet doen**:
> 1. **Analyseer de bestaande codebase** (folderstructuur, services, UI-praktijken).
> 2. **Kies de optimale implementatie** voor:
     >    - **Folderstructuur** (bestaande mappen vs nieuwe submap).
>    - **API vs Service-laag** (gebruik bestaande services indien mogelijk).
>    - **UI Componenten** (Razor Views vs Razor Components).
>    - **Styling** (bestaande CSS vs custom).
>    - **State Management** (bestaande ViewModels vs nieuwe).
> 3. **Implementeer de gekozen oplossing** en zorg dat het:
     >    - **Consistent** is met de bestaande code.
>    - **Testbaar** is.
>    - **Onderhoudbaar** is.
```