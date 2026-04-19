# Migration Summary: Domain and Namespace Refactor

This document tracks the current state after the refactor and follow-up fixes.

## Current State (April 2026)

### Completed
- Namespaces are migrated to the new split:
  - `Conversey.BL.Domain.Administration`
  - `Conversey.BL.Domain.Ideation`
  - `Conversey.BL.Domain.Survey`
  - `Conversey.BL.Domain.Common`
  - `Conversey.DAL.Administration`
  - `Conversey.DAL.Ideation`
  - `Conversey.DAL.Survey`
- `Workspace` key uses `Slug`.
- `Project` key uses `Slug` and `Title` was replaced by `Name`.
- `Youth.Token` migrated from `string` to `Guid`.
- Survey submit flow now persists answer authorship:
  - `Answer` has required `Youth` navigation.
  - `ConverseyDbContext` maps `Answer -> Youth` as required.
  - `QuestionController` resolves/creates youth by token and assigns `Youth` to each answer.
- Anonymous participation is supported without email:
  - `Youth.Email` is nullable.
  - `AddYouth` accepts nullable email.
  - Controllers create youth by token with `Email = null` when needed.
- Seed data (`DataSeeder`) is active and aligned with the current domain model.
- Integration tests compile and pass with the refactored model.

### Important Behavior Notes
- The app uses `EnsureCreated`/`EnsureDeleted` in startup, not migrations.
- Because of generic `Answer<T>` mapping in one table, the `Answers` schema contains type-specific columns (`ValueId`, `AnswerValue`, etc.). This is expected for the current model.

## Remaining Risks / Follow-up

### 1) AI Moderation Runtime Wiring
- `Program.cs` now resolves `IAiManager` by `AI:Provider` (`Mistral` or `Noop`).
- `MistralAiManager` is active when `AI:Provider` is set to `Mistral` and key/config are present.
- Keep secrets out of committed config for shared environments.

### 2) API DTO Consistency
- Some payload properties are still legacy-oriented (for example numeric `ProjectId` fields that are not route-authoritative).
- Route slugs are the source of truth; keep DTOs aligned to avoid confusion.

### 3) Data Access Cleanup
- `QuestionRepository.ReadAnswersByQuestionId` currently filters on `Answer.Id` instead of question relation, so it does not return answers by question correctly.
- Additional repository-level cleanup is still recommended for long-term maintainability.

## Recommended Next Steps
1. Add EF Core migrations and stop relying on `EnsureCreated` for non-dev environments.
2. Enable real AI moderation behind configuration and environment secrets.
3. Normalize DTO contracts so request/response fields match route-driven model semantics.
4. Add targeted integration tests for:
   - idea submit moderation paths,
   - answer/youth linkage,
   - repository query correctness.

