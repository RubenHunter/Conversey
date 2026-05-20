document.addEventListener('DOMContentLoaded', () => {
  const buttons = document.querySelectorAll<HTMLElement>('[data-project-limit-toggle]');
  buttons.forEach((button) => {
    button.addEventListener('click', () => {
      const projectId = button.getAttribute('data-project-id');
      if (!projectId) return;
      const row = document.getElementById(`limit-form-${projectId}`);
      row?.classList.toggle('hidden');
    });
  });
});


