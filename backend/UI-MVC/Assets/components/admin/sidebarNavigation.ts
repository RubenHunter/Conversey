class SidebarNavigation {
    private readonly LS_KEY = 'conversey_sidebar_sections';

    constructor(private sidebar: HTMLElement) {
        this.highlightActiveLinks();
        this.bindToggleSections();
    }

    private highlightActiveLinks() {
        const currentPath = window.location.pathname;
        const links = this.sidebar.querySelectorAll<HTMLElement>('[data-sidebar-page]');
        links.forEach((link) => {
            const page = link.getAttribute('data-sidebar-page');
            if (page && currentPath.startsWith(page)) {
                link.classList.remove('text-text/70', 'text-text/50');
                link.classList.add('text-primary', 'bg-primary/5');
            }
        });
    }

    private bindToggleSections() {
        const currentPath = window.location.pathname;
        const state = this.loadState();

        const toggles = this.sidebar.querySelectorAll<HTMLElement>('[data-sidebar-toggle]');
        toggles.forEach((toggle) => {
            const section = toggle.getAttribute('data-sidebar-toggle');
            if (!section) return;

            const content = document.querySelector<HTMLElement>(`[data-sidebar-content="${section}"]`);
            const chevron = toggle.querySelector<HTMLElement>('.sidebar-chevron');
            if (!content) return;

            const setOpen = (open: boolean) => {
                toggle.setAttribute('aria-expanded', String(open));
                if (open) {
                    content.classList.replace('grid-rows-[0fr]', 'grid-rows-[1fr]');
                    if (chevron) chevron.classList.add('rotate-180');
                } else {
                    content.classList.replace('grid-rows-[1fr]', 'grid-rows-[0fr]');
                    if (chevron) chevron.classList.remove('rotate-180');
                }
                state[section] = open;
                this.saveState(state);
            };

            const childLinks = content.querySelectorAll<HTMLElement>('[data-sidebar-page]');
            const hasActiveChild = childLinks.length > 0 &&
                Array.from(childLinks).some((link) => {
                    const page = link.getAttribute('data-sidebar-page') || '';
                    return currentPath.startsWith(page);
                });

            const shouldOpen = hasActiveChild || state[section] === true;
            if (shouldOpen) setOpen(true);

            toggle.addEventListener('click', () => {
                const isOpen = toggle.getAttribute('aria-expanded') === 'true';
                setOpen(!isOpen);
            });
        });
    }

    private loadState(): Record<string, boolean> {
        try { return JSON.parse(localStorage.getItem(this.LS_KEY) || '{}'); }
        catch { return {}; }
    }

    private saveState(state: Record<string, boolean>) {
        try { localStorage.setItem(this.LS_KEY, JSON.stringify(state)); }
        catch { /* noop */ }
    }

    static init() {
        document.querySelectorAll<HTMLElement>('[data-sidebar-nav]').forEach((sidebar) => {
            new SidebarNavigation(sidebar);
        });
    }
}

document.addEventListener('DOMContentLoaded', () => SidebarNavigation.init());
