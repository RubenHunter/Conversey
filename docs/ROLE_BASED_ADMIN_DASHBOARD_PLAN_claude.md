# Role-Based Admin Dashboard Implementation Plan

> **📋 LIVING DOCUMENT** - This file must be kept up-to-date. All changes to role-based admin dashboard components MUST be logged in the [Implementation Log](#implementation-log) section by AI assistants.

> **🎯 PRIMARY GOAL:** Create **reusable, role-aware UI components** that display **different data** based on admin type (Conversey Admin vs Workspace Admin) while maintaining **consistent UX** across all admin interfaces.

---

## Table of Contents

1. [Objectives](#objectives)
2. [Current State](#current-state)
3. [Role-Based Architecture](#role-based-architecture)
4. [Implementation Phases](#implementation-phases)
5. [Technical Specifications](#technical-specifications)
6. [Component Library](#component-library)
7. [Data Flow](#data-flow)
8. [API Requirements](#api-requirements)
9. [Implementation Log](#implementation-log)
10. [Testing Strategy](#testing-strategy)
11. [Dependencies & Risks](#dependencies--risks)

---

## Objectives

### Core Requirements

✅ **Reusable Components** - Same UI structure for all admin types  
✅ **Role-Aware Data** - Data automatically filtered by admin scope  
✅ **Consistent UX** - Identical look and feel across Conversey and Workspace admins  
✅ **Responsive** - Works on mobile, tablet, and desktop  
✅ **Maintainable** - Single codebase for all admin types  

### Key Principles

1. **Single Component, Multiple Data** - One component template, data varies by role
2. **Server-Side Filtering** - Data filtering happens in backend/API layer
3. **Progressive Enhancement** - Works without JavaScript, enhanced with it
4. **Backward Compatible** - Existing pages continue to work

---

## Current State

### What Exists

| Area | Conversey Admin | Workspace Admin | Status |
|------|-----------------|-----------------|--------|
| **/admin/conversey** | Dashboard, Workspaces | ❌ No access | ✅ Implemented |
| **/admin/workspace** | ❌ No access | Dashboard | ✅ Implemented |
| **/admin/ai** | Full access | ❌ No access | ✅ Implemented |
| **/admin/projects** | ❌ No access | Full access | ✅ Implemented |
| **Header** | `_AdminHeader` | `_AdminHeader` | ✅ Standardized (Phase 1-3) |
| **Breadcrumbs** | ✅ Working | ✅ Working | ✅ Fixed (Phase 1-3) |

### What's Missing

1. **Role-aware data filtering** - API endpoints don't automatically filter by admin scope
2. **Reusable dashboard components** - Each page has custom implementation
3. **Unified dashboard page** - No single `/admin` dashboard that adapts to role
4. **`_EngagementWidget`** - Circular gauge + labeled progress bars (see Wireframe Analysis)
5. **`_QuickLinksWidget`** - Grouped link-list container (see Wireframe Analysis)
6. **`_NavCard`** - Top navigation card building block (AI Settings, Workspaces row)

---

## Wireframe Analysis (Conversey Admin Dashboard)

> **Source:** Screenshots provided 2026-05-21 — Conversey Platform Admin Dashboard wireframe.

### Visual Map → Component Mapping

```
┌──────────────────────────────────────────────────────────────────────┐
│  HEADER: _AdminHeader (existing)                                      │
│  [C] Conversey Platform / Dashboard   Admin Portal  EN  🔔  admin@   │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│  PAGE TITLE AREA                                                      │
│  Dashboard                                                            │
│  Look at statistics, expand/manage our community.                    │
└──────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────┐  ┌─────────────────────────────────────┐
│  _NavCard  (building block) │  │  _NavCard  (building block)         │
│  🖥 AI Settings             │  │  🗂 Workspaces                      │
│  Configure providers...     │  │  Manage all workspaces...           │
└─────────────────────────────┘  └─────────────────────────────────────┘

┌──────────────────┐  ┌──────────────────┐  ┌────────────────────────┐
│ _ComparisonWidget│  │ _QuickLinksWidget │  │ _EngagementWidget  NEW │
│ (existing spec)  │  │ (NEW grouped)     │  │ (circular gauge +      │
│                  │  │                   │  │  labeled progress bars)│
│ Active project   │  │ + New workspace   │  │ Weekly Engagement Rate │
│ All project      │  │ ♥ Check health    │  │      ◯ 67%            │
│                  │  │ 🔒 Rate Limits    │  │ Response Rate   14/20  │
│ ⬤Nmbs      17   │  │                   │  │ Toxicity        8.5/10 │
│ ⬤Stad St.  13   │  │                   │  │ Idea Contrib. 1290/1954│
│ ⬤JC Goub.   8   │  │                   │  │                        │
└──────────────────┘  └──────────────────┘  └────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│  _ChartWidget (full-width, line chart)                                │
│  Usage Trend               [7d]  [1m]  [total]  ← intra-widget tabs │
│  amount of visits                                                     │
│  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~│
│  2026-04-20 ............... 2026-05-02 ............... 2026-05-18   │
└──────────────────────────────────────────────────────────────────────┘
```

### Component Breakdown

| Zone | Component | Type | Status |
|------|-----------|------|--------|
| Header | `_AdminHeader` | Building block | ✅ Existing |
| Top nav cards | `_NavCard` | Building block (NEW) | 🔲 To create |
| Left widget | `_ComparisonWidget` | Widget | ✅ Specified |
| Center widget | `_QuickLinksWidget` | Widget (NEW — grouped) | 🔲 To create |
| Right widget | `_EngagementWidget` | Widget (NEW) | 🔲 To create |
| Bottom chart | `_ChartWidget` | Widget | ✅ Existing (extend with tabs) |

### Key Observations from Wireframe

**1. `_NavCard` (top navigation cards)**
- Not widgets — they are **navigation building blocks** placed in a 2-column row above the widget board
- Each card: icon in colored square + bold title + gray description text
- Role-dependent: Conversey Admin sees "AI Settings" + "Workspaces"; Workspace Admin sees "Projects" + "AI Settings" (workspace-scoped)
- Click navigates to the admin section (no modal)

**2. `_QuickLinksWidget` (center column)**
- A **grouped container** holding multiple link rows — not individual cards
- The wireframe label says "Links" above this section
- Each row: rounded icon background + bold title + gray subtitle
- Icons are colored (yellow `+`, pink heart ♥, pink lock 🔒)
- Items are clickable (modal or redirect based on link type)
- This is **different from `_LinksWidget`** which was individual cards — this is a stacked list inside one widget card

**3. `_EngagementWidget` (right column) — NEW**
- Top: circular progress gauge showing percentage (67%) with label "Full Completion Rate"
- Gauge: thick arc, yellow/gold color, stops at the percentage angle
- Below: 3 labeled progress bars, each with:
  - Left label (e.g. "Response Rate") + right sublabel (e.g. "average responses per survey")
  - Filled progress bar with value shown inside (e.g. "14/20")
  - Colors: green for good scores, orange for near-capacity
- Data comes from engagement/moderation API

**4. `_ChartWidget` intra-widget tabs**
- The usage trend chart has 3 period buttons in the top-right: `7d`, `1m`, `total`
- Active tab has border/background; inactive are plain
- Clicking a tab re-fetches or re-filters chart data client-side
- Must be added to `ChartWidgetViewModel` as `Periods` list with active state

---

## Role-Based Architecture

### Admin Types & Permissions

| Type | Policy | Access Scope | Can Access |
|------|--------|--------------|------------|
| **Conversey Admin** | `ConverseyAdminPolicy` | Platform-wide | `/admin/conversey`, `/admin/ai`, `/admin/workspaces` |
| **Workspace Admin** | `WorkspaceAdminPolicy` | Single workspace | `/admin/workspace`, `/admin/projects`, `/admin/{workspaceSlug}/ai` |

### Role Detection

**Backend (C#):**
```csharp
// In controllers
bool IsConverseyAdmin => AdminContext.CurrentAdmin is ConverseyAdmin;
bool IsWorkspaceAdmin => AdminContext.CurrentAdmin is WorkspaceAdmin;

// Get workspace for WorkspaceAdmin
Workspace CurrentWorkspace => (AdminContext.CurrentAdmin as WorkspaceAdmin)?.Workspace;
```

**Frontend (TypeScript):**
```typescript
// In main.ts or similar
export function getAdminType(): 'conversey' | 'workspace' | null {
    const path = window.location.pathname;
    if (path.startsWith('/admin/conversey') || path.startsWith('/admin/ai') || path.startsWith('/admin/workspaces')) {
        return 'conversey';
    }
    if (path.startsWith('/admin/workspace') || path.startsWith('/admin/projects') || path.match(/\/admin\/[^\/]+\/ai/)) {
        return 'workspace';
    }
    return null;
}
```

### Context Objects

| Object | Purpose | Available in |
|--------|---------|---------------|
| `AdminContext` | Current admin user | All admin pages (Singleton) |
| `WorkspaceContext` | Current workspace | Workspace-admin pages (Scoped) |
| `ViewData["WorkspaceName"]` | Display name | Pages using `_AdminHeader` |
| `ViewData["Breadcrumbs"]` | Navigation | Pages using `_AdminHeader` |

---

## Implementation Phases

### **Phase A: Foundation (Priority: Critical)**
**Goal:** Establish role detection and context passing infrastructure.

#### Tasks

1. **Create AdminType Helper**
   - Location: `/Models/AdminExtensions.cs`
   - Methods: `IsConverseyAdmin()`, `IsWorkspaceAdmin()`, `GetWorkspaceId()`
   - Usage: All controllers

2. **Standardize WorkspaceContext Usage**
   - Ensure all WorkspaceAdmin pages have WorkspaceContext injected
   - Verify WorkspaceContext.CurrentWorkspace is set correctly

3. **Create Base Dashboard Controller**
   - Location: `/Controllers/Admin/DashboardController.cs`
   - Handles `/admin` route (redirects based on role)
   - Or creates unified dashboard view

#### Deliverables
- [ ] AdminType helper class
- [ ] Consistent WorkspaceContext usage
- [ ] Role detection working on all admin pages

---

### **Phase B: Data Layer (Priority: Critical)**
**Goal:** Create role-aware API endpoints that return filtered data.

#### Tasks

1. **Create AdminStatsService**
   - Location: `/BL/Administration/AdminStatsService.cs`
   - Methods:
     - `GetDashboardStats(Admin admin)` - Returns filtered stats
     - `GetProjects(Admin admin)` - Returns filtered projects
     - `GetWorkspaces(Admin admin)` - Returns filtered workspaces
     - `GetUsageTrend(Admin admin, string period)` - Returns filtered trend data

2. **Extend ApiController**
   - Add `/api/admin/stats` endpoints
   - Add `/api/admin/dashboard` endpoint
   - Implement automatic filtering based on admin type

3. **Data Transfer Objects (DTOs)**
   - `DashboardStatsDto` - For dashboard statistics
   - `ProjectWidgetDto` - For project widgets
   - `WorkspaceWidgetDto` - For workspace widgets

#### API Endpoint Examples

```csharp
// GET /api/admin/dashboard
[HttpGet("admin/dashboard")]
[Authorize(Policy = ConverseyAdminPolicy.Name)] // Or both policies
public async Task<IActionResult> GetDashboardData()
{
    var currentAdmin = AdminContext.CurrentAdmin;
    
    if (currentAdmin is ConverseyAdmin)
    {
        var stats = await _adminStatsService.GetPlatformStatsAsync();
        return Ok(new { type = "platform", data = stats });
    }
    else if (currentAdmin is WorkspaceAdmin workspaceAdmin)
    {
        var stats = await _adminStatsService.GetWorkspaceStatsAsync(workspaceAdmin.Workspace.Id);
        return Ok(new { type = "workspace", data = stats, workspaceName = workspaceAdmin.Workspace.Name });
    }
    
    return Forbid();
}
```

#### Deliverables
- [ ] AdminStatsService implemented
- [ ] Role-aware API endpoints
- [ ] DTOs for dashboard data

---

### **Phase C: Reusable Components (Priority: High)**
**Goal:** Create component library that adapts to role and data.

#### Component Structure

```
Views/
├── Shared/
│   └── Admin/
│       ├── Dashboard/
│       │   ├── _ComparisonWidget.cshtml # Radial circle comparison
│       │   ├── _StatWidget.cshtml        # Dashboard stat display
│       │   ├── _ChartWidget.cshtml       # Dashboard chart container
│       │   ├── _ActionWidget.cshtml      # Dashboard action buttons
│       │   ├── _ProgressWidget.cshtml    # Dashboard progress indicator
│       │   └── _TableWidget.cshtml        # Dashboard data table
│       │
│       └── Modals/
│           ├── _NewWorkspaceModal.cshtml  # Conversey Admin only
│           ├── _NewProjectModal.cshtml   # Workspace Admin only
│           └── _ConfirmModal.cshtml       # Reusable confirmation
│
└── Admin/
    └── Dashboard.cshtml                  # Unified dashboard view
```

#### Component Templates

**1. Stat Widget (`_StatWidget.cshtml`)**
```cshtml
@model StatWidgetViewModel

<div class="bg-white rounded-xl border border-secondary/10 shadow-sm p-5">
    <span class="text-[10px] text-secondary uppercase font-bold tracking-wider block">
        @Model.Label
    </span>
    <div class="text-2xl font-bold text-text mt-1">@Model.Value</div>
    @if (!string.IsNullOrEmpty(Model.SubLabel))
    {
        <p class="text-xs text-text/60 mt-1">@Model.SubLabel</p>
    }
</div>
```

**2. Chart Widget (`_ChartWidget.cshtml`)**
```cshtml
@model ChartWidgetViewModel

<div class="bg-white rounded-xl border border-secondary/10 shadow-sm p-6">
    <h2 class="font-semibold text-text mb-4">@Model.Title</h2>
    <div class="flex-1 min-h-0">
        <canvas id="@Model.CanvasId" class="w-full h-full"></canvas>
    </div>
</div>
```

<script>
    document.addEventListener('DOMContentLoaded', () => {
        const ctx = document.getElementById('@Model.CanvasId');
        if (ctx) {
            // Will be initialized by TypeScript
            window.__ChartData = window.__ChartData || {};
            window.__ChartData['@Model.CanvasId'] = @Html.Raw(Json.Serialize(Model.Data));
        }
    });
</script>
```

**3. Progress Widget (`_ProgressWidget.cshtml`)**
```cshtml
@model ProgressWidgetViewModel

<div class="bg-white rounded-xl border border-secondary/10 shadow-sm p-4 flex flex-col">
    <span class="font-semibold text-text">@Model.Label</span>
    <div class="flex justify-between text-sm mt-2">
        <span class="text-text/50">Progress</span>
        <span class="font-semibold text-text">@Model.Percentage%</span>
    </div>
    <progress class="w-full h-2 rounded-full mt-4 flex-1" value="@Model.Percentage" max="100"></progress>
</div>
```

**4. Action Widget (`_ActionWidget.cshtml`)**
```cshtml
@model ActionWidgetViewModel

<div class="bg-white rounded-xl border border-secondary/10 shadow-sm p-4 flex flex-col">
    @if (!string.IsNullOrEmpty(Model.Icon))
    {
        <div class="text-3xl mb-3 text-primary">@Model.Icon</div>
    }
    <h3 class="font-semibold text-text mb-1">@Model.Title</h3>
    @if (!string.IsNullOrEmpty(Model.Description))
    {
        <p class="text-xs text-text/50 mb-4 flex-1">@Model.Description</p>
    }
    <div class="mt-auto">
        @if (Model.IsModal)
        {
            <button data-modal-open="@Model.ModalTarget"
                    class="btn btn-primary w-full">
                @Model.ActionText
            </button>
        }
        else
        {
            <a href="@Model.ActionUrl" class="btn btn-primary w-full block text-center">
                @Model.ActionText
            </a>
        }
    </div>
</div>
```

**5. Table Widget (`_TableWidget.cshtml`)**
```cshtml
@model TableWidgetViewModel

<div class="bg-white rounded-xl border border-secondary/10 shadow-sm p-4 overflow-auto">
    <table class="w-full text-sm">
        <thead>
            <tr class="border-b border-secondary/10">
                @foreach (var col in Model.Columns)
                {
                    <th class="text-left py-2 px-3 font-semibold text-text/70">@col.Header</th>
                }
                @if (Model.Actions.Any())
                {
                    <th class="text-left py-2 px-3 font-semibold text-text/70">Actions</th>
                }
            </tr>
        </thead>
        <tbody>
            @foreach (var row in Model.Rows)
            {
                <tr class="border-b border-secondary/5 last:border-0">
                    @foreach (var col in Model.Columns)
                    {
                        <td class="py-2 px-3 text-text">@row.Values[col.Property]</td>
                    }
                    @if (Model.Actions.Any())
                    {
                        <td class="py-2 px-3 whitespace-nowrap">
                            @foreach (var action in Model.Actions)
                            {
                                <a href="@action.Url" class="@action.Class">@action.Label</a>
                            }
                        </td>
                    }
                </tr>
            }
        </tbody>
    </table>
</div>
```

#### Deliverables
- [ ] Comparison widget component
- [ ] Stat widget component
- [ ] Chart widget component
- [ ] Action widget component
- [ ] Progress widget component
- [ ] Table widget component
- [ ] Modal components (New Workspace, New Project, Confirm)

---

### **Phase D: Unified Dashboard (Priority: High)**
**Goal:** Create single dashboard page that adapts to admin role — layout is derived from the validated wireframe.

#### Wireframe-Accurate Layout Structure

```
Page
├── _AdminHeader (existing partial)
├── main.p-8
│   ├── Page Title Block (h1 + p)
│   ├── NavCards Row (2 cols, role-dependent)
│   ├── Widget Row (3 cols: Comparison | QuickLinks | Engagement)
│   └── Full-Width Chart (ChartWidget with period tabs)
```

```cshtml
@* Views/Admin/Dashboard.cshtml *@
@model DashboardViewModel

<div class="min-h-screen bg-background font-sans text-text">
    @await Html.PartialAsync("_AdminHeader")

    <main class="p-8 w-full max-w-7xl mx-auto">

        {{!-- 1. Page Title --}}
        <div class="mb-8">
            <h1 class="text-3xl font-bold text-text tracking-tight">@Model.PageTitle</h1>
            <p class="mt-2 text-sm text-text/60">@Model.PageDescription</p>
        </div>

        {{!-- 2. Navigation Cards Row (role-dependent) --}}
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-8">
            @foreach (var navCard in Model.NavCards)
            {
                @await Html.PartialAsync("_NavCard", navCard)
            }
        </div>

        {{!-- 3. Main Widget Row: Comparison | QuickLinks | Engagement --}}
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-6">
            @await Html.PartialAsync("_ComparisonWidget", Model.ComparisonWidget)
            @await Html.PartialAsync("_QuickLinksWidget", Model.QuickLinksWidget)
            @await Html.PartialAsync("_EngagementWidget", Model.EngagementWidget)
        </div>

        {{!-- 4. Full-Width Usage Trend Chart --}}
        @await Html.PartialAsync("_ChartWidget", Model.UsageTrendChart)

    </main>
</div>
```

#### Controller Logic

```csharp
[Authorize]
public class DashboardController : Controller
{
    private readonly IAdminStatsService _statsService;
    
    [HttpGet("/admin")]
    public async Task<IActionResult> Index()
    {
        var currentAdmin = AdminContext.CurrentAdmin;
        
        if (currentAdmin is ConverseyAdmin)
        {
            var stats = await _statsService.GetPlatformDashboardAsync();
            return View("Dashboard", new DashboardViewModel
            {
                PageTitle = "Dashboard",
                PageDescription = "Look at statistics, expand/manage our community.",
                
                // Top nav cards (Conversey Admin)
                NavCards = new List<NavCardViewModel>
                {
                    new NavCardViewModel
                    {
                        Title = "AI Settings",
                        Description = "Configure providers, models, prompts, and monitor costs",
                        Icon = "monitor",
                        NavigateUrl = "/admin/ai"
                    },
                    new NavCardViewModel
                    {
                        Title = "Workspaces",
                        Description = "Manage all workspaces and their settings",
                        Icon = "briefcase",
                        NavigateUrl = "/admin/workspaces"
                    }
                },
                
                // Left: Comparison widget (workspace project counts)
                ComparisonWidget = stats.ComparisonWidget,
                
                // Center: Quick links (create workspace, check health, rate limits)
                QuickLinksWidget = stats.QuickLinksWidget,
                
                // Right: Engagement metrics
                EngagementWidget = stats.EngagementWidget,
                
                // Bottom: Usage trend chart (full-width, with period tabs)
                UsageTrendChart = stats.UsageTrendChart
            });
        }
        else if (currentAdmin is WorkspaceAdmin workspaceAdmin)
        {
            var stats = await _statsService.GetWorkspaceDashboardAsync(workspaceAdmin.Workspace.Id);
            return View("Dashboard", new DashboardViewModel
            {
                PageTitle = "Dashboard",
                PageDescription = $"Manage {workspaceAdmin.Workspace.Name}",
                
                // Top nav cards (Workspace Admin)
                NavCards = new List<NavCardViewModel>
                {
                    new NavCardViewModel
                    {
                        Title = "Projects",
                        Description = "Manage projects within your workspace",
                        Icon = "folder",
                        NavigateUrl = "/admin/projects"
                    },
                    new NavCardViewModel
                    {
                        Title = "AI Settings",
                        Description = "Configure AI for your workspace",
                        Icon = "monitor",
                        NavigateUrl = $"/admin/{workspaceAdmin.Workspace.Slug}/ai"
                    }
                },
                
                ComparisonWidget = stats.ComparisonWidget,
                QuickLinksWidget = stats.QuickLinksWidget,
                EngagementWidget = stats.EngagementWidget,
                UsageTrendChart = stats.UsageTrendChart
            });
        }
        
        return Forbid();
    }
}
```

#### Deliverables
- [ ] Unified dashboard view (`Dashboard.cshtml`)
- [ ] Role-specific `DashboardViewModel`
- [ ] Controller with role detection
- [ ] `_NavCard` partial (building block)
- [ ] `_QuickLinksWidget` partial (new grouped widget)
- [ ] `_EngagementWidget` partial (new gauge + progress bars widget)
- [ ] Period tabs on `_ChartWidget`

---

### **Phase E: TypeScript Integration (Priority: Medium)**
**Goal:** Add client-side interactivity to components.

#### Tasks

1. **Chart Initialization**
   - Extend existing Chart.js integration
   - Initialize charts based on data attributes

2. **Modal System**
   - Reuse existing `adminDeleteModal.ts` pattern
   - Create generic modal handler

3. **Data Fetching**
   - Use existing `apiFetch` from apiService.ts
   - Add role-aware data fetching

#### TypeScript Examples

```typescript
// charts.ts - Extended
import { Chart, registerables } from 'chart.js';
Chart.register(...registerables);

interface ChartData {
    type: 'bar' | 'line' | 'bubble';
    data: any;
    options?: any;
}

interface ChartConfig {
    [canvasId: string]: ChartData;
}

declare global {
    interface Window {
        __ChartData?: ChartConfig;
    }
}

export function initCharts(): void {
    const chartData = window.__ChartData;
    if (!chartData) return;
    
    Object.entries(chartData).forEach(([canvasId, config]) => {
        const canvas = document.getElementById(canvasId) as HTMLCanvasElement | null;
        if (canvas) {
            new Chart(canvas, {
                type: config.type,
                data: config.data,
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    ...config.options
                }
            });
        }
    });
}

document.addEventListener('DOMContentLoaded', initCharts);
```

```typescript
// modal.ts - Generic modal handler
// NOTE: Use the complete initModals() from Modal System section above
// This simplified version is kept for reference but the full version with
// accessibility features should be used in production
export function initModals(): void {
    // Open modal
    document.querySelectorAll('[data-modal-open]').forEach(trigger => {
        trigger.addEventListener('click', () => {
            const modalId = trigger.getAttribute('data-modal-open');
            if (modalId) {
                const modal = document.getElementById(modalId);
                if (modal) {
                    modal.classList.remove('hidden');
                    modal.setAttribute('aria-hidden', 'false');
                    document.body.style.overflow = 'hidden';
                }
            }
        });
    });
    
    // Close modal
    document.querySelectorAll('[data-modal-close]').forEach(button => {
        button.addEventListener('click', () => {
            const modal = button.closest('[data-modal]');
            if (modal) {
                modal.classList.add('hidden');
                modal.setAttribute('aria-hidden', 'true');
                document.body.style.overflow = '';
            }
        });
    });
}

document.addEventListener('DOMContentLoaded', initModals);
```

#### Deliverables
- [ ] Chart initialization logic
- [ ] Modal system
- [ ] Role-aware data fetching

---

## Technical Specifications

### Naming Convention

| Type | Convention | Example | Usage |
|------|------------|---------|-------|
| **Dashboard Widget** | `{Function}Widget` | `_StatWidget`, `_ChartWidget`, `_TableWidget` | Can be placed directly on a dashboard |
| **Building Block** | `{Function}` | `_AdminHeader`, `_Button`, `_Input` | Used within widgets or pages |

**Rules:**
- Widget suffix = Dashboard-placed component
- No suffix = Reusable building block
- Simplified names (no "Card", "Bar" redundancy)

### Widget Width System

**Note:** Height is handled separately via the Widget Height System (grid-row-span). This section covers width sizing only.

| Size | Width Class | Can Resize To | Notes |
|------|-------------|---------------|-------|
| **Small** | `w-full` (or fixed) | ❌ None | Cannot expand or increase |
| **Medium** | `w-full md:w-1/2` | ↗ Large | One next size only |
| **Large** | `w-full md:w-2/3` | ↗ Extra Large, ↘ Medium | Two sizes |
| **Extra Large** | `w-full` | ↘ Large | One next size only |

**Base Style (all widgets):**
```html
class="rounded-xl border border-secondary/10 shadow-sm"
```

**Link Widget Additional Styles:**
```html
class="hover:shadow-lg hover:border-primary/30 transition-all"
```

### Dashboard Layout Structure

```html
<!-- Main Links Section -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-10">
    <!-- Main link widgets go here -->
    <!-- Note: This section already exists on admin pages, may have different naming -->
</div>

<!-- Widget Board Section (up to 4 columns) -->
<div class="bg-white rounded-xl border border-secondary/10 shadow-sm">
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 p-6">
        <!-- Dashboard widgets go here (NO link widgets - use popup modals instead) -->
    </div>
</div>
```

### Modal System (Standardized)

**Standard Attributes:** All modals use `data-modal-open`, `data-modal-close`, and `data-modal` for consistency.

**Behavior:** Clicking action links opens a popup modal with blurred background. Falls back to redirect if JavaScript is disabled.

**1. Complete Modal Structure:**
```html
<!-- Backdrop with Blur (style exists in magic mode branch common CSS) -->
<div id="modal-new-workspace" 
     class="fixed inset-0 bg-black/30 backdrop-blur-sm flex items-center justify-center z-50 hidden"
     data-modal
     aria-modal="true"
     aria-hidden="true"
     role="dialog">
    <!-- Modal Container -->
    <div class="bg-white rounded-xl border border-secondary/10 shadow-sm p-6 max-w-md w-full relative">
        <!-- Close Button (Accessible) -->
        <button data-modal-close 
                class="absolute top-4 right-4 text-text/50 hover:text-text transition-colors"
                aria-label="Close modal">
            ×
        </button>
        
        <!-- Modal Content -->
        <h2 class="text-xl font-bold mb-4">Create New Workspace</h2>
        <!-- Form or content goes here -->
    </div>
</div>
```

**2. Trigger Elements:**
```html
<!-- Primary: Button with modal trigger -->
<button data-modal-open="modal-new-workspace" 
        class="btn btn-primary">
    New Workspace
</button>

<!-- Fallback for no-JS -->
<noscript>
    <a href="/admin/workspaces/create" class="btn btn-primary">New Workspace</a>
</noscript>
```

**3. JavaScript Handler (Complete):**
```typescript
// In modal.ts or dashboard.ts
export function initModals(): void {
    const openTriggers = document.querySelectorAll('[data-modal-open]');
    const closeTriggers = document.querySelectorAll('[data-modal-close]');
    const modals = document.querySelectorAll('[data-modal]');
    
    // Open modal
    openTriggers.forEach(trigger => {
        trigger.addEventListener('click', () => {
            const modalId = trigger.getAttribute('data-modal-open');
            const modal = document.getElementById(modalId);
            if (modal) {
                openModal(modal);
            }
        });
    });
    
    // Close modal (button click)
    closeTriggers.forEach(trigger => {
        trigger.addEventListener('click', () => {
            const modal = trigger.closest('[data-modal]');
            if (modal) {
                closeModal(modal);
            }
        });
    });
    
    // Close modal (backdrop click)
    modals.forEach(modal => {
        modal.addEventListener('click', (e) => {
            if (e.target === modal) {
                closeModal(modal);
            }
        });
    });
    
    // Close modal (ESC key)
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            const openModal = document.querySelector('[data-modal]:not(.hidden)');
            if (openModal) {
                closeModal(openModal as HTMLElement);
            }
        }
    });
    
    function openModal(modal: HTMLElement): void {
        modal.classList.remove('hidden');
        modal.setAttribute('aria-hidden', 'false');
        document.body.style.overflow = 'hidden';
        // Focus trap
        const focusable = modal.querySelector<HTMLElement>('button, input, [tabindex]:not([tabindex="-1"])');
        focusable?.focus();
    }
    
    function closeModal(modal: HTMLElement): void {
        modal.classList.add('hidden');
        modal.setAttribute('aria-hidden', 'true');
        document.body.style.overflow = '';
    }
}

document.addEventListener('DOMContentLoaded', initModals);
```

**Features:**
- ✅ Blurred backdrop (existing style from magic mode branch)
- ✅ Accessible (ARIA attributes, keyboard navigation)
- ✅ Multiple close methods (button, backdrop click, ESC key)
- ✅ Focus management
- ✅ No-JS fallback via `<noscript>`

**Existing Styles:** Blur backdrop style already available in magic mode branch common CSS files.

### Form Validation

**Client-Side (TypeScript):**
```typescript
// Validate form before submission
function validateForm(form: HTMLFormElement): boolean {
    let isValid = true;
    
    // Clear previous errors
    form.querySelectorAll('.field-error').forEach(el => el.remove());
    form.querySelectorAll('.field-invalid').forEach(el => el.classList.remove('field-invalid'));
    
    // Validate required fields
    form.querySelectorAll('[data-required]').forEach(field => {
        if (!field.value.trim()) {
            isValid = false;
            field.classList.add('field-invalid');
            const label = field.getAttribute('aria-label') || field.id;
            const error = document.createElement('div');
            error.className = 'field-error text-xs text-error mt-1';
            error.textContent = `${label} is required`;
            field.after(error);
        }
    });
    
    return isValid;
}

// Usage in modal forms
document.querySelectorAll('form[data-validate]').forEach(form => {
    form.addEventListener('submit', (e) => {
        if (!validateForm(form as HTMLFormElement)) {
            e.preventDefault();
        }
    });
});
```

**Server-Side (C#):**
```csharp
// In controllers - use ModelState validation
[HttpPost]
public async Task<IActionResult> CreateWorkspace(WorkspaceViewModel model)
{
    if (!ModelState.IsValid)
    {
        // Return with errors for modal to display
        return BadRequest(new {
            errors = ModelState.ToDictionary()
        });
    }
    // Process valid model...
}
```

**Error Display Pattern:**
```html
<!-- For API validation errors in modals -->
<div id="form-errors" class="bg-error/10 border border-error/20 text-error p-3 rounded mb-4 hidden">
    <ul class="list-disc list-inside"></ul>
</div>

<script>
// Handle API validation errors
if (response.errors) {
    const errorsContainer = document.getElementById('form-errors');
    const errorsList = errorsContainer.querySelector('ul');
    errorsList.innerHTML = '';
    Object.entries(response.errors).forEach(([field, messages]) => {
        (messages as string[]).forEach(msg => {
            const li = document.createElement('li');
            li.textContent = msg;
            errorsList.appendChild(li);
        });
    });
    errorsContainer.classList.remove('hidden');
}
</script>
```

### Typography

| Element | Style | Example |
|---------|-------|---------|
| **Widget Title** | `font-semibold text-text` | "Total Projects" |
| **Important Text / Small Title** | `font-semibold text-sm` | "42" |
| **Secondary Text** | `text-xs text-text/50 mt-1` | "Across all workspaces" |

### Widget Height System

**Principle:** Height is determined by content use, not fixed numbers. M/L/XL widgets must visually align with 3 stacked Small widgets.

| Size | Content Structure | Visual Height | Grid Row Span | Usage |
|------|-------------------|----------------|---------------|-------|
| **Small** | Label + Value + SubLabel | Content-determined | `row-span-1` | Stats, compact metrics |
| **Medium** | Title + Chart/Table | = 3 × Small | `row-span-3` | Charts, simple tables |
| **Large** | Title + Extended content | = 3 × Small | `row-span-3` | Detailed tables, forms |
| **Extra Large** | Title + Full content | = 3 × Small | `row-span-3` | Complex data, multi-section |

**Optimal Implementation:**

**1. CSS Custom Properties (content-based, not fixed numbers):**
```css
:root {
    /* Typical Small widget content height (based on actual content, not arbitrary) */
    --widget-height-unit: auto;
}

/* Small widgets: single row in grid */
.widget-size-s { grid-row: span 1; }

/* M/L/XL widgets: span 3 rows to match 3 Small widgets */
.widget-size-m,
.widget-size-l,
.widget-size-xl { grid-row: span 3; }
```

**2. Grid Container:**
```html
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 
            gap-4 auto-rows-min">
    <!-- Small widget (1 row) -->
    <div class="widget-size-s">@await Html.PartialAsync("_StatWidget", smallModel)</div>
    
    <!-- Medium widget (3 rows = 3 Small widgets) -->
    <div class="widget-size-m">@await Html.PartialAsync("_ChartWidget", mediumModel)</div>
</div>
```

**3. Widget Templates (content determines height):**
```html
<!-- Small widget - content flows naturally -->
<div class="bg-white rounded-xl border border-secondary/10 shadow-sm p-4">
    <span class="text-[10px] text-secondary uppercase font-bold tracking-wider">@Model.Label</span>
    <div class="text-2xl font-bold text-text mt-1">@Model.Value</div>
    @if (!string.IsNullOrEmpty(Model.SubLabel)){
        <p class="text-xs text-text/60 mt-1">@Model.SubLabel</p>
    }
</div>

<!-- Medium/Large/XL widget - content fills 3× Small height naturally -->
<div class="bg-white rounded-xl border border-secondary/10 shadow-sm p-4">
    <h2 class="font-semibold text-text mb-4">@Model.Title</h2>
    <!-- Content area designed to take ~3× Small widget height -->
    <div class="flex-1">@* Chart, table, or form content *@</div>
</div>
```

**4. ViewModel Size Property:**
```csharp
public enum WidgetSize { Small, Medium, Large, ExtraLarge }

// Each widget ViewModel has:
public WidgetSize Size { get; set; } = WidgetSize.Medium; // or appropriate default
```

**5. Rendering with Size:**
```cshtml
<div class="widget-size-@(Model.Size.ToString().ToLower())">
    @await Html.PartialAsync("_%Model.GetType().Name", Model)
</div>
```

**How it works:**
- Grid uses `auto-rows-min` to let content determine row height
- Small widgets naturally take 1 row
- M/L/XL widgets span 3 rows, forcing them to take 3× the height of a Small widget
- Content inside each widget determines actual height within those constraints
- No fixed pixel values - everything is content-driven with proportional relationships
=======

### Backend Requirements

| Requirement | Implementation | Location |
|-------------|----------------|----------|
| Role Detection | `AdminContext.CurrentAdmin` type check | All controllers |
| Workspace Filtering | `WorkspaceContext.CurrentWorkspace` | WorkspaceAdmin controllers |
| API Authorization | `[Authorize(Policy = ...)]` | All API controllers |
| Data Filtering | Service layer methods | BL/Administration/ |

### Frontend Requirements

| Requirement | Implementation | Location |
|-------------|----------------|----------|
| Chart Library | Chart.js | Package.json |
| TypeScript | ES Modules via Vite | Assets/*.ts |
| Responsive Design | Tailwind CSS Grid | All views |
| Modal System | Custom implementation | shared/modal.ts |

### Shared View Models

```csharp
// Stat Widget
public class StatWidgetViewModel
{
    public string Label { get; set; }
    public string Value { get; set; }
    public string? SubLabel { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public WidgetSize Size { get; set; } = WidgetSize.Small;
}

// Comparison Widget
public class ComparisonWidgetViewModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); // For DOM targeting
    public string Title { get; set; }           // "Active projects"
    public string SubTitle { get; set; }       // "All projects"
    public string TitleUrl { get; set; }        // Optional link for title click
    public string SubTitleUrl { get; set; }    // Optional link for subtitle click
    public List<ComparisonItemViewModel> Items { get; set; }
    public WidgetSize Size { get; set; } = WidgetSize.Medium;
}

public class ComparisonItemViewModel
{
    public string Label { get; set; }          // "Nmbs", "Stad Stabroek", "JC Goudbergen"
    public int Value { get; set; }              // 17, 13, 8
    public string Color { get; set; }          // "primary", "secondary", "accent" - for pill and circle
    public string? LegendIcon { get; set; }    // Optional icon for legend
}

// Chart Widget
public class ChartWidgetViewModel
{
    public string Title { get; set; }
    public string CanvasId { get; set; }
    public string Type { get; set; } // 'bar', 'line', 'bubble', etc.
    public object Data { get; set; }
    public object? Options { get; set; }
    public WidgetSize Size { get; set; } = WidgetSize.Medium;
}

// Action Widget
public class ActionWidgetViewModel
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public string Icon { get; set; }
    public string ActionText { get; set; } // Text for button/link
    public string ActionUrl { get; set; }
    public string ModalTarget { get; set; } // If opens modal
    public bool IsModal { get; set; }
    public WidgetSize Size { get; set; } = WidgetSize.Medium;
}

// Progress Widget
public class ProgressWidgetViewModel
{
    public string Label { get; set; }
    public int Percentage { get; set; }
    public string? Color { get; set; } // 'primary', 'success', 'warning', etc.
    public WidgetSize Size { get; set; } = WidgetSize.Small;
}

// Table Widget
public class TableWidgetViewModel
{
    public List<ColumnModel> Columns { get; set; }
    public List<RowModel> Rows { get; set; }
    public List<ActionModel> Actions { get; set; }
    public WidgetSize Size { get; set; } = WidgetSize.Medium;
}

// Supporting Models for TableWidget
public class ColumnModel
{
    public string Header { get; set; }
    public string Property { get; set; } // Property name in RowModel.Values
    public bool Sortable { get; set; } = false;
}

public class RowModel
{
    public Dictionary<string, object> Values { get; set; } = new();
    public string? Id { get; set; }
}

public class ActionModel
{
    public string Label { get; set; }
    public string Url { get; set; }
    public string? Icon { get; set; }
    public string? Class { get; set; } = "btn-sm btn-secondary";
}

// Widget Size Enum
// Height is content-determined, not fixed. M/L/XL span 3 grid rows = 3× Small widget height
public enum WidgetSize
{
    Small,    // row-span-1 - Cannot resize
    Medium,   // row-span-3 - Can resize to Large
    Large,    // row-span-3 - Can resize to Medium or ExtraLarge
    ExtraLarge // row-span-3 - Can resize to Large
}

// Dashboard (updated to match wireframe)
public class DashboardViewModel
{
    public string PageTitle { get; set; }
    public string PageDescription { get; set; }
    
    // Top navigation cards row (role-dependent)
    public List<NavCardViewModel> NavCards { get; set; } = new();
    
    // Main 3-column widget row
    public ComparisonWidgetViewModel ComparisonWidget { get; set; }
    public QuickLinksWidgetViewModel QuickLinksWidget { get; set; }
    public EngagementWidgetViewModel EngagementWidget { get; set; }
    
    // Full-width chart (usage trend with period tabs)
    public ChartWidgetViewModel UsageTrendChart { get; set; }
    
    // Optional: additional stat widgets (for future expansion)
    public List<StatWidgetViewModel> StatWidgets { get; set; } = new();
}

// Navigation Card (building block — NOT a widget, navigates directly)
public class NavCardViewModel
{
    public string Title { get; set; }           // "AI Settings", "Workspaces"
    public string Description { get; set; }     // "Configure providers, models, prompts..."
    public string Icon { get; set; }            // Icon name or SVG path
    public string NavigateUrl { get; set; }     // Direct navigation URL (no modal)
    public string? IconBackground { get; set; } // Tailwind bg class, e.g. "bg-primary/10"
}

// Quick Links Widget (grouped container — distinct from individual _LinksWidget)
public class QuickLinksWidgetViewModel
{
    public string? Title { get; set; }           // Optional header e.g. "Links"
    public List<QuickLinkItemViewModel> Items { get; set; } = new();
    public WidgetSize Size { get; set; } = WidgetSize.Medium;
}

public class QuickLinkItemViewModel
{
    public string Title { get; set; }           // "New workspace", "Check health", "Rate Limits"
    public string Description { get; set; }     // "Create a new workspace", "Check Ai provider"
    public string Icon { get; set; }            // Icon name or SVG
    public string IconBackground { get; set; }  // Tailwind bg class e.g. "bg-yellow-100"
    public string IconColor { get; set; }       // Tailwind text class e.g. "text-yellow-500"
    public string? ModalTarget { get; set; }    // Open modal if set
    public string NavigateUrl { get; set; }     // Fallback URL
    public bool IsModal => !string.IsNullOrEmpty(ModalTarget);
}

// Engagement Widget (circular gauge + labeled progress bars) — NEW
public class EngagementWidgetViewModel
{
    public string Title { get; set; } = "Weekly Engagement Rate";
    
    // Circular gauge
    public int GaugePercentage { get; set; }    // 0–100 (e.g. 67)
    public string GaugeLabel { get; set; }      // "Full Completion Rate"
    public string GaugeColor { get; set; } = "text-yellow-400"; // Tailwind color class
    
    // Progress bars (below gauge)
    public List<EngagementBarViewModel> Bars { get; set; } = new();
    
    public WidgetSize Size { get; set; } = WidgetSize.Medium;
}

public class EngagementBarViewModel
{
    public string Label { get; set; }           // "Response Rate", "Toxicity", "Idea Contributions"
    public string SubLabel { get; set; }        // "average responses per survey", "safe content ratio"
    public int Current { get; set; }            // 14, 8 (numerator)
    public int Max { get; set; }                // 20, 10 (denominator)
    public string DisplayValue { get; set; }    // "14/20", "8,5/10", "1290 / 1954"
    public string Color { get; set; } = "bg-green-500"; // Tailwind class; orange if near capacity
    
    // Computed percentage for bar fill
    public int Percentage => Max > 0 ? (int)Math.Round((double)Current / Max * 100) : 0;
}
```

---

## Widget Types

### 1. Comparison Widget (`_ComparisonWidget`)

**Purpose:** Visual comparison of amounts across multiple items using proportionally-sized circles arranged in a radial pattern.

**Structure:**
```
+-------------------------------------+
| Title (button)                     |  
| Subtitle (button, faded)           |  
|                                     |  
| Legend:                           |  
| ● Item A                          |  ← Left side
| ● Item B                          |  
| ● Item C                          |  
+-------------------------------------+
         ↑                          ↑    
    Left panel                 Right panel
                                  (centered ensemble)
                                    ●   ●
                                   / \ / \
                                  ●   ●   ← Circles form equilateral △
```

**Example:** Comparing 3 workspaces - Nmbs (17 projects), Stad Stabroek (13), JC Goudbergen (8)

**Behavior:**
- Title and subtitle are clickable buttons with hover effects
- Legend items (colored pill + text) fade others on hover
- Circles are positioned in radial pattern (equilateral triangle for 3, square for 4, etc.)
- Entire circle ensemble is centered in the right-side space
- Circle sizes are proportional to √(value) for visual area accuracy

**ViewModel:**
```csharp
public class ComparisonWidgetViewModel
{
    public string Title { get; set; }           // "Active projects"
    public string SubTitle { get; set; }       // "All projects" 
    public string TitleUrl { get; set; }        // Optional link for title click
    public string SubTitleUrl { get; set; }    // Optional link for subtitle click
    public List<ComparisonItemViewModel> Items { get; set; }
    public WidgetSize Size { get; set; } = WidgetSize.Medium;
}

public class ComparisonItemViewModel
{
    public string Label { get; set; }          // "Nmbs", "Stad Stabroek", "JC Goudbergen"
    public int Value { get; set; }              // 17, 13, 8
    public string Color { get; set; }          // "primary", "secondary", "accent" - for pill and circle
    public string? LegendIcon { get; set; }    // Optional icon for legend
}
```

**Template (`_ComparisonWidget.cshtml`):**
```cshtml
@model ComparisonWidgetViewModel

<div class="bg-white rounded-xl border border-secondary/10 shadow-sm p-4">
    <div class="flex flex-col md:flex-row gap-6 h-full">
        <!-- Left Panel: Titles + Legend -->
        <div class="flex flex-col justify-between min-w-[180px]">
            <!-- Title (button) -->
            <a href="@Model.TitleUrl" 
               class="font-semibold text-text hover:text-primary transition-colors">
                @Model.Title
            </a>
            
            <!-- Subtitle (button, faded) -->
            <a href="@Model.SubTitleUrl"
               class="font-semibold text-text/50 hover:text-text/80 transition-colors mt-1">
                @Model.SubTitle
            </a>
            
            <!-- Legend -->
            <div class="mt-8 space-y-3 legend-container" data-comparison-widget="@Model.Id">
                @foreach (var item in Model.Items)
                {
                    <div class="flex items-center gap-2 legend-item group cursor-pointer"
                         data-item-id="@item.Label">
                        <!-- Colored pill -->
                        <div class="w-3 h-3 rounded-full bg-[@item.Color] group-hover:opacity-100 
                                    transition-opacity opacity-35"></div>
                        <span class="text-sm text-text/70 group-hover:text-text transition-colors">
                            @item.Label
                        </span>
                    </div>
                }
            </div>
        </div>
        
        <!-- Right Panel: Radial Circles -->
        <div class="flex-1 flex items-center justify-center relative circle-container">
            @{ 
                int count = Model.Items.Count;
                double angleStep = 360.0 / count;
                double radius = 100; // Distance from center to circle centers
            }
            
            @foreach (var item in Model.Items)
            {
                int index = Model.Items.IndexOf(item);
                double angle = angleStep * index;
                double rad = angle * Math.PI / 180.0;
                double x = Math.Cos(rad) * radius;
                double y = Math.Sin(rad) * radius;
                
                // Circle radius proportional to sqrt(value) for area accuracy
                double maxValue = Model.Items.Max(i => i.Value);
                double circleRadius = 20 + (60 * Math.Sqrt(item.Value) / Math.Sqrt(maxValue));
                
                <div class="absolute circle-item" 
                     style="left: 50%; top: 50%; transform: translate(-50%, -50%) translate(@(x)px, @(y)px)"
                     data-item-id="@item.Label">
                    <div class="rounded-full bg-[@item.Color]/20 border-2 border-[@item.Color] 
                                flex items-center justify-center shadow-md
                                transition-all hover:shadow-lg hover:scale-105"
                         style="width: @(circleRadius*2)px; height: @(circleRadius*2)px">
                        <span class="font-semibold text-text">@item.Value</span>
                    </div>
                </div>
            }
        </div>
    </div>
</div>

<script>
// Bidirectional hover effect: legend ↔ circles
document.querySelectorAll('[data-comparison-widget]').forEach(container => {
    const widget = container.closest('.bg-white');
    const legendItems = container.querySelectorAll('.legend-item');
    const circleItems = widget.querySelectorAll('.circle-item');
    
    // Unified hover handler for both legend items and circles
    const handleHover = (hoveredItemId, isHover = true) => {
        const opacity = isHover ? '0.35' : '';
        
        // Fade legend items
        legendItems.forEach(item => {
            if (item.getAttribute('data-item-id') !== hoveredItemId) {
                item.style.opacity = opacity;
            }
        });
        
        // Fade circles
        circleItems.forEach(circle => {
            if (circle.getAttribute('data-item-id') !== hoveredItemId) {
                circle.style.opacity = opacity;
            }
        });
    };
    
    // Legend item hover
    legendItems.forEach(item => {
        const itemId = item.getAttribute('data-item-id');
        item.addEventListener('mouseenter', () => handleHover(itemId, true));
        item.addEventListener('mouseleave', () => handleHover(itemId, false));
    });
    
    // Circle hover (bidirectional)
    circleItems.forEach(circle => {
        const circleId = circle.getAttribute('data-item-id');
        circle.addEventListener('mouseenter', () => handleHover(circleId, true));
        circle.addEventListener('mouseleave', () => handleHover(circleId, false));
    });
});
</script>
```

**CSS (recommended in common styles):**
```css
/* Smooth transitions for hover effects */
.legend-item, .circle-item {
    transition: opacity 0.25s ease, transform 0.25s ease;
}

/* Circle hover: subtle scale up */
.circle-item:hover .rounded-full {
    transform: scale(1.05);
}
```

**Key Design Decisions:**
- **Circle Sizing:** `radius = 20 + (60 * √(value) / √(maxValue))` ensures proportional areas while maintaining minimum size
- **Radial Distance:** Fixed `100px` from center - adjust based on widget size
- **Angular Spacing:** `360° / count` creates regular polygon arrangement (equilateral △ for 3, □ for 4)
- **Hover Effect:** 35% opacity for non-hovered items, **bidirectional** - hovering legend item OR circle fades the others
- **Centering:** Entire circle ensemble centered via `left: 50%; top: 50%; transform: translate(-50%, -50%)`

**Data Example:**
```csharp
new ComparisonWidgetViewModel
{
    Title = "Active projects",
    SubTitle = "All projects",
    TitleUrl = "/admin/projects",
    SubTitleUrl = "/admin/projects?filter=all",
    Size = WidgetSize.Medium,
    Items = new List<ComparisonItemViewModel>
    {
        new ComparisonItemViewModel { Label = "Nmbs", Value = 17, Color = "primary" },
        new ComparisonItemViewModel { Label = "Stad Stabroek", Value = 13, Color = "secondary" },
        new ComparisonItemViewModel { Label = "JC Goudbergen", Value = 8, Color = "accent" }
    }
}
```

---

### 2. Links Widget (`_LinksWidget`)

**Purpose:** Display a collection of links as reusable components that open popup modals for creating/editing entities. Follows SOLID principles with type-safe TypeScript implementation.

**Reusability Principle:**
- Same component structure works for different entity types (workspaces, projects, users, etc.)
- Data filtering handled by backend based on admin role
- No JavaScript required (fallback to redirect), enhanced with modals

**ViewModel:**
```csharp
public class LinkWidgetViewModel
{
    public string Title { get; set; }           // "Workspaces", "Projects", "Users"
    public string Icon { get; set; }            // Icon class or SVG
    public string ModalTarget { get; set; }     // ID of modal to open (e.g., "modal-new-workspace")
    public string CreateUrl { get; set; }       // Fallback URL for no-JS: "/admin/workspaces/create"
    public WidgetSize Size { get; set; } = WidgetSize.Small;
}
```

**Simple Template (`_LinksWidget.cshtml`):**
```cshtml
@model LinkWidgetViewModel

<div class="bg-white rounded-xl border border-secondary/10 shadow-sm hover:shadow-lg 
            hover:border-primary/30 transition-all p-4 cursor-pointer group"
     onclick="window.location.href='@Model.CreateUrl'"
     data-modal-open="@Model.ModalTarget"
     data-link-widget="@Model.ModalTarget">
    <div class="flex items-center gap-3">
        <div class="text-xl text-primary">@Model.Icon</div>
        <span class="font-semibold text-text group-hover:text-primary transition-colors">
            @Model.Title
        </span>
    </div>
    <div class="text-xs text-text/50 mt-1">Click to create new</div>
</div>
```

**SOLID-Conform TypeScript Implementation:**

**Interfaces (Abstractions - Open/Closed & Dependency Inversion):**
```typescript
// IContentLoader.ts - Loads content for modal (fetch or DOM)
export interface IContentLoader {
    loadContent(url: string): Promise<string>;
}

// IModalManager.ts - Manages modal state (open/close)
export interface IModalManager {
    open(modalId: string, content: string): void;
    close(modalId: string): void;
}

// IFormHandler.ts - Handles form submission
export interface IFormHandler {
    handleSubmit(form: HTMLFormElement, url: string): Promise<void>;
}

// ILinkWidgetConfig.ts - Configuration for link widget
export interface ILinkWidgetConfig {
    modalTarget: string;
    createUrl: string;
    contentUrl?: string;
    onContentLoaded?: (content: string) => void;
}
```

**Concrete Implementations (Single Responsibility):**

```typescript
// HttpContentLoader.ts - Implements IContentLoader for HTTP fetching
export class HttpContentLoader implements IContentLoader {
    async loadContent(url: string): Promise<string> {
        const response = await fetch(url, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'Accept': 'text/html'
            }
        });
        
        if (!response.ok) {
            throw new Error(`Failed to load content: ${response.status}`);
        }
        
        return response.text();
    }
}

// DomContentLoader.ts - Implements IContentLoader for DOM selection (caching)
export class DomContentLoader implements IContentLoader {
    private cache: Map<string, string> = new Map();
    
    async loadContent(selector: string): Promise<string> {
        if (this.cache.has(selector)) {
            return this.cache.get(selector)!;
        }
        
        const element = document.querySelector(selector);
        if (!element) {
            throw new Error(`Element not found: ${selector}`);
        }
        
        const content = element.innerHTML;
        this.cache.set(selector, content);
        return content;
    }
}

// ModalManager.ts - Implements IModalManager
export class ModalManager implements IModalManager {
    private activeModals: Set<string> = new Set();
    
    open(modalId: string, content: string): void {
        const modal = document.getElementById(modalId);
        if (!modal) {
            console.warn(`Modal not found: ${modalId}`);
            return;
        }
        
        const contentContainer = modal.querySelector('[data-modal-content]');
        if (contentContainer) {
            contentContainer.innerHTML = content;
        }
        
        modal.classList.remove('hidden');
        modal.setAttribute('aria-hidden', 'false');
        document.body.style.overflow = 'hidden';
        this.activeModals.add(modalId);
        
        // Focus first focusable element
        const focusable = modal.querySelector<HTMLElement>('button, input, [tabindex]:not([tabindex="-1"])');
        focusable?.focus();
    }
    
    close(modalId: string): void {
        const modal = document.getElementById(modalId);
        if (!modal) return;
        
        modal.classList.add('hidden');
        modal.setAttribute('aria-hidden', 'true');
        document.body.style.overflow = '';
        this.activeModals.delete(modalId);
    }
}

// FormHandler.ts - Implements IFormHandler
export class FormHandler implements IFormHandler {
    async handleSubmit(form: HTMLFormElement, url: string): Promise<void> {
        const formData = new FormData(form);
        
        try {
            const response = await fetch(url, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });
            
            if (!response.ok) {
                throw new Error(`Form submission failed: ${response.status}`);
            }
            
            // Handle success (close modal, refresh, etc.)
            const modal = form.closest('[data-modal]');
            if (modal) {
                (modal as HTMLElement).classList.add('hidden');
            }
            
            // Refresh page or update UI
            window.location.reload();
        } catch (error) {
            console.error('Form submission error:', error);
            // Display error to user
        }
    }
}

// LinkWidgetHandler.ts - Orchestrates link widget behavior (Liskov Substitution)
export class LinkWidgetHandler {
    constructor(
        private contentLoader: IContentLoader,
        private modalManager: IModalManager,
        private formHandler: IFormHandler
    ) {}
    
    async handleClick(event: MouseEvent, config: ILinkWidgetConfig): Promise<void> {
        // Only handle left clicks
        if (event.button !== 0) return;
        
        event.preventDefault();
        
        try {
            // Load content for modal
            let content = '';
            if (config.contentUrl) {
                content = await this.contentLoader.loadContent(config.contentUrl);
            }
            
            // Open modal with content
            this.modalManager.open(config.modalTarget, content);
            
            // Set up form handler for this modal
            const modal = document.getElementById(config.modalTarget);
            const form = modal?.querySelector('form');
            if (form) {
                form.addEventListener('submit', (e) => {
                    e.preventDefault();
                    this.formHandler.handleSubmit(form as HTMLFormElement, config.createUrl);
                });
            }
        } catch (error) {
            console.error('Link widget error:', error);
            // Fallback: redirect to create URL
            window.location.href = config.createUrl;
        }
    }
}
```

**Initialization Code:**
```typescript
// linkWidget.ts - Main initialization
import { HttpContentLoader } from './HttpContentLoader';
import { ModalManager } from './ModalManager';
import { FormHandler } from './FormHandler';
import { LinkWidgetHandler } from './LinkWidgetHandler';

export function initLinkWidgets(): void {
    // Create instances with dependencies injected
    const contentLoader: IContentLoader = new HttpContentLoader();
    const modalManager: IModalManager = new ModalManager();
    const formHandler: IFormHandler = new FormHandler();
    const widgetHandler = new LinkWidgetHandler(contentLoader, modalManager, formHandler);
    
    // Find all link widget triggers
    document.querySelectorAll('[data-link-widget]').forEach(trigger => {
        const modalTarget = trigger.getAttribute('data-link-widget');
        const createUrl = trigger.getAttribute('data-create-url') || '';
        const contentUrl = trigger.getAttribute('data-content-url');
        
        if (!modalTarget) return;
        
        trigger.addEventListener('click', (e) => {
            e.preventDefault();
            widgetHandler.handleClick(e, { modalTarget, createUrl, contentUrl });
        });
    });
}

// Call on DOM ready
document.addEventListener('DOMContentLoaded', initLinkWidgets);
```

**Required Modal HTML Structure:**
```html
<!-- Modal for link widget (e.g., New Workspace) -->
<div id="modal-new-workspace"
     class="fixed inset-0 bg-black/30 backdrop-blur-sm flex items-center 
            justify-center z-50 hidden"
     data-modal
     aria-modal="true"
     aria-hidden="true"
     role="dialog">
    <div class="bg-white rounded-xl border border-secondary/10 shadow-sm p-6 
                max-w-md w-full relative">
        <!-- Close Button -->
        <button data-modal-close
                class="absolute top-4 right-4 text-text/50 hover:text-text"
                aria-label="Close modal">
            ×
        </button>
        
        <!-- Content Container (loaded dynamically) -->
        <div data-modal-content>
            <!-- Content will be loaded here by LinkWidgetHandler -->
        </div>
    </div>
</div>
```

**Backend Support (Optional - X-Request-Type Header Handling):**
```csharp
// In controllers - detect AJAX requests for modal content
[HttpGet]
public async Task<IActionResult> CreateWorkspace()
{
    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
    {
        // Return partial view for modal
        return PartialView("_WorkspaceForm");
    }
    
    // Return full view for direct navigation
    return View();
}
```

**Usage Example:**
```cshtml
<!-- In dashboard view -->
@await Html.PartialAsync("_LinksWidget", new LinkWidgetViewModel
{
    Title = "New Workspace",
    Icon = "<svg>...</svg>",
    ModalTarget = "modal-new-workspace",
    CreateUrl = "/admin/workspaces/create",
    Size = WidgetSize.Small
})

@await Html.PartialAsync("_LinksWidget", new LinkWidgetViewModel
{
    Title = "New Project",
    Icon = "<svg>...</svg>",
    ModalTarget = "modal-new-project",
    CreateUrl = "/admin/projects/create",
    Size = WidgetSize.Small
})
```

**SOLID Compliance:**

| Principle | Implementation | Verification |
|-----------|----------------|--------------|
| **S**ingle Responsibility | Each class has one job: ContentLoader loads, ModalManager manages, FormHandler handles forms, LinkWidgetHandler orchestrates | ✅ Each class does exactly one thing |
| **O**pen/Closed | Extend by adding new implementations of interfaces, not modifying existing code | ✅ Can add `CacheContentLoader` without touching existing code |
| **L**iskov Substitution | Any `IContentLoader` can replace another (HttpContentLoader ↔ DomContentLoader) | ✅ Both implement same interface, used interchangeably |
| **I**nterface Segregation | Small, focused interfaces (4 interfaces, each with 1-3 methods) | ✅ No "god" interfaces, clients depend only on what they need |
| **D**ependency Inversion | High-level `LinkWidgetHandler` depends on abstractions (`IContentLoader`, `IModalManager`, `IFormHandler`), not concretions | ✅ Dependencies injected via constructor, easy to mock for testing |

---

### 3. Nav Card (`_NavCard`) — Building Block

**Purpose:** Top-of-dashboard navigation cards linking to admin sections. NOT a widget — placed in a dedicated nav row above the widget board. Clicking navigates directly (no modal).

**Differs from `_LinksWidget`:** Nav cards live outside the widget grid, always navigate (no modal), and are always rendered as a 2-column row.

**ViewModel:**
```csharp
public class NavCardViewModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public string NavigateUrl { get; set; }
    public string? IconBackground { get; set; } = "bg-primary/10";
}
```

**Template (`_NavCard.cshtml`):**
```cshtml
@model NavCardViewModel

<a href="@Model.NavigateUrl"
   class="bg-white rounded-xl border border-secondary/10 shadow-sm p-5
          flex items-center gap-4 hover:shadow-md hover:border-primary/20
          transition-all group">
    <div class="w-12 h-12 rounded-xl @(Model.IconBackground ?? "bg-primary/10")
                flex items-center justify-center flex-shrink-0">
        @* Render icon by name or SVG *@
        <span class="text-primary text-xl">@Model.Icon</span>
    </div>
    <div>
        <span class="font-semibold text-text group-hover:text-primary transition-colors block">
            @Model.Title
        </span>
        <span class="text-xs text-text/50 mt-0.5 block">@Model.Description</span>
    </div>
</a>
```

**Role usage:**

| Admin Type | Card 1 | Card 2 |
|------------|--------|--------|
| Conversey Admin | AI Settings → `/admin/ai` | Workspaces → `/admin/workspaces` |
| Workspace Admin | Projects → `/admin/projects` | AI Settings → `/admin/{slug}/ai` |

---

### 4. Quick Links Widget (`_QuickLinksWidget`) — NEW

**Purpose:** Grouped container of actionable links displayed as a stacked list inside one widget card. Distinct from individual `_LinksWidget` items — this is the complete widget holding all link rows together.

**Visual from wireframe:**
```
┌──────────────────────────────────┐
│  [🟡 +]  New workspace           │
│          Create a new workspace  │
├──────────────────────────────────┤
│  [🩷 ♥]  Check health            │
│          Check Ai provider       │
├──────────────────────────────────┤
│  [🩷 🔒]  Rate Limits            │
│          Limits for AI endpoints │
└──────────────────────────────────┘
```

**ViewModel:** (see Shared View Models section — `QuickLinksWidgetViewModel` + `QuickLinkItemViewModel`)

**Template (`_QuickLinksWidget.cshtml`):**
```cshtml
@model QuickLinksWidgetViewModel

<div class="bg-white rounded-xl border border-secondary/10 shadow-sm overflow-hidden">
    @if (!string.IsNullOrEmpty(Model.Title))
    {
        <div class="px-5 pt-4 pb-2">
            <h3 class="font-semibold text-text/50 text-xs uppercase tracking-wider">@Model.Title</h3>
        </div>
    }
    
    @foreach (var (item, index) in Model.Items.Select((item, i) => (item, i)))
    {
        bool isLast = index == Model.Items.Count - 1;
        string borderClass = isLast ? "" : "border-b border-secondary/10";
        
        if (item.IsModal)
        {
            <button data-modal-open="@item.ModalTarget"
                    class="w-full text-left flex items-center gap-4 px-5 py-4
                           hover:bg-secondary/5 transition-colors group @borderClass">
                <div class="w-11 h-11 rounded-xl @item.IconBackground flex items-center
                            justify-center flex-shrink-0">
                    <span class="@item.IconColor text-lg">@item.Icon</span>
                </div>
                <div>
                    <span class="font-semibold text-text group-hover:text-primary 
                                 transition-colors block text-sm">@item.Title</span>
                    <span class="text-xs text-text/50 mt-0.5 block">@item.Description</span>
                </div>
            </button>
        }
        else
        {
            <a href="@item.NavigateUrl"
               class="flex items-center gap-4 px-5 py-4
                      hover:bg-secondary/5 transition-colors group @borderClass">
                <div class="w-11 h-11 rounded-xl @item.IconBackground flex items-center
                            justify-center flex-shrink-0">
                    <span class="@item.IconColor text-lg">@item.Icon</span>
                </div>
                <div>
                    <span class="font-semibold text-text group-hover:text-primary
                                 transition-colors block text-sm">@item.Title</span>
                    <span class="text-xs text-text/50 mt-0.5 block">@item.Description</span>
                </div>
            </a>
        }
    }
</div>
```

**Data Example (Conversey Admin):**
```csharp
new QuickLinksWidgetViewModel
{
    Items = new List<QuickLinkItemViewModel>
    {
        new QuickLinkItemViewModel
        {
            Title = "New workspace",
            Description = "Create a new workspace",
            Icon = "+",
            IconBackground = "bg-yellow-100",
            IconColor = "text-yellow-500",
            ModalTarget = "modal-new-workspace",
            NavigateUrl = "/admin/workspaces/create"
        },
        new QuickLinkItemViewModel
        {
            Title = "Check health",
            Description = "Check Ai provider",
            Icon = "♥",
            IconBackground = "bg-pink-100",
            IconColor = "text-pink-400",
            NavigateUrl = "/admin/ai/health"
        },
        new QuickLinkItemViewModel
        {
            Title = "Rate Limits",
            Description = "Limits for AI endpoints",
            Icon = "🔒",
            IconBackground = "bg-pink-100",
            IconColor = "text-pink-400",
            NavigateUrl = "/admin/ai/rate-limits"
        }
    }
}
```

---

### 5. Engagement Widget (`_EngagementWidget`) — NEW

**Purpose:** Displays platform/workspace engagement metrics with a circular gauge at the top and labeled progress bars below. Corresponds to the "Weekly Engagement Rate" card in the wireframe (Scores area).

**Visual structure:**
```
┌─────────────────────────────┐
│  Weekly Engagement Rate     │
│                             │
│      ╭────────────╮         │
│    ╭─╯    67%     ╰─╮       │  ← SVG arc gauge (yellow/gold)
│    │   Full Comple-  │       │
│    ╰─────────────────╯       │
│                             │
│  Response Rate   avg/survey │
│  [████████████] 14/20       │  ← green bar, value shown inside
│                             │
│  Toxicity       safe ratio  │
│  [████████████] 8.5/10      │  ← green bar
│                             │
│  Idea Contributions  sub rt │
│  [████████     ] 1290/1954  │  ← orange bar (partial fill)
└─────────────────────────────┘
```

**ViewModel:** (see Shared View Models — `EngagementWidgetViewModel` + `EngagementBarViewModel`)

**Template (`_EngagementWidget.cshtml`):**
```cshtml
@model EngagementWidgetViewModel

@{
    // SVG arc gauge parameters
    double radius = 40;
    double cx = 50; double cy = 50;
    double circumference = Math.PI * radius; // half-circle arc
    double strokeOffset = circumference * (1 - Model.GaugePercentage / 100.0);
}

<div class="bg-white rounded-xl border border-secondary/10 shadow-sm p-5">
    <h3 class="font-semibold text-text mb-4">@Model.Title</h3>
    
    {{!-- Circular Gauge --}}
    <div class="flex flex-col items-center mb-6">
        <div class="relative w-28 h-16">
            <svg viewBox="0 0 100 60" class="w-full h-full" aria-hidden="true">
                {{!-- Track arc --}}
                <path d="M 10 50 A 40 40 0 0 1 90 50"
                      fill="none" stroke="currentColor"
                      class="text-secondary/15"
                      stroke-width="10" stroke-linecap="round"/>
                {{!-- Filled arc --}}
                <path d="M 10 50 A 40 40 0 0 1 90 50"
                      fill="none"
                      class="@Model.GaugeColor"
                      stroke="currentColor"
                      stroke-width="10" stroke-linecap="round"
                      stroke-dasharray="@circumference"
                      stroke-dashoffset="@strokeOffset"/>
            </svg>
            <div class="absolute inset-0 flex flex-col items-center justify-end pb-1">
                <span class="text-xl font-bold text-text leading-none">@Model.GaugePercentage%</span>
            </div>
        </div>
        <span class="text-xs text-text/50 mt-1">@Model.GaugeLabel</span>
    </div>
    
    {{!-- Progress Bars --}}
    <div class="space-y-4">
        @foreach (var bar in Model.Bars)
        {
            <div>
                <div class="flex justify-between text-xs mb-1">
                    <span class="font-semibold text-text/70">@bar.Label</span>
                    <span class="text-text/40">@bar.SubLabel</span>
                </div>
                <div class="relative w-full h-5 bg-secondary/10 rounded-full overflow-hidden">
                    <div class="h-full rounded-full @bar.Color transition-all duration-500"
                         style="width: @bar.Percentage%">
                    </div>
                    <span class="absolute inset-0 flex items-center justify-center
                                 text-[10px] font-semibold text-white mix-blend-difference">
                        @bar.DisplayValue
                    </span>
                </div>
            </div>
        }
    </div>
</div>
```

**Data Example:**
```csharp
new EngagementWidgetViewModel
{
    Title = "Weekly Engagement Rate",
    GaugePercentage = 67,
    GaugeLabel = "Full Completion Rate",
    GaugeColor = "text-yellow-400",
    Bars = new List<EngagementBarViewModel>
    {
        new EngagementBarViewModel
        {
            Label = "Response Rate",
            SubLabel = "average responses per survey",
            Current = 14, Max = 20,
            DisplayValue = "14/20",
            Color = "bg-green-500"
        },
        new EngagementBarViewModel
        {
            Label = "Toxicity",
            SubLabel = "safe content ratio",
            Current = 85, Max = 100, // 8.5/10 → 85/100
            DisplayValue = "8,5/10",
            Color = "bg-green-500"
        },
        new EngagementBarViewModel
        {
            Label = "Idea Contributions",
            SubLabel = "ideas submission rate",
            Current = 1290, Max = 1954,
            DisplayValue = "1290 / 1954",
            Color = "bg-orange-500"
        }
    }
}
```

**API endpoint for engagement data:**
```csharp
// GET /api/admin/stats/engagement
// Returns: fullCompletionRate, bars[]
// Role-filtered: Conversey Admin → platform-wide; Workspace Admin → workspace scoped
```

### Available Components

| Component | File | Purpose | Props | Default Size |
|-----------|------|---------|-------|--------------|
| `_ComparisonWidget` | `/Shared/Admin/Dashboard/_ComparisonWidget.cshtml` | Proportional circle comparison | Title, SubTitle, Items(Label, Value, Color) | Medium |
| `_QuickLinksWidget` | `/Shared/Admin/Dashboard/_QuickLinksWidget.cshtml` | **NEW** — Grouped action link list (modal or navigate) | Title?, Items[] | Medium |
| `_EngagementWidget` | `/Shared/Admin/Dashboard/_EngagementWidget.cshtml` | **NEW** — Circular gauge + labeled progress bars | Title, GaugePercentage, GaugeLabel, Bars[] | Medium |
| `_LinksWidget` | `/Shared/Admin/Dashboard/_LinksWidget.cshtml` | Individual modal link card (building block) | Title, Icon, ModalTarget, CreateUrl | Small |
| `_StatWidget` | `/Shared/Admin/Dashboard/_StatWidget.cshtml` | Display metric | Label, Value, SubLabel, Icon, Color | Small |
| `_ChartWidget` | `/Shared/Admin/Dashboard/_ChartWidget.cshtml` | Container for charts (supports period tabs) | Title, CanvasId, Type, Data, Options, Periods? | Medium |
| `_ActionWidget` | `/Shared/Admin/Dashboard/_ActionWidget.cshtml` | Quick action button | Title, Description, Icon, ActionText, ActionUrl, ModalTarget, IsModal | Medium |
| `_ProgressWidget` | `/Shared/Admin/Dashboard/_ProgressWidget.cshtml` | Visual progress | Label, Percentage, Color | Small |
| `_TableWidget` | `/Shared/Admin/Dashboard/_TableWidget.cshtml` | Data grid | Columns, Rows, Actions | Medium |
| `_NavCard` | `/Shared/Admin/_NavCard.cshtml` | **NEW** — Top nav card (direct nav, not a widget) | Title, Description, Icon, NavigateUrl | N/A |
| `_AdminModal` | `/Shared/Admin/Modals/_AdminModal.cshtml` | Modal dialog | Title, Content, Actions | N/A |

### Usage Examples

**Stat Widget (Small by default):**
```cshtml
@await Html.PartialAsync("_StatWidget", new StatWidgetViewModel
{
    Label = "Total Projects",
    Value = "42",
    SubLabel = "Across all workspaces",
    Icon = "project-icon",
    Color = "primary",
    Size = WidgetSize.Small // Optional - Small is default
})
```

**Chart Widget (Medium by default):**
```cshtml
@await Html.PartialAsync("_ChartWidget", new ChartWidgetViewModel
{
    Title = "Usage Trend",
    CanvasId = "usage-trend-chart",
    Type = "line",
    Size = WidgetSize.Medium, // Optional - Medium is default
    Data = new {
        labels = new[] { "Jan", "Feb", "Mar" },
        datasets = new[] { new { label = "Visits", data = new[] { 100, 200, 150 } } }
    }
})
```

**Action Widget with Modal:**
```cshtml
@await Html.PartialAsync("_ActionWidget", new ActionWidgetViewModel
{
    Title = "Create New",
    Description = "Add a new workspace to your account",
    Icon = "plus-icon",
    ActionText = "New Workspace",
    ActionUrl = "/admin/workspaces/create",
    ModalTarget = "modal-new-workspace",
    IsModal = true,
    Size = WidgetSize.Medium
})
```

**Table Widget:**
```cshtml
@await Html.PartialAsync("_TableWidget", new TableWidgetViewModel
{
    Size = WidgetSize.Medium,
    Columns = new List<ColumnModel>
    {
        new ColumnModel { Header = "Name", Property = "name", Sortable = true },
        new ColumnModel { Header = "Status", Property = "status" }
    },
    Rows = new List<RowModel>
    {
        new RowModel { Id = "1", Values = new Dictionary<string, object> { ["name"] = "Project A", ["status"] = "Active" } }
    },
    Actions = new List<ActionModel>
    {
        new ActionModel { Label = "Edit", Url = "/edit/1" },
        new ActionModel { Label = "Delete", Url = "/delete/1", Class = "btn-sm btn-danger" }
    }
})
```

---

## Data Flow

```
User Request
     ↓
[Controller] Detect Admin Type (Conversey/Workspace)
     ↓
[Service Layer] Filter Data by Admin Scope
     ↓
[View Model] Prepare Role-Specific Data
     ↓
[View] Render Component with Filtered Data
     ↓
[TypeScript] Initialize Interactive Elements (Charts, Modals)
     ↓
User Sees Role-Appropriate Dashboard
```

### Data Filtering Logic

```csharp
// Example: GetDashboardStats
public async Task<DashboardViewModel> GetDashboardStats(Admin admin)
{
    var statWidgets = new List<StatWidgetViewModel>();
    var charts = new List<ChartWidgetViewModel>();
    
    if (admin is ConverseyAdmin)
    {
        // Platform-wide data
        statWidgets.Add(new StatWidgetViewModel {
            Label = "Total Users",
            Value = (await _userManager.GetTotalUsersAsync()).ToString(),
            SubLabel = "All workspaces"
        });
        
        charts.Add(new ChartWidgetViewModel {
            Title = "Projects by Workspace",
            CanvasId = "projects-by-workspace-chart",
            Type = "bar",
            Data = await GetWorkspaceProjectsDataAsync()
        });
    }
    else if (admin is WorkspaceAdmin workspaceAdmin)
    {
        // Workspace-specific data
        statWidgets.Add(new StatWidgetViewModel {
            Label = "Total Users",
            Value = (await _userManager.GetUsersByWorkspaceAsync(workspaceAdmin.Workspace.Id)).Count().ToString(),
            SubLabel = workspaceAdmin.Workspace.Name
        });
        
        charts.Add(new ChartWidgetViewModel {
            Title = "Project Activity",
            CanvasId = "project-activity-chart",
            Type = "line",
            Data = await GetWorkspaceActivityDataAsync(workspaceAdmin.Workspace.Id)
        });
    }
    
    return new DashboardViewModel { StatWidgets = statWidgets, MainCharts = charts }; 
}
```

---

## API Requirements

### New Endpoints Needed

| Endpoint | Method | Purpose | Auth | Response |
|----------|--------|---------|------|----------|
| `/api/admin/dashboard` | GET | Get dashboard data | Both | `DashboardViewModel` |
| `/api/admin/stats/usage-trend` | GET | Usage trend data (supports `?period=7d\|1m\|total`) | Both | `{ labels, values, type }` |
| `/api/admin/stats/active-projects` | GET | Active projects (for ComparisonWidget) | Both | `{ datasets, labels }` |
| `/api/admin/stats/engagement` | GET | Engagement metrics (for EngagementWidget) | Both | `{ fullCompletionRate, gaugeLabel, bars[] }` |
| `/api/admin/workspaces` | GET | List workspaces | Conversey | `WorkspaceDto[]` |
| `/api/admin/workspaces/{id}/projects` | GET | Projects by workspace | Both | `ProjectDto[]` |

### Response Adaptation

**Same endpoint, different responses:**

```json
// Conversey Admin response:
{
  "type": "platform",
  "data": {
    "totalUsers": 1500,
    "totalProjects": 240,
    "totalWorkspaces": 15
  }
}

// Workspace Admin response:
{
  "type": "workspace", 
  "data": {
    "totalUsers": 45,
    "totalProjects": 8,
    "workspaceName": "Acme Corp"
  }
}
```

---

## Implementation Log

> **🤖 AI ASSISTANT INSTRUCTIONS:** After making any changes to role-based admin dashboard components, you MUST update this section with:
> - Date of change
> - Modified/created files
> - Description of changes
> - Your name/identifier
> 
> Use format: `[YYYY-MM-DD] - Files: description - Assistant: {name}`

### Latest Changes

[2026-05-21] - Files: docs/ROLE_BASED_ADMIN_DASHBOARD_PLAN.md - Integrated validated wireframe (Conversey Admin Dashboard screenshots 2026-05-21). Added: Wireframe Analysis section with full visual map, `_NavCard` building block, `_QuickLinksWidget` grouped list spec, `_EngagementWidget` (SVG arc gauge + progress bars), updated Phase D to wireframe-accurate 3-column layout, added NavCardViewModel / QuickLinksWidgetViewModel / QuickLinkItemViewModel / EngagementWidgetViewModel / EngagementBarViewModel, updated DashboardViewModel, updated Component Library table, updated API table with period param, resolved "Awaiting wireframe" placeholder. - Assistant: Claude Sonnet 4.6

[2025-01-17] - Files: docs/ROLE_BASED_ADMIN_DASHBOARD_PLAN.md - Updated all component names to follow Widget naming convention (Widget suffix = dashboard-placed, no suffix = building block). Renamed: _StatCard→_StatWidget, _ChartCard→_ChartWidget, _ActionCard→_ActionWidget, _ProgressBar→_ProgressWidget, _DataTable→_TableWidget. Updated all ViewModel classes accordingly. - Assistant: Mistral Vibe

[2025-01-17] - Files: docs/ROLE_BASED_ADMIN_DASHBOARD_PLAN.md - Added Widget Height System (content-based, not fixed numbers: M/L/XL = 3× Small via grid-row-span), standardized modal attributes (data-modal-open, data-modal-close, data-modal), added missing component templates (ActionWidget, TableWidget), defined supporting models (ColumnModel, RowModel, ActionModel), added WidgetSize enum, added Form Validation section, updated Usage Examples, added accessibility features to modals, reconciled layout structures. - Assistant: Mistral Vibe

[2025-01-17] - Files: docs/ROLE_BASED_ADMIN_DASHBOARD_PLAN.md - Added Comparison Widget type with radial circle arrangement: left panel (title, subtitle, legend with colored pills), right panel (proportionally-sized circles arranged in equilateral triangle/square pattern, centered as ensemble). Includes ViewModels (ComparisonWidgetViewModel, ComparisonItemViewModel), template with **bidirectional** hover effects (legend ↔ circles), CSS recommendations. - Assistant: Mistral Vibe

[2025-01-17] - Files: docs/ROLE_BASED_ADMIN_DASHBOARD_PLAN.md - Added Links Widget (#2) with SOLID-conform TypeScript implementation. Includes: LinkWidgetViewModel (Title, Icon, ModalTarget, CreateUrl, Size), simple template `_LinksWidget.cshtml`, complete SOLID implementation with interfaces (IContentLoader, IModalManager, IFormHandler, ILinkWidgetConfig) and concrete classes (HttpContentLoader, DomContentLoader, ModalManager, FormHandler, LinkWidgetHandler), initialization code, modal HTML structure, backend support for X-Requested-With header, usage examples, and SOLID compliance table. - Assistant: Mistral Vibe

[2026-05-22] - Files: UI-MVC/Models/AdminExtensions.cs, UI-MVC/Models/Admin/DashboardViewModel.cs, UI-MVC/Controllers/Admin/DashboardController.cs, UI-MVC/Views/Admin/Dashboard.cshtml, UI-MVC/Views/Shared/Admin/_NavCard.cshtml - Implemented Phase A: Foundation. Created AdminExtensions.cs with IsConverseyAdmin(), IsWorkspaceAdmin(), GetWorkspaceId(), GetWorkspace() extension methods for AdminContext and IdentityUser. Created DashboardViewModel with all widget ViewModels (NavCard, Comparison, QuickLinks, Engagement, Chart, Stat). Created DashboardController with role-based routing (/admin, /admin/dashboard/conversey, /admin/dashboard/workspace) and legacy redirects. Created Dashboard.cshtml view with wireframe-accurate layout structure. Created _NavCard.cshtml partial. Verified WorkspaceContext usage is standardized. Build succeeded. - Assistant: Mistral Vibe

---

### Historical Changes

---

## Testing Strategy

### Test Cases by Role

| Test | Conversey Admin | Workspace Admin |
|------|-----------------|-----------------|
| Dashboard loads | ✅ All data | ✅ Workspace data only |
| Stat widgets show | ✅ Platform totals | ✅ Workspace totals |
| Charts render | ✅ All workspaces | ✅ Single workspace |
| Quick actions visible | ✅ New Workspace | ✅ New Project |
| API endpoints accessible | ✅ All | ✅ Filtered |

### Test Types

1. **Unit Tests** (Backend)
   - Role detection logic
   - Data filtering in services
   - API endpoint responses

2. **Integration Tests**
   - Full request/response cycle
   - Authentication/authorization

3. **UI Tests**
   - Component rendering
   - Responsive behavior
   - Accessibility compliance

4. **Manual Testing**
   - Cross-browser (Chrome, Firefox, Safari)
   - Cross-device (Mobile, Tablet, Desktop)
   - Role switching simulation

---

## Dependencies & Risks

### Dependencies

| Dependency | Required For | Status |
|------------|--------------|--------|
| Chart.js | Data visualization | ⏳ Not installed |
| WorkspaceContext | Workspace data | ✅ Available |
| AdminContext | Admin data | ✅ Available |
| Backend API | Data fetching | ✅ Available |
| Tailwind CSS | Styling | ✅ Available |

### Risks & Mitigations

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Chart.js compatibility | Medium | High | Test in dev environment |
| API endpoint conflicts | Low | Medium | Use versioned routes |
| Data filtering bugs | Medium | High | Comprehensive unit tests |
| Performance issues | Medium | Medium | Lazy load charts, paginate data |
| Cross-role data leakage | Low | Critical | Strict authorization checks |
| Accessibility gaps | Medium | Medium | ARIA attributes, keyboard nav |

---

## File Structure Reference

```
backend/UI-MVC/
├── Controllers/
│   ├── Admin/
│   │   ├── DashboardController.cs       # NEW: Unified dashboard
│   │   └── ... (existing)
│   └── Api/
│       ├── AdminStatsController.cs     # NEW: Role-aware stats
│       └── ... (existing)
│
├── Views/
│   ├── Admin/
│   │   └── Dashboard.cshtml             # NEW: Unified dashboard view
│   │
│   └── Shared/
│       └── Admin/
│           ├── _NavCard.cshtml          # NEW: Top navigation card (building block)
│           │
│           ├── Dashboard/               # NEW: Component library
│           │   ├── _ComparisonWidget.cshtml
│           │   ├── _QuickLinksWidget.cshtml  # NEW: Grouped link-list widget
│           │   ├── _EngagementWidget.cshtml  # NEW: Gauge + progress bars widget
│           │   ├── _StatWidget.cshtml
│           │   ├── _ChartWidget.cshtml
│           │   ├── _ActionWidget.cshtml
│           │   ├── _ProgressWidget.cshtml
│           │   ├── _LinksWidget.cshtml
│           │   └── _TableWidget.cshtml
│           │
│           └── Modals/                   # NEW: Modal components
│               ├── _NewWorkspaceModal.cshtml
│               ├── _NewProjectModal.cshtml
│               └── _ConfirmModal.cshtml
│
├── Assets/
│   ├── components/
│   │   └── admin/
│   │       ├── dashboard.ts            # NEW: Dashboard initialization
│   │       ├── charts.ts               # NEW: Chart logic + period tabs
│   │       └── modals.ts                # NEW: Modal logic
│   │
│   └── services/
│       └── adminStatsService.ts         # NEW: Client-side data fetching
│
└── Models/
    ├── Admin/
    │   ├── DashboardViewModel.cs        # NEW: Updated per wireframe
    │   ├── NavCardViewModel.cs          # NEW: Top nav card model
    │   ├── QuickLinksWidgetViewModel.cs # NEW: Grouped link list widget
    │   ├── QuickLinkItemViewModel.cs    # NEW: Individual link row
    │   ├── EngagementWidgetViewModel.cs # NEW: Gauge + progress bars widget
    │   ├── EngagementBarViewModel.cs    # NEW: Individual progress bar
    │   ├── ComparisonWidgetViewModel.cs # NEW: Proportional circle comparison
    │   ├── ComparisonItemViewModel.cs   # NEW: Individual comparison item
    │   ├── StatWidgetViewModel.cs
    │   ├── ChartWidgetViewModel.cs
    │   ├── ActionWidgetViewModel.cs
    │   ├── ProgressWidgetViewModel.cs
    │   ├── TableWidgetViewModel.cs
    │   └── ...
    │
    └── AdminExtensions.cs              # NEW: Role detection helpers
```

---

## Next Steps

1. **Start with Phase A** — Role detection (`AdminExtensions.cs`, `WorkspaceContext` consistency)
2. **Phase B** — `AdminStatsService` with engagement and comparison data methods
3. **Phase C** — Build new components in order:
   - `_NavCard` (simplest — pure HTML)
   - `_QuickLinksWidget` (grouped list, connect modal system)
   - `_EngagementWidget` (SVG gauge + bars)
   - Extend `_ChartWidget` with period tabs (7d / 1m / total)
4. **Phase D** — Assemble `Dashboard.cshtml` from the wireframe layout
5. **Phase E** — TypeScript: chart period-tab fetching, modal system, engagement data refresh
6. **Update this document** after each change (Implementation Log)

---

## Document Maintenance

**This document must be updated automatically by AI assistants after every change.**

**AI Checklist after changes:**
- [ ] Update the [Implementation Log](#implementation-log) section
- [ ] Verify file structure reference is accurate
- [ ] Update any affected technical specifications
- [ ] Ensure all dependencies are listed
- [ ] Confirm with user before committing

---

*Last verified: 2026-05-21*
*Document version: 1.4*
*Owner: Matéo Rohr*
