```markdown
# 🔊 Fase 2: STT-Integratie
**Voorbeeldcode - Mistral Vibe bepaalt de optimale implementatie**

---

## 📌 Doel
In deze fase wordt de **STT-integratie** voor Magic Mode geïmplementeerd. Mistral Vibe zal:
1. Analyseren of **directe API calls** naar Voxtrall het beste passen, of dat een **service-laag** moet worden gebruikt.
2. De **bestaande STT-logica** hergebruiken (bijv. de 5-seconden buffer en periodieke transcriptie om de 2 seconden).
3. Zorgen dat de opname correct start, stopt, en transcripties worden verwerkt.
4. **Testen** of de STT-functionaliteit werkt zoals verwacht.

---
## 📌 Belangrijke Richtlijnen voor Mistral Vibe
> **⚠️ BELANGRIJK**:
> Alle code in dit bestand is **voorbeeldimplementatie** en dient als **inspiratie**. Mistral Vibe zal:
> 1. De **bestaande Voxtrall-integratie** analyseren.
> 2. Bepalen of:
>    - **Directe API calls** naar Voxtrall de beste oplossing zijn.
>    - Een **service-laag** (`IVoxtrallSttService`) moet worden geïmplementeerd.
>    - De **bestaande STT-logica** moet worden hergebruikt of aangepast.
> 3. De **optimalste integratie** selecteren gebaseerd op:
>    - Bestaande **folderstructuur** (bijv. `Services/`, `Models/`).
>    - **Theme variabelen** en **stijlrichtlijnen**.
>    - **Bestaande services** (bijv. `IVoxtrallSttService`).

### Richtlijnen voor Mistral Vibe:
| Aspect               | Richtlijn                                                                                   |
|----------------------|---------------------------------------------------------------------------------------------|
| **API vs Service**   | Als er al een `IVoxtrallSttService` bestaat, **hergebruik deze**. Anders gebruik **directe API calls**. |
| **Bestaande Logica** | Als de bestaande STT-logica een **5s buffer** en **periodieke transcriptie om de 2s** gebruikt, **hergebruik deze**. |
| **State Management** | Zorg dat de STT-status (opname/pauze) wordt bijgehouden in het `MagicModeViewModel`.        |
| **Testbaarheid**     | Zorg dat de STT-functionaliteit **afzonderlijk testbaar** is (bijv. met mock API calls).   |

---
---

## 🛠 Stappenplan (Met Flexibiliteit voor Mistral Vibe)

---

### **Stap 1: Bepaal de Locatie van STT-Logica**
Mistral Vibe zal bepalen waar de STT-logica het beste past in de bestaande folderstructuur.

#### **Optie 1: In de MagicMode Service Map**
```
UI-MVC/
├── Services/
│   └── MagicMode/
│       ├── SttService.cs               # Voorbeeld: Service voor Voxtrall STT
│       └── ISttService.cs              # Voorbeeld: Interface voor STT-logica
└── ...
```

#### **Optie 2: Direct in de MagicModeController**
```csharp
// UI-MVC/Controllers/MagicModeController.cs
// Voorbeeld: STT-logica rechtstreeks in de controller
```

#### **Optie 3: In een Bestaande Service Map**
```
UI-MVC/
├── Services/
│   └── Stt/                           # Voorbeeld: Bestaande STT-service map
│       ├── VoxtrallSttService.cs      # Voorbeeld: Implementatie voor Voxtrall
│       └── ISttService.cs
└── ...
```

> **Mistral Vibe beslist**:
> - Moet de STT-logica in een **aparte service-map** (bijv. `Services/MagicMode/`)?
> - Moet de STT-logica **direct in de controller** worden geplaatst?
> - Moet de STT-logica **in een bestaande service-map** (bijv. `Services/Stt/`)?

---
---

### **Stap 2: Implementeer de STT-Logica**
Hier zijn **voorbeeldimplementaties** voor STT-integratie. Mistral Vibe zal de beste optie kiezen.

---

#### **Voorbeeld 1: STT-Interface (ISttService.cs)**
```csharp
// UI-MVC/Services/MagicMode/ISttService.cs
using System.Threading.Tasks;

namespace UI_MVC.Services.MagicMode
{
    public interface ISttService
    {
        Task<string> StartRecording();
        void StopRecording();
        bool IsRecording { get; }
    }
}
```

> **Mistral Vibe beslist**:
> - Moet de interface **aangepast** worden aan bestaande interfaces?
> - Moet de interface in een **bestaande map** (bijv. `Services/`) of in een **submap** (bijv. `Services/MagicMode/`)?

---

#### **Voorbeeld 2: STT-Service met Directe API Calls**
```csharp
// UI-MVC/Services/MagicMode/SttService.cs
using System.Net.Http;
using System.Threading.Tasks;

namespace UI_MVC.Services.MagicMode
{
    public class SttService : ISttService
    {
        private readonly HttpClient _httpClient;
        private bool _isRecording = false;

        public SttService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("SttApi");
        }

        public bool IsRecording => _isRecording;

        public async Task<string> StartRecording()
        {
            _isRecording = true;
            // Voorbeeld: Call naar Voxtrall API om opname te starten
            var response = await _httpClient.PostAsync("/api/voxtrall/start", null);
            if (response.IsSuccessStatusCode)
            {
                // Voorbeeld: Wacht 5 seconden voor de buffer (Voxtrall-specificatie)
                await Task.Delay(5000);
                return await response.Content.ReadAsStringAsync();
            }
            throw new HttpRequestException("STT opname mislukt");
        }

        public void StopRecording()
        {
            _isRecording = false;
            _httpClient.PostAsync("/api/voxtrall/stop", null);
        }
    }
}
```

> **Mistral Vibe beslist**:
> - Moet de service **directe API calls** doen naar Voxtrall?
> - Moet de **5s buffer** en **periodieke transcriptie om de 2s** handmatig worden geïmplementeerd?

---
#### **Voorbeeld 3: STT-Service met Bestaande Voxtrall-Logica**
Als er al een `IVoxtrallSttService` bestaat in de applicatie:

```csharp
// UI-MVC/Services/MagicMode/SttService.cs
using System.Threading.Tasks;

namespace UI_MVC.Services.MagicMode
{
    public class SttService : ISttService
    {
        private readonly IVoxtrallSttService _voxtrallService;
        private bool _isRecording = false;

        public SttService(IVoxtrallSttService voxtrallService)
        {
            _voxtrallService = voxtrallService;
        }

        public bool IsRecording => _isRecording;

        public async Task<string> StartRecording()
        {
            _isRecording = true;
            // Hergebruik bestaande Voxtrall-logica
            return await _voxtrallService.StartRecording();
        }

        public void StopRecording()
        {
            _isRecording = false;
            _voxtrallService.StopRecording();
        }
    }
}
```

> **Mistral Vibe beslist**:
> - Moet de bestaande `IVoxtrallSttService` worden **hergebruikt**?
> - Moet de service **aanpassingen** ondergaan om compatibel te zijn met Magic Mode?

---
#### **Voorbeeld 4: STT-Logica Direct in de Controller**
Als Mistral Vibe beslist om de STT-logica **direct in de controller** te plaatsen:

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
        private bool _isRecording = false;

        public MagicModeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("SttApi");
        }

        [HttpPost]
        public async Task<IActionResult> StartRecording()
        {
            _isRecording = true;
            // Voorbeeld: Call naar Voxtrall API om opname te starten
            var response = await _httpClient.PostAsync("/api/voxtrall/start", null);
            if (response.IsSuccessStatusCode)
            {
                // Voorbeeld: Wacht 5 seconden voor de buffer
                await Task.Delay(5000);
                var transcript = await response.Content.ReadAsStringAsync();
                return Ok(transcript);
            }
            return StatusCode(500, "STT opname mislukt");
        }

        [HttpPost]
        public IActionResult StopRecording()
        {
            _isRecording = false;
            _httpClient.PostAsync("/api/voxtrall/stop", null);
            return Ok();
        }
    }
}
```

> **Mistral Vibe beslist**:
> - Moet de STT-logica **in de controller** worden geplaatst?
> - Moet de **status (IsRecording)** in de controller worden bijgehouden?

---
---

### **Stap 3: Update de MagicModeController voor STT**
Pas de `MagicModeController` aan om de STT-logica te integreren.

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
            return PartialView("_BubblesPartial", bubbles);
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
> - Moet de controller **de STT-status (IsRecording)** bijhouden?
> - Moet de controller **direct de STT-logica** aanroepen of een **service** gebruiken?

---
---

### **Stap 4: Update de MagicModeViewModel voor STT-Status**
Voeg de `IsRecording` status toe aan het `MagicModeViewModel`:

```csharp
// UI-MVC/Models/MagicMode/MagicModeViewModel.cs
using System.Collections.Generic;

namespace UI_MVC.Models.MagicMode
{
    public class MagicModeViewModel
    {
        public string QuestionText { get; set; }
        public bool IsRecording { get; set; } // Voorbeeld: Status van de opname
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
> - Moet het `MagicModeViewModel` worden **uitgebreid** met STT-status?
> - Moet de STT-status in een **bestaand ViewModel** worden geïntegreerd?

---
---

### **Stap 5: Update de Client-Side Logica (magicMode.js)**
Pas het JavaScript aan om de microfoonknop te laten reageren op de STT-status:

```javascript
// UI-MVC/wwwroot/js/magic-mode/magicMode.js
let isRecording = false;

function toggleRecording() {
    const button = document.getElementById('microphoneButton');

    if (!isRecording) {
        // Start opname
        fetch('/MagicMode/StartRecording', { method: 'POST' })
            .then(response => {
                if (response.ok) {
                    isRecording = true;
                    button.classList.add('recording');
                    button.querySelector('span').textContent = ' Pauzeer';
                } else {
                    alert("STT opname mislukt. Probeer opnieuw.");
                }
            });
    } else {
        // Pauzeer opname
        fetch('/MagicMode/StopRecording', { method: 'POST' })
            .then(() => {
                isRecording = false;
                button.classList.remove('recording');
                button.querySelector('span').textContent = ' Opnemen';
            });
    }
}
```

> **Mistral Vibe beslist**:
> - Moet de client-side logica **de STT-status (isRecording)** bijhouden?
> - Moet de client-side logica **foutafhandeling** bevatten voor STT mislukkingen?

---
---

### **Stap 6: Configureer HttpClient voor STT in Program.cs**
Voeg de `HttpClient` configuratie toe voor STT-API calls:

```csharp
// UI-MVC/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Voorbeeld: Configureer HttpClient voor STT-API calls
// Mistral Vibe beslist: Is dit nodig of wordt een andere methode gebruikt?
builder.Services.AddHttpClient("SttApi", client =>
{
    client.BaseAddress = new Uri("https://api.voxtrall.com/"); // Vervang door de juiste Voxtrall API URL
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

> **Mistral Vibe beslist**:
> - Moet een **aparte HttpClient** worden aangemaakt voor STT?
> - Moet de `BaseAddress` worden aangepast aan de **juiste Voxtrall API URL**?

---
---

## 🧪 Testinstructies

### **1. Handmatige Test**
1. **Start de applicatie**:
    - Run de applicatie via Visual Studio (`F5`) of met `dotnet run`.
2. **Navigeer naar Magic Mode**:
    - Voeg een link toe in je survey-pagina (bijv. `<a asp-controller="MagicMode" asp-action="Index">Open Magic Mode</a>`).
3. **Test de STT-functionaliteit**:
    - Klik op de **microfoonknop** om de opname te starten.
    - Spreek een zin en wacht tot de **bubbels** verschijnen.
    - Klik op de microfoonknop om de opname te pauzeren.
    - Controleer of:
        - De knop **pulseert** tijdens opname.
        - De `IsRecording` status correct wordt bijgehouden.
        - De transcriptie **correct wordt gegenereerd** en bubbels worden getoond.

### **2. Automatische Test (Optioneel)**
Voeg een **xUnit-test** toe om de STT-service te testen:

```csharp
// Tests/SttServiceTests.cs
using Xunit;
using UI_MVC.Services.MagicMode;
using Moq;
using System.Threading.Tasks;

namespace UI_MVC.Tests
{
    public class SttServiceTests
    {
        [Fact]
        public async Task StartRecording_ReturnsTranscript()
        {
            // Arrange
            var mockHttp = new Mock<IHttpClientFactory>();
            var mockClient = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockClient.Object);
            mockHttp.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var service = new SttService(mockHttp.Object);

            // Act
            var transcript = await service.StartRecording();

            // Assert
            Assert.NotNull(transcript);
            Assert.True(service.IsRecording);
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
| **STT opname start niet**                     | Controleer de **API endpoint URL** in `Program.cs` en de **authenticatie**.       |
| **Transcriptie komt niet binnen**             | Mistral Vibe kan de **buffer-tijd (5s)** of **periodieke transcriptie (om de 2s)** aanpassen. |
| **Microfoonknop reageert niet**              | Mistral Vibe kan de **client-side logica** of **server-side controller** aanpassen. |
| **STT-status (IsRecording) wordt niet bijgehouden** | Mistral Vibe kan de **controller** of **service** aanpassen om de status bij te houden. |
| **Fouten in de console**                     | Mistral Vibe kan **foutafhandeling** toevoegen aan de STT-logica.                 |

---
---

## 📌 Changelog
| Datum       | Wijziging                                  | Door          |
|-------------|--------------------------------------------|---------------|
| 2026-04-26  | Initiële STT-integratie (voorbeeld)        | Mistral Vibe  |
| 2026-04-26  | Voorbeelden voor API vs Service-laag       | Mistral Vibe  |
| 2026-04-26  | Client-side logica voor STT bijgewerkt    | Mistral Vibe  |
```