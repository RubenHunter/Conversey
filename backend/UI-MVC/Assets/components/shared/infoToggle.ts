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

document.addEventListener('DOMContentLoaded', initInfoToggles);

