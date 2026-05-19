type ProviderSetupState = {
  version: number;
  currentStep: number;
  fields: Record<string, string>;
};

type ProviderSetupApi = {
  saveFields: () => void;
  restoreFields: () => void;
  getCurrentStep: () => number;
  setCurrentStep: (step: number) => void;
  clear: () => void;
  storageKey: string;
};

declare global {
  interface Window {
    __ProviderSetupState?: ProviderSetupApi;
  }
}

const STORAGE_KEY = 'conversey-ai-provider-setup-state-v1';
const STORAGE_VERSION = 1;

const fieldDefaults: Record<string, string> = {
  setupProvider: '',
  setupBaseUrl: '',
  setupApiKey: '',
  setupKeyExpiry: '',
  setupCompletionsModel: '',
  setupModerationModel: '',
  setupSttModel: '',
  setupTtsModel: '',
  setupTemperature: '0.2'
};

const fieldIds = Object.keys(fieldDefaults);

function readState(): ProviderSetupState | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as ProviderSetupState;
    if (!parsed || parsed.version !== STORAGE_VERSION || typeof parsed !== 'object') return null;
    return parsed;
  } catch {
    return null;
  }
}

function writeState(nextState: ProviderSetupState): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(nextState));
}

function getCurrentFieldValues(): Record<string, string> {
  const values: Record<string, string> = {};
  fieldIds.forEach((id) => {
    const el = document.getElementById(id) as HTMLInputElement | HTMLSelectElement | null;
    values[id] = el ? el.value || '' : fieldDefaults[id];
  });
  return values;
}

function saveFields(): void {
  const state = readState() ?? { version: STORAGE_VERSION, currentStep: 1, fields: {} };
  state.version = STORAGE_VERSION;
  state.fields = getCurrentFieldValues();
  writeState(state);
}

function restoreFields(): void {
  const state = readState();
  const fields = state && state.fields && typeof state.fields === 'object' ? state.fields : null;

  fieldIds.forEach((id) => {
    const el = document.getElementById(id) as HTMLInputElement | HTMLSelectElement | null;
    if (!el) return;

    const desired = fields && Object.prototype.hasOwnProperty.call(fields, id)
      ? fields[id] || ''
      : fieldDefaults[id];

    if (el.tagName === 'SELECT') {
      const selectEl = el as HTMLSelectElement;
      selectEl.value = desired;
      if (desired && selectEl.value !== desired) {
        const opt = document.createElement('option');
        opt.value = desired;
        opt.textContent = `${desired} (restored)`;
        opt.setAttribute('data-restored', '1');
        selectEl.appendChild(opt);
        selectEl.value = desired;
      }
      return;
    }

    el.value = desired;
  });
}

function getCurrentStep(): number {
  const state = readState();
  return state && typeof state.currentStep === 'number' ? state.currentStep : 1;
}

function setCurrentStep(step: number): void {
  const state = readState() ?? {
    version: STORAGE_VERSION,
    currentStep: 1,
    fields: getCurrentFieldValues()
  };

  state.version = STORAGE_VERSION;
  state.currentStep = step;
  if (!state.fields) {
    state.fields = getCurrentFieldValues();
  }
  writeState(state);
}

function clearState(): void {
  localStorage.removeItem(STORAGE_KEY);
}

function initProviderSetupState(): void {
  window.__ProviderSetupState = {
    saveFields,
    restoreFields,
    getCurrentStep,
    setCurrentStep,
    clear: clearState,
    storageKey: STORAGE_KEY
  };

  restoreFields();

  fieldIds.forEach((id) => {
    const el = document.getElementById(id) as HTMLInputElement | HTMLSelectElement | null;
    if (!el) return;
    el.addEventListener('input', saveFields);
    el.addEventListener('change', saveFields);
  });

  window.addEventListener('pageshow', (event) => {
    restoreFields();

    if (event.persisted && !readState()) {
      document.dispatchEvent(new CustomEvent('provider-setup:reset'));
    }
  });
}

document.addEventListener('DOMContentLoaded', initProviderSetupState);

export {};


