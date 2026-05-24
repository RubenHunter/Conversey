/**
 * Admin Dashboard Module Index
 * Main entry point for dashboard TypeScript components.
 * Exports all dashboard-related functionality.
 */

export * from './chartWidget';
export * from './comparisonWidget';
export * from './quickLinksWidget';
export * from './healthCheck';

import { initAllCharts, isChartLoaded } from './chartWidget';
import { initAllComparisonWidgets } from './comparisonWidget';
import { initAllQuickLinksWidgets } from './quickLinksWidget';
import { initHealthCheck } from './healthCheck';


/**
 * Initialize all dashboard components on the page.
 * Chart.js must be loaded as a blocking CDN script before this runs.
 */
export function initDashboard(): void {
    initAllComparisonWidgets();
    initAllQuickLinksWidgets();
    initHealthCheck();

    if (isChartLoaded()) {
        initAllCharts();
    } else {
        console.warn('Chart.js not found. Add the CDN script to the page before module scripts.');
    }
}

/**
 * Dashboard API client for fetching stats data
 */
export class DashboardApiClient {
    private readonly baseUrl: string;
    private readonly headers: Record<string, string>;

    constructor(baseUrl: string = '', customHeaders: Record<string, string> = {}) {
        this.baseUrl = baseUrl || '';
        this.headers = {
            'Content-Type': 'application/json',
            ...customHeaders
        };
    }

    /**
     * Fetch dashboard data for the current admin
     */
    async getDashboardData(period?: string): Promise<any> {
        const url = `${this.baseUrl}/api/admin/dashboard${period ? `?period=${period}` : ''}`;
        const response = await fetch(url, {
            method: 'GET',
            headers: this.headers,
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error(`Failed to fetch dashboard data: ${response.statusText}`);
        }
        
        return response.json();
    }

    /**
     * Fetch platform-wide statistics
     */
    async getPlatformStats(): Promise<any> {
        const url = `${this.baseUrl}/api/admin/stats/platform`;
        const response = await fetch(url, {
            method: 'GET',
            headers: this.headers,
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error(`Failed to fetch platform stats: ${response.statusText}`);
        }
        
        return response.json();
    }

    /**
     * Fetch workspace-specific statistics
     */
    async getWorkspaceStats(workspaceId: string): Promise<any> {
        const url = `${this.baseUrl}/api/admin/stats/workspace/${workspaceId}`;
        const response = await fetch(url, {
            method: 'GET',
            headers: this.headers,
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error(`Failed to fetch workspace stats: ${response.statusText}`);
        }
        
        return response.json();
    }

    /**
     * Fetch usage trend data
     */
    async getUsageTrend(period: string = '7d'): Promise<any> {
        const url = `${this.baseUrl}/api/admin/stats/usage-trend?period=${period}`;
        const response = await fetch(url, {
            method: 'GET',
            headers: this.headers,
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error(`Failed to fetch usage trend: ${response.statusText}`);
        }
        
        return response.json();
    }

    /**
     * Fetch comparison widget data
     */
    async getComparisonData(): Promise<any> {
        const url = `${this.baseUrl}/api/admin/stats/comparison`;
        const response = await fetch(url, {
            method: 'GET',
            headers: this.headers,
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error(`Failed to fetch comparison data: ${response.statusText}`);
        }
        
        return response.json();
    }

    /**
     * Fetch quick links widget data
     */
    async getQuickLinksData(): Promise<any> {
        const url = `${this.baseUrl}/api/admin/stats/quick-links`;
        const response = await fetch(url, {
            method: 'GET',
            headers: this.headers,
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error(`Failed to fetch quick links data: ${response.statusText}`);
        }
        
        return response.json();
    }

    /**
     * Fetch engagement widget data
     */
    async getEngagementData(): Promise<any> {
        const url = `${this.baseUrl}/api/admin/stats/engagement`;
        const response = await fetch(url, {
            method: 'GET',
            headers: this.headers,
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error(`Failed to fetch engagement data: ${response.statusText}`);
        }
        
        return response.json();
    }
}

/**
 * Global dashboard API client instance
 */
export const dashboardApi = new DashboardApiClient();

