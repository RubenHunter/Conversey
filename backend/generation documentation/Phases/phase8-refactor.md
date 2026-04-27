Hier is de **uitgebreide code review** en **Optimization Plan** voor de Magic Mode implementatie, gebaseerd op de fasen die we hebben doorlopen en de code-review template die je hebt gedeeld.

---

---

## **🔍 Code Review van Magic Mode Implementatie**

### **1. Code Organization & Structure**

#### **Observaties:**
- **Folderstructuur**:
    - De gebruikte folderstructuur (`Services/MagicMode/`, `Models/MagicMode/`, etc.) volgt de **Mistral Vibe**-richtlijnen en is **logisch georganiseerd**.
    - Er is een duidelijke scheiding tussen **UI**, **services**, **models**, en **state management**.
    - **Voorbeeld van goede organisatie**:
      ```
      UI-MVC/
      ├── Services/
      │   └── MagicMode/
      │       ├── ISttService.cs
      │       ├── SttService.cs
      │       ├── IAiService.cs
      │       └── AiService.cs
      ├── Models/
      │   └── MagicMode/
      │       ├── MagicModeViewModel.cs
      │       └── BubbleModel.cs
      ├── Views/
      │   └── MagicMode/
      │       ├── Index.cshtml
      │       └── PartialViews/
      │           ├── _BubblesPartial.cshtml
      │           └── _BubblePartial.cshtml
      └── wwwroot/
          ├── css/
          │   └── magic-mode/
          │       └── styles.css
          └── js/
              └── magic-mode/
                  └── magicMode.js
      ```
    - **Mistral Vibe-opmerking**: De structuur is **consistent met ASP.NET Core MVC** en maakt gebruik van **dependency injection** waar van toepassing.

- **Scheiding van Concerns**:
    - De **STT-logica** is gescheiden van de **AI-logica**, en beide zijn geïsoleerd in services.
    - De **UI-componenten** (Razor Views/Blazor Components) zijn herbruikbaar en scheiden duidelijk de **weergave** van de **logica**.
    - **State management** is geïmplementeerd in zowel het **ViewModel** als de **service-laag**, wat flexibiliteit biedt.

- **Potentiële Verbeteringen**:
    - **Overweeg een dedicated `State/` map** voor state management services (bijv. `Services/State/MagicModeStateService.cs`) om de **separatie van concerns** nog duidelijker te maken.
    - **Gebruik van partial views** voor herbruikbare UI-componenten (bijv. `_MicrophoneButton.cshtml`) is een goede keuze, maar overweeg om **Razor Components** te gebruiken als de applicatie Blazor ondersteunt voor betere onderhoudbaarheid.

---

### **2. Code Quality & Best Practices**

#### **Observaties:**
- **TypeScript/Type Safety**:
    - De C# code is **type-safe** en gebruikt **interfaces** (`ISttService`, `IAiService`) voor duidelijkheid en testbaarheid.
    - **Voorbeeld van goede type safety**:
      ```csharp
      public interface ISttService
      {
          Task<string> StartRecording();
          void StopRecording();
          bool IsRecording { get; }
      }
      ```
    - **Mistral Vibe-opmerking**: Het gebruik van **interfaces** is een **best practice** en maakt de code **testbaarder** en **onderhoudbaarder**.

- **Error Handling**:
    - Er is **basisfoutafhandeling** ingebouwd (bijv. in `StartRecording()` wordt een `HttpRequestException` gegooid als de API call mislukt).
    - **Voorbeeld van error handling**:
      ```csharp
      public async Task<string> StartRecording()
      {
          var response = await _httpClient.PostAsync("/api/voxtrall/start", null);
          if (response.IsSuccessStatusCode)
          {
              await Task.Delay(5000);
              return await response.Content.ReadAsStringAsync();
          }
          throw new HttpRequestException("STT opname mislukt");
      }
      ```
    - **Mistral Vibe-opmerking**: Foutafhandeling is aanwezig, maar **overweeg om specifieke exceptions** (bijv. `VoxtrallApiException`) te gebruiken voor betere debugging.

- **Naming Conventions**:
    - De **naming** (bijv. `MagicModeViewModel`, `BubbleModel`) is **duidelijk** en consistent met C#-conventies.
    - **Mistral Vibe-opmerking**: De naming is **goed**, maar overweeg om **consistente prefixen** te gebruiken (bijv. `Mm` voor Magic Mode, zoals `MmViewModel`, `MmBubble`).

- **Code Duplicatie**:
    - Er is **minimale duplicatie** in de code, maar sommige logica (bijv. bubble rendering) is herhaald in zowel de **controller** als de **client-side JavaScript**.
    - **Mistral Vibe-opmerking**: Overweeg om **helper-methodes** te maken voor herhaalde logica (bijv. `RenderBubbles()` in een utility klasse).

- **Documentatie**:
    - De code bevat **XML-documentatie** voor publieke methodes en klassen, wat **onderhoudbaarheid** verbetert.
    - **Voorbeeld van documentatie**:
      ```csharp
      /// <summary>
      /// Voegt een bubbel toe aan de state.
      /// </summary>
      /// <param name="text">De tekst van de bubbel.</param>
      public void AddBubble(string text)
      {
          Bubbles.Add(new BubbleModel { Id = Bubbles.Count + 1, Text = text });
      }
      ```
    - **Mistral Vibe-opmerking**: De documentatie is **uitstekend**, maar voeg **voorbeelden** toe waar nuttig (bijv. hoe de finale tekst wordt gegenereerd).

- **Logging**:
    - Er is **geen logging** geïmplementeerd voor debugging en monitoring.
    - **Mistral Vibe-opmerking**: Voeg **logging** toe voor kritieke acties (bijv. STT-start, AI-bubblegeneratie) met behulp van `ILogger`.

- **Asynchrone Code**:
    - De code gebruikt **async/await** correct voor API calls en STT-verwerking.
    - **Mistral Vibe-opmerking**: Het gebruik van **async/await** is een **best practice** en zorgt voor **niet-blokkerende uitvoering**.

---
### **3. UI/UX**

#### **Observaties:**
- **UI Consistentie**:
    - De UI maakt gebruik van **bestaande theme variabelen** (bijv. `--theme-primary`, `--theme-secondary`) en is **consistent** met de rest van de applicatie.
    - **Voorbeeld van theming**:
      ```css
      .bubble {
          background-color: var(--theme-secondary, #f5f5f5);
          color: var(--theme-text, #333333);
      }
      ```
    - **Mistral Vibe-opmerking**: Het gebruik van **theme variabelen** is een **uitstekende keuze** en zorgt voor **consistente styling**.

- **Responsiveness**:
    - De UI is **responsief** en gebruikt **CSS Grid** voor de bubbelcontainer.
    - **Voorbeeld van responsiveness**:
      ```css
      .bubble-container {
          display: grid;
          grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
          gap: 15px;
          max-height: 500px;
          overflow-y: auto;
      }
      ```
    - **Mistral Vibe-opmerking**: De UI is **goed ontworpen** voor alle schermformaten, maar test op **mobiele apparaten** om te zorgen dat de bubbels **leesbaar** blijven.

- **Animaties**:
    - Er zijn **vloeiende animaties** voor bubbels en de microfoonknop.
    - **Voorbeeld van animatie**:
      ```css
      @keyframes fadeIn {
          from { opacity: 0; transform: translateY(10px); }
          to { opacity: 1; transform: translateY(0); }
      }
      ```
    - **Mistral Vibe-opmerking**: De animaties zijn **goed geïmplementeerd**, maar overweeg om **CSS transitions** te gebruiken voor betere prestaties.

- **Toegankelijkheid**:
    - Er zijn **ARIA-labels** toegevoegd voor knoppen en interactieve elementen.
    - **Voorbeeld van toegankelijkheid**:
      ```html
      <button
          onclick="removeBubble(@Model.Id)"
          class="bubble-close"
          aria-label="Verwijder bubbel">
          X
      </button>
      ```
    - **Mistral Vibe-opmerking**: De toegankelijkheid is **goed**, maar test met een **schermlezer** om ervoor te zorgen dat alle interacties werken.

- **Foutmeldingen**:
    - Er zijn **basisfoutmeldingen** voor mislukte API calls (bijv. `alert("STT opname mislukt. Probeer opnieuw.")`).
    - **Mistral Vibe-opmerking**: Voeg **duidelijkere foutmeldingen** toe en overweeg om **inline foutmeldingen** te gebruiken in plaats van `alert()`.

- **Gebruikerservaring**:
    - De **Magic Mode** is **intuïtief** en volgt de **gebruikersverwachtingen**.
    - **Mistral Vibe-opmerking**: Voeg een **korte handleiding** toe in de UI (bijv. een tooltip of modal) om gebruikers te helpen Magic Mode te gebruiken.

---

---

## **📝 Optimization Plan**

Hier is de **optimized implementation plan** voor Magic Mode, met focus op **code kwaliteit**, **UI/UX verbeteringen**, en **onderhoudbaarheid**.

---

```markdown
# Optimization Plan voor Magic Mode

---

## **📌 Context**
Dit plan bouwt voort op de bestaande implementatie en richt zich op **code kwaliteit**, **UI/UX verbeteringen**, en **onderhoudbaarheid**. De stappen zijn **atomair** (max 20 file modificaties per stap) en behouden de **bestaande functionaliteit**.

---

## **🔧 Code Structure & Organization**

---

### **📌 Stap 1: Herorganiseer State Management in een Dedicatie Map**
- **Task**: Verplaats state management logica naar een dedicated `Services/State/` map voor betere scheiding van concerns.
- **Files**:
  - `Services/MagicMode/MagicModeStateService.cs` → `Services/State/MagicModeStateService.cs`
  - `Services/MagicMode/IMagicModeStateService.cs` → `Services/State/IMagicModeStateService.cs`
- **Step Dependencies**: Geen.
- **User Instructions**: Geen.
- **Success Criteria**:
  - State management logica is **verplaatst** naar `Services/State/`.
  - Alle referenties naar de oude locatie zijn **geüpdatet**.

---

### **📌 Stap 2: Voeg Logging Toe voor Kritieke Acties**
- **Task**: Voeg logging toe voor debugging en monitoring van Magic Mode.
- **Files**:
  - `Program.cs`: Voeg `builder.Logging.AddConsole()` toe.
  - `Services/MagicMode/SttService.cs`:
    ```csharp
    private readonly ILogger<SttService> _logger;

    public SttService(IHttpClientFactory httpClientFactory, ILogger<SttService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("SttApi");
        _logger = logger;
    }

    public async Task<string> StartRecording()
    {
        _logger.LogInformation("STT opname gestart.");
        // Rest van de logica...
    }
    ```
  - `Services/MagicMode/AiService.cs`: Voeg logging toe voor AI-bubblegeneratie.
  - `Controllers/MagicModeController.cs`: Voeg logging toe voor controller-acties.
- **Step Dependencies**: Stap 1 (logging vereist `ILogger`).
- **User Instructions**: Geen.
- **Success Criteria**:
  - Logging is toegevoegd voor **STT-start**, **STT-stop**, **AI-bubblegeneratie**, en **controller-acties**.
  - Logs zijn zichtbaar in de console en kunnen worden **gefiterd op niveau** (bijv. `Information`, `Error`).

---

### **📌 Stap 3: Implementeer Specifieke Exceptions voor Foutafhandeling**
- **Task**: Vervang algemene exceptions door specifieke exceptions voor betere debugging.
- **Files**:
  - `Exceptions/SttException.cs`:
    ```csharp
    public class SttException : Exception
    {
        public SttException(string message) : base(message) { }
        public SttException(string message, Exception innerException) : base(message, innerException) { }
    }
    ```
  - `Exceptions/AiException.cs`:
    ```csharp
    public class AiException : Exception
    {
        public AiException(string message) : base(message) { }
        public AiException(string message, Exception innerException) : base(message, innerException) { }
    }
    ```
  - `Services/MagicMode/SttService.cs`:
    ```csharp
    if (!response.IsSuccessStatusCode)
    {
        _logger.LogError($"STT API call mislukt: {response.StatusCode}");
        throw new SttException($"STT opname mislukt met statuscode: {response.StatusCode}");
    }
    ```
  - `Services/MagicMode/AiService.cs`:
    ```csharp
    if (!response.IsSuccessStatusCode)
    {
        _logger.LogError($"Mistral AI API call mislukt: {response.StatusCode}");
        throw new AiException($"AI bubblegeneratie mislukt met statuscode: {response.StatusCode}");
    }
    ```
- **Step Dependencies**: Stap 2 (logging is vereist voor error logging).
- **User Instructions**: Geen.
- **Success Criteria**:
  - Specifieke exceptions (`SttException`, `AiException`) worden gebruikt voor foutafhandeling.
  - Foutberichten zijn **duidelijk** en bevatten **relevante informatie** (bijv. statuscode).

---

### **📌 Stap 4: Verbeter Naming Conventions met Prefixen**
- **Task**: Voeg **consistente prefixen** toe aan Magic Mode klassen en methodes voor betere herkenbaarheid.
- **Files**:
  - `Models/MagicMode/MagicModeViewModel.cs` → `Models/MagicMode/MmViewModel.cs`
  - `Models/MagicMode/BubbleModel.cs` → `Models/MagicMode/MmBubbleModel.cs`
  - `Services/MagicMode/SttService.cs` → `Services/MagicMode/MmSttService.cs`
  - `Services/MagicMode/AiService.cs` → `Services/MagicMode/MmAiService.cs`
  - `Controllers/MagicModeController.cs` → `Controllers/MmMagicModeController.cs`
  - `Views/MagicMode/Index.cshtml` → `Views/MmMagicMode/Index.cshtml`
  - `wwwroot/js/magic-mode/magicMode.js` → `wwwroot/js/mmMagicMode/magicMode.js`
- **Step Dependencies**: Geen.
- **User Instructions**: Geen.
- **Success Criteria**:
  - Alle Magic Mode klassen en methodes gebruiken het **`Mm` prefix**.
  - Bestandsnamen zijn **geüpdatet** en consistent met de nieuwe naming.

---

### **📌 Stap 5: Voeg Helper Methodes Toe voor Herhaalde Logica**
- **Task**: Maak helper methodes voor herhaalde logica (bijv. bubble rendering, state updates).
- **Files**:
  - `Utilities/MmMagicModeHelper.cs`:
    ```csharp
    public static class MmMagicModeHelper
    {
        /// <summary>
        /// Rendert een lijst met bubbels in HTML.
        /// </summary>
        public static string RenderBubbles(List<MmBubbleModel> bubbles)
        {
            var html = new StringBuilder();
            foreach (var bubble in bubbles)
            {
                html.Append($@"
                    <div class='bubble' id='bubble-{bubble.Id}'>
                        <p>{bubble.Text}</p>
                        <button onclick='removeBubble({bubble.Id})' class='bubble-close' aria-label='Verwijder bubbel'>X</button>
                    </div>
                ");
            }
            return html.ToString();
        }
    }
    ```
  - `Controllers/MmMagicModeController.cs`:
    ```csharp
    [HttpPost]
    public async Task<IActionResult> StartRecording()
    {
        var transcript = await _sttService.StartRecording();
        var bubbles = await _aiService.GenerateBubbles(transcript);
        foreach (var text in bubbles)
        {
            _stateService.AddBubble(text);
        }
        var html = MmMagicModeHelper.RenderBubbles(_stateService.GetBubbles());
        return Content(html, "text/html");
    }
    ```
- **Step Dependencies**: Stap 4 (naming is geüpdatet).
- **User Instructions**: Geen.
- **Success Criteria**:
  - Herhaalde logica is **verplaatst** naar helper methodes.
  - De controller gebruikt de helper methode voor bubble rendering.

---

## **🔧 Code Quality & Best Practices**

---

### **📌 Stap 6: Voeg XML-Documentatie Voorbeelden Toe**
- **Task**: Voeg **voorbeelden** toe aan de XML-documentatie voor publieke methodes.
- **Files**:
  - `Models/MagicMode/MmViewModel.cs`:
    ```csharp
    /// <summary>
    /// Genereert de finale tekst uit de bubbels.
    /// <example>
    /// <code>
    /// var model = new MmViewModel();
    /// model.AddBubble("Dit is bubbel 1");
    /// model.AddBubble("Dit is bubbel 2");
    /// var finalText = model.GetFinalText(); // "Dit is bubbel 1. Dit is bubbel 2"
    /// </code>
    /// </example>
    /// </summary>
    /// <returns>De gegenereerde tekst.</returns>
    public string GetFinalText()
    {
        return string.Join(". ", Bubbles.Select(b => b.Text));
    }
    ```
  - `Services/MagicMode/MmSttService.cs`:
    ```csharp
    /// <summary>
    /// Start de STT-opname.
    /// <example>
    /// <code>
    /// var service = new MmSttService(httpClientFactory);
    /// var transcript = await service.StartRecording(); // "Dit is een transcriptie"
    /// </code>
    /// </example>
    /// </summary>
    /// <returns>De gegenereerde transcriptie.</returns>
    /// <exception cref="SttException">Gegooid als de STT-opname mislukt.</exception>
    public async Task<string> StartRecording()
    {
        // ...
    }
    ```
- **Step Dependencies**: Stap 5 (helper methodes zijn toegevoegd).
- **User Instructions**: Geen.
- **Success Criteria**:
  - XML-documentatie bevat **voorbeelden** voor alle publieke methodes.
  - De voorbeelden zijn **kort** en **duidelijk**.

---

### **📌 Stap 7: Implementeer Input Validatie**
- **Task**: Voeg **input validatie** toe om fouten te voorkomen.
- **Files**:
  - `Models/MagicMode/MmViewModel.cs`:
    ```csharp
    public class MmViewModel
    {
        private string _questionText = string.Empty;

        [Required(ErrorMessage = "Vraagtekst is verplicht.")]
        [StringLength(500, ErrorMessage = "Vraagtekst mag maximaal 500 karakters bevatten.")]
        public string QuestionText
        {
            get => _questionText;
            set => _questionText = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
    ```
  - `Controllers/MmMagicModeController.cs`:
    ```csharp
    [HttpPost]
    public IActionResult Index([FromBody] MmViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        // Rest van de logica...
    }
    ```
- **Step Dependencies**: Stap 6 (documentatie is toegevoegd).
- **User Instructions**: Geen.
- **Success Criteria**:
  - Input validatie is toegevoegd voor alle publieke methodes.
  - Foutmeldingen worden **duidelijk** weergegeven aan de gebruiker.

---

## **🎨 UI/UX Improvements**

---

### **📌 Stap 8: Verbeter Foutmeldingen in de UI**
- **Task**: Vervang `alert()` door **inline foutmeldingen** en voeg feedback toe tijdens het laden.
- **Files**:
  - `wwwroot/css/magic-mode/styles.css`:
    ```css
    .error-message {
        color: var(--theme-error, #e74c3c);
        background-color: #ffebee;
        padding: 10px;
        border-radius: 4px;
        margin: 10px 0;
        display: none;
    }

    .loading-indicator {
        display: inline-block;
        width: 20px;
        height: 20px;
        border: 3px solid rgba(0, 0, 0, 0.1);
        border-radius: 50%;
        border-top-color: var(--theme-primary, #3498db);
        animation: spin 1s ease-in-out infinite;
        margin-left: 10px;
    }

    @keyframes spin {
        to { transform: rotate(360deg); }
    }
    ```
  - `wwwroot/js/mmMagicMode/magicMode.js`:
    ```javascript
    function showError(message) {
        const errorElement = document.createElement('div');
        errorElement.className = 'error-message';
        errorElement.textContent = message;
        document.getElementById('error-container').appendChild(errorElement);
        errorElement.style.display = 'block';
    }

    function showLoading() {
        const button = document.getElementById('microphoneButton');
        const span = button.querySelector('span');
        span.textContent += ' ';
        const loadingIndicator = document.createElement('span');
        loadingIndicator.className = 'loading-indicator';
        span.appendChild(loadingIndicator);
    }

    function hideLoading() {
        const loadingIndicator = document.querySelector('.loading-indicator');
        if (loadingIndicator) {
            loadingIndicator.remove();
        }
    }
    ```
  - `Views/MmMagicMode/Index.cshtml`:
    ```html
    <div id="error-container"></div>
    <button id="microphoneButton" class="btn-microphone" onclick="toggleRecording()">
        @(Model.IsRecording ? "⏸" : "🎤")
        <span>@(Model.IsRecording ? " Pauzeer" : " Opnemen")</span>
    </button>
    ```
  - `Controllers/MmMagicModeController.cs`:
    ```csharp
    [HttpPost]
    public async Task<IActionResult> StartRecording()
    {
        try
        {
            showLoading();
            var transcript = await _sttService.StartRecording();
            // ...
        }
        catch (SttException ex)
        {
            _logger.LogError(ex, "STT-opname mislukt");
            return StatusCode(500, new { error = "STT-opname mislukt. Probeer opnieuw." });
        }
    }
    ```
- **Step Dependencies**: Stap 7 (input validatie is toegevoegd).
- **User Instructions**: Geen.
- **Success Criteria**:
  - Foutmeldingen worden **inline** weergegeven in plaats van `alert()`.
  - Er is een **loading indicator** tijdens het laden.
  - De UI blijft **responsief** tijdens het laden.

---
### **📌 Stap 9: Voeg Een Korte Handleiding Toe in de UI**
- **Task**: Voeg een **tooltip of modal** toe met een korte handleiding voor Magic Mode.
- **Files**:
  - `Views/MmMagicMode/Index.cshtml`:
    ```html
    <div class="help-tooltip">
        <span class="tooltiptext">
            <h3>Hoe gebruik je Magic Mode?</h3>
            <ol>
                <li>Klik op de microfoonknop om te starten met spreken.</li>
                <li>Spreek je antwoord in.</li>
                <li>Klik op een bubbel om deze te selecteren of verwijder deze met "X".</li>
                <li>Klik op "Sluiten" om Magic Mode af te sluiten.</li>
            </ol>
        </span>
        <button class="help-button" onclick="toggleHelp()">?</button>
    </div>
    ```
  - `wwwroot/css/magic-mode/styles.css`:
    ```css
    .help-tooltip {
        position: relative;
        display: inline-block;
        margin-left: 10px;
    }

    .help-button {
        background: none;
        border: none;
        font-size: 18px;
        cursor: pointer;
        color: var(--theme-primary);
    }

    .tooltiptext {
        visibility: hidden;
        background-color: var(--theme-surface);
        color: var(--theme-text);
        text-align: left;
        border-radius: 6px;
        padding: 10px;
        position: absolute;
        z-index: 1;
        bottom: 125%;
        left: 50%;
        transform: translateX(-50%);
        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    }

    .help-tooltip:hover .tooltiptext {
        visibility: visible;
    }
    ```
- **Step Dependencies**: Stap 8 (UI is al verbeterd).
- **User Instructions**: Geen.
- **Success Criteria**:
  - Een **tooltip** met een korte handleiding is toegevoegd aan de UI.
  - De tooltip is **duidelijk** en **gemakkelijk te begrijpen**.

---
### **📌 Stap 10: Test Responsiveness op Mobiele Apparaten**
- **Task**: Test en pas de UI aan voor **mobiele apparaten**.
- **Files**:
  - `wwwroot/css/magic-mode/styles.css`:
    ```css
    @media (max-width: 768px) {
        .bubble-container {
            grid-template-columns: 1fr;
        }

        .modal-content {
            width: 95%;
            margin: 5% auto;
        }
    }
    ```
- **Step Dependencies**: Stap 9 (UI is volledig verbeterd).
- **User Instructions**:
  - Test Magic Mode op een **mobiel apparaat** (bijv. iPhone, Android).
  - Controleer of:
    - De bubbels **leesbaar** zijn.
    - De container **scrollbaar** is.
    - De knoppen **gemakkelijk te bedienen** zijn.
- **Success Criteria**:
  - Magic Mode werkt **goed op mobiele apparaten**.
  - De UI is **responsief** en **gebruiksvriendelijk**.

---
### **📌 Stap 11: Voer Toegankelijkheidstests Uit**
- **Task**: Voer **toegankelijkheidstests** uit met een schermlezer en pas de UI aan waar nodig.
- **User Instructions**:
  - Gebruik een **schermlezer** (bijv. NVDA, JAWS) om Magic Mode te testen.
  - Controleer of:
    - Alle knoppen **ARIA-labels** hebben.
    - De focusindicatoren **zichtbaar** zijn.
    - De UI **duidelijk** is voor schermlezers.
- **Success Criteria**:
  - Magic Mode voldoet aan **WCAG-richtlijnen**.
  - De UI is **toegankelijk** voor alle gebruikers.

---
## **📌 Stap 12: Optimaliseer Prestaties**
- **Task**: Optimaliseer Magic Mode voor **snelle laadtijden** en **responsiviteit**.
- **Files**:
  - `vite.config.ts` (indien gebruikt voor bundeling):
    ```javascript
    export default defineConfig({
        build: {
            minify: true,
            rollupOptions: {
                output: {
                    manualChunks: {
                        vendor: ['react', 'react-dom'],
                    },
                },
            },
        },
    });
    ```
  - `Program.cs`:
    ```csharp
    builder.Services.AddResponseCaching();
    app.UseResponseCaching();
    ```
  - `Controllers/MmMagicModeController.cs`:
    ```csharp
    [ResponseCache(Duration = 60)] // Cache response voor 60 seconden
    [HttpGet]
    public IActionResult Index()
    {
        // ...
    }
    ```
- **Step Dependencies**: Stap 11 (toegankelijkheid is getest).
- **User Instructions**: Geen.
- **Success Criteria**:
  - Magic Mode laadt **snel** (< 2 seconden).
  - De UI is **vloeiend** en **responsief**.

---
## **📌 Stap 13: Voeg End-to-End Tests Toe**
- **Task**: Voeg **Cypress/Playwright tests** toe voor end-to-end validatie.
- **Files**:
  - `cypress/e2e/magicMode.cy.js`:
    ```javascript
    describe('Magic Mode', () => {
        beforeEach(() => {
            cy.visit('/Survey');
        });

        it('start opname, genereert bubbels, en sluit Magic Mode af', () => {
            cy.intercept('POST', '/api/voxtrall/start', {
                body: 'Dit is een test transcriptie.'
            }).as('startRecording');

            cy.intercept('POST', '/api/mistral/key-phrases', {
                body: '["Bubbel 1", "Bubbel 2"]'
            }).as('generateBubbles');

            cy.contains('🎤 Magic Mode').click();
            cy.get('#microphoneButton').click();
            cy.wait('@startRecording');
            cy.wait('@generateBubbles');

            cy.get('.bubble-container').should('contain', 'Bubbel 1');
            cy.get('.bubble-close').first().click();
            cy.get('.bubble-container').should('not.contain', 'Bubbel 1');

            cy.get('#closeButton').click();
            cy.get('#surveyTextarea').should('not.be.empty');
        });
    });
    ```
- **Step Dependencies**: Stap 12 (prestaties zijn geoptimaliseerd).
- **User Instructions**: Voer de tests uit met:
  ```bash
  npx cypress run
  ```
- **Success Criteria**:
    - Alle end-to-end tests **slagen**.
    - Magic Mode werkt **correct in een productieomgeving**.

---
## **📌 Stap 14: Maak Een Productie-Ready Build**
- **Task**: Genereer een **productie-ready build** met alle optimalisaties.
- **User Instructions**:
    - Voor een **React/TypeScript frontend**:
      ```bash
      npm run build
      ```
    - Voor een **ASP.NET Core backend**:
      ```bash
      dotnet publish -c Release -o ./publish
      ```
- **Success Criteria**:
    - De build is **succesvol gegenereerd**.
    - Er zijn **geen fouten** in de console.

---
## **📌 Stap 15: Maak Een Onderhoudsdocumentatie**
- **Task**: Maak een **README.md** met instructies voor onderhoud en uitbreiding.
- **Files**:
    - `README.md`:
      ```markdown
      # Magic Mode - Onderhoudsdocumentatie
  
      ## 📌 Overzicht
      Magic Mode is een spraakgestuurde interface voor open vragen in surveys.
  
      ## 📋 Algemene Richtlijnen
      - **Gebruik**: Klik op de "🎤 Magic Mode" knop naast een open vraag.
      - **STT**: Spreek je antwoord in en wacht tot de bubbels verschijnen.
      - **Bubbels**: Klik op een bubbel om deze te selecteren of te verwijderen.
      - **Sluiten**: Klik op "Sluiten" om Magic Mode af te sluiten.
  
      ## 🧪 Tests
      - **Unit Tests**: `dotnet test`
      - **E2E Tests**: `npx cypress run`
      - **Toegankelijkheidstests**: Gebruik een schermlezer (bijv. NVDA).
  
      ## 🔧 Uitbreidingen
      - **Nieuwe AI-modellen**: Voeg een nieuwe service toe in `Services/MagicMode/`.
      - **Nieuwe UI-componenten**: Maak een nieuwe partial view of component in `Views/MagicMode/PartialViews/` of `Components/MagicMode/`.
  
      ## 📝 Logging
      - Logs zijn beschikbaar in de console en kunnen worden gefilterd op niveau.
      ```

- **Step Dependencies**: Stap 14 (build is gegenereerd).
- **User Instructions**: Geen.
- **Success Criteria**:
    - De documentatie is **compleet** en **duidelijk**.
    - Toekomstige ontwikkelaars kunnen Magic Mode **gemakkelijk onderhouden**.

---
## **📌 Logische Volgende Stap**
Na het implementeren van deze stappen is Magic Mode **klaar voor productie**. De volgende stap is:
- **Deploy Magic Mode naar de productieomgeving** en monitor de prestaties.
- **Verzamel feedback** van gebruikers en pas de UI/UX aan waar nodig.

---
```

---
Dit **optimization plan** zorgt ervoor dat Magic Mode **perfect geïmplementeerd** wordt, met focus op **code kwaliteit**, **UI/UX verbeteringen**, en **onderhoudbaarheid**. Elke stap is **atomair** en kan onafhankelijk worden geïmplementeerd.

Laat me weten als je hulp nodig hebt bij het implementeren van een van de stappen! 😊