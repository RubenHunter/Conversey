```markdown
# 🔗 Fase 6: Integratie van Magic Mode met Survey-Pagina
**Voorbeeldcode - Mistral Vibe bepaalt de optimale implementatie**

---

## 📌 Doel
In deze fase wordt **Magic Mode geïntegreerd** met de bestaande **survey-pagina**. Mistral Vibe zal:
1. Bepalen hoe Magic Mode **toegankelijk** wordt gemaakt vanuit de survey.
2. Zorgen dat de **gegenereerde tekst** correct in de survey-textarea wordt geplaatst.
3. **Testen** of de integratie soepel verloopt en de gebruikerservaring consistent blijft.

---
## 📌 Belangrijke Richtlijnen voor Mistral Vibe
> **⚠️ BELANGRIJK**:
> Alle code in dit bestand is **voorbeeldimplementatie** en dient als **inspiratie**. Mistral Vibe zal:
> 1. De **bestaande survey-pagina** analyseren (bijv. Razor View, Blazor Component, of JavaScript-logica).
> 2. Bepalen of:
>    - Magic Mode een **aparte pagina** wordt (bijv. `/Survey/MagicMode`).
>    - Magic Mode een **modal/popup** is die vanuit de survey-pagina wordt geopend.
>    - Magic Mode **inline** in de survey-pagina wordt geïntegreerd.
> 3. De **beste integratiemethode** kiezen gebaseerd op:
>    - Bestaande **navigatiestructuur**.
>    - **Gebruikerservaring** (bijv. modaal vs inline).
>    - **Code-onderhoudbaarheid** en **scalability**.

### Richtlijnen voor Mistral Vibe:
| Aspect               | Richtlijn                                                                                   |
|----------------------|---------------------------------------------------------------------------------------------|
| **Integratiemethode** | Gebruik een **modaal** als de survey-pagina veel content heeft. Gebruik **inline** als de survey-pagina eenvoudig is. |
| **Navigatie**        | Zorg dat Magic Mode **terugkeert naar de survey-pagina** met de gegenereerde tekst.          |
| **State Synchronisatie** | Zorg dat de **survey-textarea** wordt bijgewerkt met de finale tekst van Magic Mode.         |
| **Consistente UI**    | Zorg dat Magic Mode **visueel consistent** is met de rest van de survey.                     |

---
---

## 🛠 Stappenplan (Met Flexibiliteit voor Mistral Vibe)

---

### **Stap 1: Kies de Integratiemethode**
Mistral Vibe zal bepalen hoe Magic Mode het beste kan worden geïntegreerd met de survey-pagina.

#### **Optie 1: Magic Mode als Modal/Popup**
```html
<!-- Voorbeeld: Modal in de survey-pagina -->
<button id="openMagicMode" class="btn-primary">
    Open Magic Mode
</button>

<!-- Modal HTML -->
<div id="magicModeModal" class="modal">
    <div class="modal-content">
        @Html.Action("Index", "MagicMode")
    </div>
</div>

<script>
    document.getElementById('openMagicMode').addEventListener('click', () => {
        document.getElementById('magicModeModal').style.display = 'block';
    });

    // Sluit modal en plaats tekst in textarea
    function closeMagicMode(text) {
        document.getElementById('magicModeModal').style.display = 'none';
        document.getElementById('surveyTextarea').value = text;
    }
</script>
```

#### **Optie 2: Magic Mode als Afzonderlijke Pagina**
```csharp
// Voorbeeld: Link naar Magic Mode vanuit de survey
<a asp-controller="MagicMode" asp-action="Index" class="btn-secondary">
    Gebruik Magic Mode
</a>
```

#### **Optie 3: Magic Mode Inline in de Survey-Pagina**
```html
<!-- Voorbeeld: Inline Magic Mode -->
<div id="magicModeInline" style="display: none;">
    @Html.Action("Index", "MagicMode")
</div>

<button onclick="toggleMagicModeInline()" class="btn-secondary">
    Toggle Magic Mode
</button>

<script>
    function toggleMagicModeInline() {
        const inlineMode = document.getElementById('magicModeInline');
        inlineMode.style.display = inlineMode.style.display === 'none' ? 'block' : 'none';
    }

    function closeMagicMode(text) {
        document.getElementById('magicModeInline').style.display = 'none';
        document.getElementById('surveyTextarea').value = text;
    }
</script>
```

> **Mistral Vibe beslist**:
> - Moet Magic Mode een **modaal/popup** zijn?
> - Moet Magic Mode een **aparte pagina** zijn?
> - Moet Magic Mode **inline** in de survey-pagina worden geïntegreerd?

---
---

### **Stap 2: Update de Survey-Pagina om Magic Mode te Openen**
Pas de survey-pagina aan om Magic Mode te openen. Hier zijn **voorbeeldimplementaties** voor verschillende integratiemethoden.

---

#### **Voorbeeld 1: Survey-Pagina met Modal**
```html
@* UI-MVC/Views/Survey/Index.cshtml *@
@model SurveyViewModel

<h1>@Model.Title</h1>

<!-- Survey vragen -->
@foreach (var question in Model.Questions)
{
    <div class="survey-question">
        <h3>@question.Text</h3>
        <textarea id="surveyTextarea-@question.Id" class="form-control"></textarea>

        <!-- Voorbeeld: Knop om Magic Mode te openen -->
        <button
            onclick="openMagicMode('@question.Id')"
            class="btn-secondary"
            aria-label="Gebruik Magic Mode voor deze vraag">
            🎤 Magic Mode
        </button>
    </div>
}

<!-- Modal voor Magic Mode -->
<div id="magicModeModal" class="modal">
    <div class="modal-content">
        @Html.Action("Index", "MagicMode", new { questionId = "dynamicQuestionId" })
    </div>
</div>

@section Scripts {
    <script>
        function openMagicMode(questionId) {
            // Voorbeeld: Stel de vraag-ID in voor Magic Mode
            document.getElementById('magicModeModal').dataset.questionId = questionId;
            document.getElementById('magicModeModal').style.display = 'block';
        }

        function closeMagicMode(text) {
            const questionId = document.getElementById('magicModeModal').dataset.questionId;
            document.getElementById(`surveyTextarea-${questionId}`).value = text;
            document.getElementById('magicModeModal').style.display = 'none';
        }
    </script>
}
```

---

#### **Voorbeeld 2: Survey-Pagina met Afzonderlijke Pagina**
```csharp
// UI-MVC/Controllers/SurveyController.cs
public class SurveyController : Controller
{
    public IActionResult Index()
    {
        var model = new SurveyViewModel
        {
            Title = "Survey over onze dienst",
            Questions = new List<SurveyQuestion>
            {
                new SurveyQuestion { Id = 1, Text = "Vertel ons over je ervaring..." },
                new SurveyQuestion { Id = 2, Text = "Wat vind je van onze aanpak?" }
            }
        };
        return View(model);
    }
}
```

```html
@* UI-MVC/Views/Survey/Index.cshtml *@
@model SurveyViewModel

<h1>@Model.Title</h1>

<!-- Survey vragen -->
@foreach (var question in Model.Questions)
{
    <div class="survey-question">
        <h3>@question.Text</h3>
        <textarea id="surveyTextarea-@question.Id" class="form-control"></textarea>

        <!-- Voorbeeld: Link naar Magic Mode voor deze vraag -->
        <a
            asp-controller="MagicMode"
            asp-action="Index"
            asp-route-questionId="@question.Id"
            class="btn-secondary">
            🎤 Magic Mode
        </a>
    </div>
}
```

---
---

### **Stap 3: Update de MagicModeController om de Vraag-ID te Accepteren**
Zorg dat de `MagicModeController` de **vraag-ID** accepteert en de gegenereerde tekst in de juiste textarea plaatst.

#### **Voorbeeld: MagicModeController met Vraag-ID**
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

        public IActionResult Index(int questionId)
        {
            var model = new MagicModeViewModel
            {
                QuestionText = "Beantwoord de vraag:", // Vraagtekst uit de survey
                QuestionId = questionId,
                IsRecording = _sttService.IsRecording,
                Bubbles = _stateService.GetBubbles()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> StartRecording(int questionId)
        {
            var transcript = await _sttService.StartRecording();
            var bubbles = await _aiService.GenerateBubbles(transcript);

            foreach (var text in bubbles)
            {
                _stateService.AddBubble(text);
            }

            var model = new MagicModeViewModel
            {
                QuestionText = "Beantwoord de vraag:",
                QuestionId = questionId,
                IsRecording = _sttService.IsRecording,
                Bubbles = _stateService.GetBubbles()
            };

            return PartialView("_BubblesPartial", model);
        }

        [HttpPost]
        public IActionResult CloseMagicMode(int questionId)
        {
            var finalText = _stateService.GetFinalText();
            // Voorbeeld: Stuur de finale tekst terug naar de survey
            return Ok(new { questionId, text = finalText });
        }
    }
}
```

> **Mistral Vibe beslist**:
> - Moet de `MagicModeController` de **vraag-ID** accepteren?
> - Moet de controller **de finale tekst teruggeven** aan de survey?

---
---

### **Stap 4: Update de Client-Side Logica voor Modal/Popup**
Voeg JavaScript toe om de modal te beheren en de gegenereerde tekst in de survey-textarea te plaatsen.

#### **Voorbeeld: Client-Side Logica voor Modal**
```javascript
// UI-MVC/wwwroot/js/survey.js

/**
 * Opent Magic Mode voor een specifieke vraag.
 * @param {number} questionId - De ID van de vraag.
 */
function openMagicMode(questionId) {
    // Voorbeeld: Stel de vraag-ID in voor Magic Mode
    document.getElementById('magicModeModal').dataset.questionId = questionId;
    document.getElementById('magicModeModal').style.display = 'block';
}

/**
 * Sluit Magic Mode en plaatst de gegenereerde tekst in de survey-textarea.
 * @param {string} text - De gegenereerde tekst.
 */
function closeMagicMode(text) {
    const questionId = document.getElementById('magicModeModal').dataset.questionId;
    document.getElementById(`surveyTextarea-${questionId}`).value = text;
    document.getElementById('magicModeModal').style.display = 'none';
}
```

---
---

### **Stap 5: Stijl de Modal/Popup Consistent met de Survey**
Zorg dat de modal **visueel consistent** is met de rest van de survey. Hier is een **voorbeeldimplementatie** voor de styling:

```css
/* UI-MVC/wwwroot/css/modal.css */
.modal {
    display: none;
    position: fixed;
    z-index: 1000;
    left: 0;
    top: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.5);
}

.modal-content {
    background-color: var(--theme-surface, #ffffff);
    margin: 10% auto;
    padding: 20px;
    border-radius: var(--theme-border-radius, 8px);
    width: 80%;
    max-width: 800px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
}

.modal-header h2 {
    margin: 0;
    color: var(--theme-text, #333333);
}

.close {
    font-size: 24px;
    cursor: pointer;
    color: var(--theme-text-secondary, #777777);
    background: none;
    border: none;
    padding: 0 10px;
}
```

---
---

### **Stap 6: Test de Integratie**
Test de integratie om te controleren of:
- Magic Mode **correct wordt geopend** vanuit de survey.
- De **gegenereerde tekst** correct in de survey-textarea wordt geplaatst.
- De **modal/popup** correct sluit na het genereren van de tekst.

#### **Voorbeeld: Handmatige Test**
1. **Start de applicatie**:
    - Run de applicatie via Visual Studio (`F5`) of met `dotnet run`.
2. **Navigeer naar de survey**:
    - Open de survey-pagina.
3. **Test de integratie**:
    - Klik op de **Magic Mode knop** voor een vraag.
    - Open Magic Mode en spreek een zin.
    - Klik op **"Sluiten"** en controleer of:
        - De modal/popup sluit.
        - De **gegenereerde tekst** in de survey-textarea verschijnt.
        - De **gebruikerservaring** soepel verloopt.

---
---

## ⚠ Veelvoorkomende Problemen en Oplossingen
Mistral Vibe zal deze problemen analyseren en oplossingen voorstellen gebaseerd op de bestaande codebase.

| Probleem                                      | Mogelijke Oplossing                                                                 |
|-----------------------------------------------|------------------------------------------------------------------------------------|
| **Magic Mode opent niet**                     | Mistral Vibe kan de **event listeners** of **navigatie-logica** aanpassen.         |
| **De gegenereerde tekst wordt niet geplaatst** | Mistral Vibe kan de **client-side logica** of **controller** aanpassen om de tekst correct door te geven. |
| **De modal/popup ziet er inconsistent uit**   | Mistral Vibe kan de **CSS-styling** of **bestaande modal-component** gebruiken.   |
| **State wordt niet gedeeld tussen Magic Mode en Survey** | Mistral Vibe kan **globale state** (bijv. Vuex, Redux, of een service) implementeren. |

---
---

## 📌 Changelog
| Datum       | Wijziging                                  | Door          |
|-------------|--------------------------------------------|---------------|
| 2026-04-26  | Initiële integratie (voorbeeld)            | Mistral Vibe  |
| 2026-04-26  | Modal/Popup en Afzonderlijke Pagina       | Mistral Vibe  |
| 2026-04-26  | Client-side logica voor integratie        | Mistral Vibe  |
```