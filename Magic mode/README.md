# Magic Mode — Main Documentation

**Magic Mode** is a voice-driven input mode for open questions in surveys. The user speaks their answer, AI extracts key phrases as clickable "bubbles", and when closed, the curated text is placed in the textarea.

---

## Quick Start

### Overview Flow
```
[Magic Mode button] → [Modal opens]
    ↓
[STTManager.start()] → Periodic transcription (every ~2s)
    ↓
[POST /api/magic-mode/key-phrases] → IAiManager → Key phrases
    ↓
[Bubbles appear in UI] ← User removes unwanted bubbles
    ↓
[Close] → Bubble texts join → Placed in survey textarea
```

### Prerequisites
- Browser with microphone access
- Mistral API key configured (or use `NoopAiManager` for testing)
- Node.js + pnpm for TypeScript build

---

## Documentation Structure

| File | Purpose |
|---|---|
| `README.md` | **You are here** — Overview, quick start, navigation |
| `ARCHITECTURE.md` | Technical stack, file locations, design patterns |
| `IMPLEMENTATION.md` | AI post-processing, phrase filtering, algorithms |
| `PHASES.md` | Detailed implementation phases 1-7 |
| `STT-OPTIMIZATION.md` | STT dual buffer system, caching, performance |
| `SIGNALR-PLAN.md` | Real-time communication planning |
| `CHECKLIST.md` | Validation and testing checklist |
| `CHANGELOG.md` | Complete change history |

---

## UI Components

### Modal Structure
1. **Question text** — Static display at the top (from open text question)
2. **Bubble container** — Central, scrollable; new bubbles appear with fade-in animation
3. **Microphone button** — Central at bottom; pulses (DaisyUI `animate-pulse`) during recording
4. **Close button** — Top right; stops STT and places curated text in textarea

### Bubble Behavior
- Click × to remove a bubble
- Removed phrases are sent to AI as "rejected" to prevent re-suggestion
- Final text: remaining bubbles joined with `, ` (comma + space)

---

## Technical Stack

| Component | Technology | Location |
|---|---|---|
| Backend Logic | C# (BL layer) | `BL/MagicMode/` |
| REST Endpoint | ASP.NET Core API | `UI-MVC/Controllers/Api/MagicModeController.cs` |
| DTOs | C# records | `Domain/DTOs/MagicMode/` |
| Frontend Entry | TypeScript (Vite) | `Assets/components/survey/magicMode/` |
| Styling | Tailwind CSS 4 + DaisyUI 5 | `Assets/styles/pages/magic-mode.css` |
| STT | Reuse `STTManager` | `Assets/services/speechService.ts` |
| AI | Reuse `IAiManager` | `BL/Ai/Managers/` |
| Icons | Heroicons | SVG inline in TypeScript |

**Not Used:**
- No React/Vue/Blazor/jQuery
- No `wwwroot/js/` or `wwwroot/css/` direct writes
- No server-side state for bubbles (client-side only)
- No separate `IAiService` — existing `IAiManager` is extended
- No Voxtrall or external STT APIs

---

## Workflow Details

### STT Flow
```
User clicks microphone button
    ↓
STTManager.start(null, detectLocale(), onText)
    ↓
[every ~2s] onText callback receives transcript
    ↓
POST /api/magic-mode/key-phrases
    ↓
New bubbles added to state + DOM
```

### AI Flow
```
transcript (string)
    ↓
POST /api/magic-mode/key-phrases { transcript, language, maxPhrases, existingPhrases, rejectedPhrases }
    ↓
IAiManager.ExtractKeyPhrases(transcript, language, maxPhrases, existingPhrases, rejectedPhrases)
    ↓
string[] phrases → Render bubbles
```

---

## Common Problems & Solutions

| Problem | Solution |
|---|---|
| STT doesn't start | Check browser microphone permissions; `STTManager` logs errors in console |
| Bubbles overlap | Use flex-wrap or flex-container in Tailwind (`flex flex-wrap gap-2`) |
| AI returns empty list | Fallback in `onText`: if `phrases.length === 0`, add entire transcript as one bubble |
| Animations don't work | Check Tailwind `animate-` classes; ensure `magic-mode.css` is imported |
| Microphone button unresponsive | Check `STTManager.start()` is called correctly; see logs in DevTools |
| TypeScript build fails | No `any` types, no `@ts-ignore`, check browser console for path/line |

---

## Security

- REST endpoint `POST /api/magic-mode/key-phrases` has **no** `[Authorize]` — survey participants are anonymous Youth tokens
- Same pattern as `SpeechController` and `QuestionController` — no auth on survey-facing endpoints
- No hardcoded API keys in code
- Mistral API key in user secrets or environment variables

---

## Files at a Glance

```
Magic mode/
├── README.md                    # This file
├── ARCHITECTURE.md              # Technical details
├── IMPLEMENTATION.md            # AI logic, phrase processing
├── PHASES.md                    # All 7 implementation phases
├── STT-OPTIMIZATION.md          # STT performance improvements
├── SIGNALR-PLAN.md              # Real-time communication
├── CHECKLIST.md                 # Validation & testing
└── CHANGELOG.md                 # Version history

Backend/
├── BL/MagicMode/                # Business logic
├── UI-MVC/Controllers/Api/MagicModeController.cs
├── UI-MVC/DTOs/MagicMode/       # Data transfer objects
└── Domain/DTOs/MagicMode/       # Domain DTOs

Frontend/
├── Assets/components/survey/magicMode/
│   ├── magicModeModal.ts         # Modal lifecycle, STT wiring
│   ├── bubbleList.ts             # Bubble state + DOM
│   └── index.ts                  # Re-exports
└── Assets/styles/pages/magic-mode.css
```

---

## See Also

- [ARCHITECTURE.md](./ARCHITECTURE.md) — Technical stack and patterns
- [PHASES.md](./PHASES.md) — Step-by-step implementation guide
- [IMPLEMENTATION.md](./IMPLEMENTATION.md) — AI details and algorithms
- [CHECKLIST.md](./CHECKLIST.md) — Validation checklist
