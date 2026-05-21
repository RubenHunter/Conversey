import { t } from '../../utils/adminI18n';

type ModelsResponse = { models?: string[]; error?: string };
type ProbeResult = { ok?: boolean; error?: string; durationMs?: number; preview?: string };
type HealthResponse = { healthy?: boolean; moderation?: ProbeResult; completions?: ProbeResult };

function getVal(id: string): string {
  const el = document.getElementById(id) as HTMLInputElement | HTMLSelectElement | null;
  return el?.value || '';
}

function goBackToStep1(): void {
  const prevBtn = document.getElementById('prevBtn') as HTMLButtonElement | null;
  prevBtn?.click();
}

function initSetupVerifyStep(): void {
  const summary = document.getElementById('setupSummary');
  const bar = document.getElementById('healthCheckBar');
  const saveBtn = document.getElementById('saveProviderBtn');
  if (!summary || !bar || !saveBtn) return;

  const badge = (text: string, color: string): string =>
    `<span class="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-medium ${color}">${text}</span>`;

  const updateSummary = (): void => {
    const provider = getVal('setupProvider');
    const baseUrl = getVal('setupBaseUrl');
    const apiKey = getVal('setupApiKey');
    const compModel = getVal('setupCompletionsModel');
    const modModel = getVal('setupModerationModel');
    const sttModel = getVal('setupSttModel');
    const ttsModel = getVal('setupTtsModel');
    const temp = getVal('setupTemperature');

    let html = '';
    if (provider) html += `<div class="flex justify-between items-center py-2 border-b border-secondary/5"><span class="text-text/50 text-xs">${t('Provider name', 'Provider name')}</span><span class="font-medium text-text">${provider}</span></div>`;
    if (baseUrl) html += `<div class="flex justify-between items-center py-2 border-b border-secondary/5"><span class="text-text/50 text-xs">${t('Base URL', 'Base URL')}</span><span class="font-mono text-[11px] text-text break-all max-w-[60%] text-right">${baseUrl}</span></div>`;
    html += `<div class="flex justify-between items-center py-2 border-b border-secondary/5"><span class="text-text/50 text-xs">${t('API Key', 'API Key')}</span><span class="font-mono text-[11px] text-text">${apiKey ? `${apiKey.substring(0, 8)}...` : `<span class="text-amber-600">${t('Not set', 'Not set')}</span>`}</span></div>`;
    html += `<div class="flex justify-between items-center py-2 border-b border-secondary/5"><span class="text-text/50 text-xs">${t('Completions model', 'Completions model')}</span><span class="font-mono text-[11px] text-text">${compModel || `<span class="text-red-500">${t('Not selected', 'Not selected')}</span>`}</span></div>`;

    if (modModel) {
      html += `<div class="flex justify-between items-center py-2 border-b border-secondary/5"><span class="text-text/50 text-xs">${t('Moderation model', 'Moderation model')}</span><span class="font-mono text-[11px] text-text">${modModel}</span></div>`;
    } else {
      html += `<div class="flex justify-between items-center py-2 border-b border-secondary/5"><span class="text-text/50 text-xs">${t('Moderation model', 'Moderation model')}</span><span class="text-[11px] text-text/40">${badge(t('prompt fallback', 'prompt fallback'), 'bg-amber-50 text-amber-700')} <span class="text-text/40">${t('Completions model will be used for moderation via prompt.', 'Completions model will be used for moderation via prompt.')}</span></span></div>`;
    }

    if (sttModel) {
      html += `<div class="flex justify-between items-center py-2 border-b border-secondary/5"><span class="text-text/50 text-xs">${t('STT Model', 'STT Model')}</span><span class="font-mono text-[11px] text-text">${sttModel}</span></div>`;
    }
    if (ttsModel) {
      html += `<div class="flex justify-between items-center py-2 border-b border-secondary/5"><span class="text-text/50 text-xs">${t('TTS Model', 'TTS Model')}</span><span class="font-mono text-[11px] text-text">${ttsModel}</span></div>`;
    }
    html += `<div class="flex justify-between items-center py-2"><span class="text-text/50 text-xs">${t('Temperature', 'Temperature')}</span><span class="text-text">${temp || '0.2'}</span></div>`;
    summary.innerHTML = html || `<p class="text-text/60">${t('Fill in the previous steps to see a summary here.', 'Fill in the previous steps to see a summary here.')}</p>`;
  };

  const runHealthCheck = async (): Promise<void> => {
    const provider = getVal('setupProvider');
    const baseUrl = getVal('setupBaseUrl');
    const apiKey = getVal('setupApiKey');
    const compModel = getVal('setupCompletionsModel');
    const modModel = getVal('setupModerationModel');
    const temp = getVal('setupTemperature') || '0.2';

    if (!baseUrl) {
      bar.className = 'rounded-lg p-3 mb-4 text-xs bg-amber-50 text-amber-700 border border-amber-200 hidden';
      return;
    }

    bar.className = 'rounded-lg p-3 mb-4 text-xs bg-secondary/5 text-text/50 border border-secondary/10';
    bar.innerHTML = `<span class="flex items-center gap-2"><svg class="w-3.5 h-3.5 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/></svg> ${t('Testing provider connection...', 'Testing provider connection...')}</span>`;

    const body = JSON.stringify({
      providerName: provider,
      baseUrl,
      apiKey,
      completionsModel: compModel,
      moderationModel: modModel,
      temperature: Number.parseFloat(temp)
    });

    try {
      const [modelsData, healthData] = await Promise.all([
        fetch('/admin/ai/providers/test-models', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ providerName: provider, baseUrl, apiKey })
        }).then(async (r) => (await r.json()) as ModelsResponse),
        fetch('/admin/ai/providers/test-health', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body
        }).then(async (r) => (await r.json()) as HealthResponse)
      ]);

      const models = modelsData.models ?? [];
      const modelError = modelsData.error;

      const lines: string[] = [];
      if (modelError) {
        lines.push(`<span class="text-red-600"><strong>${t('Connection:', 'Connection:')}</strong> ${modelError}</span>`);
      } else {
        lines.push(`<span class="text-green-600"><strong>${t('Connection:', 'Connection:')}</strong> ${t('OK', 'OK')}, ${models.length} ${t('model(s) available', 'model(s) available')}</span>`);
      }

      const healthy = !!healthData.healthy;
      lines.push(`<span class="${healthy ? 'text-green-600' : 'text-red-600'}"><strong>${t('Health:', 'Health:')}</strong> ${healthy ? t('All probes passed', 'All probes passed') : t('Probes failed', 'Probes failed')}</span>`);

      if (healthData.moderation) {
        const mod = healthData.moderation;
        lines.push(`<span class="${mod.ok ? 'text-green-600' : 'text-red-600'}">  ${t('Moderation:', 'Moderation:')} ${mod.ok ? t('OK', 'OK') : mod.error || t('Failed', 'Failed')} (${mod.durationMs ?? 0}ms)</span>`);
      }

      if (healthData.completions) {
        const comp = healthData.completions;
        const compDetail = comp.ok && comp.preview ? ` -> "${comp.preview}"` : '';
        lines.push(`<span class="${comp.ok ? 'text-green-600' : 'text-red-600'}">  ${t('Completions:', 'Completions:')} ${comp.ok ? t('OK', 'OK') + compDetail : comp.error || t('Failed', 'Failed')} (${comp.durationMs ?? 0}ms)</span>`);
      }

      const allOk = !modelError && healthy;
      bar.className = `rounded-lg p-3 mb-4 text-xs border ${allOk ? 'bg-green-50 text-green-700 border-green-200' : 'bg-red-50 text-red-700 border-red-200'}`;
      bar.innerHTML = `<div class="flex flex-col gap-1">${lines.join('<br>')}</div>`;
    } catch (error) {
      const message = error instanceof Error ? error.message : '';
      bar.className = 'rounded-lg p-3 mb-4 text-xs bg-red-50 text-red-700 border border-red-200';
      bar.innerHTML =
        `<span class="flex items-center gap-2"><svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>` +
        `<strong>${t('Connection failed:', 'Connection failed:')}</strong> ${message} ` +
        `<button type="button" class="underline ml-1 font-medium js-step-back">${t('Go back to step 1', 'Go back to step 1')}</button></span>`;
    }
  };

  bar.addEventListener('click', (event) => {
    const target = event.target as HTMLElement | null;
    if (target?.closest('.js-step-back')) {
      goBackToStep1();
    }
  });

  document
    .querySelectorAll<HTMLElement>('#setupProvider, #setupBaseUrl, #setupApiKey, #setupCompletionsModel, #setupModerationModel, #setupSttModel, #setupTtsModel, #setupTemperature')
    .forEach((el) => {
      el.addEventListener('input', updateSummary);
      el.addEventListener('change', updateSummary);
    });
  updateSummary();

  document.addEventListener('stepper:step-enter', (event) => {
    const detail = (event as CustomEvent<{ step?: number }>).detail;
    if (detail?.step === 3) {
      updateSummary();
      void runHealthCheck();
    }
  });

  saveBtn.addEventListener('click', () => {
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = '/admin/ai/providers';

    const fields: Record<string, string> = {
      ProviderName: getVal('setupProvider'),
      BaseUrl: getVal('setupBaseUrl'),
      ApiKey: getVal('setupApiKey'),
      CompletionsModel: getVal('setupCompletionsModel'),
      ModerationModel: getVal('setupModerationModel'),
      SttModel: getVal('setupSttModel'),
      TtsModel: getVal('setupTtsModel'),
      Temperature: getVal('setupTemperature') || '0.2',
      IsEnabled: 'true'
    };

    const expiry = getVal('setupKeyExpiry');
    if (expiry) {
      fields.ApiKeyExpiresAt = expiry;
    }

    Object.keys(fields).forEach((key) => {
      const input = document.createElement('input');
      input.type = 'hidden';
      input.name = key;
      input.value = fields[key];
      form.appendChild(input);
    });

    const token = document.querySelector<HTMLInputElement>('input[name="__RequestVerificationToken"]');
    if (token) {
      const tokenInput = document.createElement('input');
      tokenInput.type = 'hidden';
      tokenInput.name = '__RequestVerificationToken';
      tokenInput.value = token.value;
      form.appendChild(tokenInput);
    }

    document.body.appendChild(form);
    form.submit();
  });
}

document.addEventListener('DOMContentLoaded', initSetupVerifyStep);

