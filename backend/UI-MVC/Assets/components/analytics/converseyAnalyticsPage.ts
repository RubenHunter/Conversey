import {
  getChartColor,
  filterTrendByDays,
  filterTrendByDateRange,
  createTrendChart,
  createBarChart,
  setDynamicChartHeight,
  renderWorkspaceCircles,
  setupButtonToggle,
  type TrendPoint,
  type PlatformWorkspaceStat,
  type WorkspaceCircle,
} from './shared/chartHelpers';
import { fetchTrendData } from '../../services/analyticsService';

let trendChartInstance: any = null;
let trendChartData: TrendPoint[] = [];
let trendOverrideFrom: string | null = null;
let trendOverrideTo: string | null = null;

function updateTrendChart(): void {
  if (trendChartData.length === 0) return;

  let data = trendChartData;

  if (trendOverrideFrom || trendOverrideTo) {
    data = filterTrendByDateRange(data, trendOverrideFrom, trendOverrideTo);
  } else {
    const activeBtn = document.querySelector('#trend-period-buttons .bg-white') as HTMLElement | null;
    const days = activeBtn ? parseInt(activeBtn.dataset.days || '30') : 30;
    data = filterTrendByDays(data, days);
  }

  const canvas = document.getElementById('usage-trend-chart') as HTMLCanvasElement | null;
  if (!canvas) return;

  setDynamicChartHeight(canvas.parentElement, data.length, 280, 4, 600);

  trendChartInstance = createTrendChart(canvas, data, trendChartInstance);
}

async function loadTrendForWorkspace(): Promise<void> {
  const wsSelect = document.getElementById('usage-trend-workspace') as HTMLSelectElement | null;
  const workspaceId = wsSelect?.value || '';

  try {
    trendChartData = await fetchTrendData({ workspaceId });
    updateTrendChart();
  } catch (e) {
    console.error('Failed to load trend data', e);
  }
}

function createPlatformComparisonChart(data: PlatformWorkspaceStat[]): void {
  const canvas = document.getElementById('platform-comparison-chart') as HTMLCanvasElement | null;
  if (!canvas) return;

  setDynamicChartHeight(canvas.parentElement, data.length, 280, 24, 800);

  createBarChart(canvas, data.map(d => d.workspaceName), [
    { label: 'Youth', data: data.map(d => d.youthCount), backgroundColor: getChartColor(0) },
    { label: 'Ideas', data: data.map(d => d.ideaCount), backgroundColor: getChartColor(1) },
    { label: 'Answers', data: data.map(d => d.answerCount), backgroundColor: getChartColor(2) },
  ]);
}

function handleTrendPeriodClick(_btn: HTMLElement): void {
  updateTrendChart();
}

document.addEventListener('DOMContentLoaded', () => {
  const statsEl = document.getElementById('platform-stats-data');
  if (statsEl?.textContent) {
    try {
      const data: PlatformWorkspaceStat[] = JSON.parse(statsEl.textContent);
      if (data.length > 0) createPlatformComparisonChart(data);
    } catch (e) {
      console.error('Failed to parse platform stats', e);
    }
  }

  const trendEl = document.getElementById('usage-trend-data');
  if (trendEl?.textContent) {
    try {
      trendChartData = JSON.parse(trendEl.textContent);
    } catch (e) {
      console.error('Failed to parse usage trend data', e);
    }
  }

  const filterDatesEl = document.getElementById('trend-filter-dates');
  if (filterDatesEl?.textContent) {
    try {
      const fd = JSON.parse(filterDatesEl.textContent);
      if (fd.dateFrom) trendOverrideFrom = fd.dateFrom;
      if (fd.dateTo) trendOverrideTo = fd.dateTo;
    } catch {
      /* empty */
    }
  }

  if (trendChartData.length > 0) {
    const canvas = document.getElementById('usage-trend-chart') as HTMLCanvasElement | null;
    if (canvas) {
      setDynamicChartHeight(canvas.parentElement, trendChartData.length, 280, 4, 600);
    }
    updateTrendChart();
  }

  setupButtonToggle('trend-period-buttons', handleTrendPeriodClick);

  const circlesEl = document.getElementById('workspace-circles-data');
  if (circlesEl?.textContent) {
    try {
      const circlesData: WorkspaceCircle[] = JSON.parse(circlesEl.textContent);
      if (circlesData.length > 0) {
        renderWorkspaceCircles(
          document.getElementById('workspace-circles-container'),
          document.getElementById('workspace-circles-legend'),
          circlesData,
          'youth'
        );
      }
    } catch (e) {
      console.error('Failed to parse circles data', e);
    }
  }

  setupButtonToggle('circles-mode-buttons', btn => {
    const mode = btn.dataset.mode || 'youth';
    const ce = document.getElementById('workspace-circles-data');
    if (ce?.textContent) {
      try {
        const data: WorkspaceCircle[] = JSON.parse(ce.textContent);
        renderWorkspaceCircles(
          document.getElementById('workspace-circles-container'),
          document.getElementById('workspace-circles-legend'),
          data,
          mode
        );
      } catch {
        /* empty */
      }
    }
  });

  const wsSelect = document.getElementById('usage-trend-workspace') as HTMLSelectElement | null;
  wsSelect?.addEventListener('change', loadTrendForWorkspace);
});

(globalThis as any).loadTrendForWorkspace = loadTrendForWorkspace;
