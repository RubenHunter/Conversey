import { t } from '../../../utils/adminI18n';

export const CHART_PALETTE = [
  '#6366f1', '#8b5cf6', '#d946ef', '#ec4899', '#f43f5e',
  '#f97316', '#eab308', '#22c55e', '#14b8a6', '#06b6d4',
  '#3b82f6', '#a855f7', '#f472b6', '#fb923c',
] as const;

export const STATUS_COLORS: Record<string, string> = {
  Approved: '#22c55e',
  Pending: '#eab308',
  Rejected: '#f43f5e',
};

export interface TrendPoint {
  date: string;
  ideaCount: number;
  uniqueYouth: number;
}

export interface IdeaCountItem {
  label: string;
  count: number;
}

export interface PlatformWorkspaceStat {
  workspaceSlug: string;
  workspaceName: string;
  projectCount: number;
  youthCount: number;
  ideaCount: number;
  answerCount: number;
  conversionRate: number;
}

export interface WorkspaceCircle {
  name: string;
  slug: string;
  youthCount: number;
  projectCount: number;
  ideaCount: number;
}

export interface AiSummaryResponse {
  overview: string;
  trends: string[];
  minorityViews: string[];
  notableQuotes: string[];
  suggestedActions: string[];
  generatedAt?: string;
}

export function getChartColor(index: number): string {
  return CHART_PALETTE[index % CHART_PALETTE.length];
}

export function filterTrendByDays(data: TrendPoint[], days: number): TrendPoint[] {
  if (days <= 0) return data;
  const cutoff = new Date();
  cutoff.setDate(cutoff.getDate() - days);
  cutoff.setHours(0, 0, 0, 0);
  return data.filter(d => new Date(d.date + 'T00:00:00') >= cutoff);
}

export function filterTrendByDateRange(data: TrendPoint[], from: string | null, to: string | null): TrendPoint[] {
  if (!from && !to) return data;
  return data.filter(d => {
    const date = d.date;
    if (from && date < from) return false;
    if (to && date > to) return false;
    return true;
  });
}

export function renderNoDataMessage(parent: HTMLElement | null, message?: string): void {
  if (!parent) return;
  parent.innerHTML = `<div class="flex items-center justify-center h-full text-text/40 text-sm">${message || t('analytics.noData', 'No data for selected period')}</div>`;
}

export function setDynamicChartHeight(parent: HTMLElement | null, entryCount: number, minHeight: number = 280, heightPerEntry: number = 24, maxHeight: number = 600): void {
  if (!parent || entryCount <= 0) return;
  const dynamicHeight = Math.max(minHeight, Math.min(entryCount * heightPerEntry, maxHeight));
  parent.style.height = `${dynamicHeight}px`;
  parent.style.maxHeight = `${dynamicHeight}px`;
  parent.style.position = 'relative';
}

export function createTrendChart(
  canvas: HTMLCanvasElement,
  data: TrendPoint[],
  existingInstance: any = null
): any {
  if (existingInstance) {
    existingInstance.destroy();
    existingInstance = null;
  }

  if (data.length === 0) {
    renderNoDataMessage(canvas.parentElement);
    return null;
  }

  const maxIdeas = Math.max(...data.map(d => d.ideaCount), 0);
  const maxYouth = Math.max(...data.map(d => d.uniqueYouth), 0);
  const yMax = Math.max(maxIdeas, maxYouth, 1) + 1;

  return new (window as any).Chart(canvas, {
    type: 'line',
    data: {
      labels: data.map(d => d.date),
      datasets: [
        {
          label: t('analytics.ideas', 'Ideas'),
          data: data.map(d => d.ideaCount),
          borderColor: getChartColor(0),
          backgroundColor: getChartColor(0) + '20',
          fill: true,
          tension: 0.3,
          pointRadius: 2,
        },
        {
          label: t('analytics.activeYouth', 'Active Youth'),
          data: data.map(d => d.uniqueYouth),
          borderColor: getChartColor(2),
          backgroundColor: getChartColor(2) + '20',
          fill: true,
          tension: 0.3,
          pointRadius: 2,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        x: { display: true },
        y: { beginAtZero: true, max: yMax, ticks: { stepSize: 1 } },
      },
      plugins: {
        legend: { position: 'bottom' },
      },
    },
  });
}

export function createBarChart(
  canvas: HTMLCanvasElement,
  labels: string[],
  datasets: { label: string; data: number[]; backgroundColor: string | string[] }[]
): any {
  return new (window as any).Chart(canvas, {
    type: 'bar',
    data: {
      labels,
      datasets: datasets.map(d => ({ ...d, borderRadius: 4 })),
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } },
      plugins: { legend: { position: 'bottom' } },
    },
  });
}

export function createDoughnutChart(
  canvas: HTMLCanvasElement,
  labels: string[],
  data: number[],
  backgroundColor: string[]
): any {
  return new (window as any).Chart(canvas, {
    type: 'doughnut',
    data: {
      labels,
      datasets: [{ data, backgroundColor, borderWidth: 0 }],
    },
    options: {
      responsive: true,
      maintainAspectRatio: true,
      plugins: { legend: { position: 'bottom' } },
    },
  });
}

export function setupButtonToggle(
  containerId: string,
  onClick: (btn: HTMLElement) => void
): void {
  const container = document.getElementById(containerId);
  if (!container) return;

  container.querySelectorAll('button').forEach(btn => {
    btn.addEventListener('click', () => {
      container.querySelectorAll('button').forEach(b => {
        b.classList.remove('bg-white', 'text-text', 'shadow-sm');
        b.classList.add('text-text/50');
      });
      btn.classList.add('bg-white', 'text-text', 'shadow-sm');
      btn.classList.remove('text-text/50');
      onClick(btn);
    });
  });
}

export function escapeHtml(text: string): string {
  const d = document.createElement('div');
  d.textContent = text;
  return d.innerHTML;
}

export function renderAiSummary(container: HTMLElement | null, data: AiSummaryResponse): void {
  if (!container) return;

  let stalenessHtml = '';
  if (data.generatedAt) {
    const generated = new Date(data.generatedAt + 'Z');
    const diffMs = Date.now() - generated.getTime();
    const diffMin = Math.floor(diffMs / 60000);
    const ageLabel =
      diffMin < 1
        ? t('analytics.justNow', 'just now')
        : diffMin < 60
          ? `${diffMin} ${t('analytics.minutesAgo', 'minutes ago')}`
          : diffMin < 1440
            ? `${Math.floor(diffMin / 60)} ${t('analytics.hoursAgo', 'hours ago')}`
            : `${Math.floor(diffMin / 1440)} ${t('analytics.daysAgo', 'days ago')}`;

    stalenessHtml = `<div class="mb-3 text-xs text-accent bg-accent/5 rounded-lg px-3 py-1.5 inline-flex items-center gap-1.5">
      <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
      <span>${t('analytics.generatedAgo', 'Generated')} ${ageLabel} &mdash; ${t('analytics.mayBeOutdated', 'data may be outdated. Click "Generate Summary" to refresh.')}</span>
    </div>`;
  }

  container.innerHTML = `${stalenessHtml}
    <div class="space-y-4 mt-4">
      <div class="p-4 bg-background rounded-lg border border-secondary/10">
        <h3 class="text-sm font-semibold text-secondary mb-2">${t('analytics.overview', 'Overview')}</h3>
        <p class="text-sm">${escapeHtml(data.overview)}</p>
      </div>
      ${data.trends.length > 0 ? `<div class="p-4 bg-background rounded-lg border border-secondary/10"><h3 class="text-sm font-semibold text-secondary mb-2">${t('analytics.trends', 'Trends')}</h3><ul class="list-disc list-inside text-sm space-y-1">${data.trends.map(t => `<li>${escapeHtml(t)}</li>`).join('')}</ul></div>` : ''}
      ${data.minorityViews.length > 0 ? `<div class="p-4 bg-background rounded-lg border border-accent/20"><h3 class="text-sm font-semibold text-accent mb-2">${t('analytics.minorityViews', 'Minority Views')}</h3><ul class="list-disc list-inside text-sm space-y-1">${data.minorityViews.map(v => `<li>${escapeHtml(v)}</li>`).join('')}</ul></div>` : ''}
      ${data.notableQuotes.length > 0 ? `<div class="p-4 bg-background rounded-lg border border-secondary/10"><h3 class="text-sm font-semibold text-secondary mb-2">${t('analytics.notableQuotes', 'Notable Quotes')}</h3><ul class="text-sm space-y-2">${data.notableQuotes.map(q => `<li class="italic border-l-2 border-primary pl-3">"${escapeHtml(q)}"</li>`).join('')}</ul></div>` : ''}
      ${data.suggestedActions.length > 0 ? `<div class="p-4 bg-background rounded-lg border border-primary/20"><h3 class="text-sm font-semibold text-primary mb-2">${t('analytics.suggestedActions', 'Suggested Actions')}</h3><ul class="list-disc list-inside text-sm space-y-1">${data.suggestedActions.map(a => `<li>${escapeHtml(a)}</li>`).join('')}</ul></div>` : ''}
    </div>`;
}

export function renderWorkspaceCircles(
  container: HTMLElement | null,
  legend: HTMLElement | null,
  data: WorkspaceCircle[],
  mode: string
): void {
  if (!container || !legend) return;

  const sorted = [...data]
    .sort((a, b) => {
      const aVal = mode === 'projects' ? a.projectCount : a.youthCount;
      const bVal = mode === 'projects' ? b.projectCount : b.youthCount;
      return bVal - aVal;
    })
    .filter(ws => {
      const val = mode === 'projects' ? ws.projectCount : ws.youthCount;
      return val > 0;
    });

  if (sorted.length === 0) {
    container.innerHTML = `<p class="text-secondary text-sm text-center pt-12">${t('analytics.noData', 'No data available')}</p>`;
    legend.innerHTML = '';
    return;
  }

  const colors = [...CHART_PALETTE];
  const centerX = 50;
  const centerY = 50;
  const totalVal = sorted.reduce(
    (s, ws) => s + (mode === 'projects' ? ws.projectCount : ws.youthCount),
    0
  );

  let html = '';
  for (let ci = 0; ci < sorted.length; ci++) {
    const ws = sorted[ci];
    const val = mode === 'projects' ? ws.projectCount : ws.youthCount;
    const size = totalVal > 0 ? Math.max(70, Math.round((220 * val) / totalVal)) : 70;
    const color = colors[ci % colors.length];

    let tx: number, ty: number;
    if (ci === 0) {
      tx = centerX;
      ty = centerY;
    } else if (ci <= 3) {
      const angle = ci * 2.4;
      const radius = 25;
      tx = centerX + radius * Math.cos(angle);
      ty = centerY + radius * Math.sin(angle);
    } else if (ci <= 6) {
      const angle = (ci - 3.5) * 1.6;
      const radius = 42;
      tx = centerX + radius * Math.cos(angle);
      ty = centerY + radius * Math.sin(angle);
    } else {
      const angle = ci * 1.1;
      const radius = 55;
      tx = centerX + radius * Math.cos(angle);
      ty = centerY + radius * Math.sin(angle);
    }

    const nameShort = ws.name.length > 12 ? ws.name.substring(0, 12) : ws.name;
    html += `<a href="/admin/conversey/analytics/workspace/${ws.slug}" class="absolute rounded-full flex items-center justify-center text-white font-bold text-sm transition-transform hover:scale-110 hover:z-20 shadow-lg" style="width:${size}px;height:${size}px;background:${color};left:${tx}%;top:${ty}%;transform:translate(-50%,-50%);min-width:60px;min-height:60px;z-index:${10 - ci}"><span class="text-center leading-tight">${val}<br/><span class="text-[10px] opacity-75">${nameShort}</span></span></a>`;
  }
  container.innerHTML = html;

  legend.innerHTML = sorted
    .map(
      (ws, i) =>
        `<div class="flex items-center gap-1.5 text-xs text-secondary"><span class="w-3 h-3 rounded-full inline-block" style="background:${colors[i % colors.length]}"></span><span>${ws.name} (${mode === 'projects' ? ws.projectCount : ws.youthCount})</span></div>`
    )
    .join('');
}
