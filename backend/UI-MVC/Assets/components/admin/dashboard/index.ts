/**
 * Admin Dashboard Module Index
 * Main entry point for dashboard TypeScript components.
 * Exports all dashboard-related functionality.
 */

export * from './chartWidget';
export * from './comparisonWidget';
export * from './quickLinksWidget';
export * from './healthCheck';
export * from './usageTrendChart';

export { DashboardApiClient, dashboardApi } from '../../../services/dashboardApiClient';

import { initAllCharts, isChartLoaded } from './chartWidget';
import { initAllComparisonWidgets } from './comparisonWidget';
import { initAllQuickLinksWidgets } from './quickLinksWidget';
import { initHealthCheck } from './healthCheck';
import { initUsageTrendChart } from './usageTrendChart';


export function initDashboard(): void {
    initAllComparisonWidgets();
    initAllQuickLinksWidgets();
    initHealthCheck();

    if (isChartLoaded()) {
        initAllCharts();
        initUsageTrendChart();
    } else {
        console.warn('Chart.js not found. Add the CDN script to the page before module scripts.');
    }
}

