import { t } from '../../utils/adminI18n';

function initKeywordDeleteConfirmation(): void {
  const forms = document.querySelectorAll<HTMLFormElement>('form[data-confirm-delete-keyword]');
  forms.forEach((form) => {
    form.addEventListener('submit', (event) => {
      const keyword = form.dataset.keyword ?? '';
      const message = t('Delete keyword "{0}"?', 'Delete keyword "{0}"?').replace('{0}', keyword);
      if (!window.confirm(message)) {
        event.preventDefault();
      }
    });
  });
}

document.addEventListener('DOMContentLoaded', initKeywordDeleteConfirmation);

