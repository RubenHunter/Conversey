const input = document.getElementById('promptSearch') as HTMLInputElement | null;
const projectSelect = document.getElementById('projectFilter') as HTMLSelectElement | null;
const items = Array.from(document.querySelectorAll('.prompt-item')) as HTMLElement[];

if (input) {
  input.addEventListener('input', () => {
    const query = (input.value || '').toLowerCase().trim();
    items.forEach(item => {
      const name = (item.getAttribute('data-name') || '').toLowerCase();
      const desc = (item.getAttribute('data-desc') || '').toLowerCase();
      const match = !query || name.indexOf(query) !== -1 || desc.indexOf(query) !== -1;
      (item as HTMLElement).style.display = match ? '' : 'none';
    });
  });
}

if (projectSelect) {
  projectSelect.addEventListener('change', () => {
    const base = (window as { __PromptsBaseUrl?: string }).__PromptsBaseUrl || window.location.pathname;
    const url =
      base +
      '?search=' + encodeURIComponent(input?.value || '') +
      '&projectId=' + encodeURIComponent(projectSelect.value || '');
    window.location.href = url;
  });
}

export {};


