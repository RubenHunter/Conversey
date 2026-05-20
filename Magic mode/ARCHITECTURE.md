# Magic Mode — Architecture & Technical Stack

This document describes the technical architecture, file structure, and design patterns used in Magic Mode.

---

## Project Structure

```
backend/
├── BL/
│   └── MagicMode/                          # Business logic layer
│       └── MagicModeManager.cs             # Or similar BL classes
│
├── Domain/
│   └── DTOs/
│       └── MagicMode/                      # Domain data transfer objects
│           ├── ExtractKeyPhrasesRequest.cs
│           └── ExtractKeyPhrasesResponse.cs
│
├── UI-MVC/
│   ├── Controllers/
│   │   └── Api/
│   │       └── MagicModeController.cs     # REST API endpoint
│   │
│   ├── Assets/
│   │   ├── components/
│   │   │   └── survey/
│   │   │       └── magicMode/
│   │   │           ├── magicModeModal.ts   # Modal lifecycle
│   │   │           ├── bubbleList.ts       # Bubble state management
│   │   │           └── index.ts            # Re-exports
│   │   │
│   │   ├── services/
│   │   │   └── speechService.ts           # STTManager, audio handling
│   │   │
│   │   ├── styles/
│   │   │   └── pages/
│   │   │       └── magic-mode.css         # Tailwind + DaisyUI styles
│   │   │
│   │   └── i18n/
│   │       └── survey.ts                  # Language detection
│   │
│   └── DTOs/
│       └── MagicMode/                      # UI DTOs (if separate)
│           ├── ExtractKeyPhrasesRequest.cs
│           └── ExtractKeyPhrasesResponse.cs
│
└── Tests/
    └── MagicMode/
        └── MagicModeControllerTests.cs     # xUnit tests
```

---

## Technical Stack

### Backend (C#)

| Component | Technology | Version | Purpose |
|---|---|---|---|
| Business Logic | C# | .NET 8+ | `BL/MagicMode/` |
| REST API | ASP.NET Core | 8+ | `MagicModeController` |
| DTOs | C# Records | 8+ | Data transfer objects |
| Dependency Injection | .NET Core DI | 8+ | Service registration |
| HTTP Client | IHttpClientFactory | 8+ | Mistral API calls |

### Frontend (TypeScript)

| Component | Technology | Version | Purpose |
|---|---|---|---|
| Language | TypeScript | 5.x | All frontend code |
| Bundler | Vite | Latest | Module bundling |
| CSS Framework | Tailwind CSS | 4.x | Styling |
| Component Library | DaisyUI | 5.x | Pre-built components |
| Icons | Heroicons | Latest | SVG icons |
| Build Tool | pnpm | 8.x | Package management |

### STT (Speech-to-Text)

| Component | Technology | Location | Purpose |
|---|---|---|---|
| STT Manager | Web Audio API + MediaRecorder | `speechService.ts` | Audio capture |
| STT Provider | Mistral Speech API | External | Transcription |
| Language Detection | Custom | `i18n/survey.ts` | `detectLocale()` |

### AI

| Component | Technology | Location | Purpose |
|---|---|---|---|
| AI Manager | C# Interface | `BL/Ai/Managers/IAiManager.cs` | AI abstraction |
| Mistral Implementation | C# | `BL/Ai/MistralAiManager.cs` | Mistral API calls |
| No-op Implementation | C# | `BL/Ai/NoopAiManager.cs` | Testing fallback |
| Model Configuration | JSON | `appsettings.json` | Model IDs |

---

## Design Patterns

### 1. Factory Pattern
Used for creating modal and bubble list instances.

**`magicModeModal.ts`:**
```typescript
export function createMagicModeModal(): MagicModeModalController {
    // Creates and returns modal controller
}
```

**`bubbleList.ts`:**
```typescript
export function createBubbleList(): BubbleListController {
    // Creates and returns bubble list controller
}
```

### 2. Dependency Injection (Backend)
All services are injected via constructors.

**`MagicModeController.cs`:**
```csharp
public class MagicModeController(IAiManager aiManager) {
    // Uses injected IAiManager
}
```

### 3. Repository/Service Pattern
- **IAiManager**: Interface for AI operations
- **MistralAiManager**: Real implementation
- **NoopAiManager**: Test/mock implementation

### 4. Event-Driven Architecture (Frontend)
- STT `onText` callback triggers AI calls
- Modal close callback updates textarea
- SPA navigation events trigger cleanup

### 5. Client-Side State Management
- **No server-side state** for bubbles
- State lives in TypeScript module instances
- Two main state structures in `bubbleList.ts`:
  - `activeBubbles: string[]` — Currently visible bubbles
  - `rejectedPhrases: Set<string>` — User-rejected phrases (normalized)

---

## Key Interfaces

### Backend (C#)

```csharp
// BL/Ai/Managers/IAiManager.cs
public interface IAiManager
{
    Task<IReadOnlyList<string>> ExtractKeyPhrases(
        string transcript,
        string language,
        int maxPhrases,
        IReadOnlyList<string>? existingPhrases = null,
        IReadOnlyList<string>? rejectedPhrases = null);
}
```

```csharp
// Domain/DTOs/MagicMode/ExtractKeyPhrasesRequest.cs
public record ExtractKeyPhrasesRequest(
    string Transcript,
    string Language,
    int MaxPhrases,
    IReadOnlyList<string>? ExistingPhrases = null,
    IReadOnlyList<string>? RejectedPhrases = null);
```

```csharp
// Domain/DTOs/MagicMode/ExtractKeyPhrasesResponse.cs
public record ExtractKeyPhrasesResponse(IReadOnlyList<string> Phrases);
```

### Frontend (TypeScript)

```typescript
// magicModeModal.ts
export interface MagicModeModalController {
    open(questionText: string, onClose: (text: string) => void): void;
    destroy(): void;
}
```

```typescript
// bubbleList.ts
export interface BubbleListController {
    addBubbles(phrases: string[]): void;
    removeBubble(index: number): void;
    getBubbles(): string[];
    getRejectedPhrases(): string[];
    reset(): void;
    readonly element: HTMLElement;
}
```

---

## File Locations Quick Reference

| Purpose | Location |
|---|---|
| **REST Endpoint** | `UI-MVC/Controllers/Api/MagicModeController.cs` |
| **AI Interface** | `BL/Ai/Managers/IAiManager.cs` |
| **AI Implementations** | `BL/Ai/MistralAiManager.cs`, `BL/Ai/NoopAiManager.cs` |
| **DTOs** | `Domain/DTOs/MagicMode/`, `UI-MVC/DTOs/MagicMode/` |
| **Modal** | `Assets/components/survey/magicMode/magicModeModal.ts` |
| **Bubble List** | `Assets/components/survey/magicMode/bubbleList.ts` |
| **STT Service** | `Assets/services/speechService.ts` |
| **Styles** | `Assets/styles/pages/magic-mode.css` |
| **Language Detection** | `Assets/i18n/survey.ts` |
| **Tests** | `Tests/MagicMode/MagicModeControllerTests.cs` |

---

## Configuration

### appsettings.json

```json
{
  "AI": {
    "Mistral": {
      "ApiKey": "your-mistral-key",
      "KeyPhraseModel": "mistral-small-latest"
    }
  }
}
```

### appsettings.Development.json

```json
{
  "AI": {
    "Mistral": {
      "ApiKey": "test-key",
      "KeyPhraseModel": "mistral-small-latest"
    }
  }
}
```

### User Secrets (Recommended)

```bash
dotnet user-secrets set "AI:Mistral:ApiKey" "your-key"
```

---

## What NOT to Use

### ❌ Anti-Patterns

| Anti-Pattern | Why | Use Instead |
|---|---|---|
| `wwwroot/js/magic-mode/` | Direct writes bypass Vite | `Assets/components/survey/magicMode/` |
| `wwwroot/css/magic-mode/` | Direct writes bypass Vite | `Assets/styles/pages/magic-mode.css` |
| React/Vue/Blazor/jQuery | Unnecessary complexity | Vanilla TypeScript |
| `IAiService` or `MagicModeAiService` | Duplicates existing abstraction | Extend `IAiManager` |
| `IMagicModeStateService` | Server-side state not needed | Client-side TypeScript |
| `VoxtrallSttService` | External STT not approved | Use existing `STTManager` |
| `<script>` blocks in Razor | Mixes concerns | External TypeScript modules |
| `style=""` attributes | Inline CSS hard to maintain | Tailwind classes |
| `@ts-ignore` | Hides real problems | Fix TypeScript errors |
| `any` type | No type safety | Proper TypeScript types |

### ❌ File/Folder Patterns to Avoid

```
UI-MVC/Services/MagicMode/          # Don't create separate service layer
wwwroot/js/magic-mode/              # Don't write directly to wwwroot
wwwroot/css/magic-mode/            # Don't write directly to wwwroot
```

---

## Build & Development

### Build Commands

```bash
# Build backend
cd backend/UI-MVC
dotnet build

# Build frontend
cd backend/UI-MVC
pnpm install
pnpm run build

# Run tests
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MagicModeControllerTests"
```

### Development Workflow

1. Start backend: `dotnet run --project UI-MVC/UI-MVC.csproj`
2. Start frontend: `pnpm run dev` (if using Vite dev server)
3. Open browser: `https://localhost:5001`

---

## See Also

- [README.md](./README.md) — Main overview
- [IMPLEMENTATION.md](./IMPLEMENTATION.md) — AI details and algorithms
- [PHASES.md](./PHASES.md) — Implementation phases
