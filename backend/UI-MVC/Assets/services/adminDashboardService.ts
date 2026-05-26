/**
 * Admin Dashboard Service
 * TypeScript service for fetching and managing admin dashboard data
 * Provides reactive state management for dashboard components
 */

import type {
    DashboardStatsDto,
    PlatformStatsDto,
    WorkspaceStatsDto,
    UsageTrendDto,
    ComparisonWidgetDto,
    QuickLinksWidgetDto,
    EngagementWidgetDto
} from '../api/dtos/adminStatsDto';

/**
 * Dashboard state
 */
export interface DashboardState {
    isLoading: boolean;
    error?: string;
    data?: DashboardStatsDto;
    adminType: 'conversey' | 'workspace' | 'unknown';
    workspaceName?: string;
    workspaceId?: string;
}

/**
 * Admin Dashboard Service
 * Manages dashboard state and provides API methods
 */
export class AdminDashboardService {
    private state: DashboardState;
    private subscribers: Set<(state: DashboardState) => void> = new Set();
    private readonly baseUrl: string;

    constructor(baseUrl: string = '') {
        this.baseUrl = baseUrl;
        this.state = {
            isLoading: true,
            adminType: 'unknown'
        };
    }

    /**
     * Subscribe to state changes
     */
    subscribe(callback: (state: DashboardState) => void): () => void {
        this.subscribers.add(callback);
        // Immediately call with current state
        callback(this.state);
        
        // Return unsubscribe function
        return () => {
            this.subscribers.delete(callback);
        };
    }

    /**
     * Notify all subscribers of state change
     */
    private notify(): void {
        this.subscribers.forEach(callback => callback(this.state));
    }

    /**
     * Update state and notify subscribers
     */
    private updateState(partial: Partial<DashboardState>): void {
        this.state = { ...this.state, ...partial };
        this.notify();
    }

    /**
     * Fetch dashboard data for the current admin
     */
    async fetchDashboardData(period?: string): Promise<DashboardStatsDto | null> {
        this.updateState({ isLoading: true, error: undefined });
        
        try {
            const url = `${this.baseUrl}/api/admin/dashboard${period ? `?period=${period}` : ''}`;
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            });

            if (!response.ok) {
                throw new Error(`Failed to fetch dashboard data: ${response.statusText}`);
            }

            const result = await response.json();
            const data = result.data as DashboardStatsDto;
            const type = result.type as 'platform' | 'workspace';
            
            this.updateState({
                isLoading: false,
                data,
                adminType: type === 'platform' ? 'conversey' : 'workspace',
                workspaceName: result.workspaceName,
                workspaceId: result.workspaceId
            });

            return data;
        } catch (error) {
            const errorMessage = error instanceof Error ? error.message : 'Unknown error';
            this.updateState({
                isLoading: false,
                error: errorMessage
            });
            return null;
        }
    }

    /**
     * Fetch platform statistics
     */
    async fetchPlatformStats(): Promise<PlatformStatsDto | null> {
        try {
            const url = `${this.baseUrl}/api/admin/stats/platform`;
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            });

            if (!response.ok) {
                throw new Error(`Failed to fetch platform stats: ${response.statusText}`);
            }

            return await response.json() as PlatformStatsDto;
        } catch (error) {
            console.error('Error fetching platform stats:', error);
            return null;
        }
    }

    /**
     * Fetch workspace statistics
     */
    async fetchWorkspaceStats(workspaceId: string): Promise<WorkspaceStatsDto | null> {
        try {
            const url = `${this.baseUrl}/api/admin/stats/workspace/${workspaceId}`;
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            });

            if (!response.ok) {
                throw new Error(`Failed to fetch workspace stats: ${response.statusText}`);
            }

            return await response.json() as WorkspaceStatsDto;
        } catch (error) {
            console.error('Error fetching workspace stats:', error);
            return null;
        }
    }

    /**
     * Fetch usage trend data with period
     */
    async fetchUsageTrend(period: string = '7d'): Promise<UsageTrendDto | null> {
        try {
            const url = `${this.baseUrl}/api/admin/stats/usage-trend?period=${period}`;
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            });

            if (!response.ok) {
                throw new Error(`Failed to fetch usage trend: ${response.statusText}`);
            }

            return await response.json() as UsageTrendDto;
        } catch (error) {
            console.error('Error fetching usage trend:', error);
            return null;
        }
    }

    /**
     * Fetch comparison widget data
     */
    async fetchComparisonData(): Promise<ComparisonWidgetDto | null> {
        try {
            const url = `${this.baseUrl}/api/admin/stats/comparison`;
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            });

            if (!response.ok) {
                throw new Error(`Failed to fetch comparison data: ${response.statusText}`);
            }

            return await response.json() as ComparisonWidgetDto;
        } catch (error) {
            console.error('Error fetching comparison data:', error);
            return null;
        }
    }

    /**
     * Fetch quick links widget data
     */
    async fetchQuickLinksData(): Promise<QuickLinksWidgetDto | null> {
        try {
            const url = `${this.baseUrl}/api/admin/stats/quick-links`;
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            });

            if (!response.ok) {
                throw new Error(`Failed to fetch quick links data: ${response.statusText}`);
            }

            return await response.json() as QuickLinksWidgetDto;
        } catch (error) {
            console.error('Error fetching quick links data:', error);
            return null;
        }
    }

    /**
     * Fetch engagement widget data
     */
    async fetchEngagementData(): Promise<EngagementWidgetDto | null> {
        try {
            const url = `${this.baseUrl}/api/admin/stats/engagement`;
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include'
            });

            if (!response.ok) {
                throw new Error(`Failed to fetch engagement data: ${response.statusText}`);
            }

            return await response.json() as EngagementWidgetDto;
        } catch (error) {
            console.error('Error fetching engagement data:', error);
            return null;
        }
    }

    /**
     * Refresh all dashboard data
     */
    async refresh(period?: string): Promise<void> {
        await this.fetchDashboardData(period);
    }

    /**
     * Get current state
     */
    getState(): DashboardState {
        return { ...this.state };
    }
}

/**
 * Singleton instance
 */
export const adminDashboardService = new AdminDashboardService();
