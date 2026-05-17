import { t } from '../../utils/adminI18n';

type HealthResponse = {
  status?: string;
  activeProvider?: string;
  moderation?: { ok?: boolean };
  completions?: { ok?: boolean };
};

function cleanupSetupStateFromQuery(): void {
  const params = new URLSearchParams(window.location.search);
  if (params.get('setupCleared') !== '1') return;

  try {
    localStorage.removeItem('conversey-ai-provider-setup-state-v1');
  } catch {
  }

  params.delete('setupCleared');
  const nextUrl = window.location.pathname + (params.toString() ? `?${params.toString()}` : '');
  window.history.replaceState({}, document.title, nextUrl);
}

function initDeleteConfirmations(): void {
  const forms = document.querySelectorAll<HTMLFormElement>('form[data-confirm-delete-provider]');
  forms.forEach((form) => {
    form.addEventListener('submit', (event) => {
      const provider = form.dataset.providerName ?? '';
      const msg = t('Are you sure you want to delete this provider?', 'Are you sure you want to delete this provider?');
      if (!window.confirm(msg.replace('{0}', provider))) {
        event.preventDefault();
      }
    });
  });
}

function initHealthCheck(): void {
  const btn = document.getElementById('healthCheckBtn') as HTMLButtonElement | null;
  const resultDiv = document.getElementById('healthCheckResult');
  if (!btn || !resultDiv) return;

  const labels = {
    testing: t('Testing...', 'Testing...'),
    allOk: t('All probes successful', 'All probes successful'),
    failed: t('Health check failed', 'Health check failed'),
    testHealth: t('Test Health', 'Test Health'),
    provider: t('Provider', 'Provider'),
    moderation: t('Moderation', 'Moderation'),
    completions: t('Completions', 'Completions')
  };

  btn.addEventListener('click', async () => {
    btn.disabled = true;
    btn.innerHTML =
      '<svg class="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path></svg> ' +
      labels.testing;

    try {
      const response = await fetch('/api/ai/health', { method: 'GET', headers: { Accept: 'application/json' } });
      const data = (await response.json()) as HealthResponse;
      const isOk = data.status === 'ok';

      resultDiv.className = `mb-6 rounded-xl border p-4 ${isOk ? 'border-green-200 bg-green-50' : 'border-red-200 bg-red-50'}`;
      resultDiv.innerHTML =
        `<p class="text-sm font-medium ${isOk ? 'text-green-700' : 'text-red-700'}">${isOk ? labels.allOk : labels.failed}</p>` +
        `<p class="text-xs text-text/60 mt-1">${labels.provider}: ${data.activeProvider ?? 'N/A'} | ${labels.moderation}: ${data.moderation?.ok ? 'OK' : 'Failed'} | ${labels.completions}: ${data.completions?.ok ? 'OK' : 'Failed'}</p>`;
      resultDiv.classList.remove('hidden');
    } catch (error) {
      const message = error instanceof Error ? error.message : labels.failed;
      resultDiv.className = 'mb-6 rounded-xl border border-red-200 bg-red-50 p-4';
      resultDiv.innerHTML = `<p class="text-sm font-medium text-red-700">${labels.failed}: ${message}</p>`;
      resultDiv.classList.remove('hidden');
    } finally {
      btn.disabled = false;
      btn.innerHTML = '<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/></svg> ' + labels.testHealth;
    }
  });
}

document.addEventListener('DOMContentLoaded', () => {
  cleanupSetupStateFromQuery();
  initDeleteConfirmations();
  initHealthCheck();
});

