# Magic Mode — Validation Checklist

Use this document as the definitive check for the complete Magic Mode implementation.

---

## Execution Status Summary

| Layer | Status |
|---|---|
| BL — IAiManager extended | ✅ Done |
| BL — MistralAiManager implemented | ✅ Done |
| BL — NoopAiManager implemented | ✅ Done |
| DTOs created | ✅ Done |
| MagicModeController created | ✅ Done |
| bubbleList.ts created | ✅ Done |
| magicModeModal.ts created | ✅ Done |
| magic-mode.css created | ✅ Done |
| survey-magic-btn wired | ✅ Done |
| SPA cleanup present | ✅ Done |
| xUnit test passed | ✅ Done |
| TypeScript build passed | ✅ Done |
| Browser manual test passed | ✅ Done |

---

## Backend

### BL Layer

- [ ] `IAiManager` contains `Task<IReadOnlyList<string>> ExtractKeyPhrases(string transcript, string language, int maxPhrases, IReadOnlyList<string> existingPhrases = null, IReadOnlyList<string> rejectedPhrases = null)`
- [ ] `MistralAiManager.ExtractKeyPhrases` calls Mistral and returns parsed `string[]`
- [ ] `NoopAiManager.ExtractKeyPhrases` returns `["noop phrase 1", "noop phrase 2", "noop phrase 3"]`
- [ ] Both implementations compile without warnings

### DTOs

- [ ] `Domain/DTOs/MagicMode/ExtractKeyPhrasesRequest.cs` exists as record with `Transcript`, `Language`, `MaxPhrases`, `ExistingPhrases`, `RejectedPhrases`
- [ ] `Domain/DTOs/MagicMode/ExtractKeyPhrasesResponse.cs` exists as record with `Phrases`

### Controller

- [ ] `UI-MVC/Controllers/Api/MagicModeController.cs` exists
- [ ] Route: `POST api/magic-mode/key-phrases`
- [ ] **No** `[Authorize]` attribute (survey participants are anonymous Youth tokens)
- [ ] Empty `Transcript` returns `400 BadRequest`
- [ ] Valid request returns `200 OK` with `{ phrases: string[] }`
- [ ] Controller injects `IAiManager` via constructor

### Build

- [ ] `dotnet build Conversey.sln` succeeds without errors or warnings

---

## Frontend

### TypeScript Modules

- [ ] `Assets/components/survey/magicMode/bubbleList.ts` exists
- [ ] `Assets/components/survey/magicMode/magicModeModal.ts` exists
- [ ] `Assets/components/survey/magicMode/index.ts` exists and re-exports
- [ ] No `any` types
- [ ] No `@ts-ignore` comments
- [ ] `pnpm run build` succeeds without TypeScript errors

### Styling

- [ ] `Assets/styles/pages/magic-mode.css` exists
- [ ] No `style=""` attributes in TypeScript DOM code
- [ ] No inline CSS
- [ ] Tailwind + DaisyUI classes used

### STT Integration

- [ ] `STTManager` imported via `getSTTManager()` (not `new STTManager()`)
- [ ] `detectLocale()` used for language detection (not `navigator.language`)
- [ ] `stt.start(null, language, onTranscript)` called when recording starts
- [ ] `stt.stop()` called when stopping recording
- [ ] `stt.stop()` called in `modal.destroy()`

### Survey Integration

- [ ] `survey-magic-btn` wired to `modal.open()`
- [ ] Question text passed to `modal.open()`
- [ ] `onClose` callback sets final text in `textarea.value`
- [ ] `textarea.dispatchEvent(new Event('input'))` called after textarea update
- [ ] No `<script>` blocks in Razor Views
- [ ] "Coming soon" title removed from button

### SPA Cleanup

- [ ] `app:before-navigate` event handler present in `openTextQuestion.ts`
- [ ] Handler has `{ once: true }`
- [ ] Handler calls `modal.destroy()`
- [ ] `modal.destroy()` calls `stt.stop()`

---

## Tests

- [ ] `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MagicModeControllerTests"` passes
- [ ] Manual browser test completed (see Phase 7, Step 3)
- [ ] SPA cleanup manually tested (see Phase 7, Step 4)

---

## Security

- [ ] **No** `[Authorize]` on `MagicModeController` — accessible without login (Youth tokens)
- [ ] No hardcoded API keys in code
- [ ] Mistral API key in user secrets or environment variable

---

## What MUST NOT Be in Codebase

If any of these are present, something went wrong:

- [ ] `UI-MVC/Services/MagicMode/` folder
- [ ] `wwwroot/js/magic-mode/` files
- [ ] `wwwroot/css/magic-mode/` files
- [ ] `IVoxtrallSttService` or `VoxtrallSttService`
- [ ] `IAiService` or `MagicModeAiService`
- [ ] `IMagicModeStateService` or `MagicModeStateService`
- [ ] `<script>` blocks in survey Razor Views
- [ ] `style=""` attributes in DOM-building code
- [ ] React, Vue, or Blazor imports
- [ ] `@ts-ignore` in TypeScript files

---

## Manual Browser Test Script

### Setup
1. Start application: `dotnet run --project UI-MVC/UI-MVC.csproj`
2. Open browser to `https://localhost:5001`

### Test Steps

#### Test 1: Open Magic Mode
- [ ] Navigate to survey with open text question
- [ ] Click Magic Mode button (microphone/wand icon)
- [ ] **Expected:** Modal opens with question text and empty bubble container

#### Test 2: Start Recording
- [ ] Click microphone button in modal
- [ ] Allow microphone access if browser prompts
- [ ] Speak a sentence (e.g., "The platform is very user-friendly and fast")
- [ ] **Expected:** After ~2 seconds, bubbles appear (e.g., "user-friendly", "fast")

#### Test 3: Manage Bubbles
- [ ] Click × on a bubble
- [ ] **Expected:** Bubble disappears

#### Test 4: Close and Transfer Text
- [ ] Click close button
- [ ] **Expected:** Modal closes, remaining bubbles are comma-separated in survey textarea

#### Test 5: Validation with NoopAiManager
- [ ] Set `AI:Provider` = `Noop` in `appsettings.json`
- [ ] Bubbles are "noop phrase 1", "noop phrase 2", "noop phrase 3"
- [ ] **Expected:** Works without Mistral API key

#### Test 6: SPA Cleanup
- [ ] Open Magic Mode and start recording
- [ ] Navigate to another page **without** closing modal
- [ ] **Expected:** Modal no longer visible
- [ ] **Expected:** No error messages in browser console about MediaRecorder
- [ ] **Expected:** No microphone indicator in browser (recording stopped)

---

## Common Problems & Solutions

| Problem | Solution |
|---|---|
| TypeScript error: `null` argument for `start()` | Check `STTManager.start()` signature in `speechService.ts` — `textarea` argument is `HTMLTextAreaElement \| HTMLInputElement \| null` |
| Bubbles don't appear | Check browser console for API errors; test with `NoopAiManager` |
| Recording doesn't start | Browser needs microphone access; check HTTPS or localhost |
| `pnpm run build` fails | Read error message; TypeScript errors contain file path and line number |
| xUnit test fails | Check if `IAiManager` interface has `ExtractKeyPhrases` method |

---

## Changelog

| Date | Change | By |
|---|---|---|
| 2026-04-26 | Initial checklist | Matéo Rohr |
| 2026-05-03 | Rewritten: RTF → Markdown, architecture check added | Claude |

---

## See Also

- [README.md](./README.md) — Main overview
- [PHASES.md](./PHASES.md) — Step-by-step implementation guide
- [IMPLEMENTATION.md](./IMPLEMENTATION.md) — AI details and algorithms
