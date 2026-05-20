import { t } from '../../utils/adminI18n';

type ModelsResponse = { models?: string[]; error?: string };

function initInfoToggles(): void {
  document.querySelectorAll<HTMLElement>('.info-toggle').forEach((btn) => {
    btn.addEventListener('click', () => {
      const targetId = btn.getAttribute('data-target');
      if (!targetId) return;
      const target = document.getElementById(targetId);
      target?.classList.toggle('hidden');
    });
  });
}

function setStatus(statusEl: HTMLElement, text: string, tone: 'neutral' | 'error' | 'warn' | 'ok'): void {
  const tones: Record<typeof tone, string> = {
    neutral: 'text-text/50',
    error: 'text-red-600',
    warn: 'text-amber-600',
    ok: 'text-green-600'
  };
  statusEl.textContent = text;
  statusEl.className = `px-6 py-3 bg-background/50 border-b border-secondary/10 text-xs ${tones[tone]}`;
  statusEl.classList.remove('hidden');
}

function initModelFetcher(): void {
  const compSelect = document.getElementById('editCompletionsSelect') as HTMLSelectElement | null;
  const modSelect = document.getElementById('editModerationSelect') as HTMLSelectElement | null;
  const compInput = document.getElementById('CompletionsModel') as HTMLInputElement | null;
  const modInput = document.getElementById('ModerationModel') as HTMLInputElement | null;
  const statusEl = document.getElementById('editModelsStatus');
  const fetchBtn = document.getElementById('fetchModelsBtn');

  if (!compSelect || !modSelect || !compInput || !modInput || !statusEl || !fetchBtn) return;

  compSelect.addEventListener('change', () => {
    if (compSelect.value) compInput.value = compSelect.value;
  });
  modSelect.addEventListener('change', () => {
    if (modSelect.value) modInput.value = modSelect.value;
  });

  fetchBtn.addEventListener('click', async () => {
    const provider = (document.getElementById('ProviderName') as HTMLInputElement | null)?.value ?? '';
    const baseUrl = (document.getElementById('BaseUrl') as HTMLInputElement | null)?.value ?? '';
    const apiKey = (document.getElementById('ApiKey') as HTMLInputElement | null)?.value ?? '';

    if (!baseUrl) {
      setStatus(statusEl, t('Enter a Base URL first.', 'Enter a Base URL first.'), 'error');
      return;
    }

    setStatus(statusEl, t('Fetching models...', 'Fetching models...'), 'neutral');

    try {
      const response = await fetch('/admin/ai/providers/test-models', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ providerName: provider, baseUrl, apiKey })
      });
      const data = (await response.json()) as ModelsResponse;

      if (data.error) {
        setStatus(
          statusEl,
          `${t('Could not fetch models:', 'Could not fetch models:')} ${data.error}. ${t('Type model names manually.', 'Type model names manually.')}`,
          'warn'
        );
        return;
      }

      const models = data.models ?? [];
      setStatus(
        statusEl,
        `${t('Found', 'Found')} ${models.length} ${t('model(s). Select from dropdowns or type manually.', 'model(s). Select from dropdowns or type manually.')}`,
        'ok'
      );

      const currentComp = compInput.value;
      const currentMod = modInput.value;

      compSelect.innerHTML = `<option value="">${t('Select a model...', 'Select a model...')}</option>`;
      modSelect.innerHTML = `<option value="">${t('None (skip moderation)', 'None (skip moderation)')}</option>`;

      models.forEach((model) => {
        const compOption = document.createElement('option');
        compOption.value = model;
        compOption.textContent = model;
        if (model === currentComp) compOption.selected = true;
        compSelect.appendChild(compOption);

        const modOption = document.createElement('option');
        modOption.value = model;
        modOption.textContent = model;
        if (model === currentMod) modOption.selected = true;
        modSelect.appendChild(modOption);
      });
    } catch (error) {
      const message = error instanceof Error ? error.message : t('Failed:', 'Failed:');
      setStatus(statusEl, `${t('Failed:', 'Failed:')} ${message}`, 'error');
    }
  });
}

document.addEventListener('DOMContentLoaded', () => {
  initInfoToggles();
  initModelFetcher();
});

