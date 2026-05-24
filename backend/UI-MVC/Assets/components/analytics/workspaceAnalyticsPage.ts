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

interface IdeaCount {
  label: string;
  count: number;
}

interface DashboardData {
  choiceQuestionStats: ChoiceQuestionStat[];
  scaleQuestionStats: ScaleQuestionStat[];
  openAnswers: unknown[];
  ideas: unknown[];
  ideasByTopic: IdeaCount[];
  ideasByStatus: IdeaCount[];
  ideasByCategory: IdeaCount[];
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

interface AiSummaryResponse {
  overview: string;
  trends: string[];
  minorityViews: string[];
  notableQuotes: string[];
  suggestedActions: string[];
  generatedAt?: string;
}

const palette = [
  '#6366f1', '#8b5cf6', '#d946ef', '#ec4899', '#f43f5e',
  '#f97316', '#eab308', '#22c55e', '#14b8a6', '#06b6d4',
  '#3b82f6', '#6366f1', '#a855f7', '#f472b6', '#fb923c'
];

function getColor(index: number): string { return palette[index % palette.length]; }

(globalThis as any).submitFormPreserveFilters = function(form: HTMLFormElement): void {
  const dateFrom = (form.querySelector('input[name="dateFrom"]') as HTMLInputElement)?.value || '';
  const dateTo = (form.querySelector('input[name="dateTo"]') as HTMLInputElement)?.value || '';
  const topicId = (form.querySelector('select[name="topicId"]') as HTMLSelectElement)?.value || '';
  const status = (form.querySelector('select[name="status"]') as HTMLSelectElement)?.value || '';
  if (dateFrom) { const df = document.createElement('input'); df.type = 'hidden'; df.name = 'dateFrom'; df.value = dateFrom; form.appendChild(df); }
  if (dateTo) { const dt = document.createElement('input'); dt.type = 'hidden'; dt.name = 'dateTo'; dt.value = dateTo; form.appendChild(dt); }
  if (topicId) { const ti = document.createElement('input'); ti.type = 'hidden'; ti.name = 'topicId'; ti.value = topicId; form.appendChild(ti); }
  if (status) { const si = document.createElement('input'); si.type = 'hidden'; si.name = 'status'; si.value = status; form.appendChild(si); }
  form.submit();
};

function createTopicChart(data: IdeaCount[]): void {
  const canvas = document.getElementById('ideas-by-topic-chart') as HTMLCanvasElement | null;
  if (!canvas || !canvas.parentElement) return;
  canvas.style.maxWidth = canvas.parentElement.clientWidth + 'px';
  canvas.style.maxHeight = '280px';
  new (window as any).Chart(canvas, {
    type: 'bar',
    data: {
      labels: data.map(d => d.label),
      datasets: [{ label: 'Ideas', data: data.map(d => d.count), backgroundColor: data.map((_, i) => getColor(i)), borderRadius: 4 }]
    },
    options: { responsive: true, maintainAspectRatio: true, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } } }
  });
}

function createStatusChart(data: IdeaCount[]): void {
  const canvas = document.getElementById('ideas-by-status-chart') as HTMLCanvasElement | null;
  if (!canvas || !canvas.parentElement) return;
  canvas.style.maxWidth = canvas.parentElement.clientWidth + 'px';
  canvas.style.maxHeight = '280px';
  const statusColors: Record<string, string> = { 'Approved': '#22c55e', 'Pending': '#eab308', 'Rejected': '#f43f5e' };
  new (window as any).Chart(canvas, {
    type: 'doughnut',
    data: {
      labels: data.map(d => d.label),
      datasets: [{ data: data.map(d => d.count), backgroundColor: data.map(d => statusColors[d.label] || getColor(0)), borderWidth: 0 }]
    },
    options: { responsive: true, maintainAspectRatio: true, plugins: { legend: { position: 'bottom' } } }
  });
}

function renderSummary(data: AiSummaryResponse): void {
  const container = document.getElementById('ai-summary-container');
  if (!container) return;

  let stalenessHtml = '';
  if (data.generatedAt) {
    const generated = new Date(data.generatedAt + 'Z');
    const diffMs = Date.now() - generated.getTime();
    const diffMin = Math.floor(diffMs / 60000);
    const ageLabel = diffMin < 1 ? 'just now' :
      diffMin < 60 ? `${diffMin} minutes ago` :
      diffMin < 1440 ? `${Math.floor(diffMin / 60)} hours ago` :
      `${Math.floor(diffMin / 1440)} days ago`;
    stalenessHtml = `<div class="mb-3 text-xs text-accent bg-accent/5 rounded-lg px-3 py-1.5 inline-flex items-center gap-1.5">
      <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
      <span>Generated ${ageLabel} &mdash; data may be outdated. Click "Generate Summary" to refresh.</span>
    </div>`;
  }

  container.innerHTML = `${stalenessHtml}
    <div class="space-y-4 mt-4">
      <div class="p-4 bg-background rounded-lg border border-secondary/10">
        <h3 class="text-sm font-semibold text-secondary mb-2">Overview</h3>
        <p class="text-sm">${escapeHtml(data.overview)}</p>
      </div>
      ${data.trends.length > 0 ? `<div class="p-4 bg-background rounded-lg border border-secondary/10"><h3 class="text-sm font-semibold text-secondary mb-2">Trends</h3><ul class="list-disc list-inside text-sm space-y-1">${data.trends.map(t => `<li>${escapeHtml(t)}</li>`).join('')}</ul></div>` : ''}
      ${data.minorityViews.length > 0 ? `<div class="p-4 bg-background rounded-lg border border-accent/20"><h3 class="text-sm font-semibold text-accent mb-2">Minority Views</h3><ul class="list-disc list-inside text-sm space-y-1">${data.minorityViews.map(v => `<li>${escapeHtml(v)}</li>`).join('')}</ul></div>` : ''}
      ${data.notableQuotes.length > 0 ? `<div class="p-4 bg-background rounded-lg border border-secondary/10"><h3 class="text-sm font-semibold text-secondary mb-2">Notable Quotes</h3><ul class="text-sm space-y-2">${data.notableQuotes.map(q => `<li class="italic border-l-2 border-primary pl-3">"${escapeHtml(q)}"</li>`).join('')}</ul></div>` : ''}
      ${data.suggestedActions.length > 0 ? `<div class="p-4 bg-background rounded-lg border border-primary/20"><h3 class="text-sm font-semibold text-primary mb-2">Suggested Actions</h3><ul class="list-disc list-inside text-sm space-y-1">${data.suggestedActions.map(a => `<li>${escapeHtml(a)}</li>`).join('')}</ul></div>` : ''}
    </div>`;
}

function escapeHtml(text: string): string { const d = document.createElement('div'); d.textContent = text; return d.innerHTML; }

async function fetchAiSummary(e: Event): Promise<void> {
  e.preventDefault();
  const btn = document.getElementById('generate-summary-btn') as HTMLButtonElement;
  const originalText = btn.textContent || 'Generate Summary';
  btn.disabled = true; btn.textContent = 'Generating...';

  const container = document.getElementById('ai-summary-container');
  if (container) container.innerHTML = '<p class="text-secondary text-sm">Generating summary...</p>';

  try {
    const focusSelect = document.getElementById('ai-focus-select') as HTMLSelectElement;
    const focusType = focusSelect?.value || '';
    let focus = '';
    if (focusType === 'age') {
      const minAge = (document.getElementById('ai-focus-age-min') as HTMLInputElement)?.value || '';
      const maxAge = (document.getElementById('ai-focus-age-max') as HTMLInputElement)?.value || '';
      focus = `Age group ${minAge}-${maxAge}`;
    } else if (focusType === 'topic') {
      const topic = (document.getElementById('ai-focus-topic-input') as HTMLInputElement)?.value || '';
      focus = topic ? `Topic: ${topic}` : '';
    } else if (focusType === 'sentiment') {
      focus = 'Sentiment analysis';
    }

    const wsSlug = document.querySelector('[data-workspace-slug]')?.getAttribute('data-workspace-slug')
      || window.location.hostname.split('.')[0] || 'unknown';

    const params = new URLSearchParams();
    params.set('workspaceId', wsSlug);
    const urlParams = new URLSearchParams(window.location.search);
    const projectId = urlParams.get('projectId');
    if (projectId) params.set('projectId', projectId);

    const dateFrom = urlParams.get('dateFrom') || (document.querySelector('input[name="dateFrom"]') as HTMLInputElement)?.value;
    const dateTo = urlParams.get('dateTo') || (document.querySelector('input[name="dateTo"]') as HTMLInputElement)?.value;
    const topicId = urlParams.get('topicId') || (document.querySelector('select[name="topicId"]') as HTMLSelectElement)?.value;
    const status = urlParams.get('status') || (document.querySelector('select[name="status"]') as HTMLSelectElement)?.value;
    if (dateFrom) params.set('dateFrom', dateFrom);
    if (dateTo) params.set('dateTo', dateTo);
    if (topicId) params.set('topicId', topicId);
    if (status) params.set('status', status);

    const resp = await fetch(`/api/admin/analytics/ai-summary?${params.toString()}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ focus, language: 'English' })
    });
    if (resp.ok) { const data: AiSummaryResponse = await resp.json(); renderSummary(data); }
    else { if (container) container.innerHTML = '<p class="text-red-500 text-sm">Failed to generate summary.</p>'; }
  } catch { if (container) container.innerHTML = '<p class="text-red-500 text-sm">Error generating summary.</p>'; }
  finally { btn.disabled = false; btn.textContent = originalText; }
}

function updateAiFocusInputs(): void {
  const focusVal = (document.getElementById('ai-focus-select') as HTMLSelectElement)?.value;
  const container = document.getElementById('ai-focus-inputs');
  if (!container) return;

  if (focusVal === 'age') {
    const agesEl = document.getElementById('project-age-ranges') || document.getElementById('project-age-range');
    let globalMin = 0, globalMax = 150;
    if (agesEl) {
      try {
        const data = JSON.parse(agesEl.textContent || '{}');
        if (Array.isArray(data)) {
          const ages = data as {min:number,max:number}[];
          globalMin = Math.min(...ages.map(a => a.min));
          globalMax = Math.max(...ages.map(a => a.max));
        } else {
          globalMin = (data as {min:number,max:number}).min;
          globalMax = (data as {min:number,max:number}).max;
        }
      } catch {}
    }
    container.innerHTML = `<input type="number" id="ai-focus-age-min" min="${globalMin}" max="${globalMax}" placeholder="From ${globalMin}" class="w-16 rounded-lg border border-secondary/20 px-2 py-1.5 text-sm bg-background" />
                          <span class="text-xs text-secondary">to</span>
                          <input type="number" id="ai-focus-age-max" min="${globalMin}" max="${globalMax}" placeholder="To ${globalMax}" class="w-16 rounded-lg border border-secondary/20 px-2 py-1.5 text-sm bg-background" />`;
  } else if (focusVal === 'topic') {
    container.innerHTML = '<input type="text" id="ai-focus-topic-input" placeholder="Enter topic keyword..." class="w-40 rounded-lg border border-secondary/20 px-3 py-1.5 text-sm bg-background" />';
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
    } catch (e) { console.error('Failed to parse dashboard data', e); }
  }

  document.getElementById('ai-focus-select')?.addEventListener('change', updateAiFocusInputs);
  document.getElementById('generate-summary-btn')?.addEventListener('click', fetchAiSummary);

  const exportBtn = document.getElementById('export-dropdown-btn');
  const exportMenu = document.getElementById('export-dropdown-menu');
  if (exportBtn && exportMenu) {
    exportBtn.addEventListener('click', (e) => { e.stopPropagation(); exportMenu.classList.toggle('hidden'); });
    document.addEventListener('click', () => exportMenu.classList.add('hidden'));
    exportMenu.addEventListener('click', (e) => e.stopPropagation());
  }

  document.querySelectorAll('.export-link').forEach(link => {
    link.addEventListener('click', (e) => {
      e.preventDefault();
      const el = link as HTMLElement;
      const type = el.dataset.exportType;
      const filterAware = el.dataset.filterAware === 'true';
      const wsSlug = document.querySelector('[data-workspace-slug]')?.getAttribute('data-workspace-slug')
        || window.location.hostname.split('.')[0] || 'unknown';
      const params = new URLSearchParams();
      params.set('workspaceId', wsSlug);
      if (type) params.set('type', type);
      const urlParams = new URLSearchParams(window.location.search);
      const projectId = urlParams.get('projectId');
      if (projectId) params.set('projectId', projectId);
      if (filterAware) {
        const dateFrom = (document.querySelector('input[name="dateFrom"]') as HTMLInputElement)?.value;
        const dateTo = (document.querySelector('input[name="dateTo"]') as HTMLInputElement)?.value;
        const topicId = (document.querySelector('select[name="topicId"]') as HTMLSelectElement)?.value;
        const status = (document.querySelector('select[name="status"]') as HTMLSelectElement)?.value;
        if (dateFrom) params.set('dateFrom', dateFrom);
        if (dateTo) params.set('dateTo', dateTo);
        if (topicId) params.set('topicId', topicId);
        if (status) params.set('status', status);
      }
      window.location.href = `/api/admin/analytics/export?${params.toString()}`;
      exportMenu?.classList.add('hidden');
    });
  });
});
