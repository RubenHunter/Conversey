import { t } from '../../utils/adminI18n';

function toNum(el: Element, attr: string): number {
  const raw = el.getAttribute(attr) ?? '0';
  const parsed = Number.parseFloat(raw);
  return Number.isFinite(parsed) ? parsed : 0;
}

function initPricingPage(): void {
  const tbody = document.getElementById('pricingBody');
  const providerFilter = document.getElementById('providerFilter') as HTMLSelectElement | null;
  const sortBy = document.getElementById('sortBy') as HTMLSelectElement | null;
  const rowCount = document.getElementById('rowCount');
  if (!tbody || !providerFilter || !sortBy || !rowCount) return;

  const rows = Array.from(tbody.querySelectorAll<HTMLTableRowElement>('.pricing-row'));
  const modelsLabel = t('models', 'models');
  const ofLabel = t('of', 'of');

  const update = (): void => {
    const provider = providerFilter.value;
    const sort = sortBy.value;

    const filtered = rows.filter((row) => !provider || row.getAttribute('data-provider') === provider);

    filtered.sort((a, b) => {
      switch (sort) {
        case 'model-asc':
          return (a.getAttribute('data-model') ?? '').localeCompare(b.getAttribute('data-model') ?? '');
        case 'model-desc':
          return (b.getAttribute('data-model') ?? '').localeCompare(a.getAttribute('data-model') ?? '');
        case 'input-asc':
          return toNum(a, 'data-input') - toNum(b, 'data-input');
        case 'input-desc':
          return toNum(b, 'data-input') - toNum(a, 'data-input');
        case 'output-asc':
          return toNum(a, 'data-output') - toNum(b, 'data-output');
        case 'output-desc':
          return toNum(b, 'data-output') - toNum(a, 'data-output');
        case 'total-asc':
          return (toNum(a, 'data-input') + toNum(a, 'data-output')) - (toNum(b, 'data-input') + toNum(b, 'data-output'));
        case 'total-desc':
          return (toNum(b, 'data-input') + toNum(b, 'data-output')) - (toNum(a, 'data-input') + toNum(a, 'data-output'));
        default:
          return 0;
      }
    });

    tbody.innerHTML = '';
    filtered.forEach((row) => tbody.appendChild(row));
    rowCount.textContent = `${filtered.length} ${ofLabel} ${rows.length} ${modelsLabel}`;
  };

  providerFilter.addEventListener('change', update);
  sortBy.addEventListener('change', update);
  update();
}

document.addEventListener('DOMContentLoaded', initPricingPage);

