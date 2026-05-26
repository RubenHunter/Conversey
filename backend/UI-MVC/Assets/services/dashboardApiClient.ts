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

export const dashboardApi = new DashboardApiClient();
