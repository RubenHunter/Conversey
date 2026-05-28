import { t } from '../../utils/adminI18n';

type HealthResponse = {
  status?: string;
  activeProvider?: string;
  configSource?: string;
  moderation?: { ok?: boolean };
  completions?: { ok?: boolean };
};

async function initHealthBanner(): Promise<void> {
  const banner = document.getElementById('health-banner');
  if (!banner) return;

  try {
    const response = await fetch('/api/ai/health', { headers: { Accept: 'application/json' } });
    const health = (await response.json()) as HealthResponse;

    if (health.status === 'ok') return;

    banner.className = 'mb-6 rounded-xl border border-accent/20 bg-accent/5 p-4 flex items-start gap-3';
    banner.innerHTML =
      '<svg class="w-5 h-5 text-accent mt-0.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>' +
      '<div><p class="text-sm font-medium text-accent">' + t('AI health check degraded', 'AI health check degraded') + '</p>' +
      '<p class="text-xs text-text/60 mt-1">' +
      t('Provider', 'Provider') + ': ' + (health.activeProvider ?? 'N/A') + ', ' +
      t('Source', 'Source') + ': ' + (health.configSource ?? 'N/A') + '. ' +
      t('Moderation', 'Moderation') + ': ' + (health.moderation?.ok ? 'OK' : 'FAIL') + ', ' +
      t('Completions', 'Completions') + ': ' + (health.completions?.ok ? 'OK' : 'FAIL') +
      '</p></div>';
  } catch {
  }
}

void initHealthBanner();

