/**
 * Chart Widget Module
 * Handles Chart.js initialization and period tab switching for dashboard charts.
 */

import { Chart, ChartConfiguration, registerables } from 'chart.js';

// Register all Chart.js components
Chart.register(...registerables);

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
const chartInstances: Map<string, Chart> = new Map();

/**
 * Initialize a chart widget
 */
export function initChartWidget(config: ChartWidgetConfig): Chart | null {
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
    const defaultOptions: ChartConfiguration['options'] = {
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
                    color: 'rgba(0, 0, 0, 0.05)'
                }
            }
        }
    };

    // Merge user options with defaults
    const mergedOptions = {
        ...defaultOptions,
        ...config.options
    };

    const chartConfig: ChartConfiguration = {
        type: config.type as any,
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
    const widget = document.querySelector(`[data-chart-id="${config.canvasId}"]`) ?? 
                  document.querySelector(`canvas[id="${config.canvasId}"]`).closest('.chart-widget');
    
    if (!widget) return;

    const periodButtons = widget.querySelectorAll<HTMLElement>('[data-period]');
    periodButtons.forEach(button => {
        button.addEventListener('click', async () => {
            const periodId = button.dataset.period;
            if (!periodId) return;

            // Update active state
            periodButtons.forEach(b => {
                b.classList.remove('bg-primary/10', 'border-primary/30');
                b.classList.add('hover:bg-secondary/5');
            });
            button.classList.add('bg-primary/10', 'border-primary/30');
            button.classList.remove('hover:bg-secondary/5');

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
export function getChart(canvasId: string): Chart | undefined {
    return chartInstances.get(canvasId);
}

/**
 * Initialize all chart widgets on the page
 * Auto-discovers canvas elements with chart data
 */
export function initAllCharts(): Chart[] {
    const charts: Chart[] = [];
    
    // Find all canvas elements that are part of chart widgets
    const canvasElements = document.querySelectorAll<HTMLCanvasElement>('canvas[id]');
    
    canvasElements.forEach(canvas => {
        const canvasId = canvas.id;
        if (canvasId && window.__ChartData?.[canvasId]) {
            const chart = initChartWidget({
                canvasId,
                type: 'line', // default, will be overridden by data
                data: window.__ChartData[canvasId]
            });
            if (chart) {
                charts.push(chart);
            }
        }
    });
    
    return charts;
}
