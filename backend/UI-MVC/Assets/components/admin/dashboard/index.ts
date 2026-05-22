/**
 * Admin Dashboard Module Index
 * Main entry point for dashboard TypeScript components.
 * Exports all dashboard-related functionality.
 */

export * from './chartWidget';
export * from './comparisonWidget';
export * from './quickLinksWidget';

import { initAllCharts, isChartLoaded, updateChartData } from './chartWidget';
import { initAllComparisonWidgets } from './comparisonWidget';
import { initAllQuickLinksWidgets, registerModal, openModal, closeModal } from './quickLinksWidget';
import { adminDashboardService } from '../../../services/adminDashboardService';

// Re-export service for convenience
export { adminDashboardService } from '../../../services/adminDashboardService';
export type { DashboardState } from '../../../services/adminDashboardService';

/**
 * Initialize all dashboard components on the page.
 * Call this once when the dashboard page loads.
 */
export async function initDashboard(): Promise<void> {
    console.log('Initializing admin dashboard components...');
    
    // Initialize comparison widgets (no dependencies)
    initAllComparisonWidgets();
    
    // Initialize quick links widgets (no dependencies)
    initAllQuickLinksWidgets();
    
    // Initialize chart widgets - wait for Chart.js if needed
    if (isChartLoaded()) {
        initAllCharts();
    } else if (window.__ChartJsPromise) {
        // Wait for Chart.js to load
        try {
            await window.__ChartJsPromise;
            if (isChartLoaded()) {
                initAllCharts();
            } else {
                console.warn('Chart.js failed to load. Chart widgets will not work.');
            }
        } catch (error) {
            console.warn('Error loading Chart.js:', error);
        }
    } else {
        console.warn('Chart.js promise not found. Chart widgets will not work.');
    }
    
    console.log('Admin dashboard initialized');
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

/**
 * Initialize dashboard with real data from API
 * This is an alternative to the server-rendered approach
 */
export async function initDashboardWithData(): Promise<void> {
    console.log('Loading dashboard data from API...');
    
    // Initialize widgets first
    initAllComparisonWidgets();
    initAllQuickLinksWidgets();
    
    // Fetch data from API
    const data = await adminDashboardService.fetchDashboardData();
    
    if (data && data.usageTrendChart) {
        // Initialize charts with data from API
        initAllCharts();
    }
    
    console.log('Dashboard loaded with API data');
}
