# Magic Mode — Implementation Phases

This document consolidates all 7 implementation phases for Magic Mode. Each phase builds upon the previous one.

---

## Phase Overview

| Phase | File | Goal | Status |
|---|---|---|---|
| 1 | `phase1-setup.md` | Project structure, DTOs, BL interface | ✅ Complete |
| 2 | `phase2-ai.md` | STT integration via STTManager | ✅ Complete |
| 3 | `phase3-ai.md` | AI integration: key phrase extraction endpoint | ✅ Complete |
| 4 | `phase4-ai.md` | UI: modal, bubble container (Tailwind/DaisyUI) | ✅ Complete |
| 5 | `phase5-ai.md` | State management (client-side TypeScript) | ✅ Complete |
| 6 | `phase6-ai.md` | Integration with survey (magic btn wiring) | ✅ Complete |
| 7 | `phase7-ai.md` | Testing and final check | ✅ Complete |

---

## Phase 1: Project Structure, DTOs, and BL Interface

### Goal
Create all new files and extend existing interfaces so that later phases can build on a stable foundation.

### Checklist

| Task | Status | Notes |
|---|---|---|
| Extend IAiManager | ✅ Done | |
| Implement MistralAiManager | ✅ Done | |
| Implement NoopAiManager | ✅ Done | |
| Create DTOs | ✅ Done | |
| Create MagicModeController | ✅ Done | |
| Create frontend folders | ✅ Done | |
| Verify dotnet build | ✅ Done | Build succeeded with 4 warnings |

### Key Implementation

**BL/Ai/Managers/IAiManager.cs** — Add method to interface:
```csharp
Task<IReadOnlyList<string>> ExtractKeyPhrases(
    string transcript,
    string language,
    int maxPhrases,
    IReadOnlyList<string> existingPhrases = null,
    IReadOnlyList<string> rejectedPhrases = null);
```

**Domain/DTOs/MagicMode/ExtractKeyPhrasesRequest.cs**
```csharp
public record ExtractKeyPhrasesRequest(
    string Transcript,
    string Language,
    int MaxPhrases,
    IReadOnlyList<string>? ExistingPhrases = null,
    IReadOnlyList<string>? RejectedPhrases = null);
```

**Domain/DTOs/MagicMode/ExtractKeyPhrasesResponse.cs**
```csharp
public record ExtractKeyPhrasesResponse(IReadOnlyList<string> Phrases);
```

---

## Phase 2: STT Integration via STTManager

### Goal
Bind the existing `STTManager` from `speechService.ts` to Magic Mode. STT is **fully client-side** — no new server-side service is created. The `onText` callback from `STTManager` triggers the AI call (Phase 3).

### Key Concepts

`STTManager` (in `Assets/services/speechService.ts`) works as follows:
1. `start(textarea, language, onText, contextBias?)` — Start recording via browser MediaRecorder API
2. Every ~2 seconds, it sends audio data as base64 to `POST /api/speech/transcribe`
3. The server (`IMistralSpeechManager`) returns the transcription as text
4. The `onText(text: string)` callback is called with each interim transcription
5. `stop()` — Stops recording and removes the MediaRecorder

**Important:** `bindMicButton()` is a factory that binds a button to STT AND puts text directly into a textarea. Magic Mode does **NOT** use `bindMicButton()` but calls `STTManager.start()` directly, so the `onText` callback can invoke the AI instead of filling a textarea.

### Checklist

- [x] `STTManager` imported via `getSTTManager()` from `speechService.ts`
- [x] `detectLocale()` used for language detection
- [x] `stt.start(null, language, onTranscript)` called on microphone button click
- [x] `stt.stop()` called when stopping recording
- [x] `stt.stop()` called in `destroy()` of the modal
- [x] `app:before-navigate` handler present with `{ once: true }`
- [x] No `bindMicButton()` used
- [x] No Voxtrall, no HttpClient, no server-side STT service

### Code Examples

**Import STTManager:**
```typescript
import { getSTTManager } from '../../../services/speechService';
import { detectLocale } from '../../../i18n/survey';

const stt = getSTTManager();
```

**Start Recording:**
```typescript
function startRecording(onTranscript: (text: string) => void): void {
    const language = detectLocale();
    stt.start(
        null,           // No textarea — we process text ourselves via callback
        language,
        onTranscript,   // Callback per transcription → calls AI
        undefined
    );
}
```

**Stop Recording:**
```typescript
function stopRecording(): void {
    stt.stop();
}
```

**Microphone Button Visual State:**
```typescript
function setMicState(recording: boolean): void {
    micBtn.classList.toggle('btn-error', recording);
    micBtn.classList.toggle('animate-pulse', recording);
    micBtn.setAttribute('aria-pressed', String(recording));
}

micBtn.addEventListener('click', () => {
    isRecording = !isRecording;
    if (isRecording) {
        startRecording(onTranscript);
    } else {
        stopRecording();
    }
    setMicState(isRecording);
});
```

---

## Phase 3: AI Integration — Key Phrase Extraction

### Goal
Extend `IAiManager` with `ExtractKeyPhrases`, implement in `MistralAiManager` and `NoopAiManager`, and complete the `MagicModeController` endpoint. The AI always receives context about which phrases have already been shown and which the user has rejected, so it never suggests them again.

### Key Implementation

**IAiManager Extension:**
```csharp
Task<IReadOnlyList<string>> ExtractKeyPhrases(
    string transcript,
    string language,
    int maxPhrases,
    IReadOnlyList<string> existingPhrases = null,
    IReadOnlyList<string> rejectedPhrases = null);
```

**MistralAiManager Implementation:**
Uses a context-aware prompt. The AI explicitly receives which phrases already exist and which have been rejected, so no duplicate or already-rejected phrases are returned.

```csharp
public async Task<IReadOnlyList<string>> ExtractKeyPhrases(
    string transcript,
    string language,
    int maxPhrases,
    IReadOnlyList<string> existingPhrases = null,
    IReadOnlyList<string> rejectedPhrases = null)
{
    var existingLine = existingPhrases?.Count > 0
        ? $"Concepts already captured (do not repeat or paraphrase): {JsonSerializer.Serialize(existingPhrases)}"
        : "Concepts already captured: none";
    var rejectedLine = rejectedPhrases?.Count > 0
        ? $"Concepts dismissed by user (never suggest, even rephrased): {JsonSerializer.Serialize(rejectedPhrases)}"
        : "Concepts dismissed: none";

    var prompt = $"""
        You extract distinct opinion concepts from a survey speech transcript.

        {existingLine}
        {rejectedLine}

        Extract up to {maxPhrases} genuinely NEW concepts not already covered above.

        Rules:
        - Semantic coverage: if a candidate concept means the same thing as an already-captured concept — even with different wording — it is NOT new. Skip it.
        - Dismissed concepts: never suggest again, not even as synonyms or paraphrases.
        - Each phrase: 2–5 words, concise and specific.
        - Return ONLY a JSON array: ["phrase one","phrase two"]
        - Return [] if no new concept exists.

        Language: {language}
        Transcript: {transcript}
        """;

    var messages = new[] { new ChatMessage(ChatRole.User, prompt) };
    var options = new ChatOptions { ModelId = _keyPhraseModel };
    var response = await _chatClient.CompleteAsync(messages, options);
    var raw = response.Message.Text?.Trim() ?? "[]";

    // Strip markdown code fences if present
    if (raw.StartsWith("```"))
        raw = raw.Split('\n').Skip(1).SkipLast(1).Aggregate((a, b) => a + "\n" + b);

    return JsonSerializer.Deserialize<string[]>(raw) ?? Array.Empty<string>();
}
```

**NoopAiManager Implementation:**
```csharp
public Task<IReadOnlyList<string>> ExtractKeyPhrases(
    string transcript, string language, int maxPhrases,
    IReadOnlyList<string> existingPhrases = null,
    IReadOnlyList<string> rejectedPhrases = null)
    => Task.FromResult<IReadOnlyList<string>>(new[] { "noop phrase 1", "noop phrase 2", "noop phrase 3" });
```

**MagicModeController Endpoint:**
```csharp
[HttpPost("key-phrases")]
public async Task<IActionResult> ExtractKeyPhrases([FromBody] ExtractKeyPhrasesRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Transcript))
        return BadRequest("Transcript is required.");

    var phrases = await aiManager.ExtractKeyPhrases(
        request.Transcript,
        request.Language,
        request.MaxPhrases,
        request.ExistingPhrases,
        request.RejectedPhrases);

    return Ok(new ExtractKeyPhrasesResponse(phrases));
}
```

**Note:** The controller has **no** `[Authorize]` — survey participants are anonymous Youth tokens, not ASP.NET Identity users. Same pattern as `SpeechController`.

**appsettings.json Configuration:**
```json
{
  "AI": {
    "Mistral": {
      "KeyPhraseModel": "mistral-small-latest"
    }
  }
}
```

### Frontend: fetchKeyPhrases

```typescript
async function fetchKeyPhrases(
    transcript: string,
    existingPhrases: string[],
    rejectedPhrases: string[]
): Promise<string[]> {
    try {
        const response = await fetch('/api/magic-mode/key-phrases', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                transcript,
                language: detectLocale(),
                maxPhrases: 5,
                existingPhrases,
                rejectedPhrases
            })
        });
        if (!response.ok) return [];
        const data = await response.json() as { phrases: string[] };
        return data.phrases ?? [];
    } catch {
        return [];
    }
}
```

**Fallback:** If the response is empty or an error, the entire transcription is added as one bubble so the user is never left with nothing.

### Checklist

- [x] `IAiManager` has `ExtractKeyPhrases` method with 5 parameters
- [x] `MistralAiManager` implements method with context-aware prompt
- [x] `NoopAiManager` returns fixed noop phrases
- [x] `POST /api/magic-mode/key-phrases` endpoint works and returns `{ phrases: string[] }`
- [x] Controller passes `existingPhrases` and `rejectedPhrases` to AI manager
- [x] Frontend `fetchKeyPhrases()` sends `existingPhrases` and `rejectedPhrases` in request body
- [x] Fallback present when phrases are empty
- [x] `[Authorize]` on controller (not admin-only)
- [x] `dotnet build` succeeds without errors

---

## Phase 4: UI — Modal and Bubble Container

### Goal
Build the Magic Mode modal as a TypeScript module. No Blazor, no Razor Views, no inline CSS. Everything in `Assets/components/survey/magicMode/` with Tailwind CSS 4 + DaisyUI 5.

### File Structure

```
Assets/components/survey/magicMode/
├── magicModeModal.ts     ← Build modal, lifecycle, STT wiring
├── bubbleList.ts         ← Bubble state + DOM rendering
└── index.ts              ← Re-exports for use in openTextQuestion.ts

Assets/styles/pages/
└── magic-mode.css        ← @layer components for Magic Mode specific styles
```

### bubbleList.ts

Responsible for state AND DOM rendering of bubbles. There are two state structures:
- `activeBubbles: string[]` — bubbles currently visible
- `rejectedPhrases: Set<string>` — normalized strings of phrases the user has discarded (for deduplication and passing to AI)

Duplicates and already-rejected phrases are ignored in `addBubbles`. Comparison is always case-insensitive.

```typescript
export function createBubbleList(): BubbleListController {
    const activeBubbles: string[] = [];
    const rejectedPhrases = new Set<string>();
    const container = document.createElement('div');
    container.className = 'flex flex-wrap gap-2 p-4 min-h-24 overflow-y-auto';

    const normalize = (s: string): string => s.trim().toLowerCase();

    function addBubbles(phrases: string[]): void {
        for (const p of phrases) {
            const trimmed = p.trim();
            if (!trimmed) continue;
            const key = normalize(trimmed);
            if (rejectedPhrases.has(key)) continue;
            if (activeBubbles.some(b => normalize(b) === key)) continue;
            activeBubbles.push(trimmed);
        }
        render();
    }

    function removeBubble(index: number): void {
        const removed = activeBubbles.splice(index, 1)[0];
        if (removed) rejectedPhrases.add(normalize(removed));
        render();
    }

    function reset(): void {
        activeBubbles.length = 0;
        rejectedPhrases.clear();
        render();
    }

    // ... render method
}
```

**Important:**
- `addBubbles` silently skips a phrase if the normalized value is already in `rejectedPhrases` or `activeBubbles`
- `removeBubble` adds the phrase to `rejectedPhrases` so the AI skips it on the next call
- `reset()` clears both structures (when opening a new modal session)

### magicModeModal.ts

Builds the complete modal DOM, manages the lifecycle, and returns the modal as a controller object.

```typescript
export function createMagicModeModal(): MagicModeModalController {
    const stt = getSTTManager();
    let isRecording = false;
    let onCloseCallback: ((text: string) => void) | null = null;
    const bubbleList = createBubbleList();

    // Build modal DOM...
    // Wire STT and onTranscript...
    // Return controller
}
```

### magic-mode.css

```css
/* Assets/styles/pages/magic-mode.css */
@layer components {
    .magic-mode-badge-enter {
        animation: magic-mode-fade-in 0.2s ease-out;
    }
}

@keyframes magic-mode-fade-in {
    from { opacity: 0; transform: scale(0.8); }
    to   { opacity: 1; transform: scale(1); }
}
```

### Checklist

- [x] `bubbleList.ts` created with `activeBubbles` array + `rejectedPhrases` Set
- [x] `addBubbles()` deduplicates case-insensitively and skips rejected phrases
- [x] `removeBubble()` adds removed phrase to `rejectedPhrases`
- [x] `getRejectedPhrases()` returns a copy of the rejected set as array
- [x] `reset()` clears both `activeBubbles` and `rejectedPhrases`
- [x] `magicModeModal.ts` sends `getBubbles()` and `getRejectedPhrases()` with `fetchKeyPhrases()`
- [x] Tailwind + DaisyUI classes used (no inline CSS, no `style=""`)
- [x] Heroicons used as SVG inline
- [x] `magic-mode.css` created and imported
- [x] `index.ts` re-exports the controller
- [x] No Blazor, no Razor Views, no inline JavaScript in HTML
- [x] `pnpm run build` succeeds without TypeScript errors

---

## Phase 5: State Management (Client-Side TypeScript)

### Goal
Bubbles are managed **exclusively client-side** in TypeScript. There is **NO server-side state** for bubbles — no `MagicModeStateService`, no in-memory lists on the server, no session storage.

The state is already integrated in `bubbleList.ts` (Phase 4). This phase describes the rules and patterns around state management.

### Why Client-Side State?

- Bubbles are session-specific per modal instance — they don't need to be persisted
- Server-side state would cause race conditions with multiple concurrent users
- The survey textarea is the only persistent output point — that's enough

### State Structure

In `bubbleList.ts` (created in Phase 4) there are **two state structures**:

```typescript
const activeBubbles: string[] = [];
const rejectedPhrases = new Set<string>(); // normalized lowercase values
```

**`activeBubbles`** — the phrases currently visible as bubbles.

**`rejectedPhrases`** — all phrases the user has discarded (via the × button). Used for:
1. Client-side deduplication: `addBubbles` skips a phrase if it's in this set
2. Context for AI: sent in the request body so the model never generates them again

Comparison is always case-insensitive via `normalize = (s) => s.trim().toLowerCase()`.

### Operations

| Operation | Description |
|---|---|
| `addBubbles(phrases: string[])` | Add, skip duplicates and rejected, render |
| `removeBubble(index: number)` | Remove bubble at index, add to `rejectedPhrases`, render |
| `getBubbles(): string[]` | Return copy of `activeBubbles` |
| `getRejectedPhrases(): string[]` | Return copy of `rejectedPhrases` as array |
| `reset()` | Clear `activeBubbles` and `rejectedPhrases` (on new session) |

### Final Text Generation

When closing the modal, the remaining bubbles are joined:

```typescript
const finalText = bubbleList.getBubbles().join(', ');
```

This `finalText` is passed to the `onClose` callback of `MagicModeModalController.open()`, which then places it in the survey textarea (see Phase 6).

### State Lifecycle

```
modal.open()          → bubbleList.reset()                    (empty state, rejected cleared)
onText callback       → fetchKeyPhrases(text, existing, rejected)
                      → bubbleList.addBubbles(phrases)        (AI result add, dedup)
user clicks ×         → bubbleList.removeBubble(i)            (bubble removed + in rejected set)
next STT round        → fetchKeyPhrases sends rejected        (AI skips them)
modal.close()         → getBubbles().join(', ')               (final text)
modal.destroy()       → state disappears with module instance
```

### What NOT to Do

- No `fetch('/api/magic-mode/bubbles', { method: 'POST' })` to save bubbles
- No `sessionStorage` or `localStorage` for bubbles or rejected phrases
- No Redux, MobX, or other state management libraries
- No synchronization between modal instances (only one is active at a time)
- No server-side rejected phrases — the set lives only in the modal instance

### Checklist

- [x] `activeBubbles: string[]` and `rejectedPhrases: Set<string>` are the two state structures in `bubbleList.ts`
- [x] `removeBubble()` adds the phrase to `rejectedPhrases`
- [x] `getRejectedPhrases()` returns an array copy
- [x] `reset()` clears both structures
- [x] `reset()` is called on `modal.open()`
- [x] `getBubbles().join(', ')` is used for final text
- [x] No server-side bubble state
- [x] No `sessionStorage` or `localStorage` for bubbles

---

## Phase 6: Survey Integration — Magic Button Wiring

### Goal
Wire the existing `survey-magic-btn` in `openTextQuestion.ts` to the `MagicModeModal`. The button already exists — it's marked as "coming soon". In this phase, remove that marking and connect it to the modal.

**No new button to create. No inline JavaScript in Razor. No inline `style=""`.**

### Step 1: Locate Existing Button

**`Assets/components/survey/openTextQuestion.ts`** — around line 54:

```typescript
const magicBtn = wrapper.querySelector<HTMLElement>('.survey-magic-btn');
```

The button is already in the DOM. The `title` attribute currently says "Answer in Magic Mode (coming soon)".

### Step 2: Import MagicModeModal

Import at the top of `openTextQuestion.ts`:

```typescript
import { createMagicModeModal } from './magicMode';
```

### Step 3: Instantiate Modal and Wire Button

In the `init` function of the open-text-question component, after the existing STT setup:

```typescript
const modal = createMagicModeModal();

if (magicBtn) {
    magicBtn.title = 'Answer in Magic Mode';
    magicBtn.removeAttribute('disabled');

    magicBtn.addEventListener('click', () => {
        const questionText = wrapper.querySelector<HTMLElement>('.survey-question-text')?.textContent ?? '';
        modal.open(questionText, (finalText) => {
            if (finalText.trim()) {
                textarea.value = finalText;
                textarea.dispatchEvent(new Event('input', { bubbles: true }));
            }
        });
    });
}
```

**Note:** The `textarea.dispatchEvent(new Event('input'))` is needed so that any validation listeners pick up the new value.

### Step 4: Extend SPA Cleanup

The existing `destroy` function in `openTextQuestion.ts` must also tear down the modal:

```typescript
destroy: () => {
    unbindMic();         // existing
    modal.destroy();     // new
}
```

And the `app:before-navigate` handler (if it doesn't exist in this component):

```typescript
window.addEventListener('app:before-navigate', () => {
    modal.destroy();
}, { once: true });
```

### Step 5: Remove "Coming Soon" from Razor View

Find the magic btn markup in the survey Razor View(s). Remove the `disabled` attribute or the "coming soon" tooltip if it's set server-side. The button should be visible and clickable.

**Do NOT add `<script>` blocks to the Razor View.** All logic is in TypeScript.

### Checklist

- [x] `survey-magic-btn` is connected to `modal.open()`
- [x] `onClose` callback places final text in textarea via `textarea.value`
- [x] `textarea.dispatchEvent(new Event('input'))` is called after setting value
- [x] `modal.destroy()` is in the component's `destroy()`
- [x] `app:before-navigate` handler present with `{ once: true }`
- [x] No `<script>` blocks in Razor Views
- [x] No `style=""` attributes
- [x] "Coming soon" marking removed from button

---

## Phase 7: Testing and Final Check

### Step 1: Backend — xUnit Test for MagicModeController

The project uses xUnit with a `ManagerIntegrationTestFixture` (see `Tests/`). Follow the same pattern.

**`Tests/MagicMode/MagicModeControllerTests.cs`**

```csharp
using BL.Ai.Managers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UI_MVC.Controllers.Api;
using UI_MVC.DTOs.MagicMode;
using Xunit;

namespace Tests.MagicMode;

public class MagicModeControllerTests
{
    private readonly Mock<IAiManager> _aiManagerMock = new();
    private readonly MagicModeController _controller;

    public MagicModeControllerTests()
    {
        _controller = new MagicModeController(_aiManagerMock.Object);
    }

    [Fact]
    public async Task ExtractKeyPhrases_ValidTranscript_ReturnsOk()
    {
        _aiManagerMock
            .Setup(m => m.ExtractKeyPhrases("test transcript", "nl", 5, null, null))
            .ReturnsAsync(new[] { "phrase 1", "phrase 2" });

        var result = await _controller.ExtractKeyPhrases(
            new ExtractKeyPhrasesRequest("test transcript", "nl", 5));

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ExtractKeyPhrasesResponse>(ok.Value);
        Assert.Equal(2, response.Phrases.Count);
    }

    [Fact]
    public async Task ExtractKeyPhrases_EmptyTranscript_ReturnsBadRequest()
    {
        var result = await _controller.ExtractKeyPhrases(
            new ExtractKeyPhrasesRequest("", "nl", 5));

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
```

**Run tests:**
```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MagicModeControllerTests"
```

### Step 2: TypeScript Build Check

```bash
cd UI-MVC && pnpm run build
```

There must be **NO** TypeScript errors. Specifically check:
- No `any` types
- No unused variables or parameters
- No `@ts-ignore` comments

### Step 3: Manual Browser Test

Start the application:
```bash
dotnet run --project UI-MVC/UI-MVC.csproj
```

Perform the following steps in the browser:

#### 3a. Open Magic Mode
1. Navigate to a survey with an open text question
2. Click the Magic Mode button (microphone/wand icon)
3. **Expected:** Modal opens with question text and empty bubble container

#### 3b. Start Recording
4. Click the microphone button in the modal
5. Allow microphone access if browser prompts
6. Speak a sentence (e.g., "Ik vind het platform heel gebruiksvriendelijk en snel")
7. **Expected:** After ~2 seconds, bubbles appear (e.g., "gebruiksvriendelijk", "snel")

#### 3c. Manage Bubbles
8. Click the × on a bubble
9. **Expected:** Bubble disappears

#### 3d. Close and Transfer Text
10. Click the close button
11. **Expected:** Modal closes, remaining bubbles are comma-separated in the survey textarea

#### 3e. Validate with NoopAiManager
If `AI:Provider` = `Noop` in `appsettings.json`:
- Bubbles are "noop phrase 1", "noop phrase 2", "noop phrase 3"
- This always works, regardless of Mistral API key

### Step 4: SPA Cleanup Test

1. Open Magic Mode and start recording
2. Navigate to another page **without** closing the modal
3. **Expected:**
   - Modal is no longer visible
   - No error messages in browser console about running MediaRecorder
   - No microphone indicator in browser (recording stopped)

### Step 5: Final Check Checklist

See [CHECKLIST.md](./CHECKLIST.md) for the complete final checklist.

### Common Problems

| Problem | Solution |
|---|---|
| TypeScript error: `null` argument for `start()` | Check `STTManager.start()` signature in `speechService.ts` — `textarea` argument is `HTMLTextAreaElement \| HTMLInputElement \| null` |
| Bubbles don't appear | Check browser console for API errors; test with `NoopAiManager` |
| Recording doesn't start | Browser needs microphone access; check HTTPS or localhost |
| `pnpm run build` fails | Read error message; TypeScript errors contain file path and line number |
| xUnit test fails | Check if `IAiManager` interface has `ExtractKeyPhrases` method |

---

## Checklist Summary

All phases include detailed checklists. See [CHECKLIST.md](./CHECKLIST.md) for the consolidated validation checklist.

---

## See Also

- [README.md](./README.md) — Main overview
- [ARCHITECTURE.md](./ARCHITECTURE.md) — Technical stack and patterns
- [IMPLEMENTATION.md](./IMPLEMENTATION.md) — AI details and algorithms
- [CHECKLIST.md](./CHECKLIST.md) — Validation checklist
