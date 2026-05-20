import { t } from '../../utils/adminI18n';

type ModelsResponse = { models?: string[]; error?: string };

function goBackToStep1(): void {
  const prevBtn = document.getElementById('prevBtn') as HTMLButtonElement | null;
  prevBtn?.click();
}

function initSetupModelsStep(): void {
  const statusEl = document.getElementById('modelsStatus');
  const compSelect = document.getElementById('setupCompletionsModel') as HTMLSelectElement | null;
  const modSelect = document.getElementById('setupModerationModel') as HTMLSelectElement | null;
  const infoPanel = document.getElementById('infoTipPanel');
  const infoTitle = document.getElementById('infoTipTitle');
  const infoContent = document.getElementById('infoTipContent');
  const infoClose = document.getElementById('infoTipClose');

  if (!statusEl || !compSelect || !modSelect || !infoPanel || !infoTitle || !infoContent || !infoClose) return;

  let activeTrigger: Element | null = null;

  const showError = (html: string): void => {
    statusEl.innerHTML = html;
    statusEl.className = 'text-xs mb-4';
  };

  const loadModels = async (): Promise<void> => {
    const provider = (document.getElementById('setupProvider') as HTMLInputElement | null)?.value ?? '';
    const baseUrl = (document.getElementById('setupBaseUrl') as HTMLInputElement | null)?.value ?? '';
    const apiKey = (document.getElementById('setupApiKey') as HTMLInputElement | null)?.value ?? '';

    if (!baseUrl) {
      showError(`<span class="text-amber-600">${t('Enter provider details in step 1 first.', 'Enter provider details in step 1 first.')}</span>`);
      compSelect.innerHTML = `<option value="">${t('No base URL', 'No base URL')}</option>`;
      modSelect.innerHTML = `<option value="">${t('No base URL', 'No base URL')}</option>`;
      return;
    }

    statusEl.textContent = `${t('Testing connection to', 'Testing connection to')} ${provider}...`;
    statusEl.className = 'text-xs text-text/50 mb-4';

    try {
      const response = await fetch('/admin/ai/providers/test-models', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ providerName: provider, baseUrl, apiKey })
      });
      const data = (await response.json()) as ModelsResponse;

      if (data.error) {
        const lowered = data.error.toLowerCase();
        const isAuth = lowered.includes('unauthorized') || lowered.includes('401') || lowered.includes('api key') || lowered.includes('key');
        const actionText = isAuth
          ? t('Go back to step 1 to fix your credentials.', 'Go back to step 1 to fix your credentials.')
          : t('Go back to step 1.', 'Go back to step 1.');
        const actionClass = isAuth
          ? 'underline text-primary hover:text-primary/80 font-medium text-[11px] js-step-back'
          : 'underline text-text/40 hover:text-text/60 font-medium text-[11px] js-step-back';

        showError(
          `<span class="text-red-600">${t('Connection failed:', 'Connection failed:')} ${data.error}.</span><br>` +
          `<span class="text-text/50">${t('Select a model...', 'Select a model...')}</span> ` +
          `<button type="button" class="${actionClass}">${actionText}</button>`
        );
        compSelect.innerHTML = `<option value="">${t('Could not load from API', 'Could not load from API')}</option>`;
        modSelect.innerHTML = `<option value="">${t('None (skip moderation)', 'None (skip moderation)')}</option>`;
        return;
      }

      const models = data.models ?? [];
      statusEl.textContent = `${t('Found', 'Found')} ${models.length} ${t('model(s). Select from the dropdowns below.', 'model(s). Select from the dropdowns below.')}`;
      statusEl.className = 'text-xs text-green-600 mb-4';

      const prevComp = compSelect.value;
      const prevMod = modSelect.value;

      compSelect.innerHTML = `<option value="">${t('Select a model...', 'Select a model...')}</option>`;
      modSelect.innerHTML = `<option value="">${t('None (skip moderation)', 'None (skip moderation)')}</option>`;

      models.forEach((model) => {
        const compOption = document.createElement('option');
        compOption.value = model;
        compOption.textContent = model;
        if (model === prevComp) compOption.selected = true;
        compSelect.appendChild(compOption);

        const modOption = document.createElement('option');
        modOption.value = model;
        modOption.textContent = model;
        if (model === prevMod) modOption.selected = true;
        modSelect.appendChild(modOption);
      });
    } catch (error) {
      const message = error instanceof Error ? error.message : '';
      showError(
        `<span class="text-red-600">${t('Connection failed:', 'Connection failed:')} ${message}.</span><br>` +
        `<span class="text-text/50">${t('Select a model...', 'Select a model...')}</span> ` +
        `<button type="button" class="underline text-primary hover:text-primary/80 font-medium text-[11px] js-step-back">${t('Go back to step 1 to fix your credentials.', 'Go back to step 1 to fix your credentials.')}</button>`
      );
      compSelect.innerHTML = `<option value="">${t('Could not load from API', 'Could not load from API')}</option>`;
      modSelect.innerHTML = `<option value="">${t('None (skip moderation)', 'None (skip moderation)')}</option>`;
    }
  };

  const speechProviders = new Set(['mistral', 'openai']);
  const speechSection = document.getElementById('speechModelsSection');

  const toggleSpeechSection = (): void => {
    if (!speechSection) return;
    const provider = (document.getElementById('setupProvider') as HTMLInputElement | null)?.value?.trim().toLowerCase() ?? '';
    speechSection.classList.toggle('hidden', !speechProviders.has(provider));
  };

  toggleSpeechSection();

  document.getElementById('setupProvider')?.addEventListener('input', toggleSpeechSection);

  statusEl.addEventListener('click', (event) => {
    const target = event.target as HTMLElement | null;
    if (target?.closest('.js-step-back')) {
      goBackToStep1();
    }
  });

  document.querySelectorAll<HTMLElement>('.info-trigger').forEach((el) => {
    const titleKey = el.getAttribute('data-info-title-key') ?? '';
    const contentKey = el.getAttribute('data-info-content-key') ?? '';
    if (titleKey) el.setAttribute('data-info-title', t(titleKey, titleKey));
    if (contentKey) el.setAttribute('data-info-content', t(contentKey, contentKey));

    el.addEventListener('click', (event) => {
      event.stopPropagation();
      if (activeTrigger === el) {
        infoPanel.classList.add('hidden');
        activeTrigger = null;
        return;
      }

      infoTitle.textContent = el.getAttribute('data-info-title') ?? '';
      infoContent.innerHTML = el.getAttribute('data-info-content') ?? '';
      infoPanel.classList.remove('hidden');
      activeTrigger = el;
    });
  });

  infoPanel.addEventListener('click', (event) => event.stopPropagation());
  infoClose.addEventListener('click', () => {
    infoPanel.classList.add('hidden');
    activeTrigger = null;
  });
  document.addEventListener('click', () => {
    infoPanel.classList.add('hidden');
    activeTrigger = null;
  });

  document.addEventListener('stepper:step-enter', (event) => {
    const detail = (event as CustomEvent<{ step?: number }>).detail;
    if (detail?.step === 2) {
      void loadModels();
    }
  });
}

document.addEventListener('DOMContentLoaded', initSetupModelsStep);

