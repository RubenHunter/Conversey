interface DraftMeta {
    currentStep: number;
    slug: string;
}

function initProjectsPage(): void {
    const page = document.getElementById('projectsPage');
    const prefix = page?.dataset.draftPrefix;
    if (!prefix) return;

    const raw = localStorage.getItem(`${prefix}:meta`);
    if (!raw) return;

    let meta: DraftMeta | null = null;
    try {
        meta = JSON.parse(raw) as DraftMeta;
    } catch {
        return;
    }

    if (!meta || meta.slug !== '') return;

    const modal = document.getElementById('recoverDraftModal');
    if (!modal) return;

    modal.classList.remove('hidden');
    modal.classList.add('flex');

    document.getElementById('recoverDraftRecover')?.addEventListener('click', () => {
        sessionStorage.setItem('recoverDraft', '1');
        window.location.href = '/admin/projects/new';
    });

    document.getElementById('recoverDraftDiscard')?.addEventListener('click', () => {
        ['meta', 'step:1', 'step:2', 'step:3', 'step:4'].forEach(k =>
            localStorage.removeItem(`${prefix}:${k}`)
        );
        modal.classList.add('hidden');
        modal.classList.remove('flex');
    });
}

document.addEventListener('DOMContentLoaded', initProjectsPage);
