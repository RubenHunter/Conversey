/**
 * Reusable Usage Trend Chart
 * Handles 7d/1m/1y/All period switching with +1 max Y-axis.
 * Expects: canvas#dashboard-usage-trend, #dashboard-trend-periods, JSON in #dashboard-usage-trend-data
 */
declare var Chart: any;

interface TrendPoint {
    date: string;
    ideaCount: number;
    uniqueYouth: number;
}

let trendChartInstance: any = null;
let initialized = false;

export function initUsageTrendChart(): void {
    if (initialized) return;

    const canvas = document.getElementById('dashboard-usage-trend') as HTMLCanvasElement | null;
    if (!canvas) return;

    const jsonEl = document.getElementById('dashboard-usage-trend-data');
    if (!jsonEl?.textContent) return;

    let data: TrendPoint[];
    try {
        data = JSON.parse(jsonEl.textContent) as TrendPoint[];
    } catch { return; }

    if (!data.length) {
        const parent = canvas.parentElement;
        if (parent) parent.innerHTML = '<p class="text-secondary text-sm text-center py-8">No data for selected period.</p>';
        initialized = true;
        return;
    }

    const allValues = data.flatMap(d => [d.ideaCount, d.uniqueYouth]);
    const yMax = allValues.length > 0 ? Math.max(...allValues) + 1 : 5;

    const labels = data.map(d => d.date);
    const ideaValues = data.map(d => d.ideaCount);
    const youthValues = data.map(d => d.uniqueYouth);

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    trendChartInstance = new Chart(ctx, {
        type: 'line',
        data: {
            labels,
            datasets: [
                {
                    label: 'Ideas',
                    data: ideaValues,
                    borderColor: '#6366f1',
                    backgroundColor: 'rgba(99,102,241,0.1)',
                    tension: 0.3,
                    fill: true,
                    pointRadius: 3,
                    pointBackgroundColor: '#6366f1',
                    pointBorderColor: '#fff'
                },
                {
                    label: 'Active Youth',
                    data: youthValues,
                    borderColor: '#f97316',
                    backgroundColor: 'rgba(249,115,22,0.1)',
                    tension: 0.3,
                    fill: true,
                    pointRadius: 3,
                    pointBackgroundColor: '#f97316',
                    pointBorderColor: '#fff'
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { position: 'bottom' }
            },
            scales: {
                x: { grid: { display: false } },
                y: {
                    beginAtZero: true,
                    max: yMax,
                    ticks: { stepSize: 1 },
                    grid: { color: 'rgba(108,92,231,0.06)' }
                }
            }
        }
    });

    setupPeriodButtons(data);
    initialized = true;
}

function setupPeriodButtons(data: TrendPoint[]): void {
    const container = document.getElementById('dashboard-trend-periods');
    if (!container || !data.length) return;

    const buttons = container.querySelectorAll<HTMLElement>('[data-days]');
    buttons.forEach(btn => {
        btn.addEventListener('click', () => {
            buttons.forEach(b => { b.classList.remove('bg-white', 'text-text', 'shadow-sm'); b.classList.add('text-text/50'); });
            btn.classList.add('bg-white', 'text-text', 'shadow-sm');
            btn.classList.remove('text-text/50');

            const days = parseInt(btn.getAttribute('data-days') || '30');
            let filtered = data;
            if (days > 0) {
                const cutoff = new Date();
                cutoff.setDate(cutoff.getDate() - days);
                const cutoffStr = cutoff.toISOString().split('T')[0];
                filtered = data.filter(d => d.date >= cutoffStr);
            }

            if (!filtered.length) {
                const cvs = document.getElementById('dashboard-usage-trend');
                const p = cvs?.parentElement;
                if (p) p.innerHTML = '<p class="text-secondary text-sm text-center py-8">No data for selected period.</p>';
                if (trendChartInstance) { trendChartInstance.destroy(); trendChartInstance = null; }
                return;
            }

            const allValues = filtered.flatMap(d => [d.ideaCount, d.uniqueYouth]);
            const yMax = Math.max(...allValues) + 1;

            if (trendChartInstance) {
                trendChartInstance.data.labels = filtered.map(d => d.date);
                trendChartInstance.data.datasets[0].data = filtered.map(d => d.ideaCount);
                trendChartInstance.data.datasets[1].data = filtered.map(d => d.uniqueYouth);
                trendChartInstance.options.scales.y.max = yMax;
                trendChartInstance.update();
            }
        });
    });
}
