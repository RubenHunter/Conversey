# Stepper Generalization Plan

## Goal

Generalize `createProjectStepper.ts` to handle any project creation step. Current code is step-1-specific. Per-step local storage, multi-step exit modal, prepare for step 3 implementation.

## Storage Schema

**Current:** `{prefix}:{slug}` — one flat snapshot with all step-1 fields + `currentStep`
**New:**
- `{prefix}:step:{N}` → `{ fields: Record<string,string>, draftSynced: boolean }` (per-step)
- `{prefix}:meta` → `{ currentStep: number, slug: string }` (global state)
- On init: detect old format → migrate to `{prefix}:step:1` + `{prefix}:meta`

## Phase 1: New `StepDraftManager` class

New module `Assets/components/shared/stepDraftManager.ts`:

- Generic per-step draft handler
- Discovers form fields by iterating `form.elements`
- Records field name/value pairs (excluding CSRF token, submit buttons, file inputs)
- localStorage key: `{prefix}:step:{N}`
- Methods: `persist()`, `hydrate()`, `clear()`, `hasUnsynced()`, `markSynced()`, `validate()`, `getFieldMap()`
- Optional hooks: `beforeSave?`, `slugSource?`

## Phase 2: Refactor `ProjectDraftManager`

Replace 530 lines of step-1-hardcoded code with step-agnostic coordinator:

| Concern | Old (step 1 only) | New (multi-step) |
|---|---|---|
| Form refs | 8+ `step1*` fields | `stepManagers: Map<number, StepDraftManager>` |
| Form discovery | `querySelector('#create-project-step1-form')` | `querySelectorAll('[id^="create-project-step"]')` |
| `persistDraft()` | Reads 10 named fields manually | `stepManagers.get(currentStep)?.persist()` |
| `hydrateDraft()` | Writes 10 named fields manually | `stepManagers.get(currentStep)?.hydrate()` |
| `canAdvance(step)` | Hardcoded step=1 check | Delegate to step manager validate + step-1 image hook |
| `onComplete()` | `submitStep1Form()` | Collect all step data → submit combined |
| `saveDraftToServer()` | POSTs step-1 FormData only | Iterate all step managers → collect all fields → POST combined FormData |
| `shouldBlockExit()` | Checks step-1 snapshot | `[...stepManagers.values()].some(m => m.hasUnsynced())` |
| `clearLocalDraft()` | Clears specific keys | Clear all `{prefix}:step:*` + `{prefix}:meta` |
| `seedCopyDraft()` | Seeds full snapshot | Seeds only `{prefix}:step:1` |
| Image upload | Embedded in canAdvance | Step-1-only `beforeSave` hook on step-1's StepDraftManager |

## Phase 3: View prep

No changes to `_Stepper.cshtml`. Each step's partial renders its own `<form id="create-project-step{N}-form">`. Step 1 already does this.

## Phase 4: Server-side prep

No immediate changes. When step 3 is implemented:
- `SaveDraft` endpoint reads new ViewModel properties
- `CreateProject` endpoint reads new ViewModel properties

## Phase 5: Implement Step 3 (Ideation)

After TS generalized:
1. Create `CreateStep3IdeationViewModel.cs`
2. Create `_ProjectStepIdeationForm.cshtml` with `id="create-project-step3-form"`
3. Add `CreateStep3ViewModel` property to `ProjectViewModel.cs`
4. Update `WorkspaceAdminController.CreateFormVm()` — replace step 3 placeholder with real partial
5. Update `SaveDraft` and `CreateProject` controller actions to read step 3 data
6. TS draft manager auto-discovers the new form — zero TS changes needed

## Files touched

| File | Action |
|---|---|
| `Assets/components/shared/stepDraftManager.ts` | **New** — generic per-step draft handler |
| `Assets/components/shared/createProjectStepper.ts` | **Major refactor** — remove step-1 specificity, become coordinator |
| `Models/WorkspaceAdmin/ProjectViewModel.cs` | **Add** `CreateStep3ViewModel` property (Phase 5) |
| `Controllers/Admin/WorkspaceAdminController.cs` | **Add** step 3 handling in save/create (Phase 5) |
| New: `_ProjectStepIdeationForm.cshtml` | **New** (Phase 5) |
| New: `CreateStep3IdeationViewModel.cs` | **New** (Phase 5) |
