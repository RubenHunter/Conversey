```markdown
# 🧪 Fase 7: Eindtesten, Optimalisatie en Documentatie
**Voorbeeldcode - Mistral Vibe bepaalt de optimale implementatie**

---

## 📌 Doel
In deze fase wordt **Magic Mode finaal getest**, **geoptimaliseerd** en **gedocumenteerd**. Mistral Vibe zal:
1. **Eindtesten** uitvoeren om te controleren of alle functionaliteit werkt zoals verwacht.
2. **Optimalisaties** doorvoeren voor prestaties, gebruikerservaring en codekwaliteit.
3. **Documentatie** aanmaken zodat Magic Mode onderhoudbaar en schaalbaar is.
4. Zorgen dat Magic Mode **klaar is voor productie**.

---
## 📌 Belangrijke Richtlijnen voor Mistral Vibe
> **⚠️ BELANGRIJK**:
> Alle code, tests en documentatie in dit bestand zijn **voorbeeldimplementatie** en dienen als **inspiratie**. Mistral Vibe zal:
> 1. De **bestaande teststrategieën** (unit tests, integratietests, e2e tests) analyseren.
> 2. Bepalen of:
>    - **Jest/React Testing Library** wordt gebruikt voor frontend tests.
>    - **xUnit/Moq** wordt gebruikt voor backend tests.
>    - **Cypress/Playwright** wordt gebruikt voor end-to-end tests.
> 3. De **beste test- en optimalisatiestrategie** selecteren gebaseerd op:
>    - Bestaande **testframeworks**.
>    - **Prestatie-eisen** (bijv. laadtijden, reactietijden).
>    - **Onderhoudbaarheid** en **schaalbaarheid**.

### Richtlijnen voor Mistral Vibe:
| Aspect               | Richtlijn                                                                                   |
|----------------------|---------------------------------------------------------------------------------------------|
| **Test Framework**   | Gebruik **bestaande frameworks** (bijv. Jest, xUnit) voor consistentie.                    |
| **Test Coverage**    | Zorg dat **alle kritieke functionaliteit** wordt getest (STT, AI, UI-interacties).           |
| **Prestatie**        | Optimaliseer voor **snelle laadtijden** en **responsiviteit**.                              |
| **Documentatie**     | Documenteer **installatie, gebruik en onderhoud** voor toekomstige ontwikkelaars.            |

---
---

## 🛠 Stappenplan (Met Flexibiliteit voor Mistral Vibe)

---

### **Stap 1: Maak een Testplan**
Mistral Vibe zal een **testplan** maken dat alle kritieke functionaliteit van Magic Mode dekt.

#### **Voorbeeld Testplan**
| **Test Scenario**                     | **Testmethode**               | **Verwachting**                                                                 |
|---------------------------------------|-------------------------------|---------------------------------------------------------------------------------|
| Magic Mode starten en opname activeren | Handmatig + E2E Test          | Opname start, microfoonknop pulseert.                                           |
| STT-transcriptie wordt gegenereerd    | Handmatig + Unit Test         | Transcriptie verschijnt in de UI.                                               |
| Mistral AI genereert bubbels           | Handmatig + Mock Test         | Bubbels verschijnen in de bubbelcontainer.                                     |
| Bubbels kunnen worden verwijderd      | Handmatig + UI Test           | Bubbel verdwijnt na klik op "X".                                                |
| Finale tekst wordt gegenereerd        | Handmatig + Integration Test  | Finale tekst verschijnt in de survey-textarea.                                 |
| Magic Mode sluit correct              | Handmatig + E2E Test          | Modal/popup sluit, geen fouten in console.                                      |
| UI is responsive op alle apparaten    | Handmatig + Responsive Test   | UI ziet er goed uit op desktop, tablet en mobiel.                               |

---
---

### **Stap 2: Voer Unit Tests Uit**
Mistral Vibe zal **unit tests** schrijven voor de logica van Magic Mode.

#### **Voorbeeld: Unit Tests voor State Management**
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

#### **Voorbeeld: Unit Tests voor AI-Service**
```csharp
// Tests/AiServiceTests.cs
using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using UI_MVC.Services.MagicMode;

namespace UI_MVC.Tests
{
    public class AiServiceTests
    {
        [Fact]
        public async Task GenerateBubbles_ReturnsListOfBubbles()
        {
            // Arrange
            var mockHttp = new Mock<IHttpClientFactory>();
            var mockHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri("https://api.mistral.ai/")
            };
            mockHttp.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var service = new AiService(mockHttp.Object);

            // Mock de API response
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("[\"Bubbel 1\", \"Bubbel 2\"]")
                });

            // Act
            var bubbles = await service.GenerateBubbles("Test transcriptie");

            // Assert
            Assert.Equal(2, bubbles.Count);
            Assert.Equal("Bubbel 1", bubbles[0]);
            Assert.Equal("Bubbel 2", bubbles[1]);
        }
    }
}
```

---
---

### **Stap 3: Voer Integratietests Uit**
Mistral Vibe zal **integratietests** schrijven om te controleren of de componenten samenwerken.

#### **Voorbeeld: Integratietest voor MagicModeController**
```csharp
// Tests/MagicModeControllerTests.cs
using Xunit;
using Microsoft.AspNetCore.Mvc;
using UI_MVC.Controllers;
using UI_MVC.Models.MagicMode;
using UI_MVC.Services.MagicMode;
using Moq;

namespace UI_MVC.Tests
{
    public class MagicModeControllerTests
    {
        [Fact]
        public async Task StartRecording_ReturnsBubbles()
        {
            // Arrange
            var mockSttService = new Mock<ISttService>();
            var mockAiService = new Mock<IAiService>();
            var mockStateService = new Mock<IMagicModeStateService>();

            mockSttService.Setup(s => s.StartRecording()).ReturnsAsync("Test transcriptie");
            mockAiService.Setup(a => a.GenerateBubbles("Test transcriptie"))
                         .ReturnsAsync(new List<string> { "Bubbel 1", "Bubbel 2" });

            var controller = new MagicModeController(
                mockSttService.Object,
                mockAiService.Object,
                mockStateService.Object
            );

            // Act
            var result = await controller.StartRecording();

            // Assert
            var viewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<MagicModeViewModel>(viewResult.Model);
            Assert.Equal(2, model.Bubbles.Count);
        }
    }
}
```

---
---

### **Stap 4: Voer End-to-End (E2E) Tests Uit**
Mistral Vibe zal **E2E tests** schrijven om de volledige gebruikerservaring te valideren.

#### **Voorbeeld: E2E Test met Cypress**
```javascript
// cypress/e2e/magicMode.cy.js
describe('Magic Mode', () => {
  beforeEach(() => {
    cy.visit('/Survey');
  });

  it('start opname en genereert bubbels', () => {
    // Open Magic Mode
    cy.contains('🎤 Magic Mode').click();

    // Mock STT response
    cy.intercept('POST', '/api/voxtrall/start', {
      statusCode: 200,
      body: 'Dit is een test transcriptie.'
    }).as('startRecording');

    // Start opname
    cy.get('#microphoneButton').click();
    cy.wait('@startRecording');

    // Mock Mistral AI response
    cy.intercept('POST', '/api/mistral/key-phrases', {
      statusCode: 200,
      body: '["Bubbel 1", "Bubbel 2"]'
    }).as('generateBubbles');

    // Controleer of bubbels verschijnen
    cy.get('.bubble-container').should('contain', 'Bubbel 1');
    cy.get('.bubble-container').should('contain', 'Bubbel 2');

    // Verwijder een bubbel
    cy.get('.bubble-close').first().click();
    cy.get('.bubble-container').should('not.contain', 'Bubbel 1');

    // Sluit Magic Mode
    cy.get('#closeButton').click();
    cy.get('#surveyTextarea-1').should('not.be.empty');
  });
});
```

---
---

### **Stap 5: Optimaliseer Prestaties**
Mistral Vibe zal **prestatieoptimalisaties** doorvoeren voor Magic Mode.

#### **Voorbeeld: Optimalisaties**
| **Optimalisatie**                     | **Implementatie**                                                                 |
|---------------------------------------|-----------------------------------------------------------------------------------|
| **Lazy Loading van Bubbels**          | Laad bubbels pas in wanneer ze nodig zijn (bijv. bij scrollen).                  |
| **Debounce van API Calls**            | Voorkom te veel API calls door debouncing toe te passen op STT/AI calls.         |
| **Caching van API Responses**         | Cache de laatste STT/AI responses om herhaalde calls te voorkomen.               |
| **Compressie van CSS/JS**             | Gebruik bundlers (bijv. Webpack, Vite) om bestanden te minifyen en bundelen.      |
| **Responsive Images**                 | Zorg dat afbeeldingen zijn geoptimaliseerd voor mobiel.                           |
| **Lazy Loading van Images**           | Laad afbeeldingen pas in wanneer ze in beeld komen.                              |

---
---

### **Stap 6: Test Responsiveness**
Controleer of Magic Mode **goed werkt op alle apparaten**.

#### **Voorbeeld: Responsiveness Test**
1. **Desktop**:
    - Test op schermen van **1920px** tot **1200px**.
    - Controleer of de bubbelcontainer **correct wordt weergegeven**.
2. **Tablet**:
    - Test op schermen van **768px** tot **1024px**.
    - Controleer of de bubbels **in een grid van 2 kolommen** worden weergegeven.
3. **Mobiel**:
    - Test op schermen kleiner dan **768px**.
    - Controleer of de bubbels **in een grid van 1 kolom** worden weergegeven en de container **scrollbaar** is.

---
---

### **Stap 7: Test Toegankelijkheid**
Mistral Vibe zal **toegankelijkheidstests** uitvoeren om te controleren of Magic Mode voldoet aan **WCAG-richtlijnen**.

#### **Voorbeeld: Toegankelijkheidstests**
| **Test**                              | **Hoe te Testen**                                                                 |
|---------------------------------------|-----------------------------------------------------------------------------------|
| **ARIA-labels**                       | Controleer of knoppen en interactieve elementen **ARIA-labels** hebben.             |
| **Toetsenbordnavigatie**              | Gebruik alleen het toetsenbord om Magic Mode te bedienen.                          |
| **Kleurcontrast**                     | Controleer of de kleuren voldoende contrast hebben (minimaal 4.5:1 voor tekst).    |
| **Focusindicatoren**                  | Controleer of focusindicatoren zichtbaar zijn voor alle interactieve elementen.     |
| **Schermlezercompatibiliteit**         | Gebruik een schermlezer (bijv. NVDA, JAWS) om Magic Mode te testen.                |

---
---

### **Stap 8: Optimaliseer de UI/UX**
Mistral Vibe zal **UI/UX-optimalisaties** doorvoeren voor een betere gebruikerservaring.

#### **Voorbeeld: UI/UX Optimalisaties**
| **Optimalisatie**                     | **Implementatie**                                                                 |
|---------------------------------------|-----------------------------------------------------------------------------------|
| **Verbeterde Animaties**              | Zorg dat animaties **vloeiend** zijn en niet te traag.                           |
| **Betere Foutmeldingen**              | Voeg duidelijke foutmeldingen toe bij mislukte API calls.                         |
| **Loading States**                    | Voeg **loading indicators** toe tijdens het verwerken van STT/AI.                  |
| **Inline Hulptekst**                  | Voeg **tooltips** toe om gebruikers te helpen Magic Mode te gebruiken.             |
| **Automatische Focus**                | Focus de microfoonknop automatisch wanneer Magic Mode wordt geopend.              |
| **Terug naar Survey**                 | Zorg dat de gebruiker **terug kan naar de survey** zonder de gegenereerde tekst te verliezen. |

---
---

### **Stap 9: Maak Een Productie-Ready Build**
Mistral Vibe zal een **productie-ready build** maken met alle optimalisaties.

#### **Voorbeeld: Productie-Ready Build met Vite**
```bash
# Voor een React/TypeScript frontend
npm run build

# Voor een ASP.NET Core backend
dotnet publish -c Release -o ./publish
```

---
---

### **Stap 10: Documenteer Magic Mode**
Mistral Vibe zal **documentatie** schrijven zodat Magic Mode onderhoudbaar en schaalbaar is.

#### **Voorbeeld: README.md voor Magic Mode**
```markdown
# 🎤 Magic Mode - Documentatie

## 📌 Overzicht
Magic Mode is een **interactieve, spraakgestuurde interface** voor open vragen in surveys. Gebruikers kunnen:
- **Spreken** in plaats van typen.
- **AI-gegenereerde bubbels** selecteren/verwijderen.
- Een **finale tekst** genereren gebaseerd op geselecteerde bubbels.

---

## 📋 Algemene Richtlijnen
- **Gebruik**: Klik op de "🎤 Magic Mode" knop naast een open vraag om Magic Mode te starten.
- **STT**: Spreek je antwoord in en wacht tot de bubbels verschijnen.
- **Bubbels**: Klik op een bubbel om deze te **selecteren** of klik op "X" om deze te **verwijderen**.
- **Sluiten**: Klik op "Sluiten" om Magic Mode af te sluiten en de gegenereerde tekst in de survey te plaatsen.

---

## 🧪 Testen
### Unit Tests
```bash
dotnet test
```

### Integratietests
```bash
dotnet test --filter MagicModeControllerTests
```

### End-to-End Tests
```bash
npx cypress run
```

---

## 🧩 Technische Details
### State Management
- **Locatie**: `MagicModeStateService.cs`
- **Methodes**: `AddBubble`, `RemoveBubble`, `GetFinalText`

### API Calls
- **STT**: `POST /api/voxtrall/start`
- **AI**: `POST /api/mistral/key-phrases`

### UI Componenten
- **Bubble**: `Bubble.razor` / `_BubblePartial.cshtml`
- **BubbleContainer**: `BubbleContainer.razor` / `_BubblesPartial.cshtml`
- **MicrofoonButton**: `_MicrophoneButton.cshtml`
- **CloseButton**: `_CloseButton.cshtml`

---
```

---
---

### **Stap 11: Maak een Checklist voor Productie-Ready**
Mistral Vibe zal een **checklist** maken om te controleren of Magic Mode klaar is voor productie.

#### **Voorbeeld: Checklist voor Productie-Ready**
| **Item**                                      | **Status** | **Opmerkingen**                     |
|-----------------------------------------------|------------|--------------------------------------|
| Alle unit tests slagen                        | ⬜         |                                      |
| Alle integratietests slagen                   | ⬜         |                                      |
| Alle E2E tests slagen                         | ⬜         |                                      |
| Responsiveness getest op alle apparaten       | ⬜         |                                      |
| Toegankelijkheidstests voldoen aan WCAG        | ⬜         |                                      |
| Prestatieoptimalisaties doorgevoerd           | ⬜         |                                      |
| UI/UX-optimalisaties doorgevoerd              | ⬜         |                                      |
| Documentatie compleet                         | ⬜         |                                      |
| Productie-ready build gegenereerd             | ⬜         |                                      |
| Monitoring en Logging ingesteld               | ⬜         |                                      |
| Back-up en herstelplan opgesteld              | ⬜         |                                      |

---
---

## ⚠ Veelvoorkomende Problemen en Oplossingen
Mistral Vibe zal deze problemen analyseren en oplossingen voorstellen gebaseerd op de bestaande codebase.

| Probleem                                      | Mogelijke Oplossing                                                                 |
|-----------------------------------------------|------------------------------------------------------------------------------------|
| **Tests falen**                               | Controleer de **testdata** en **mock responses**.                                  |
| **Prestatieproblemen**                        | Optimaliseer **bubble rendering**, **API calls**, en **bundeling**.                |
| **Toegankelijkheidsfouten**                   | Gebruik **ARIA-labels**, **toetsenbordnavigatie**, en **kleurcontrast-checkers**.   |
| **UI ziet er inconsistent uit**               | Zorg dat **CSS variabelen** en **bestaande theming** worden gebruikt.              |
| **Documentatie mist details**                 | Voeg **screenshots**, **codevoorbeelden**, en **FAQ** toe.                        |

---
---

## 📌 Changelog
| Datum       | Wijziging                                  | Door          |
|-------------|--------------------------------------------|---------------|
| 2026-04-26  | Initiële versie (voorbeeld)                | Mistral Vibe  |
| 2026-04-26  | Unit Tests, Integratietests, E2E Tests     | Mistral Vibe  |
| 2026-04-26  | Prestatieoptimalisaties toegevoegd         | Mistral Vibe  |
| 2026-04-26  | Documentatie en Checklist gegenereerd     | Mistral Vibe  |
```