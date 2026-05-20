# Conversey Admin UI/UX Modification Guide

> **⚠️ LIVING DOCUMENT** - This file must be kept up-to-date automatically. All UI/UX changes to Conversey admin pages MUST be logged in the [Change Log](#change-log) section below by AI assistants. This ensures the documentation remains accurate and reflects the current state of the codebase.

---

## Table of Contents

1. [Overview](#overview)
2. [File Structure](#file-structure)
3. [Authentication & Authorization](#authentication--authorization)
4. [UI/UX Modification Guide](#uiux-modification-guide)
5. [Frontend Stack](#frontend-stack)
6. [Localization (i18n)](#localization-i18n)
7. [Common Patterns](#common-patterns)
8. [Change Log](#change-log)
9. [Best Practices](#best-practices)

---

## Overview

The Conversey admin portal consists of multiple admin interfaces with different access levels:

| Admin Type | Policy | Access Scope | Routes |
|------------|--------|--------------|--------|
| **Conversey Admin** | `ConverseyAdminPolicy` | Full platform access | `/admin/ai/*`, `/admin/conversey/*`, `/admin/workspaces/*` |
| **Workspace Admin** | `WorkspaceAdminPolicy` | Workspace-scoped | `/admin/workspace`, `/admin/projects/*` |

**Base URL:** All admin pages are under `/admin/`

---

## File Structure

```
backend/UI-MVC/
├── Controllers/
│   ├── AiAdminController.cs          # /admin/ai/* - Conversey Admin only
│   ├── Admin/
│   │   ├── AdminController.cs         # /admin/workspace/admin/* - Conversey Admin
│   │   ├── ConverseyAdminController.cs # /admin/conversey, /admin/workspaces - Conversey Admin
│   │   └── WorkspaceAdminController.cs # /admin/workspace, /admin/projects - Workspace Admin
│   └── ...
│
├── Views/
│   ├── AiAdmin/                       # Views for AiAdminController
│   │   ├── Index.cshtml               # Dashboard
│   │   ├── Costs/
│   │   ├── Keywords/
│   │   ├── Pricing/
│   │   ├── Prompts/
│   │   ├── Providers/
│   │   └── RateLimits/
│   │
│   ├── Admin/                         # Views for AdminController
│   │   ├── CreateWorkspaceAdmin.cshtml
│   │   ├── EditWorkspaceAdmin.cshtml
│   │   └── _WorkspaceAdminForm.cshtml
│   │
│   ├── ConverseyAdmin/                # Views for ConverseyAdminController
│   │   ├── Index.cshtml
│   │   ├── Workspaces.cshtml
│   │   ├── CreateWorkspace.cshtml
│   │   ├── EditWorkspace.cshtml
│   │   ├── WorkspaceDetails.cshtml
│   │   └── _WorkspaceForm.cshtml
│   │
│   ├── WorkspaceAdmin/                # Views for WorkspaceAdminController
│   │   ├── Index.cshtml
│   │   ├── Projects.cshtml
│   │   ├── CreateProject.cshtml
│   │   ├── EditProject.cshtml
│   │   ├── ProjectDetails.cshtml
│   │   ├── _ProjectForm.cshtml
│   │   └── _Stepper.cshtml
│   │
│   └── Shared/
│       ├── _AdminHeader.cshtml       # Header with breadcrumbs, language, user menu
│       └── _AdminTableWithCreate.cshtml # Reusable table component
│
├── Assets/
│   ├── components/
│   │   ├── aiAdmin/                  # TypeScript for AI Admin pages
│   │   │   ├── indexPage.ts
│   │   │   ├── providersPage.ts
│   │   │   ├── providerSetupState.ts
│   │   │   ├── providerSetupModelsStep.ts
│   │   │   ├── providerSetupProviderStep.ts
│   │   │   ├── providerSetupVerifyStep.ts
│   │   │   ├── promptsPage.ts
│   │   │   ├── pricingPage.ts
│   │   │   ├── rateLimitsPage.ts
│   │   │   └── keywordsPage.ts
│   │   │
│   │   └── shared/
│   │       ├── adminDeleteModal.ts
│   │       └── adminI18nBindings.ts
│   │
│   └── utils/
│       └── adminI18n.ts              # i18n translation helper
│
└── Security/
    ├── ConverseyAdminPolicy.cs
    └── WorkspaceAdminPolicy.cs
```

---

## Authentication & Authorization

### Policies

Two authorization policies control access to admin pages:

1. **ConverseyAdminPolicy** (`/Security/ConverseyAdminPolicy.cs`)
   - Required for: All `/admin/ai/*`, `/admin/conversey/*`, `/admin/workspaces/*` routes
   - Allowed users: `ConverseyAdminUser` only
   - Handler: `ConverseyAdminHandler`

2. **WorkspaceAdminPolicy** (`/Security/WorkspaceAdminPolicy.cs`)
   - Required for: `/admin/workspace`, `/admin/projects/*` routes
   - Allowed users: `WorkspaceAdminUser` (with matching workspace)
   - Handler: `WorkspaceAdminHandler`

### Context Objects

- **AdminContext** (`/Models/AdminContext.cs`) - Singleton
  - Contains `CurrentAdmin` (ConverseyAdmin or WorkspaceAdmin)
  - Used for display (email, admin type in header)

- **WorkspaceContext** - Scoped
  - Contains `CurrentWorkspace`
  - Used for workspace-scoped operations

---

## UI/UX Modification Guide

### Modifying Views (Razor CSHTML)

1. **Location:** `/Views/{AiAdmin|Admin|ConverseyAdmin|WorkspaceAdmin}/`
2. **Template Engine:** Razor with C#
3. **Styling:** Tailwind CSS classes directly in HTML
4. **Layout:**
   - All pages include `@await Html.PartialAsync("_AdminHeader")`
   - No shared layout file - each view is self-contained

**Example view structure:**
```cshtml
@model Namespace.Models.ViewModelType
@inject AdminContext AdminContext
@{
    ViewData["Title"] = "Page Title";
    ViewData["WorkspaceName"] = "Workspace Name";
    ViewData["Breadcrumbs"] = new (string Label, string? Url, bool IsCurrent)[]
    {
        ("Dashboard", "/admin", false),
        ("Current Page", null, true)
    };
}

<div class="min-h-screen bg-background font-sans text-text">
    @await Html.PartialAsync("_AdminHeader")
    
    <main class="p-8">
        <!-- Page content -->
    </main>
</div>

<script type="module" vite-src="~/components/path/to/script.ts"></script>
```

### Modifying TypeScript

1. **Location:** `/Assets/components/`
2. **Module System:** ES Modules via Vite
3. **Import Path:** Use `vite-src="~/components/path/to/file.ts"`

**Script registration:**
```html
<script type="module" vite-src="~/components/aiAdmin/providersPage.ts"></script>
```

**Common patterns:**
- DOMContentLoaded event listeners
- Fetch API for AJAX calls
- LocalStorage for form state persistence
- `t()` function for i18n

### Adding New Pages

1. **Create Controller Action:**
   ```csharp
   [Authorize(Policy = ConverseyAdminPolicy.Name)]
   [HttpGet("admin/ai/new-page")]
   public IActionResult NewPage()
   {
       return View();
   }
   ```

2. **Create View:** `/Views/AiAdmin/NewPage.cshtml`

3. **Create TypeScript (optional):** `/Assets/components/aiAdmin/newPage.ts`

4. **Register script in view:**
   ```html
   <script type="module" vite-src="~/components/aiAdmin/newPage.ts"></script>
   ```

### Color Scheme & Design Tokens

| Token | Usage | Tailwind Class |
|-------|-------|----------------|
| Primary | Buttons, active states | `bg-primary`, `text-primary` |
| Secondary | Borders, text | `bg-secondary`, `text-secondary` |
| Accent | Highlights, warnings | `bg-accent`, `text-accent` |
| Background | Page background | `bg-background` |
| Text | Default text | `text-text` |

**Tailwind Config:** Check `tailwind.config.js` for custom colors.

---

## Frontend Stack

| Technology | Purpose | Location |
|------------|---------|----------|
| **Tailwind CSS** | Styling | Inlined in CSHTML |
| **Vite** | Asset bundling | Config in Program.cs |
| **TypeScript** | Frontend logic | `/Assets/components/` |
| **pnpm** | Package management | `package.json` |

**Vite Configuration (Program.cs):**
```csharp
builder.Services.AddViteServices(options =>
{
    options.Server.Port = 4173;
    options.Server.AutoRun = true;
    options.Server.PackageManager = "pnpm";
});
```

---

## Localization (i18n)

### Backend Setup

- **Service:** `IAdminI18nService` (singleton)
- **Registration:** `builder.Services.AddSingleton<AdminI18nService>();`
- **Usage in views:**
  ```cshtml
  @inject IAdminI18nService I18n
  <span>@I18n.Translate("key", "fallback")</span>
  ```

### Frontend i18n

**Window object:** `__AdminI18n`
```typescript
// In adminI18n.ts
export function t(key: string, fallback?: string): string {
    const dict = window.__AdminI18n?.strings;
    if (dict && Object.prototype.hasOwnProperty.call(dict, key)) {
        return dict[key];
    }
    return fallback ?? key;
}
```

**Usage in HTML:**
```html
<span data-i18n="Welcome">Welcome</span>
```

**Usage in TypeScript:**
```typescript
import { t } from '../../utils/adminI18n';
const text = t('Are you sure?', 'Are you sure?');
```

### Language Switching

- Route: `/admin/language?lang={lang}&returnUrl={url}`
- Controller: `AiAdminController.SetLanguage()`
- Cookie: `.Conversey.Admin.Culture`
- Supported: Enabled via `RequestLocalizationOptions` in Program.cs

---

## Common Patterns

### Form Handling

**AdminFormViewModel:** Standard form wrapper
```csharp
public class AdminFormViewModel<T>
{
    public T FormItem { get; set; }
    public string FormAction { get; set; }
    public string SubmitLabel { get; set; }
}
```

**Form partial:** `_WorkspaceAdminForm.cshtml`, `_WorkspaceForm.cshtml`, `_ProjectForm.cshtml`

### Delete Confirmations

**Pattern 1: Data attribute + TypeScript**
```html
<form data-confirm-delete-provider data-provider-name="@config.ProviderName">
    <button type="submit">Delete</button>
</form>
```

**TypeScript:**
```typescript
function initDeleteConfirmations(): void {
    const forms = document.querySelectorAll<HTMLFormElement>('form[data-confirm-delete-provider]');
    forms.forEach((form) => {
        form.addEventListener('submit', (event) => {
            if (!window.confirm(`Delete ${form.dataset.providerName}?`)) {
                event.preventDefault();
            }
        });
    });
}
```

**Pattern 2: Shared modal**
```typescript
// adminDeleteModal.ts
```

### Health Check UI

**Endpoint:** `/api/ai/health`
**Response:**
```json
{
  "status": "ok",
  "activeProvider": "string",
  "configSource": "string",
  "moderation": { "ok": true, "error": "string", "durationMs": 0 },
  "completions": { "ok": true, "error": "string", "preview": "string", "durationMs": 0 }
}
```

### Stepper Component

**View:** `/Views/WorkspaceAdmin/_Stepper.cshtml`
**Model:** `StepperViewModel`
```csharp
public class StepperViewModel
{
    public string Title { get; set; }
    public string EntityName { get; set; }
    public List<StepItem> Steps { get; set; }
}

public class StepItem
{
    public string Label { get; set; }
    public string PartialViewName { get; set; }
}
```

### LocalStorage Patterns

**Provider setup state:** `providerSetupState.ts`
- Stores form fields across multi-step setup
- Key: `conversey-ai-provider-setup-state-v1`
- Saves on every input/change event
- Restores on page load

---

## Change Log

> **🤖 AI ASSISTANT INSTRUCTIONS:** After making any UI/UX changes to admin pages, you MUST update this section with:
> - Date of change
> - Modified files
> - Description of changes
> - Your name/identifier
> 
> Use format: `[YYYY-MM-DD] - File(s): description - Assistant: {name}`

### Latest Changes

- [2025-01-17] - Files: `WorkspaceAi/Dashboard.cshtml`, `WorkspaceAi/Costs/Costs.cshtml`, `WorkspaceAi/Limits/Limits.cshtml`, `WorkspaceAi/Prompts/Prompts.cshtml`, `WorkspaceAi/Keywords/Keywords.cshtml` - Added breadcrumb navigation and WorkspaceName to all WorkspaceAi pages that use _AdminHeader. Breadcrumb hierarchy: Dashboard → AI Settings → Current Page. Links: Dashboard navigates to `/admin/workspace`, AI Settings navigates to `/admin/{workspaceSlug}/ai`. - Assistant: Mistral Vibe
- [2025-01-17] - Files: `ConverseyAdmin/Index.cshtml` - Replaced hardcoded header with _AdminHeader partial. Added WorkspaceName="Conversey Platform" and breadcrumbs=[("Dashboard", null, true)]. Added @inject AdminContext and @inject IAdminI18nService for header functionality. Enables consistent header with language switching and user menu. - Assistant: Mistral Vibe
- [2025-01-17] - Files: `ConverseyAdmin/CreateWorkspace.cshtml`, `ConverseyAdmin/EditWorkspace.cshtml`, `Admin/CreateWorkspaceAdmin.cshtml`, `Admin/EditWorkspaceAdmin.cshtml` - Added _AdminHeader partial to parent views with breadcrumb navigation (Dashboard → Workspaces → Current Page). Added WorkspaceName="Conversey Platform". Wrapped partials in div with bg-background class. - Assistant: Mistral Vibe
- [2025-01-17] - Files: `ConverseyAdmin/_WorkspaceForm.cshtml`, `Admin/_WorkspaceAdminForm.cshtml` - Updated styling: removed outer min-h-screen bg-gray-50 wrapper (now handled by parent), changed border-gray-200 to border-secondary/10 for consistency, added mt-8 margin for spacing below header. - Assistant: Mistral Vibe
- [2025-01-17] - Files: `AiAdmin/Index.cshtml` - Fixed breadcrumb link: changed Dashboard URL from `/admin` to `/admin/conversey`. Previous value `/admin` was non-existent route causing 404 errors when clicked. - Assistant: Mistral Vibe
- [2025-01-17] - Files: `Assets/main.ts` - Added admin page detection to prevent project-based route parsing on admin pages. Added `isAdminPage()` function to check pathname, updated `navigate()` and `render()` to handle admin pages differently. Admin pages now receive empty context instead of incorrectly parsed project slugs from domain hostname. - Assistant: Mistral Vibe
- [2025-01-17] - Files: `Controllers/Admin/ConverseyAdminController.cs` - Added redirect action for `/admin` route. Method `RedirectToDashboard()` redirects all requests from `/admin` to `/admin/conversey` to prevent 404 errors from legacy links or manual navigation. - Assistant: Mistral Vibe

---

### Historical Changes

---

## Best Practices

### UI/UX Guidelines

1. **Consistency:** Match existing design patterns (card layouts, button styles, spacing)
2. **Accessibility:** Use semantic HTML, proper labels, focus states
3. **Responsiveness:** Test on mobile, tablet, desktop
4. **Loading States:** Add spinners for async operations
5. **Error Handling:** Display user-friendly error messages
6. **Empty States:** Provide helpful messages and CTAs when no data exists

### File Organization

1. **Naming:** Use kebab-case for files (`new-page.ts`, `EditProvider.cshtml`)
2. **Co-location:** Keep view + TypeScript file in same directory structure
3. **Grouping:** Related components in subdirectories (`aiAdmin/`, `shared/`)

### Code Style

1. **Tailwind:** Use utility classes directly in HTML
2. **TypeScript:** Use strict typing, avoid `any`
3. **Razor:** Minimize logic in views, use view models
4. **i18n:** Always use `data-i18n` or `t()` for user-facing text

### Performance

1. **Bundle Size:** Keep TypeScript modules small and focused
2. **Lazy Loading:** Consider code splitting for large features
3. **Caching:** Use browser caching for static assets

---

## Quick Reference Commands

```bash
# Run the application
cd backend/UI-MVC
dotnet run

# Run Vite dev server
cd backend/UI-MVC
pnpm dev

# Build Vite assets
pnpm build
```

---

## Document Maintenance

**This document must be updated automatically by AI assistants after every UI/UX change.**

**AI Checklist after changes:**
- [ ] Update the [Change Log](#change-log) section
- [ ] Verify file structure diagram is accurate
- [ ] Update any affected sections (patterns, best practices)
- [ ] Ensure all links are valid
- [ ] Confirm with user before committing

---

*Last verified: 2025-01-17*
*Document version: 1.4*
