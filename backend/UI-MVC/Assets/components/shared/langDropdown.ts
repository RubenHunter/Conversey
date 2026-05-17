// Moves language dropdown behavior out of the partial into a module
document.addEventListener('DOMContentLoaded', () => {
  const dropdowns = Array.from(document.querySelectorAll('.lang-dropdown')) as HTMLElement[];

  dropdowns.forEach(dd => {
    const toggle = dd.querySelector('.lang-toggle') as HTMLElement | null;
    const menu = dd.querySelector('.lang-menu') as HTMLElement | null;
    if (!toggle || !menu) return;

    toggle.addEventListener('click', (e) => {
      e.stopPropagation();
      menu.classList.toggle('hidden');
    });
  });

  document.addEventListener('click', () => {
    dropdowns.forEach(dd => {
      const menu = dd.querySelector('.lang-menu') as HTMLElement | null;
      if (menu) menu.classList.add('hidden');
    });
  });
});

export {};

