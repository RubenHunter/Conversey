# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

**Run (development)**
```bash
dotnet run --project UI-MVC/UI-MVC.csproj
```
On startup in Development mode, the database is dropped and recreated (`Database:ResetOnStart: true`). Vite dev server runs alongside on port 4173.

**Build**
```bash
dotnet build Conversey.sln
cd UI-MVC && pnpm install && pnpm run build   # frontend assets → wwwroot/
```

**Test**
```bash
dotnet test Tests/Tests.csproj
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~IdeaManagerIntegrationTests"
```

**Frontend**
```bash
cd UI-MVC && pnpm install
pnpm run build    # production bundle
```

## Architecture

Conversey is a collaborative ideation and survey platform built with ASP.NET Core MVC (.NET 10) and a TypeScript/Vite/Tailwind frontend for user, razorpages for administrator frontend.

**Project layout:**
- `Domain/` — entity models, no logic
- `DAL/` — EF Core repositories + `ConverseyDbContext`, PostgreSQL via Npgsql
- `BL/` — manager interfaces and implementations (business logic)
- `UI-MVC/` — controllers, views, DTOs, Vite-bundled frontend assets
- `Tests/` — xUnit integration tests using a shared `ManagerIntegrationTestFixture`

**Domain subdomains:**
- `Administration` — Workspace, Project, Topic, WorkspaceAdmin
- `Ideation` — Idea, IdeaResponse, IdeaReaction, ResponseReaction
- `Survey` — generic `Question<T>` / `Answer<T>` hierarchy (Scale, SingleChoice, MultipleChoice, Open)
- `Ai` — AiAuditLog, AiModel, moderation and ranking

**Key patterns:**
- **Slug-based routing**: Workspace and Project are identified by `Slug` (URL-safe string), not numeric ID. Routes follow `/api/{WorkspaceSlug}/projects/{ProjectSlug}/...`.
- **Generic survey types**: `Question<T>` and `Answer<T>` provide type-safe survey handling; concrete types are `OpenQuestion`, `ScaleQuestion`, `SingleChoiceQuestion`, `MultipleChoiceQuestion` and their answer counterparts.
- **Youth identity**: Participants are represented as `Youth` entities identified by a `Guid` token. Email is optional (anonymous participation supported). `Answer` requires a `Youth` FK.
- **Manager pattern**: All business logic is behind manager interfaces (`IWorkspaceManager`, `IIdeaManager`, `IQuestionManager`, etc.) injected via DI.
- **AI integration**: `IAiManager` has a `MistralAiManager` (live) and a `NoopAiManager` (testing). Configured via `AI:Provider` in appsettings.

**Database:**
- EF Core Code-First using `EnsureCreated` (not Migrations). `ConverseyDbContext.CreateDatabase(drop)` is called at startup.
- `DataSeeder.Seed()` populates test data after DB creation.
- Migration to proper EF Core Migrations is tracked in `MIGRATION_GUIDE.md`.

**Frontend:**
- TypeScript entry points live in `UI-MVC/Assets/`. Vite bundles them to `UI-MVC/wwwroot/`.
- jQuery + jquery-validation-unobtrusive for server-side form validation.
- Tailwind CSS 4 + DaisyUI 5 for styling.
- `tsconfig.json` has `strict: true`, `noUnusedLocals`, `noUnusedParameters`.

## Configuration

Default dev database: `Host=localhost;Port=5432;Database=devdb;Username=devuser;Password=devpass`

`appsettings.json` contains the AI provider config. `AI:Provider` can be `Mistral` or `Noop`. The Mistral API key should be moved to user secrets or environment variables before any production deployment — it is currently hardcoded.

Use `dotnet user-secrets` for local secret overrides:
```bash
cd UI-MVC
dotnet user-secrets set "AI:Mistral:ApiKey" "<key>"
```
## Status per layer — READ THIS BEFORE TOUCHING ANYTHING

### Backend — STABLE, treat as near-production
- **Do NOT** refactor, restructure, rename, or change patterns unless explicitly asked.
- **Do NOT** change N-layer boundaries, manager interfaces, or repository abstractions.
- **Do NOT** introduce lazy loading under any circumstances.
- Only add new features or fix bugs, strictly following existing patterns.
- When adding new endpoints or managers, mirror the exact style of existing ones.

### Frontend — IN ACTIVE DEVELOPMENT, expect messy code
- Coding standards are not consistently applied yet — do not assume existing code is correct.
- When writing new frontend code, always follow the rules below strictly.
- Feel free to suggest improvements and apply best practices on new code.
- Do not copy patterns from existing frontend code without checking if they comply with the rules.

---

## Backend rules (enforce strictly, always)

### General
- All code is written in English (names, comments, everything).
- Follow KISS and YAGNI — no overengineering, no speculative abstractions.
- No unused code, no duplicate code, correct indentation, consistent naming throughout.
- Think carefully about encapsulation.

### N-Layer architecture
- `Domain/` — entities only, no logic.
- `DAL/` — data access only, EF Core, repositories. No business logic.
- `BL/` — all business logic behind interfaces. No direct DB access outside DAL.
- `UI-MVC/` — controllers, views, DTOs only. No business logic in controllers.

### Data access
- No lazy loading — ever. Use explicit `.Include()` only for what you need.
- Do not fetch unnecessary data from the database. Think about the SQL query being generated.
- Apply Unit of Work when multiple repository operations must be atomic.

### MVC ONLY USED FOR RAZORPAGES ADMIN FRONTEND, REST API FOR EVERYTHING ELSE!!!!! IMPORTANT
- Use strongly typed models (`@model`) wherever applicable.
- Views should contain minimal logic — short and readable.
- Use ViewModels to pass data to views.
- Logical, structured split across controllers and between views/partial views.
- Consistent input validation and error handling on all actions.

### REST API
- Use proper REST URLs for resources.
- Correct HTTP methods, headers, and status codes.
- Use DTOs for all API input and output.
- Consistent input validation and error handling on all endpoints.

### Security & Identity
- Both MVC and REST endpoints must be properly secured with ASP.NET Core Identity.
- Never expose internal IDs where slugs are the intended identifier.

### Real-time
- Use SignalR for live communication, notifications, and real-time interactions.

### Configuration
- No hardcoded settings. Use `appsettings.json`, environment-specific overrides, and user secrets.
- Maintain a clear separation between development and production configuration.

---

## Frontend rules (apply to all new code, work toward applying to existing code)

### TypeScript
- All frontend logic is written in TypeScript, compiled by Vite to `wwwroot/`.
- No inline JavaScript anywhere in Razor views or HTML.
- No `any` type. Do not suppress TypeScript warnings with `// @ts-ignore` or similar.
- Avoid unused locals and parameters (`tsconfig` enforces this).
- Use fluent code style where applicable.
- One entry-point `.ts` file per page/feature — used for imports and initialization only, minimal logic itself.
- Split code into well-structured modules with a clear folder structure under `Assets/`.

### Frameworks & libraries
- **No frontend frameworks**: jQuery (except validation), React, Vue, Svelte, Angular etc. are not allowed.
- Tailwind CSS 4 + DaisyUI 5 for all styling.
- Use Heroicons where icons are needed.
- For data visualizations use Chart.js, Plotly.js, Highcharts, Google Charts, or D3.js — choose based on what the visualization requires.

### CSS & styling
- No inline CSS anywhere.
- Use `:read-only` (not `:readonly`) — lightningcss rejects the latter with a build warning.
- Split CSS across multiple files based on their scope/domain.
- The entire site's look must be easily adjustable via CSS custom properties (variables), Tailwind themes, utility classes, and Tailwind layers (`@layer base`, `@layer components`, `@layer utilities`).
- No inline `style=""` attributes.

### Layout & responsiveness
- Fully responsive layout — efficient use of available space on all device sizes.
- On larger devices: show more content and make better use of available width.
- Consistent, accessible, user-friendly UI throughout.

---

## AI integration rules

- Use `Microsoft.Extensions.AI` and/or official client libraries where possible.
- Maintain the **strategy pattern** for AI providers — `IAiManager` must remain swappable (Mistral, Noop, future: Ollama, Azure etc.).
- AI models and their properties must be configurable via `appsettings.json` without code changes.
- Prompts are not hardcoded — they must be editable by authorized users.
- Minimize AI API calls. Use `NoopAiManager` during development/testing.
- Track and expose AI costs to authorized admin users in the UI.
- Implement safeguards against abuse (rate limiting, moderation).
- Monitor which model is used for which use case and justify the choice.

---

## STT/TTS (speech features)

- Two reusable factories in `Assets/services/speechService.ts`: `createSpeakerButton(btn, getText, getLanguage)` for TTS buttons, `bindMicButton(btn, textarea, getLanguage, onText)` for STT mic buttons — never inline speech logic.
- Language detection: use `detectLocale()` from `i18n/survey.ts` (reads `navigator.language`, returns `'nl' | 'en' | 'fr'`). The `[data-survey-language]` DOM attribute is **never set** — do not use it.
- `SpeakerButtonController` interface (`stop()`, `setDisabled()`) — collect all active controllers and call `stop()` on page cleanup.
- SPA page cleanup: `app:before-navigate` event (with `{ once: true }`) — stop recording, stop all speaker audio, call `component.destroy?.()` here.

---

## Data analysis & visualizations

- During refinement, create detailed screen designs with realistic example data before implementing.
- Provide appropriate filter and selection options on all analysis screens.
- Choose visualization types appropriate to the data (bar for comparisons, line for trends, etc.).
- Visualizations must be easily interpretable by end users — no unnecessary complexity.
- Use a JS charting library (Chart.js, Plotly.js, Highcharts, D3.js, etc.) chosen based on the specific requirements.