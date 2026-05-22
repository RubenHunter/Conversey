using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.DAL.Administration;
using Conversey.DAL.Ideation;
using Conversey.DAL.Subplatform.Ai;

namespace Conversey.BL.Administration;

/// <summary>
/// Service for retrieving admin statistics and dashboard data.
/// Filters data based on admin role (Conversey Admin vs Workspace Admin).
/// </summary>
public class AdminStatsService : IAdminStatsService
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IAdminRepository _adminRepository;
    private readonly IIdeaRepository _ideaRepository;
    private readonly IAuditRepository _auditRepository;

    public AdminStatsService(
        IWorkspaceRepository workspaceRepository,
        IProjectRepository projectRepository,
        IAdminRepository adminRepository,
        IIdeaRepository ideaRepository,
        IAuditRepository auditRepository)
    {
        _workspaceRepository = workspaceRepository;
        _projectRepository = projectRepository;
        _adminRepository = adminRepository;
        _ideaRepository = ideaRepository;
        _auditRepository = auditRepository;
    }

    public Task<DashboardStatsDto> GetPlatformDashboardAsync()
    {
        var platformStats = GetPlatformStats();
        
        return Task.FromResult(new DashboardStatsDto
        {
            PlatformStats = platformStats,
            StatWidgets = GetPlatformStatWidgets(platformStats),
            MainCharts = GetPlatformCharts(),
            ActionWidgets = GetPlatformActionWidgets(),
            ProgressWidgets = GetPlatformProgressWidgets(),
            ComparisonWidget = GetPlatformComparisonWidget(),
            QuickLinksWidget = GetPlatformQuickLinksWidget(),
            EngagementWidget = GetPlatformEngagementWidget(),
            UsageTrendChart = GetPlatformUsageTrendChart()
        });
    }

    public Task<DashboardStatsDto> GetWorkspaceDashboardAsync(Slug workspaceId)
    {
        var workspaceStats = GetWorkspaceStats(workspaceId);
        
        return Task.FromResult(new DashboardStatsDto
        {
            WorkspaceStats = workspaceStats,
            StatWidgets = GetWorkspaceStatWidgets(workspaceStats),
            MainCharts = GetWorkspaceCharts(workspaceStats),
            ActionWidgets = GetWorkspaceActionWidgets(workspaceId),
            ProgressWidgets = GetWorkspaceProgressWidgets(),
            ComparisonWidget = GetWorkspaceComparisonWidget(workspaceId),
            QuickLinksWidget = GetWorkspaceQuickLinksWidget(),
            EngagementWidget = GetWorkspaceEngagementWidget(workspaceStats),
            UsageTrendChart = GetWorkspaceUsageTrendChart()
        });
    }

    public Task<PlatformStatsDto> GetPlatformStatsAsync()
    {
        return Task.FromResult(GetPlatformStats());
    }

    public Task<WorkspaceStatsDto> GetWorkspaceStatsAsync(Slug workspaceId)
    {
        return Task.FromResult(GetWorkspaceStats(workspaceId));
    }

    public Task<UsageTrendDto> GetUsageTrendAsync(Admin admin, string period = "7d")
    {
        return Task.FromResult(new UsageTrendDto
        {
            Title = "Usage Trend",
            Type = "line",
            Labels = new List<string> { "May 15", "May 16", "May 17", "May 18", "May 19", "May 20", "May 21" },
            Values = new List<int> { 10, 15, 12, 18, 22, 19, 25 },
            Period = period
        });
    }

    public Task<ComparisonWidgetDto> GetComparisonDataAsync(Admin admin)
    {
        if (admin is ConverseyAdmin)
            return Task.FromResult(GetPlatformComparisonWidget());
        else if (admin is WorkspaceAdmin workspaceAdmin)
            return Task.FromResult(GetWorkspaceComparisonWidget(workspaceAdmin.Workspace.Id));
        
        return Task.FromResult(new ComparisonWidgetDto());
    }

    public Task<QuickLinksWidgetDto> GetQuickLinksDataAsync(Admin admin)
    {
        if (admin is ConverseyAdmin)
            return Task.FromResult(GetPlatformQuickLinksWidget());
        else if (admin is WorkspaceAdmin)
            return Task.FromResult(GetWorkspaceQuickLinksWidget());
        
        return Task.FromResult(new QuickLinksWidgetDto());
    }

    public Task<EngagementWidgetDto> GetEngagementDataAsync(Admin admin)
    {
        if (admin is ConverseyAdmin)
            return Task.FromResult(GetPlatformEngagementWidget());
        else if (admin is WorkspaceAdmin workspaceAdmin)
        {
            var workspaceStats = GetWorkspaceStats(workspaceAdmin.Workspace.Id);
            return Task.FromResult(GetWorkspaceEngagementWidget(workspaceStats));
        }
        
        return Task.FromResult(new EngagementWidgetDto());
    }

    #region Platform Data

    private PlatformStatsDto GetPlatformStats()
    {
        var workspaces = _workspaceRepository.ReadAllWorkspaces();
        var allProjects = new List<Project>();
        var allIdeas = new List<Conversey.BL.Domain.Ideation.Idea>();
        
        foreach (var workspace in workspaces)
        {
            var projects = _projectRepository.ReadAllProjectsFromWorkspaceId(workspace.Id);
            allProjects.AddRange(projects);
            
            foreach (var project in projects)
            {
                var topics = project.Topic?.ToList() ?? new List<Topic>();
                foreach (var topic in topics)
                {
                    var topicIdeas = _ideaRepository.ReadIdeasByTopicId(topic.Id);
                    allIdeas.AddRange(topicIdeas);
                }
            }
        }
        
        return new PlatformStatsDto
        {
            TotalWorkspaces = workspaces.Count,
            TotalProjects = allProjects.Count,
            TotalUsers = 0,
            TotalIdeas = allIdeas.Count,
            ActiveAiProviders = 1,
            LastActivityDate = DateTime.UtcNow
        };
    }

    private List<StatWidgetDto> GetPlatformStatWidgets(PlatformStatsDto stats)
    {
        return new List<StatWidgetDto>
        {
            new() { Label = "Workspaces", Value = stats.TotalWorkspaces.ToString(), SubLabel = "Total active workspaces" },
            new() { Label = "Projects", Value = stats.TotalProjects.ToString(), SubLabel = "Across all workspaces" },
            new() { Label = "Ideas", Value = stats.TotalIdeas.ToString(), SubLabel = "Total submissions" },
            new() { Label = "AI Usage", Value = "0", SubLabel = "Today's requests" }
        };
    }

    private List<ChartWidgetDto> GetPlatformCharts()
    {
        return new List<ChartWidgetDto>
        {
            new()
            {
                Title = "Projects by Workspace",
                Type = "bar",
                Data = new { labels = new[] { "Workspace 1", "Workspace 2", "Workspace 3" }, datasets = new[] { new { label = "Projects", data = new[] { 5, 10, 8 } } } },
                Periods = new List<PeriodDto>
                {
                    new() { Id = "7d", Label = "7d", IsActive = true },
                    new() { Id = "1m", Label = "1m", IsActive = false },
                    new() { Id = "total", Label = "Total", IsActive = false }
                },
                ActivePeriod = "7d"
            }
        };
    }

    private List<ActionWidgetDto> GetPlatformActionWidgets()
    {
        return new List<ActionWidgetDto>
        {
            new() { Title = "Create New", Description = "Add a new workspace to your platform", Icon = "<svg class='w-6 h-6' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M12 4.5v15m7.5-7.5h-15'/></svg>", ActionText = "New Workspace", ActionUrl = "/admin/workspaces/create", IsModal = false }
        };
    }

    private List<ProgressWidgetDto> GetPlatformProgressWidgets()
    {
        return new List<ProgressWidgetDto>
        {
            new() { Label = "Storage Usage", Percentage = 35, Color = "success" }
        };
    }

    private ComparisonWidgetDto GetPlatformComparisonWidget()
    {
        var workspaces = _workspaceRepository.ReadAllWorkspaces();
        return new ComparisonWidgetDto
        {
            Title = "Active projects",
            SubTitle = "All projects",
            TitleUrl = "/admin/projects",
            SubTitleUrl = "/admin/projects?filter=all",
            Items = workspaces.Select((w, i) => new ComparisonItemDto
            {
                Label = w.Name,
                Value = w.Projects?.Count() ?? 0,
                Color = i == 0 ? "primary" : i == 1 ? "secondary" : "accent"
            }).ToList()
        };
    }

    private QuickLinksWidgetDto GetPlatformQuickLinksWidget()
    {
        return new QuickLinksWidgetDto
        {
            Title = "Links",
            Items = new List<QuickLinkItemDto>
            {
                new() { Title = "New workspace", Description = "Create a new workspace", Icon = "<svg class='w-6 h-6' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M12 4.5v15m7.5-7.5h-15'/></svg>", IconBackground = "bg-yellow-100", IconColor = "text-yellow-600", ModalTarget = "modal-new-workspace", NavigateUrl = "/admin/workspaces/create" },
                new() { Title = "Check health", Description = "Check AI provider", Icon = "<svg class='w-6 h-6' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12Z'/></svg>", IconBackground = "bg-rose-100", IconColor = "text-rose-600", NavigateUrl = "/admin/ai/health" },
                new() { Title = "Rate Limits", Description = "Limits for AI endpoints", Icon = "<svg class='w-6 h-6' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M10.5 6h9.75M10.5 6a1.5 1.5 0 11-3 0m3 0a1.5 1.5 0 10-3 0M3.75 6H7.5m3 12h9.75m-9.75 0a1.5 1.5 0 01-3 0m3 0a1.5 1.5 0 00-3 0m-3.75 0H7.5m9-6h3.75m-3.75 0a1.5 1.5 0 01-3 0m3 0a1.5 1.5 0 00-3 0m-9.75 0h9.75'/></svg>", IconBackground = "bg-lime-100", IconColor = "text-lime-600", NavigateUrl = "/admin/ai/rate-limits" }
            }
        };
    }

    private EngagementWidgetDto GetPlatformEngagementWidget()
    {
        return new EngagementWidgetDto
        {
            Title = "Weekly Engagement Rate",
            GaugePercentage = 67,
            GaugeLabel = "Full Completion Rate",
            GaugeColor = "text-yellow-400",
            Bars = new List<EngagementBarDto>
            {
                new() { Label = "Response Rate", SubLabel = "average responses per survey", Current = 14, Max = 20, DisplayValue = "14/20", Color = "bg-green-500" },
                new() { Label = "Toxicity", SubLabel = "safe content ratio", Current = 85, Max = 100, DisplayValue = "8.5/10", Color = "bg-green-500" },
                new() { Label = "Idea Contributions", SubLabel = "ideas submission rate", Current = 1290, Max = 1954, DisplayValue = "1290 / 1954", Color = "bg-orange-500" }
            }
        };
    }

    private ChartWidgetDto GetPlatformUsageTrendChart()
    {
        return new ChartWidgetDto
        {
            Title = "Usage Trend",
            Type = "line",
            Data = new
            {
                labels = new[] { "May 15", "May 16", "May 17", "May 18", "May 19", "May 20", "May 21" },
                datasets = new[] { new { label = "Visits", data = new[] { 10, 15, 12, 18, 22, 19, 25 }, borderColor = "#3b82f6", backgroundColor = "rgba(59, 130, 246, 0.1)" } }
            },
            Periods = new List<PeriodDto>
            {
                new() { Id = "7d", Label = "7d", IsActive = true },
                new() { Id = "1m", Label = "1m", IsActive = false },
                new() { Id = "total", Label = "Total", IsActive = false }
            },
            ActivePeriod = "7d"
        };
    }

    #endregion

    #region Workspace Data

    private WorkspaceStatsDto GetWorkspaceStats(Slug workspaceId)
    {
        var workspace = _workspaceRepository.ReadWorkspaceById(workspaceId);
        if (workspace == null)
            return new WorkspaceStatsDto { WorkspaceId = workspaceId, WorkspaceName = "Unknown" };

        var projects = _projectRepository.ReadAllProjectsFromWorkspaceId(workspaceId);
        int totalTopics = 0, totalYouths = 0, totalIdeas = 0;
        
        foreach (var project in projects)
        {
            totalTopics += project.Topic?.Count() ?? 0;
            totalYouths += project.Youth?.Count() ?? 0;
            
            var topics = project.Topic == null ? new List<Topic>() : project.Topic.ToList();
            foreach (var topic in topics)
            {
                var topicIdeas = _ideaRepository.ReadIdeasByTopicId(topic.Id);
                totalIdeas += topicIdeas.Count;
            }
        }

        return new WorkspaceStatsDto
        {
            WorkspaceId = workspaceId,
            WorkspaceName = workspace.Name,
            TotalProjects = projects.Count,
            TotalTopics = totalTopics,
            TotalYouths = totalYouths,
            TotalIdeas = totalIdeas,
            ActiveUsers = 0,
            LastActivityDate = DateTime.UtcNow
        };
    }

    private List<StatWidgetDto> GetWorkspaceStatWidgets(WorkspaceStatsDto stats)
    {
        return new List<StatWidgetDto>
        {
            new() { Label = "Projects", Value = stats.TotalProjects.ToString(), SubLabel = "In this workspace" },
            new() { Label = "Topics", Value = stats.TotalTopics.ToString(), SubLabel = "Across all projects" },
            new() { Label = "Ideas", Value = stats.TotalIdeas.ToString(), SubLabel = "Total submissions" },
            new() { Label = "Youths", Value = stats.TotalYouths.ToString(), SubLabel = "Active participants" }
        };
    }

    private List<ChartWidgetDto> GetWorkspaceCharts(WorkspaceStatsDto stats)
    {
        return new List<ChartWidgetDto>
        {
            new()
            {
                Title = "Project Activity",
                Type = "bar",
                Data = new { labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" }, datasets = new[] { new { label = "Activity", data = new[] { 10, 15, 12, 18, 22 } } } },
                Periods = new List<PeriodDto>
                {
                    new() { Id = "7d", Label = "7d", IsActive = true },
                    new() { Id = "1m", Label = "1m", IsActive = false },
                    new() { Id = "total", Label = "Total", IsActive = false }
                },
                ActivePeriod = "7d"
            }
        };
    }

    private List<ActionWidgetDto> GetWorkspaceActionWidgets(Slug workspaceId)
    {
        return new List<ActionWidgetDto>
        {
            new() { Title = "Create New", Description = "Add a new project to your workspace", Icon = "<svg class='w-6 h-6' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M12 4.5v15m7.5-7.5h-15'/></svg>", ActionText = "New Project", ActionUrl = "/admin/projects/create", IsModal = false }
        };
    }

    private List<ProgressWidgetDto> GetWorkspaceProgressWidgets()
    {
        return new List<ProgressWidgetDto>
        {
            new() { Label = "Project Completion", Percentage = 75, Color = "success" }
        };
    }

    private ComparisonWidgetDto GetWorkspaceComparisonWidget(Slug workspaceId)
    {
        var projects = _projectRepository.ReadAllProjectsFromWorkspaceId(workspaceId);
        return new ComparisonWidgetDto
        {
            Title = "Project Topics",
            SubTitle = "All topics",
            TitleUrl = "/admin/projects",
            SubTitleUrl = "/admin/projects?filter=all",
            Items = projects.Select((p, i) => new ComparisonItemDto
            {
                Label = p.Name,
                Value = p.Topic?.Count() ?? 0,
                Color = i == 0 ? "primary" : i == 1 ? "secondary" : "accent"
            }).ToList()
        };
    }

    private QuickLinksWidgetDto GetWorkspaceQuickLinksWidget()
    {
        return new QuickLinksWidgetDto
        {
            Title = "Links",
            Items = new List<QuickLinkItemDto>
            {
                new() { Title = "New project", Description = "Create a new project", Icon = "<svg class='w-6 h-6' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M12 4.5v15m7.5-7.5h-15'/></svg>", IconBackground = "bg-yellow-100", IconColor = "text-yellow-600", ModalTarget = "modal-new-project", NavigateUrl = "/admin/projects/create" },
                new() { Title = "AI Settings", Description = "Configure AI for workspace", Icon = "<svg class='w-6 h-6' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'/></svg>", IconBackground = "bg-green-100", IconColor = "text-green-600", NavigateUrl = "/admin/{workspaceSlug}/ai" }
            }
        };
    }

    private EngagementWidgetDto GetWorkspaceEngagementWidget(WorkspaceStatsDto workspaceStats)
    {
        int totalIdeas = workspaceStats.TotalIdeas;
        int approvedIdeas = totalIdeas / 2;
        int max = Math.Max(totalIdeas, 100);
        
        return new EngagementWidgetDto
        {
            Title = "Workspace Engagement",
            GaugePercentage = totalIdeas > 0 ? (approvedIdeas * 100 / totalIdeas) : 0,
            GaugeLabel = "Idea Approval Rate",
            GaugeColor = "text-green-400",
            Bars = new List<EngagementBarDto>
            {
                new() { Label = "Ideas Submitted", SubLabel = "total count", Current = totalIdeas, Max = max, DisplayValue = $"{totalIdeas}/100", Color = "bg-green-500" },
                new() { Label = "Ideas Approved", SubLabel = "approved submissions", Current = approvedIdeas, Max = max, DisplayValue = $"{approvedIdeas}/{totalIdeas}", Color = "bg-blue-500" }
            }
        };
    }

    private ChartWidgetDto GetWorkspaceUsageTrendChart()
    {
        return new ChartWidgetDto
        {
            Title = "Usage Trend",
            Type = "line",
            Data = new
            {
                labels = new[] { "May 15", "May 16", "May 17", "May 18", "May 19", "May 20", "May 21" },
                datasets = new[] { new { label = "Activity", data = new[] { 10, 15, 12, 18, 22, 19, 25 }, borderColor = "#10b981", backgroundColor = "rgba(16, 185, 129, 0.1)" } }
            },
            Periods = new List<PeriodDto>
            {
                new() { Id = "7d", Label = "7d", IsActive = true },
                new() { Id = "1m", Label = "1m", IsActive = false },
                new() { Id = "total", Label = "Total", IsActive = false }
            },
            ActivePeriod = "7d"
        };
    }

    #endregion
}
