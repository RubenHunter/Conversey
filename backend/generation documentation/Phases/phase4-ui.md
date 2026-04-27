```markdown
# 🎨 Fase 4: UI-Componenten voor Magic Mode
**Voorbeeldcode - Mistral Vibe bepaalt de optimale implementatie**

---

## 📌 Doel
In deze fase worden de **UI-componenten** voor Magic Mode geïmplementeerd. Mistral Vibe zal:
1. De **beste UI-architectuur** kiezen (Razor Views, Razor Components, of een combinatie).
2. **Herbruikbare componenten** maken voor bubbels, de bubbelcontainer, de microfoonknop en de sluitknop.
3. Zorgen dat de UI **consistent** is met de bestaande theming en stijlrichtlijnen.
4. **Testen** of de UI-componenten correct renderen en interactief zijn.

---
## 📌 Belangrijke Richtlijnen voor Mistral Vibe
> **⚠️ BELANGRIJK**:
> Alle code in dit bestand is **voorbeeldimplementatie** en dient als **inspiratie**. Mistral Vibe zal:
> 1. De **bestaande UI-praktijken** (Razor Views, Blazor, CSS, JavaScript) analyseren.
> 2. Bepalen of:
>    - **Razor Views** de beste oplossing zijn.
>    - **Razor Components** (Blazor) moeten worden gebruikt.
>    - **Bestaande componenten** (bijv. knoppen, animaties) moeten worden hergebruikt.
> 3. De **optimalste UI-architectuur** selecteren gebaseerd op:
>    - Bestaande **folderstructuur** (bijv. `Views/`, `Components/`).
>    - **Theme variabelen** (bijv. `--theme-secondary`, `--theme-primary`).
>    - **Bestaande CSS-bestanden** (bijv. `site.css`, `styles.css`).

### Richtlijnen voor Mistral Vibe:
| Aspect               | Richtlijn                                                                                   |
|----------------------|---------------------------------------------------------------------------------------------|
| **UI-Architectuur**  | Gebruik **Razor Views** als de applicatie traditioneel MVC is. Gebruik **Razor Components** als Blazor wordt gebruikt. |
| **Hergebruik**       | Gebruik **bestaande knoppen, animaties en kleuren** uit de theming.                         |
| **Modulariteit**     | Maak **herbruikbare partial views** of **componenten** voor bubbels en de container.         |
| **Testbaarheid**     | Zorg dat elke UI-component **afzonderlijk testbaar** is (visueel en met unit tests).         |

---
---

## 🛠 Stappenplan (Met Flexibiliteit voor Mistral Vibe)

---

### **Stap 1: Bepaal de UI-Architectuur**
Mistral Vibe zal bepalen welke UI-architectuur het beste past bij de bestaande codebase.

#### **Optie 1: Razor Views (Traditioneel MVC)**
```
UI-MVC/
├── Views/
│   └── MagicMode/
│       ├── Index.cshtml              # Hoofdview voor Magic Mode
│       ├── PartialViews/
│       │   ├── _BubblePartial.cshtml # Voorbeeld: Partial view voor een bubbel
│       │   └── _BubblesPartial.cshtml # Voorbeeld: Partial view voor de bubbelcontainer
│       └── ...
└── wwwroot/
└── css/magic-mode/               # Voorbeeld: Styling voor Magic Mode
└── styles.css
```

#### **Optie 2: Razor Components (Blazor)**
```
UI-MVC/
├── Components/
│   └── MagicMode/
│       ├── Bubble.razor              # Voorbeeld: Razor component voor een bubbel
│       ├── BubbleContainer.razor     # Voorbeeld: Razor component voor de bubbelcontainer
│       ├── MicrophoneButton.razor    # Voorbeeld: Razor component voor de microfoonknop
│       └── MagicMode.razor           # Voorbeeld: Hoofdcomponent voor Magic Mode
└── wwwroot/
└── css/magic-mode/               # Voorbeeld: Styling voor Magic Mode
└── styles.css
```

#### **Optie 3: Hybride Aanpak**
- Gebruik **Razor Views** voor de hoofdview (`Index.cshtml`).
- Gebruik **Razor Components** voor herbruikbare onderdelen (bijv. bubbels).

> **Mistral Vibe beslist**:
> - Moet de applicatie **Razor Views** of **Razor Components** gebruiken?
> - Moet een **hybride aanpak** worden gebruikt (bijv. Views voor de hoofdview en Components voor bubbels)?

---
---

### **Stap 2: Maak de Bubble-Component**
Hier zijn **voorbeeldimplementaties** voor de `Bubble`-component. Mistral Vibe zal de beste optie kiezen.

---

#### **Voorbeeld 1: Bubble als Razor Partial View**
```html
@* UI-MVC/Views/MagicMode/PartialViews/_BubblePartial.cshtml *@
@model BubbleModel

<div class="bubble" id="bubble-@Model.Id">
    <p>@Model.Text</p>
    <button
        onclick="removeBubble(@Model.Id)"
        class="bubble-close"
        aria-label="Verwijder bubbel">
        X
    </button>
</div>
```

---

#### **Voorbeeld 2: Bubble als Razor Component (Blazor)**
```razor
@* UI-MVC/Components/MagicMode/Bubble.razor *@
@using UI_MVC.Models.MagicMode

<div class="bubble" id="bubble-@Id">
    <p>@Text</p>
    <button
        @onclick="() => OnRemove.Invoke(Id)"
        class="bubble-close"
        aria-label="Verwijder bubbel">
        X
    </button>
</div>

@code {
    [Parameter]
    public int Id { get; set; }

    [Parameter]
    public string Text { get; set; }

    [Parameter]
    public EventCallback<int> OnRemove { get; set; }
}
```

> **Mistral Vibe beslist**:
> - Moet de `Bubble` een **partial view** of een **Razor Component** zijn?
> - Moet de `Bubble` **parameters** (bijv. `Id`, `Text`) of een **model** accepteren?

---
---

### **Stap 3: Maak de BubbleContainer-Component**
Hier zijn **voorbeeldimplementaties** voor de `BubbleContainer`-component. Mistral Vibe zal de beste optie kiezen.

---

#### **Voorbeeld 1: BubbleContainer als Razor Partial View**
```html
@* UI-MVC/Views/MagicMode/PartialViews/_BubblesPartial.cshtml *@
@model MagicModeViewModel

<div class="bubble-container">
    @if (Model.Bubbles.Any())
    {
        @foreach (var bubble in Model.Bubbles)
        {
            @Html.Partial("_BubblePartial", bubble)
        }
    }
    else
    {
        <p class="placeholder">Klik op de microfoon om te beginnen met spreken...</p>
    }
</div>
```

---

#### **Voorbeeld 2: BubbleContainer als Razor Component (Blazor)**
```razor
@* UI-MVC/Components/MagicMode/BubbleContainer.razor *@
@using UI_MVC.Models.MagicMode

<div class="bubble-container">
    @if (Bubbles.Any())
    {
        @foreach (var bubble in Bubbles)
        {
            <Bubble
                Id="@bubble.Id"
                Text="@bubble.Text"
                OnRemove="RemoveBubble" />
        }
    }
    else
    {
        <p class="placeholder">Klik op de microfoon om te beginnen met spreken...</p>
    }
</div>

@code {
    [Parameter]
    public List<BubbleModel> Bubbles { get; set; } = new();

    [Parameter]
    public EventCallback<int> OnRemove { get; set; }

    private void RemoveBubble(int id)
    {
        OnRemove.InvokeAsync(id);
    }
}
```

> **Mistral Vibe beslist**:
> - Moet de `BubbleContainer` een **partial view** of een **Razor Component** zijn?
> - Moet de `BubbleContainer` **events** (bijv. `OnRemove`) ondersteunen of alleen **data-binding**?

---
---

### **Stap 4: Maak de MicrofoonButton-Component**
Hier zijn **voorbeeldimplementaties** voor de `MicrofoonButton`-component. Mistral Vibe zal de beste optie kiezen.

---

#### **Voorbeeld 1: MicrofoonButton als Razor Partial View**
```html
@* UI-MVC/Views/Shared/_MicrophoneButton.cshtml *@
@model bool

<button
    id="microphoneButton"
    class="btn-microphone @(Model ? "recording" : "")"
    onclick="toggleRecording()"
    aria-label="@(Model ? "Pauzeer opname" : "Start opname")">
    @(Model ? "⏸" : "🎤")
    <span>@(Model ? " Pauzeer" : " Opnemen")</span>
</button>
```

---

#### **Voorbeeld 2: MicrofoonButton als Razor Component (Blazor)**
```razor
@* UI-MVC/Components/MagicMode/MicrophoneButton.razor *@
<button
    class="btn-microphone @(IsRecording ? "recording" : "")"
    @onclick="ToggleRecording"
    aria-label="@(IsRecording ? "Pauzeer opname" : "Start opname")">
    @(IsRecording ? "⏸" : "🎤")
    <span>@(IsRecording ? " Pauzeer" : " Opnemen")</span>
</button>

@code {
    [Parameter]
    public bool IsRecording { get; set; }

    [Parameter]
    public EventCallback OnToggle { get; set; }

    private async Task ToggleRecording()
    {
        await OnToggle.InvokeAsync();
    }
}
```

> **Mistral Vibe beslist**:
> - Moet de `MicrofoonButton` een **partial view** of een **Razor Component** zijn?
> - Moet de `MicrofoonButton` **events** (bijv. `OnToggle`) ondersteunen of alleen **data-binding**?

---
---

### **Stap 5: Maak de CloseButton-Component**
Hier zijn **voorbeeldimplementaties** voor de `CloseButton`-component. Mistral Vibe zal de beste optie kiezen.

---

#### **Voorbeeld 1: CloseButton als Razor Partial View**
```html
@* UI-MVC/Views/Shared/_CloseButton.cshtml *@
<button
    id="closeButton"
    class="btn-close"
    onclick="closeMagicMode()"
    aria-label="Sluit Magic Mode">
    Sluiten
</button>
```

---

#### **Voorbeeld 2: CloseButton als Razor Component (Blazor)**
```razor
@* UI-MVC/Components/MagicMode/CloseButton.razor *@
<button
    class="btn-close"
    @onclick="CloseMagicMode"
    aria-label="Sluit Magic Mode">
    Sluiten
</button>

@code {
    [Parameter]
    public EventCallback OnClose { get; set; }

    private async Task CloseMagicMode()
    {
        await OnClose.InvokeAsync();
    }
}
```

> **Mistral Vibe beslist**:
> - Moet de `CloseButton` een **partial view** of een **Razor Component** zijn?
> - Moet de `CloseButton` **events** (bijv. `OnClose`) ondersteunen?

---
---

### **Stap 6: Maak de Hoofdview (Index.cshtml)**
Hier zijn **voorbeeldimplementaties** voor de hoofdview van Magic Mode. Mistral Vibe zal de beste optie kiezen.

---

#### **Voorbeeld 1: Hoofdview als Razor View**
```html
@* UI-MVC/Views/MagicMode/Index.cshtml *@
@model MagicModeViewModel

<div class="magic-mode-container">
    <h2>@Model.QuestionText</h2>

    @Html.Partial("_BubblesPartial", Model)

    @Html.Partial("_MicrophoneButton", Model.IsRecording)

    @Html.Partial("_CloseButton")
</div>

@section Styles {
    <link rel="stylesheet" href="~/css/magic-mode/styles.css" />
}

@section Scripts {
    <script src="~/js/magic-mode/magicMode.js"></script>
}
```

---

#### **Voorbeeld 2: Hoofdview als Razor Component (Blazor)**
```razor
@* UI-MVC/Components/MagicMode/MagicMode.razor *@
@using UI_MVC.Models.MagicMode

<h2>@QuestionText</h2>

<BubbleContainer
    Bubbles="Bubbles"
    OnRemove="RemoveBubble" />

<MicrophoneButton
    IsRecording="IsRecording"
    OnToggle="ToggleRecording" />

<CloseButton OnClose="CloseMagicMode" />

@code {
    [Parameter]
    public string QuestionText { get; set; }

    [Parameter]
    public bool IsRecording { get; set; }

    [Parameter]
    public List<BubbleModel> Bubbles { get; set; } = new();

    private void ToggleRecording()
    {
        // Logica voor het starten/pauzeren van de opname
    }

    private void RemoveBubble(int id)
    {
        Bubbles.RemoveAll(b => b.Id == id);
    }

    private void CloseMagicMode()
    {
        // Logica voor het sluiten van Magic Mode
    }
}
```

> **Mistral Vibe beslist**:
> - Moet de hoofdview een **Razor View** of een **Razor Component** zijn?
> - Moet de hoofdview **partial views** gebruiken voor herbruikbare onderdelen?

---
---

### **Stap 7: Maak de Styling voor Magic Mode**
Zorg dat de styling **consistent** is met de bestaande theming. Hier is een **voorbeeldimplementatie** voor `styles.css`:

```css
/* UI-MVC/wwwroot/css/magic-mode/styles.css */

/* Container voor Magic Mode */
.magic-mode-container {
    max-width: 800px;
    margin: 0 auto;
    padding: 20px;
    font-family: var(--theme-font-family, 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif);
}

/* Container voor bubbels */
.bubble-container {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
    gap: 15px;
    max-height: 500px;
    overflow-y: auto;
    padding: 10px;
    margin: 20px 0;
    border: 1px solid var(--theme-border-color, #e0e0e0);
    border-radius: var(--theme-border-radius, 8px);
    background-color: var(--theme-surface, #ffffff);
}

/* Stijl voor een enkele bubbel */
.bubble {
    background-color: var(--theme-secondary, #f5f5f5);
    color: var(--theme-text, #333333);
    padding: 12px 15px;
    border-radius: var(--theme-border-radius, 8px);
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    animation: fadeIn 0.3s ease-out;
}

/* Knop om een bubbel te verwijderen */
.bubble-close {
    background: none;
    border: none;
    color: var(--theme-error, #e74c3c);
    cursor: pointer;
    float: right;
    font-size: 16px;
    padding: 0 5px;
    line-height: 1;
}

/* Knoppen voor microfoon en sluit */
.btn-microphone, .btn-close {
    padding: 10px 20px;
    border: none;
    border-radius: var(--theme-border-radius, 8px);
    font-size: 16px;
    cursor: pointer;
    margin: 5px;
}

/* Stijl voor de microfoonknop */
.btn-microphone {
    background-color: var(--theme-primary, #3498db);
    color: white;
}

.btn-microphone.recording {
    background-color: var(--theme-error, #e74c3c);
    animation: pulse 1.5s infinite;
}

/* Stijl voor de sluitknop */
.btn-close {
    background-color: var(--theme-secondary, #f5f5f5);
    color: var(--theme-text, #333333);
}

/* Animatie voor het verschijnen van bubbels */
@keyframes fadeIn {
    from { opacity: 0; transform: translateY(10px); }
    to { opacity: 1; transform: translateY(0); }
}

/* Animatie voor pulserende microfoonknop */
@keyframes pulse {
    0% { transform: scale(1); }
    50% { transform: scale(1.1); }
    100% { transform: scale(1); }
}

/* Placeholder tekst als er geen bubbels zijn */
.placeholder {
    color: var(--theme-text-secondary, #777777);
    font-style: italic;
    text-align: center;
    margin-top: 50px;
}
```

> **Mistral Vibe beslist**:
> - Moet de styling **inline** of in een **afzonderlijk CSS-bestand**?
> - Moeten **bestaande animaties** of **custom CSS** worden gebruikt?
> - Moet de styling **alleen de Magic Mode** betreffen of **bestaande stijlen overschrijven**?

---
---

### **Stap 8: Maak de Client-Side Logica**
Voeg JavaScript toe om de UI-interacties te beheren. Hier is een **voorbeeldimplementatie** voor `magicMode.js`:

```javascript
// UI-MVC/wwwroot/js/magic-mode/magicMode.js

/**
 * Verwijdert een bubbel uit de UI.
 * @param {number} bubbleId - De ID van de bubbel.
 */
function removeBubble(bubbleId) {
    const bubble = document.getElementById(`bubble-${bubbleId}`);
    if (bubble) {
        bubble.style.opacity = '0';
        setTimeout(() => bubble.remove(), 300);
    }
}

/**
 * Start of pauzeert de opname.
 */
function toggleRecording() {
    const button = document.getElementById('microphoneButton');
    const isRecording = button.classList.contains('recording');

    if (isRecording) {
        // Pauzeer opname
        fetch('/MagicMode/StopRecording', { method: 'POST' })
            .then(() => {
                button.classList.remove('recording');
                button.querySelector('span').textContent = ' Opnemen';
            })
            .catch(error => {
                console.error('Fout bij het pauzeren van de opname:', error);
                alert('Er is een fout opgetreden bij het pauzeren.');
            });
    } else {
        // Start opname
        fetch('/MagicMode/StartRecording', { method: 'POST' })
            .then(response => {
                if (!response.ok) {
                    throw new Error('STT opname mislukt');
                }
                return response.text();
            })
            .then(bubblesHtml => {
                document.getElementById('bubbles').innerHTML = bubblesHtml;
                button.classList.add('recording');
                button.querySelector('span').textContent = ' Pauzeer';
            })
            .catch(error => {
                console.error('Fout bij het starten van de opname:', error);
                alert('Er is een fout opgetreden bij het starten.');
            });
    }
}

/**
 * Sluit Magic Mode af.
 */
function closeMagicMode() {
    // Voorbeeld: Voeg logica toe om de finale tekst te genereren en terug te keren naar de survey
    alert("Magic Mode afgesloten. De gegenereerde tekst wordt in de survey geplaatst.");
    // In een echte implementatie: Navigeer terug naar de survey of plaats de tekst in de textarea
}
```

> **Mistral Vibe beslist**:
> - Moet de client-side logica **inline** of in een **afzonderlijk JS-bestand**?
> - Moet de client-side logica **foutafhandeling** bevatten?
> - Moet de client-side logica **events** of **directe DOM-manipulatie** gebruiken?

---
---

## 🧪 Testinstructies

### **1. Handmatige Test**
1. **Start de applicatie**:
    - Run de applicatie via Visual Studio (`F5`) of met `dotnet run`.
2. **Navigeer naar Magic Mode**:
    - Voeg een link toe in je survey-pagina (bijv. `<a asp-controller="MagicMode" asp-action="Index">Open Magic Mode</a>`).
3. **Test de UI-functionaliteit**:
    - Klik op de **microfoonknop** om de opname te starten.
    - Spreek een zin en controleer of:
        - De **bubbels** verschijnen in de container.
        - De **microfoonknop pulseert** tijdens opname.
        - De **bubbels kunnen worden verwijderd** met de "X"-knop.
    - Klik op de **sluitknop** en controleer of Magic Mode correct sluit.

### **2. Visuele Test**
- Controleer of:
    - De **UI consistent** is met de bestaande theming.
    - De **animaties** vloeiend werken.
    - De **container scrollt** als er te veel bubbels zijn.

### **3. Responsiviteitstest**
- Test op verschillende schermformaten (desktop, tablet, mobiel) om te controleren of:
    - De **UI responsive** is.
    - De **bubbels correct worden weergegeven**.

---
---

## ⚠ Veelvoorkomende Problemen en Oplossingen
Mistral Vibe zal deze problemen analyseren en oplossingen voorstellen gebaseerd op de bestaande codebase.

| Probleem                                      | Mogelijke Oplossing                                                                 |
|-----------------------------------------------|------------------------------------------------------------------------------------|
| **Bubbels overlappen elkaar**                 | Mistral Vibe kan de **grid-lay-out** of **CSS spacing** aanpassen.                 |
| **Animaties werken niet**                     | Mistral Vibe kan de **CSS animaties** of **JavaScript** aanpassen.                 |
| **UI is niet consistent met bestaande theming**| Mistral Vibe kan **theme variabelen** of **bestaande CSS klassen** gebruiken.        |
| **Bubbels renderen niet in de UI**            | Mistral Vibe kan de **partial view** of **Razor Component** aanpassen.             |
| **Client-side logica werkt niet**             | Mistral Vibe kan de **JavaScript functies** of **event listeners** aanpassen.       |

---
---
## 📌 Changelog
| Datum       | Wijziging                                  | Door          |
|-------------|--------------------------------------------|---------------|
| 2026-04-26  | Initiële UI-componenten (voorbeeld)       | Mistral Vibe  |
| 2026-04-26  | Voorbeelden voor Razor Views en Components | Mistral Vibe  |
| 2026-04-26  | Styling voor Magic Mode toegevoegd        | Mistral Vibe  |
```