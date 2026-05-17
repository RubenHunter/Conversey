import { t } from '../../utils/adminI18n';

function initSetupProviderStep(): void {
  const providerInput = document.getElementById('setupProvider') as HTMLInputElement | null;
  const baseUrlInput = document.getElementById('setupBaseUrl') as HTMLInputElement | null;
  const hint = document.getElementById('baseUrlHint');
  if (!providerInput || !baseUrlInput || !hint) return;

  const examples: Record<string, string> = {
    Mistral: 'https://api.mistral.ai/v1/',
    Azure: 'https://YOUR_RESOURCE.openai.azure.com/openai/deployments/YOUR_DEPLOYMENT/',
    Ollama: 'http://localhost:11434/v1/',
    Groq: 'https://api.groq.com/openai/v1/',
    Google: 'https://generativelanguage.googleapis.com/v1beta/openai/',
    Together: 'https://api.together.xyz/v1/',
    NVIDIA: 'https://integrate.api.nvidia.com/v1/',
    OpenAI: 'https://api.openai.com/v1/'
  };

  providerInput.addEventListener('input', () => {
    const url = examples[providerInput.value] ?? '';
    baseUrlInput.value = url;
    hint.textContent = url
      ? `${t('Default URL for', 'Default URL for')} ${providerInput.value}`
      : t('Select a provider to see the default URL.', 'Select a provider to see the default URL.');
  });
}

document.addEventListener('DOMContentLoaded', initSetupProviderStep);

