declare var Chart: {
	new(ctx: CanvasRenderingContext2D, config: Record<string, unknown>): ChartInstance;
};

interface ChartInstance {
	destroy(): void;
}

interface TimelineItem {
	date: string;
	cost: number;
}

interface AuditItem {
	date: string;
	model: string;
	cost: number;
}

const colors = ['#6c5ce7', '#e17055', '#00b894', '#0984e3', '#fdcb6e', '#e84393',
	'#636e72', '#74b9ff', '#55efc4', '#fab1a0', '#a29bfe', '#81ecec',
	'#ff7675', '#dfe6e9', '#2d3436'];

function init(): void {
	const dataEl = document.getElementById('cost-chart-data');
	if (!dataEl || !dataEl.textContent) return;

	let parsed: { timeline: TimelineItem[]; audit: AuditItem[]; timelineDays?: number };
	try {
		parsed = JSON.parse(dataEl.textContent);
	} catch {
		return;
	}

	const { timeline } = parsed;
	const timelineDays: number = parsed.timelineDays ?? 30;
	const cutoffDate = new Date();
	cutoffDate.setDate(cutoffDate.getDate() - timelineDays);
	const cutoffStr = cutoffDate.toISOString().split('T')[0];
	const audit = parsed.audit.filter(r => r.date >= cutoffStr);
	const canvas = document.getElementById('cost-timeline-chart') as HTMLCanvasElement | null;
	if (!canvas) return;

	const ctx = canvas.getContext('2d');
	if (!ctx) return;

	let currentType = 'line';
	let chart: ChartInstance | null = null;

	function buildBarData() {
		const modelDates: Record<string, number> = {};
		audit.forEach(row => {
			const key = row.model + '|' + row.date;
			modelDates[key] = (modelDates[key] || 0) + row.cost;
		});
		const models: string[] = [];
		const datesSet = new Set<string>();
		for (const k of Object.keys(modelDates)) {
			const parts = k.split('|');
			if (models.indexOf(parts[0]) === -1) models.push(parts[0]);
			datesSet.add(parts[1]);
		}
		const dates = Array.from(datesSet).sort();
		const datasets = models.map((model, i) => ({
			label: model,
			data: dates.map(d => modelDates[model + '|' + d] || 0),
			backgroundColor: colors[i % colors.length],
			borderRadius: 3
		}));
		return { labels: dates, datasets };
	}

	function buildPieData() {
		const modelCosts: Record<string, number> = {};
		audit.forEach(row => {
			modelCosts[row.model] = (modelCosts[row.model] || 0) + row.cost;
		});
		const entries = Object.entries(modelCosts).sort((a, b) => b[1] - a[1]);
		return {
			labels: entries.map(e => e[0]),
			datasets: [{
				data: entries.map(e => Math.round(e[1] * 10000) / 10000),
				backgroundColor: entries.map((_, i) => colors[i % colors.length])
			}]
		};
	}

	function renderChart(type: string): void {
		if (chart) chart.destroy();
		const isEmpty = type !== 'line' && audit.length === 0 && timeline.length === 0;

		if (isEmpty) {
			canvas!.parentElement!.innerHTML = '<div class="flex items-center justify-center h-full text-text/40 text-sm">No data for selected period</div>';
			return;
		}

		let config: Record<string, unknown>;
		if (type === 'line') {
			config = {
				type: 'line',
				data: {
					labels: timeline.map(d => d.date),
					datasets: [{
						label: 'Cost (EUR)',
						data: timeline.map(d => d.cost),
						borderColor: '#6c5ce7',
						backgroundColor: 'rgba(108,92,231,0.1)',
						fill: true, tension: 0.3,
						pointRadius: 3, pointHoverRadius: 5
					}]
				},
				options: {
					responsive: true, maintainAspectRatio: false,
					plugins: { legend: { display: false } },
					scales: {
						x: { ticks: { maxTicksLimit: 10, font: { size: 10 } }, grid: { display: false } },
						y: { ticks: { font: { size: 10 }, callback: (v: number) => '€' + v.toFixed(2) }, grid: { color: 'rgba(0,0,0,0.05)' } }
					}
				}
			};
		} else if (type === 'bar') {
			const bar = buildBarData();
			config = {
				type: 'bar',
				data: { labels: bar.labels, datasets: bar.datasets },
				options: {
					responsive: true, maintainAspectRatio: false,
					plugins: { legend: { position: 'bottom', labels: { boxWidth: 12, font: { size: 10 } } } },
					scales: {
						x: { stacked: true, ticks: { maxTicksLimit: 12, font: { size: 10 } }, grid: { display: false } },
						y: { stacked: true, ticks: { font: { size: 10 }, callback: (v: number) => '€' + v.toFixed(2) }, grid: { color: 'rgba(0,0,0,0.05)' } }
					}
				}
			};
		} else {
			const pie = buildPieData();
			config = {
				type: 'pie',
				data: { labels: pie.labels, datasets: pie.datasets },
				options: {
					responsive: true, maintainAspectRatio: false,
					plugins: {
						legend: { position: 'right', labels: { boxWidth: 12, font: { size: 10 }, padding: 12 } },
						tooltip: { callbacks: { label: (ctx: { label: string; raw: number }) => ctx.label + ': €' + ctx.raw.toFixed(4) } }
					}
				}
			};
		}

		chart = new Chart(ctx, config);
	}

	renderChart('line');

	document.querySelectorAll('.chart-toggle').forEach(btn => {
		btn.addEventListener('click', function(this: HTMLElement) {
			document.querySelectorAll('.chart-toggle').forEach(b => {
				b.className = 'chart-toggle px-3 py-1 text-xs font-medium rounded-md transition-colors text-text/50 hover:text-text';
			});
			this.className = 'chart-toggle px-3 py-1 text-xs font-medium rounded-md transition-colors bg-white text-text shadow-sm';
			currentType = this.getAttribute('data-type') || 'line';
			renderChart(currentType);
		});
	});
}

document.addEventListener('DOMContentLoaded', init);
