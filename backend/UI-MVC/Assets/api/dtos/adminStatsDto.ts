/**
 * Admin Statistics DTOs
 * TypeScript type definitions for admin dashboard data
 * Mirrors the C# DTOs in BL/Administration/IAdminStatsService.cs
 */

/**
 * Platform-wide statistics DTO
 */
export interface PlatformStatsDto {
    totalWorkspaces: number;
    totalProjects: number;
    totalUsers: number;
    totalIdeas: number;
    activeAiProviders: number;
    lastActivityDate: string;
}

/**
 * Workspace-specific statistics DTO
 */
export interface WorkspaceStatsDto {
    workspaceId: string;
    workspaceName: string;
    totalProjects: number;
    totalTopics: number;
    totalYouths: number;
    totalIdeas: number;
    activeUsers: number;
    lastActivityDate: string;
}

/**
 * Stat widget data
 */
export interface StatWidgetDto {
    label: string;
    value: string;
    subLabel?: string;
    icon?: string;
    color?: string;
}

/**
 * Period filter option
 */
export interface PeriodDto {
    id: string;
    label: string;
    isActive: boolean;
}

/**
 * Chart widget data
 */
export interface ChartWidgetDto {
    title: string;
    canvasId: string;
    type: string;
    data: any;
    options?: any;
    periods: PeriodDto[];
    activePeriod: string;
}

/**
 * Action widget data
 */
export interface ActionWidgetDto {
    title: string;
    description?: string;
    icon: string;
    actionText: string;
    actionUrl: string;
    modalTarget?: string;
    isModal: boolean;
}

/**
 * Progress widget data
 */
export interface ProgressWidgetDto {
    label: string;
    percentage: number;
    color?: string;
}

/**
 * Comparison item data
 */
export interface ComparisonItemDto {
    label: string;
    value: number;
    color: string;
    legendIcon?: string;
}

/**
 * Comparison widget data
 */
export interface ComparisonWidgetDto {
    title: string;
    subTitle: string;
    titleUrl: string;
    subTitleUrl: string;
    items: ComparisonItemDto[];
}

/**
 * Quick link item data
 */
export interface QuickLinkItemDto {
    title: string;
    description: string;
    icon: string;
    iconBackground: string;
    iconColor: string;
    modalTarget?: string;
    navigateUrl: string;
    isModal: boolean;
}

/**
 * Quick links widget data
 */
export interface QuickLinksWidgetDto {
    title?: string;
    items: QuickLinkItemDto[];
}

/**
 * Engagement progress bar data
 */
export interface EngagementBarDto {
    label: string;
    subLabel: string;
    current: number;
    max: number;
    displayValue: string;
    color: string;
}

/**
 * Engagement widget data
 */
export interface EngagementWidgetDto {
    title: string;
    gaugePercentage: number;
    gaugeLabel: string;
    gaugeColor: string;
    bars: EngagementBarDto[];
}

/**
 * Usage trend data
 */
export interface UsageTrendDto {
    title: string;
    type: string;
    labels: string[];
    values: number[];
    period: string;
}

/**
 * Complete dashboard statistics DTO
 */
export interface DashboardStatsDto {
    platformStats?: PlatformStatsDto;
    workspaceStats?: WorkspaceStatsDto;
    statWidgets: StatWidgetDto[];
    mainCharts: ChartWidgetDto[];
    actionWidgets: ActionWidgetDto[];
    progressWidgets: ProgressWidgetDto[];
    comparisonWidget?: ComparisonWidgetDto;
    quickLinksWidget?: QuickLinksWidgetDto;
    engagementWidget?: EngagementWidgetDto;
    usageTrendChart?: ChartWidgetDto;
}

/**
 * Admin type for dashboard context
 */
export type AdminType = 'conversey' | 'workspace' | 'unknown';

/**
 * Dashboard view model (mirrors C# DashboardViewModel)
 */
export interface DashboardViewModel {
    pageTitle: string;
    pageDescription: string;
    adminType: AdminType;
    workspaceName?: string;
    workspaceId?: string;
    navCards: NavCardViewModel[];
    comparisonWidget?: ComparisonWidgetViewModel;
    quickLinksWidget?: QuickLinksWidgetViewModel;
    engagementWidget?: EngagementWidgetViewModel;
    usageTrendChart?: ChartWidgetViewModel;
    statWidgets: StatWidgetViewModel[];
}

/**
 * Navigation card view model
 */
export interface NavCardViewModel {
    title: string;
    description: string;
    icon: string;
    navigateUrl: string;
    iconBackground: string;
}

/**
 * Widget size enum (mirrors C# WidgetSize)
 */
export type WidgetSize = 'Small' | 'Medium' | 'Large' | 'ExtraLarge';

/**
 * Comparison widget view model
 */
export interface ComparisonWidgetViewModel {
    id: string;
    title: string;
    subTitle: string;
    titleUrl: string;
    subTitleUrl: string;
    items: ComparisonItemViewModel[];
    size: WidgetSize;
}

/**
 * Comparison item view model
 */
export interface ComparisonItemViewModel {
    label: string;
    value: number;
    color: string;
    legendIcon?: string;
}

/**
 * Quick links widget view model
 */
export interface QuickLinksWidgetViewModel {
    title?: string;
    items: QuickLinkItemViewModel[];
    size: WidgetSize;
}

/**
 * Quick link item view model
 */
export interface QuickLinkItemViewModel {
    title: string;
    description: string;
    icon: string;
    iconBackground: string;
    iconColor: string;
    modalTarget?: string;
    navigateUrl: string;
}

/**
 * Engagement widget view model
 */
export interface EngagementWidgetViewModel {
    title: string;
    gaugePercentage: number;
    gaugeLabel: string;
    gaugeColor: string;
    bars: EngagementBarViewModel[];
    size: WidgetSize;
}

/**
 * Engagement bar view model
 */
export interface EngagementBarViewModel {
    label: string;
    subLabel: string;
    current: number;
    max: number;
    displayValue: string;
    color: string;
}

/**
 * Chart widget view model
 */
export interface ChartWidgetViewModel {
    title: string;
    canvasId: string;
    type: string;
    data: any;
    options?: any;
    periods: PeriodViewModel[];
    activePeriod: string;
    size: WidgetSize;
}

/**
 * Period view model
 */
export interface PeriodViewModel {
    id: string;
    label: string;
    isActive: boolean;
}

/**
 * Stat widget view model
 */
export interface StatWidgetViewModel {
    label: string;
    value: string;
    subLabel?: string;
    icon?: string;
    color?: string;
    size: WidgetSize;
}
