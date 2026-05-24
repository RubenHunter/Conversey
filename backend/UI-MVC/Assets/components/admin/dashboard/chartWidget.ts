/**
 * Chart Widget Module
 * Handles Chart.js initialization and period tab switching for dashboard charts.
 * 
 * NOTE: Chart.js must be loaded via CDN before this module is used.
 * Add this to your layout: <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
 */

// Chart.js is loaded as a blocking CDN script in Dashboard.cshtml before module scripts run
declare var Chart: any;

/**
 * Chart data stored globally for access across modules
 */
export interface ChartData {
    [canvasId: string]: any;
}

/**
 * Period tab configuration
 */
export interface PeriodConfig {
    id: string;
    label: string;
    isActive: boolean;
}

/**
 * Chart widget configuration
 */
export interface ChartWidgetConfig {
    canvasId: string;
    type: string;
    data: any;
    options?: any;
    periods?: PeriodConfig[];
    activePeriod?: string;
    onPeriodChange?: (periodId: string) => Promise<any> | void;
}

/**
 * Cache for chart instances
 */
const chartInstances: Map<string, any> = new Map();

/**
 * Check if Chart.js is loaded
 */
export function isChartLoaded(): boolean {
    return typeof Chart !== 'undefined';
}

/**
 * Initialize a chart widget
 */
export function initChartWidget(config: ChartWidgetConfig): any | null {
    if (!isChartLoaded()) {
        console.warn('Chart.js is not loaded. Please include: <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>');
        return null;
    }

    const canvas = document.getElementById(config.canvasId) as HTMLCanvasElement;
    if (!canvas) {
        console.warn(`Chart canvas not found: ${config.canvasId}`);
        return null;
    }

    const ctx = canvas.getContext('2d');
    if (!ctx) {
        console.warn(`Could not get 2d context for canvas: ${config.canvasId}`);
        return null;
    }

    // Default options for dashboard charts
    const defaultOptions: any = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                display: false
            }
        },
        scales: {
            x: {
                grid: {
                    display: false
                }
            },
            y: {
                grid: {
                    color: 'rgba(108, 92, 231, 0.06)'
                }
            }
        }
    };

    // Merge user options with defaults
    const mergedOptions = {
        ...defaultOptions,
        ...config.options
    };

    const chartConfig: any = {
        type: config.type,
        data: config.data,
        options: mergedOptions
    };

    // Destroy existing chart if it exists
    if (chartInstances.has(config.canvasId)) {
        chartInstances.get(config.canvasId)!.destroy();
        chartInstances.delete(config.canvasId);
    }

    const chart = new Chart(ctx, chartConfig);
    chartInstances.set(config.canvasId, chart);

    // Setup period tabs if provided
    if (config.periods && config.periods.length > 0) {
        setupPeriodTabs(config);
    }

    return chart;
}

/**
 * Setup period tab click handlers
 */
function setupPeriodTabs(config: ChartWidgetConfig): void {
    const widget = document.querySelector(`[data-chart-widget="${config.canvasId}"]`);
    
    if (!widget) return;

    const periodButtons = widget.querySelectorAll<HTMLElement>('[data-period]');
    periodButtons.forEach(button => {
        button.addEventListener('click', async () => {
            const periodId = button.dataset.period;
            if (!periodId) return;

            // Update active state
            periodButtons.forEach(b => b.classList.remove('active'));
            button.classList.add('active');

            // Call period change handler if provided
            if (config.onPeriodChange) {
                try {
                    const newData = await config.onPeriodChange(periodId);
                    if (newData) {
                        updateChartData(config.canvasId, newData);
                    }
                } catch (error) {
                    console.error('Error fetching period data:', error);
                }
            }
        });
    });
}

/**
 * Update chart data
 */
export function updateChartData(canvasId: string, newData: any): void {
    const chart = chartInstances.get(canvasId);
    if (chart) {
        chart.data = newData;
        chart.update();
    }
}

/**
 * Destroy a chart instance
 */
export function destroyChart(canvasId: string): void {
    if (chartInstances.has(canvasId)) {
        chartInstances.get(canvasId)!.destroy();
        chartInstances.delete(canvasId);
    }
}

/**
 * Get chart instance by canvas ID
 */
export function getChart(canvasId: string): any | undefined {
    return chartInstances.get(canvasId);
}

/**
 * Initialize all chart widgets on the page.
 * Wires period-tab switching from pre-rendered period datasets when available.
 */
export function initAllCharts(): any[] {
    if (!isChartLoaded()) {
        console.warn('Chart.js not loaded. Skipping chart initialization.');
        return [];
    }

    const charts: any[] = [];
    const chartWidgets = document.querySelectorAll('[data-chart-widget]');

    chartWidgets.forEach(widget => {
        const canvasId = widget.getAttribute('data-chart-widget');
        if (!canvasId || !window.__ChartData?.[canvasId]) return;

        const canvas = document.getElementById(canvasId) as HTMLCanvasElement;
        if (!canvas) return;

        const stored = window.__ChartData[canvasId];

        // Patch datasets with brand colors if not already set
        const brandColors = ['#6c5ce7', '#db99c8', '#cd6f88'];
        if (stored.data && (stored.data as any).datasets) {
            (stored.data as any).datasets.forEach((ds: any, i: number) => {
                const c = brandColors[i % brandColors.length];
                if (!ds.borderColor) ds.borderColor = c;
                if (!ds.backgroundColor) ds.backgroundColor = c + '18';
                if (!ds.pointBackgroundColor) ds.pointBackgroundColor = c;
                if (!ds.pointBorderColor) ds.pointBorderColor = '#fff';
                if (!ds.tension) ds.tension = 0.4;
                if (ds.fill === undefined) ds.fill = true;
            });
        }

        const chart = initChartWidget({
            canvasId,
            type: (stored.type as string) || 'line',
            data: stored.data,
            periods: (stored.periods as PeriodConfig[]) || [],
            onPeriodChange: stored.periodData
                ? async (periodId: string) => (stored.periodData as Record<string, unknown>)[periodId] ?? null
                : undefined
        });

        if (chart) charts.push(chart);
    });

    return charts;
}
