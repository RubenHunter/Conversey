```markdown
# 🗃️ Fase 5: State Management voor Magic Mode
**Voorbeeldcode - Mistral Vibe bepaalt de optimale implementatie**

---

## 📌 Doel
In deze fase wordt het **state management** voor Magic Mode geïmplementeerd. Mistral Vibe zal:
1. Bepalen hoe de **state** (bubbels, STT-status, AI-gegenereerde content) wordt bijgehouden.
2. De **beste aanpak** kiezen voor state management (lokaal in de controller, service-laag, of een dedicated state service).
3. Zorgen dat de state **consistent** is tussen de UI, services en backend.
4. **Testen** of de state correct wordt bijgehouden en bijgewerkt.

---
## 📌 Belangrijke Richtlijnen voor Mistral Vibe
> **⚠️ BELANGRIJK**:
> Alle code in dit bestand is **voorbeeldimplementatie** en dient als **inspiratie**. Mistral Vibe zal:
> 1. De **bestaande state management-praktijken** (lokaal in controllers, service-laag, ViewModels) analyseren.
> 2. Bepalen of:
>    - De state **lokaal in de controller** moet worden bijgehouden.
>    - Een **service-laag** (bijv. `IMagicModeStateService`) moet worden gebruikt.
>    - Een **dedicated state service** (bijv. `MagicModeStateService`) moet worden geïmplementeerd.
>    - **Bestaande ViewModels** moeten worden uitgebreid of een nieuw ViewModel moet worden gemaakt.
> 3. De **optimalste state management-aanpak** selecteren gebaseerd op:
>    - Bestaande **folderstructuur** (bijv. `Services/`, `Models/`).
>    - **Testbaarheid** en **onderhoudbaarheid**.
>    - **Integratie met bestaande code**.

### Richtlijnen voor Mistral Vibe:
| Aspect               | Richtlijn                                                                                   |
|----------------------|---------------------------------------------------------------------------------------------|
| **State Locatie**    | Gebruik **ViewModels** als de applicatie traditioneel MVC is. Gebruik een **service-laag** als er al veel services zijn. |
| **Hergebruik**       | Breid **bestaande ViewModels** uit als ze al worden gebruikt. Maak een **nieuw ViewModel** als dat beter past. |
| **Testbaarheid**     | Zorg dat de state **afzonderlijk testbaar** is (bijv. met unit tests).                       |
| **Consistentie**     | Zorg dat de state **consistent** is tussen de UI, services en backend.                       |

---
---

## 🛠 Stappenplan (Met Flexibiliteit voor Mistral Vibe)

---

### **Stap 1: Bepaal de Locatie van de State**
Mistral Vibe zal bepalen waar de state het beste kan worden bijgehouden.

#### **Optie 1: State in het ViewModel**
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

        // Voorbeeld: Methode om een bubbel toe te voegen
        public void AddBubble(string text)
        {
            Bubbles.Add(new BubbleModel { Id = Bubbles.Count + 1, Text = text });
        }

        // Voorbeeld: Methode om een bubbel te verwijderen
        public void RemoveBubble(int id)
        {
            Bubbles.RemoveAll(b => b.Id == id);
        }

        // Voorbeeld: Methode om de finale tekst te genereren
        public string GetFinalText()
        {
            return string.Join(". ", Bubbles.Select(b => b.Text));
        }
    }

    public class BubbleModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}
```

#### **Optie 2: State in een Service-Laag**
```
UI-MVC/
├── Services/
│   └── MagicMode/
│       ├── IMagicModeStateService.cs  # Voorbeeld: Interface voor state management
│       └── MagicModeStateService.cs   # Voorbeeld: Implementatie van state management
└── ...
```

#### **Optie 3: State in een Dedicated State Service**
```
UI-MVC/
├── Services/
│   └── State/
│       ├── IMagicModeStateService.cs
│       └── MagicModeStateService.cs
└── ...
```

> **Mistral Vibe beslist**:
> - Moet de state **in het ViewModel** worden bijgehouden?
> - Moet een **service-laag** worden gebruikt voor state management?
> - Moet een **dedicated state service** worden geïmplementeerd?

---
---

### **Stap 2: Implementeer State Management in het ViewModel**
Als Mistral Vibe kiest voor **state in het ViewModel**, pas dan het `MagicModeViewModel` aan om de state te beheren.

#### **Voorbeeld: ViewModel met State Management**
```csharp
// UI-MVC/Models/MagicMode/MagicModeViewModel.cs
using System.Collections.Generic;
using System.Linq;

namespace UI_MVC.Models.MagicMode
{
    public class MagicModeViewModel
    {
        public string QuestionText { get; set; }
        public bool IsRecording { get; set; }
        public List<BubbleModel> Bubbles { get; set; } = new();

        /// <summary>
        /// Voegt een bubbel toe aan de state.
        /// </summary>
        /// <param name="text">De tekst van de bubbel.</param>
        public void AddBubble(string text)
        {
            Bubbles.Add(new BubbleModel
            {
                Id = Bubbles.Count > 0 ? Bubbles.Max(b => b.Id) + 1 : 1,
                Text = text
            });
        }

        /// <summary>
        /// Verwijdert een bubbel uit de state.
        /// </summary>
        /// <param name="id">De ID van de bubbel.</param>
        public void RemoveBubble(int id)
        {
            Bubbles.RemoveAll(b => b.Id == id);
        }

        /// <summary>
        /// Genereert de finale tekst uit de state.
        /// </summary>
        /// <returns>De gegenereerde tekst.</returns>
        public string GetFinalText()
        {
            return string.Join(". ", Bubbles.Select(b => b.Text));
        }
    }

    public class BubbleModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}
```

> **Mistral Vibe beslist**:
> - Moet het ViewModel **methodes** bevatten voor state manipulatie (bijv. `AddBubble`, `RemoveBubble`)?
> - Moet het ViewModel **een methode** bevatten voor het genereren van de finale tekst?

---
---

### **Stap 3: Implementeer State Management in een Service-Laag**
Als Mistral Vibe kiest voor een **service-laag**, implementeer dan de volgende interface en service.

#### **Voorbeeld 1: Interface voor State Management**
```csharp
// UI-MVC/Services/MagicMode/IMagicModeStateService.cs
using System.Collections.Generic;

namespace UI_MVC.Services.MagicMode
{
    public interface IMagicModeStateService
    {
        void AddBubble(string text);
        void RemoveBubble(int id);
        List<BubbleModel> GetBubbles();
        string GetFinalText();
    }

    public class BubbleModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}
```

#### **Voorbeeld 2: Implementatie van State Management**
```csharp
// UI-MVC/Services/MagicMode/MagicModeStateService.cs
using System.Collections.Generic;
using System.Linq;

namespace UI_MVC.Services.MagicMode
{
    public class MagicModeStateService : IMagicModeStateService
    {
        private List<BubbleModel> _bubbles = new();

        public void AddBubble(string text)
        {
            _bubbles.Add(new BubbleModel
            {
                Id = _bubbles.Count > 0 ? _bubbles.Max(b => b.Id) + 1 : 1,
                Text = text
            });
        }

        public void RemoveBubble(int id)
        {
            _bubbles.RemoveAll(b => b.Id == id);
        }

        public List<BubbleModel> GetBubbles()
        {
            return _bubbles;
        }

        public string GetFinalText()
        {
            return string.Join(". ", _bubbles.Select(b => b.Text));
        }
    }
}
```

> **Mistral Vibe beslist**:
> - Moet de service **een interface** implementeren?
> - Moet de service **bubbels** opslaan in een **lokale lijst** of een **database**?

---
---

### **Stap 4: Update de Controller voor State Management**
Pas de `MagicModeController` aan om de state te beheren.

#### **Voorbeeld 1: Controller met State in ViewModel**
```csharp
// UI-MVC/Controllers/MagicModeController.cs
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UI_MVC.Models.MagicMode;
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

            var model = new MagicModeViewModel
            {
                QuestionText = "Vertel ons over je ervaring met onze dienst...",
                IsRecording = _sttService.IsRecording,
                Bubbles = new List<BubbleModel>()
            };

            // Voorbeeld: Voeg bubbels toe aan de state
            foreach (var text in bubbles)
            {
                model.AddBubble(text);
            }

            return PartialView("_BubblesPartial", model);
        }

        [HttpPost]
        public IActionResult RemoveBubble(int id)
        {
            var model = new MagicModeViewModel
            {
                QuestionText = "Vertel ons over je ervaring met onze dienst...",
                IsRecording = _sttService.IsRecording,
                Bubbles = new List<BubbleModel>()
            };

            // Voorbeeld: Verwijder bubbel uit de state
            model.RemoveBubble(id);

            return PartialView("_BubblesPartial", model);
        }

        [HttpPost]
        public IActionResult StopRecording()
        {
            _sttService.StopRecording();
            return Ok();
        }

        [HttpPost]
        public IActionResult CloseMagicMode()
        {
            var model = new MagicModeViewModel
            {
                QuestionText = "Vertel ons over je ervaring met onze dienst...",
                IsRecording = _sttService.IsRecording,
                Bubbles = new List<BubbleModel>()
            };

            var finalText = model.GetFinalText();
            // Voorbeeld: Plaats de finale tekst in de textarea van de survey
            // Hier moet je de logica toevoegen om de tekst terug te geven aan de survey
            return Ok(finalText);
        }
    }
}
```

#### **Voorbeeld 2: Controller met State in Service-Laag**
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
        private readonly IMagicModeStateService _stateService;

        public MagicModeController(
            ISttService sttService,
            IAiService aiService,
            IMagicModeStateService stateService)
        {
            _sttService = sttService;
            _aiService = aiService;
            _stateService = stateService;
        }

        public IActionResult Index()
        {
            var model = new MagicModeViewModel
            {
                QuestionText = "Vertel ons over je ervaring met onze dienst...",
                IsRecording = _sttService.IsRecording,
                Bubbles = _stateService.GetBubbles()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> StartRecording()
        {
            var transcript = await _sttService.StartRecording();
            var bubbles = await _aiService.GenerateBubbles(transcript);

            // Voorbeeld: Voeg bubbels toe aan de state
            foreach (var text in bubbles)
            {
                _stateService.AddBubble(text);
            }

            var model = new MagicModeViewModel
            {
                QuestionText = "Vertel ons over je ervaring met onze dienst...",
                IsRecording = _sttService.IsRecording,
                Bubbles = _stateService.GetBubbles()
            };

            return PartialView("_BubblesPartial", model);
        }

        [HttpPost]
        public IActionResult RemoveBubble(int id)
        {
            _stateService.RemoveBubble(id);

            var model = new MagicModeViewModel
            {
                QuestionText = "Vertel ons over je ervaring met onze dienst...",
                IsRecording = _sttService.IsRecording,
                Bubbles = _stateService.GetBubbles()
            };

            return PartialView("_BubblesPartial", model);
        }

        [HttpPost]
        public IActionResult CloseMagicMode()
        {
            var finalText = _stateService.GetFinalText();
            // Voorbeeld: Plaats de finale tekst in de textarea van de survey
            return Ok(finalText);
        }
    }
}
```

> **Mistral Vibe beslist**:
> - Moet de controller **state direct in het ViewModel** beheren?
> - Moet de controller **een state service** gebruiken voor state management?

---
---

### **Stap 5: Update de Client-Side Logica voor State Management**
Voeg JavaScript toe om de state te beheren op de client-side.

#### **Voorbeeld: Client-Side State Management**
```javascript
// UI-MVC/wwwroot/js/magic-mode/magicMode.js

// Voorbeeld: State voor bubbels (kan worden vervangen door een server-side call)
let bubblesState = [];

/**
 * Voegt een bubbel toe aan de state.
 * @param {string} text - De tekst van de bubbel.
 */
function addBubble(text) {
    const newId = bubblesState.length > 0 ? Math.max(...bubblesState.map(b => b.id)) + 1 : 1;
    bubblesState.push({ id: newId, text });
    renderBubbles();
}

/**
 * Verwijdert een bubbel uit de state.
 * @param {number} id - De ID van de bubbel.
 */
function removeBubble(id) {
    bubblesState = bubblesState.filter(b => b.id !== id);
    renderBubbles();

    // Voorbeeld: Stuur een request naar de server om de bubbel te verwijderen
    fetch(`/MagicMode/RemoveBubble?id=${id}`, { method: 'POST' });
}

/**
 * Rendert de bubbels in de UI.
 */
function renderBubbles() {
    const container = document.getElementById('bubbles');
    container.innerHTML = '';

    if (bubblesState.length === 0) {
        container.innerHTML = '<p class="placeholder">Klik op de microfoon om te beginnen met spreken...</p>';
        return;
    }

    bubblesState.forEach(bubble => {
        const bubbleElement = document.createElement('div');
        bubbleElement.className = 'bubble';
        bubbleElement.id = `bubble-${bubble.id}`;
        bubbleElement.innerHTML = `
            <p>${bubble.text}</p>
            <button onclick="removeBubble(${bubble.id})" class="bubble-close" aria-label="Verwijder bubbel">X</button>
        `;
        container.appendChild(bubbleElement);
    });
}

/**
 * Sluit Magic Mode af en genereert de finale tekst.
 */
function closeMagicMode() {
    const finalText = bubblesState.map(b => b.text).join('. ');
    alert(`Magic Mode afgesloten. Finale tekst: ${finalText}`);
    // Voorbeeld: Stuur de finale tekst naar de server of plaats deze in de textarea
    fetch('/MagicMode/CloseMagicMode', {
        method: 'POST',
        body: JSON.stringify({ finalText }),
        headers: { 'Content-Type': 'application/json' }
    });
}
```

> **Mistral Vibe beslist**:
> - Moet de client-side logica **state lokaal beheren**?
> - Moet de client-side logica **server-side state** bijwerken via API calls?

---
---

### **Stap 6: Test de State Management**
Test de state management om te controleren of:
- Bubbels correct worden toegevoegd.
- Bubbels correct worden verwijderd.
- De finale tekst correct wordt gegenereerd.

#### **Voorbeeld: Unit Test voor State Management**
```csharp
// Tests/MagicModeStateServiceTests.cs
using Xunit;
using UI_MVC.Services.MagicMode;
using System.Linq;

namespace UI_MVC.Tests
{
    public class MagicModeStateServiceTests
    {
        [Fact]
        public void AddBubble_AddsBubbleToState()
        {
            // Arrange
            var service = new MagicModeStateService();

            // Act
            service.AddBubble("Test bubbel 1");
            service.AddBubble("Test bubbel 2");

            // Assert
            var bubbles = service.GetBubbles();
            Assert.Equal(2, bubbles.Count);
            Assert.Equal("Test bubbel 1", bubbles[0].Text);
            Assert.Equal("Test bubbel 2", bubbles[1].Text);
        }

        [Fact]
        public void RemoveBubble_RemovesBubbleFromState()
        {
            // Arrange
            var service = new MagicModeStateService();
            service.AddBubble("Test bubbel 1");
            service.AddBubble("Test bubbel 2");

            // Act
            service.RemoveBubble(1);

            // Assert
            var bubbles = service.GetBubbles();
            Assert.Single(bubbles);
            Assert.Equal("Test bubbel 2", bubbles[0].Text);
        }

        [Fact]
        public void GetFinalText_ReturnsCorrectFinalText()
        {
            // Arrange
            var service = new MagicModeStateService();
            service.AddBubble("Dit is bubbel 1");
            service.AddBubble("Dit is bubbel 2");

            // Act
            var finalText = service.GetFinalText();

            // Assert
            Assert.Equal("Dit is bubbel 1. Dit is bubbel 2", finalText);
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
| **State wordt niet bijgewerkt**               | Mistral Vibe kan de **state management-logica** in de controller of service aanpassen.|
| **Bubbels worden niet correct weergegeven**  | Mistral Vibe kan de **rendering-logica** of **partial view** aanpassen.             |
| **Finale tekst is incorrect**                 | Mistral Vibe kan de **methode voor het genereren van de finale tekst** aanpassen.   |
| **State is niet consistent tussen client en server** | Mistral Vibe kan **server-side state** en **client-side state** synchroniseren.  |

---
---

## 📌 Changelog
| Datum       | Wijziging                                  | Door          |
|-------------|--------------------------------------------|---------------|
| 2026-04-26  | Initiële state management (voorbeeld)     | Mistral Vibe  |
| 2026-04-26  | State in ViewModel en Service-Laag         | Mistral Vibe  |
| 2026-04-26  | Client-side state management toegevoegd   | Mistral Vibe  |
```