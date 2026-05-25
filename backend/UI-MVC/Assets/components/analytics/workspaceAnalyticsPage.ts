import {
  getChartColor,
  filterTrendByDays,
  createTrendChart,
  createBarChart,
  createDoughnutChart,
  STATUS_COLORS,
  renderAiSummary,
  setDynamicChartHeight,
  setupButtonToggle,
  type TrendPoint,
  type IdeaCountItem,
} from './shared/chartHelpers';
import { fetchAiSummary, buildExportUrl } from '../../services/analyticsService';
import { submitFormPreserveFilters } from '../../utils/formHelpers';
import { t } from '../../utils/adminI18n';

interface ChoiceQuestionStat {
  questionId: number;
  questionText: string;
  questionType: string;
  choices: { choiceId: number; choiceText: string; count: number }[];
}

interface ScaleQuestionStat {
  questionId: number;
  questionText: string;
  lowerBound: number;
  upperBound: number;
  average: number;
  count: number;
  distribution: Record<string, number>;
}

interface DashboardData {
  choiceQuestionStats: ChoiceQuestionStat[];
  scaleQuestionStats: ScaleQuestionStat[];
  openAnswers: unknown[];
  ideas: unknown[];
  ideasByTopic: IdeaCountItem[];
  ideasByStatus: IdeaCountItem[];
  ideasByCategory: IdeaCountItem[];
  participation: {
    totalYouth: number;
    youthWithAnswers: number;
    youthWithIdeas: number;
    youthWithBoth: number;
    conversionRate: number;
    avgAnswersPerYouth: number;
    avgIdeasPerYouth: number;
  };
}

(globalThis as any).submitFormPreserveFilters = submitFormPreserveFilters;

function createTopicChart(data: IdeaCountItem[]): void {
  const canvas = document.getElementById('ideas-by-topic-chart') as HTMLCanvasElement | null;
  if (!canvas) return;

  setDynamicChartHeight(canvas.parentElement, data.length, 280, 28, 600);

  createBarChart(canvas, data.map(d => d.label), [
    {
      label: t('analytics.ideas', 'Ideas'),
      data: data.map(d => d.count),
      backgroundColor: data.map((_, i) => getChartColor(i)),
    },
  ]);
}

function createStatusChart(data: IdeaCountItem[]): void {
  const canvas = document.getElementById('ideas-by-status-chart') as HTMLCanvasElement | null;
  if (!canvas) return;

  createDoughnutChart(
    canvas,
    data.map(d => d.label),
    data.map(d => d.count),
    data.map(d => STATUS_COLORS[d.label] || getChartColor(0))
  );
}

let trendChartInstance: any = null;
let trendChartData: TrendPoint[] = [];
let trendHasUrlDates = false;

function updateTrendChartPeriod(days: number): void {
  const filtered = filterTrendByDays(trendChartData, days);
  const canvas = document.getElementById('usage-trend-chart') as HTMLCanvasElement | null;
  if (!canvas) return;

  setDynamicChartHeight(canvas.parentElement, trendChartData.length, 280, 4, 600);

  trendChartInstance = createTrendChart(canvas, filtered, trendChartInstance);
}

async function handleGenerateSummary(e: Event): Promise<void> {
  e.preventDefault();
  const btn = document.getElementById('generate-summary-btn') as HTMLButtonElement | null;
  if (!btn) return;

  const originalText = btn.textContent || t('analytics.generateSummary', 'Generate Summary');
  btn.disabled = true;
  btn.textContent = t('analytics.generating', 'Generating...');

  const container = document.getElementById('ai-summary-container');
  if (container) {
    container.innerHTML = `<p class="text-secondary text-sm">${t('analytics.generating', 'Generating summary...')}</p>`;
  }

  try {
    let focus = '';
    const focusSelect = document.getElementById('ai-focus-select') as HTMLSelectElement | null;
    const focusType = focusSelect?.value || '';

    if (focusType === 'age') {
      const minAge =
        (document.getElementById('ai-focus-age-min') as HTMLInputElement)?.value || '';
      const maxAge =
        (document.getElementById('ai-focus-age-max') as HTMLInputElement)?.value || '';
      focus = `Age group ${minAge}-${maxAge}`;
    } else if (focusType === 'topic') {
      const topic =
        (document.getElementById('ai-focus-topic-input') as HTMLInputElement)?.value || '';
      focus = topic ? `Topic: ${topic}` : '';
    } else if (focusType === 'sentiment') {
      focus = 'Sentiment analysis';
    }

    const wsSlug =
      document.querySelector('[data-workspace-slug]')?.getAttribute('data-workspace-slug') ||
      window.location.hostname.split('.')[0] ||
      'unknown';

    const urlParams = new URLSearchParams(window.location.search);
    const currentFilters = {
      workspaceId: wsSlug,
      projectId: urlParams.get('projectId') || undefined,
    };

    const data = await fetchAiSummary(wsSlug, currentFilters, focus);
    renderAiSummary(container, data);
  } catch {
    if (container) {
      container.innerHTML = `<p class="text-red-500 text-sm">${t('analytics.summaryError', 'Error generating summary.')}</p>`;
    }
  } finally {
    btn.disabled = false;
    btn.textContent = originalText;
  }
}

function updateAiFocusInputs(): void {
  const focusVal = (document.getElementById('ai-focus-select') as HTMLSelectElement)?.value;
  const container = document.getElementById('ai-focus-inputs');
  if (!container) return;

  if (focusVal === 'age') {
    const agesEl =
      document.getElementById('project-age-ranges') ||
      document.getElementById('project-age-range');
    let globalMin = 0,
      globalMax = 150;
    if (agesEl) {
      try {
        const raw = JSON.parse(agesEl.textContent || '{}');
        if (Array.isArray(raw)) {
          const ages = raw as { min: number; max: number }[];
          globalMin = Math.min(...ages.map(a => a.min));
          globalMax = Math.max(...ages.map(a => a.max));
        } else {
          globalMin = (raw as { min: number; max: number }).min;
          globalMax = (raw as { min: number; max: number }).max;
        }
      } catch {
        /* empty */
      }
    }
    container.innerHTML = `<input type="number" id="ai-focus-age-min" min="${globalMin}" max="${globalMax}" placeholder="From ${globalMin}" class="w-16 rounded-lg border border-secondary/20 px-2 py-1.5 text-sm bg-background" />
                          <span class="text-xs text-secondary">to</span>
                          <input type="number" id="ai-focus-age-max" min="${globalMin}" max="${globalMax}" placeholder="To ${globalMax}" class="w-16 rounded-lg border border-secondary/20 px-2 py-1.5 text-sm bg-background" />`;
  } else if (focusVal === 'topic') {
    container.innerHTML =
      '<input type="text" id="ai-focus-topic-input" placeholder="Enter topic keyword..." class="w-40 rounded-lg border border-secondary/20 px-3 py-1.5 text-sm bg-background" />';
  } else {
    container.innerHTML = '';
  }
}

document.addEventListener('DOMContentLoaded', () => {
  const dashboardEl = document.getElementById('analytics-dashboard-data');
  if (dashboardEl?.textContent) {
    try {
      const data: DashboardData = JSON.parse(dashboardEl.textContent);
      if (data.ideasByTopic.length > 0) createTopicChart(data.ideasByTopic);
      if (data.ideasByStatus.length > 0) createStatusChart(data.ideasByStatus);
    } catch (e) {
      console.error('Failed to parse dashboard data', e);
    }
  }

  const trendEl = document.getElementById('usage-trend-data');
  if (trendEl?.textContent) {
    try {
      const data: TrendPoint[] = JSON.parse(trendEl.textContent);
      trendChartData = data;
      const urlParams = new URLSearchParams(window.location.search);
      trendHasUrlDates = !!(urlParams.get('dateFrom') || urlParams.get('dateTo'));
      if (data.length > 0) {
        const canvas = document.getElementById('usage-trend-chart') as HTMLCanvasElement | null;
        if (canvas) {
          setDynamicChartHeight(canvas.parentElement, data.length, 280, 4, 600);
          if (trendHasUrlDates) {
            trendChartInstance = createTrendChart(canvas, data, trendChartInstance);
          } else {
            trendChartInstance = createTrendChart(
              canvas,
              filterTrendByDays(data, 30),
              trendChartInstance
            );
          }
        }
      }
    } catch (e) {
      console.error('Failed to parse usage trend data', e);
    }
  }

  setupButtonToggle('trend-period-buttons', btn => {
    const days = parseInt(btn.getAttribute('data-days') || '30');
    if (trendHasUrlDates && days > 0) return;
    updateTrendChartPeriod(days);
  });

  document.getElementById('ai-focus-select')?.addEventListener('change', updateAiFocusInputs);
  document
    .getElementById('generate-summary-btn')
    ?.addEventListener('click', handleGenerateSummary);

  const exportBtn = document.getElementById('export-dropdown-btn');
  const exportMenu = document.getElementById('export-dropdown-menu');
  if (exportBtn && exportMenu) {
    exportBtn.addEventListener('click', e => {
      e.stopPropagation();
      exportMenu.classList.toggle('hidden');
    });
    document.addEventListener('click', () => exportMenu.classList.add('hidden'));
    exportMenu.addEventListener('click', e => e.stopPropagation());
  }

  document.querySelectorAll('.export-link').forEach(link => {
    link.addEventListener('click', e => {
      e.preventDefault();
      const el = link as HTMLElement;
      const type = el.dataset.exportType || 'combined';
      const filterAware = el.dataset.filterAware === 'true';

      const wsSlug =
        document.querySelector('[data-workspace-slug]')?.getAttribute('data-workspace-slug') ||
        window.location.hostname.split('.')[0] ||
        'unknown';

      const urlParams = new URLSearchParams(window.location.search);
      const filters: Record<string, string | undefined> = {
        projectId: urlParams.get('projectId') || undefined,
      };

      if (filterAware) {
        filters.dateFrom =
          (document.querySelector('input[name="dateFrom"]') as HTMLInputElement)?.value;
        filters.dateTo =
          (document.querySelector('input[name="dateTo"]') as HTMLInputElement)?.value;
        filters.topicId =
          (document.querySelector('select[name="topicId"]') as HTMLSelectElement)?.value;
        filters.status =
          (document.querySelector('select[name="status"]') as HTMLSelectElement)?.value;
      }

      window.location.href = buildExportUrl(wsSlug, type, filters);
      exportMenu?.classList.add('hidden');
    });
  });
});
