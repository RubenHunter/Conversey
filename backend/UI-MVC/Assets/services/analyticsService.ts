import type { TrendPoint, AiSummaryResponse } from '../components/analytics/shared/chartHelpers';

export interface AnalyticsFilters {
  workspaceId?: string;
  projectId?: string;
  dateFrom?: string;
  dateTo?: string;
  topicId?: string;
  status?: string;
}

export async function fetchTrendData(filters: AnalyticsFilters = {}): Promise<TrendPoint[]> {
  const params = new URLSearchParams();
  if (filters.workspaceId) params.set('workspaceId', filters.workspaceId);
  if (filters.projectId) params.set('projectId', filters.projectId);
  if (filters.dateFrom) params.set('from', filters.dateFrom);
  if (filters.dateTo) params.set('to', filters.dateTo);

  const resp = await fetch(`/api/admin/analytics/usage-trend?${params.toString()}`);
  if (!resp.ok) throw new Error(`Failed to load trend data: ${resp.statusText}`);
  return resp.json();
}

export async function fetchAiSummary(
  workspaceSlug: string,
  filters: AnalyticsFilters = {},
  focus?: string,
  focusDetails?: Record<string, string>
): Promise<AiSummaryResponse> {
  const params = new URLSearchParams();
  params.set('workspaceId', workspaceSlug);
  if (filters.projectId) params.set('projectId', filters.projectId);
  if (filters.dateFrom) params.set('dateFrom', filters.dateFrom);
  if (filters.dateTo) params.set('dateTo', filters.dateTo);
  if (filters.topicId) params.set('topicId', filters.topicId);
  if (filters.status) params.set('status', filters.status);

  const body: Record<string, unknown> = { focus: focus || '', language: 'English' };
  if (focusDetails) Object.assign(body, focusDetails);

  const resp = await fetch(`/api/admin/analytics/ai-summary?${params.toString()}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });

  if (!resp.ok) throw new Error(`Failed to generate summary: ${resp.statusText}`);
  return resp.json();
}

export async function fetchModerate(
  type: string,
  id: number,
  action: string,
  reason?: string | null
): Promise<boolean> {
  const resp = await fetch('/api/admin/analytics/moderate', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ type, id, action, reason }),
  });
  return resp.ok;
}

export function buildExportUrl(
  workspaceSlug: string,
  type: string,
  filters: AnalyticsFilters = {}
): string {
  const params = new URLSearchParams();
  params.set('workspaceId', workspaceSlug);
  params.set('type', type);
  if (filters.projectId) params.set('projectId', filters.projectId);
  if (filters.dateFrom) params.set('dateFrom', filters.dateFrom);
  if (filters.dateTo) params.set('dateTo', filters.dateTo);
  if (filters.topicId) params.set('topicId', filters.topicId);
  if (filters.status) params.set('status', filters.status);
  return `/api/admin/analytics/export?${params.toString()}`;
}
