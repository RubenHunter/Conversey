interface PlatformWorkspaceStat {
  workspaceSlug: string;
  workspaceName: string;
  projectCount: number;
  youthCount: number;
  ideaCount: number;
  answerCount: number;
  conversionRate: number;
}

interface UsageTrendPoint {
  date: string;
  ideaCount: number;
  uniqueYouth: number;
}

interface WorkspaceCircle {
  name: string;
  slug: string;
  youthCount: number;
  projectCount: number;
  ideaCount: number;
}

const palette = [
  '#6366f1', '#8b5cf6', '#d946ef', '#ec4899', '#f43f5e',
  '#f97316', '#eab308', '#22c55e', '#14b8a6', '#06b6d4',
  '#3b82f6', '#6366f1', '#a855f7', '#f472b6', '#fb923c'
];

function getColor(index: number): string {
  return palette[index % palette.length];
}

let trendChartInstance: any = null;
let trendChartData: UsageTrendPoint[] = [];
let trendOverrideFrom: string | null = null;
let trendOverrideTo: string | null = null;

function filterTrendByDays(data: UsageTrendPoint[], days: number): UsageTrendPoint[] {
  if (days <= 0) return data;
  const cutoff = new Date();
  cutoff.setDate(cutoff.getDate() - days);
  cutoff.setHours(0, 0, 0, 0);
  return data.filter(d => new Date(d.date + 'T00:00:00') >= cutoff);
}

function filterTrendByDateRange(data: UsageTrendPoint[], from: string | null, to: string | null): UsageTrendPoint[] {
  if (!from && !to) return data;
  return data.filter(d => {
    const date = d.date;
    if (from && date < from) return false;
    if (to && date > to) return false;
    return true;
  });
}

function createPlatformComparisonChart(data: PlatformWorkspaceStat[]): void {
  const canvas = document.getElementById('platform-comparison-chart') as HTMLCanvasElement | null;
  if (!canvas) return;

  new (window as any).Chart(canvas, {
    type: 'bar',
    data: {
      labels: data.map(d => d.workspaceName),
      datasets: [
        { label: 'Youth', data: data.map(d => d.youthCount), backgroundColor: getColor(0), borderRadius: 4 },
        { label: 'Ideas', data: data.map(d => d.ideaCount), backgroundColor: getColor(1), borderRadius: 4 },
        { label: 'Answers', data: data.map(d => d.answerCount), backgroundColor: getColor(2), borderRadius: 4 }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } },
      plugins: { legend: { position: 'bottom' } }
    }
  });
}

function createUsageTrendChart(data: UsageTrendPoint[]): void {
  const canvas = document.getElementById('usage-trend-chart') as HTMLCanvasElement | null;
  if (!canvas) return;

  if (trendChartInstance) { trendChartInstance.destroy(); trendChartInstance = null; }

  if (data.length === 0) {
    const parent = canvas.parentElement;
    if (parent) parent.innerHTML = '<div class="flex items-center justify-center h-full text-text/40 text-sm">No data for selected period</div>';
    return;
  }

  const maxIdeas = Math.max(...data.map(d => d.ideaCount), 0);
  const maxYouth = Math.max(...data.map(d => d.uniqueYouth), 0);
  const yMax = Math.max(maxIdeas, maxYouth, 1) + 1;

  trendChartInstance = new (window as any).Chart(canvas, {
    type: 'line',
    data: {
      labels: data.map(d => d.date),
      datasets: [
        { label: 'Ideas', data: data.map(d => d.ideaCount), borderColor: getColor(0), backgroundColor: getColor(0) + '20', fill: true, tension: 0.3, pointRadius: 2 },
        { label: 'Active Youth', data: data.map(d => d.uniqueYouth), borderColor: getColor(2), backgroundColor: getColor(2) + '20', fill: true, tension: 0.3, pointRadius: 2 }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      scales: { x: { display: true }, y: { beginAtZero: true, max: yMax, ticks: { stepSize: 1 } } },
      plugins: { legend: { position: 'bottom' } }
    }
  });
}

function updateTrendChart(): void {
  if (trendChartData.length === 0) return;

  let data = trendChartData;

  if (trendOverrideFrom || trendOverrideTo) {
    data = filterTrendByDateRange(data, trendOverrideFrom, trendOverrideTo);
  } else {
    const activeBtn = document.querySelector('#trend-period-buttons .bg-white') as HTMLElement;
    const days = activeBtn ? parseInt(activeBtn.dataset.days || '30') : 30;
    data = filterTrendByDays(data, days);
  }

  createUsageTrendChart(data);
}

function renderWorkspaceCircles(data: WorkspaceCircle[], mode: string): void {
  const container = document.getElementById('workspace-circles-container');
  const legend = document.getElementById('workspace-circles-legend');
  if (!container || !legend) return;

  const sorted = [...data].sort((a, b) => {
    const aVal = mode === 'projects' ? a.projectCount : a.youthCount;
    const bVal = mode === 'projects' ? b.projectCount : b.youthCount;
    return bVal - aVal;
  }).filter(ws => {
    const val = mode === 'projects' ? ws.projectCount : ws.youthCount;
    return val > 0;
  });

  if (sorted.length === 0) {
    container.innerHTML = '<p class="text-secondary text-sm text-center pt-12">No data available</p>';
    legend.innerHTML = '';
    return;
  }

  const colors = palette;
  const centerX = 50, centerY = 50;
  const totalVal = sorted.reduce((s, ws) => s + (mode === 'projects' ? ws.projectCount : ws.youthCount), 0);

  let html = '';
  for (let ci = 0; ci < sorted.length; ci++) {
    const ws = sorted[ci];
    const val = mode === 'projects' ? ws.projectCount : ws.youthCount;
    const size = totalVal > 0 ? Math.max(70, Math.round(220 * val / totalVal)) : 70;
    const color = colors[ci % colors.length];

    let tx: number, ty: number;
    if (ci === 0) { tx = centerX; ty = centerY; }
    else if (ci <= 3) { const angle = ci * 2.4; const radius = 25; tx = centerX + radius * Math.cos(angle); ty = centerY + radius * Math.sin(angle); }
    else if (ci <= 6) { const angle = (ci - 3.5) * 1.6; const radius = 42; tx = centerX + radius * Math.cos(angle); ty = centerY + radius * Math.sin(angle); }
    else { const angle = ci * 1.1; const radius = 55; tx = centerX + radius * Math.cos(angle); ty = centerY + radius * Math.sin(angle); }

    const nameShort = ws.name.length > 12 ? ws.name.substring(0, 12) : ws.name;
    html += `<a href="/admin/conversey/analytics/workspace/${ws.slug}" class="absolute rounded-full flex items-center justify-center text-white font-bold text-sm transition-transform hover:scale-110 hover:z-20 shadow-lg" style="width:${size}px;height:${size}px;background:${color};left:${tx}%;top:${ty}%;transform:translate(-50%,-50%);min-width:60px;min-height:60px;z-index:${10-ci}"><span class="text-center leading-tight">${val}<br/><span class="text-[10px] opacity-75">${nameShort}</span></span></a>`;
  }
  container.innerHTML = html;

  legend.innerHTML = sorted.map((ws, i) =>
    `<div class="flex items-center gap-1.5 text-xs text-secondary"><span class="w-3 h-3 rounded-full inline-block" style="background:${colors[i % colors.length]}"></span><span>${ws.name} (${mode === 'projects' ? ws.projectCount : ws.youthCount})</span></div>`
  ).join('');
}

async function loadTrendForWorkspace(): Promise<void> {
  const wsSelect = document.getElementById('usage-trend-workspace') as HTMLSelectElement;
  const workspaceId = wsSelect?.value || '';
  const params = new URLSearchParams();
  if (workspaceId) params.set('workspaceId', workspaceId);

  try {
    const resp = await fetch(`/api/admin/analytics/usage-trend?${params.toString()}`);
    if (resp.ok) {
      trendChartData = await resp.json();
      updateTrendChart();
    }
  } catch (e) {
    console.error('Failed to load trend data', e);
  }
}

document.addEventListener('DOMContentLoaded', () => {
  const statsEl = document.getElementById('platform-stats-data');
  if (statsEl?.textContent) {
    try {
      const data: PlatformWorkspaceStat[] = JSON.parse(statsEl.textContent);
      if (data.length > 0) createPlatformComparisonChart(data);
    } catch (e) { console.error('Failed to parse platform stats', e); }
  }

  const trendEl = document.getElementById('usage-trend-data');
  if (trendEl?.textContent) {
    try {
      trendChartData = JSON.parse(trendEl.textContent);
    } catch (e) { console.error('Failed to parse usage trend data', e); }
  }

  const filterDatesEl = document.getElementById('trend-filter-dates');
  if (filterDatesEl?.textContent) {
    try {
      const fd = JSON.parse(filterDatesEl.textContent);
      if (fd.dateFrom) trendOverrideFrom = fd.dateFrom;
      if (fd.dateTo) trendOverrideTo = fd.dateTo;
    } catch {}
  }

  if (trendChartData.length > 0) updateTrendChart();

  document.querySelectorAll('#trend-period-buttons button').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('#trend-period-buttons button').forEach(b => { b.classList.remove('bg-white', 'text-text', 'shadow-sm'); b.classList.add('text-text/50'); });
      btn.classList.add('bg-white', 'text-text', 'shadow-sm');
      btn.classList.remove('text-text/50');
      updateTrendChart();
    });
  });

  const circlesEl = document.getElementById('workspace-circles-data');
  if (circlesEl?.textContent) {
    try {
      const circlesData: WorkspaceCircle[] = JSON.parse(circlesEl.textContent);
      if (circlesData.length > 0) renderWorkspaceCircles(circlesData, 'youth');
    } catch (e) { console.error('Failed to parse circles data', e); }
  }

  document.querySelectorAll('#circles-mode-buttons button').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('#circles-mode-buttons button').forEach(b => { b.classList.remove('bg-white', 'text-text', 'shadow-sm'); b.classList.add('text-text/50'); });
      btn.classList.add('bg-white', 'text-text', 'shadow-sm');
      btn.classList.remove('text-text/50');
      const mode = btn.dataset.mode || 'youth';
      const ce = document.getElementById('workspace-circles-data');
      if (ce?.textContent) {
        try {
          const data: WorkspaceCircle[] = JSON.parse(ce.textContent);
          renderWorkspaceCircles(data, mode);
        } catch {}
      }
    });
  });
});
