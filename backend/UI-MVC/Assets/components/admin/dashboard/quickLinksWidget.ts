/**
 * Quick Links Widget Module
 * Handles modal functionality for quick links that open modals.
 */

/**
 * Configuration for a quick link item
 */
export interface QuickLinkItem {
    title: string;
    description: string;
    icon: string;
    navigateUrl: string;
    modalTarget?: string;
    isModal: boolean;
}

/**
 * Configuration for the quick links widget
 */
export interface QuickLinksWidgetConfig {
    widgetId?: string;
    items: QuickLinkItem[];
}

/**
 * Modal configuration
 */
export interface ModalConfig {
    id: string;
    title?: string;
    content?: string | HTMLElement;
    onClose?: () => void;
    onOpen?: () => void;
}

/**
 * Cache of registered modals
 */
const modals: Map<string, ModalConfig> = new Map();

/**
 * Initialize quick links widget
 */
export function initQuickLinksWidget(config: QuickLinksWidgetConfig): void {
    // Find all quick link items in the widget
    const widget = config.widgetId 
        ? document.querySelector(`[data-quick-links-widget="${config.widgetId}"]`)
        : document.querySelector('.quick-links-widget');
    
    if (!widget) {
        console.warn(`Quick links widget not found: ${config.widgetId || 'default'}`);
        return;
    }

    const links = widget.querySelectorAll<HTMLElement>('[data-quick-link]');
    
    links.forEach(link => {
        const modalTarget = link.dataset.modalTarget;
        const navigateUrl = link.dataset.navigateUrl;
        
        if (modalTarget) {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                if (document.getElementById(modalTarget)) {
                    openModal(modalTarget);
                } else if (navigateUrl) {
                    window.location.href = navigateUrl;
                }
            });
            link.style.cursor = 'pointer';
        } else if (navigateUrl) {
            // Navigation link
            link.addEventListener('click', () => {
                window.location.href = navigateUrl;
            });
            link.style.cursor = 'pointer';
        }
    });
}

/**
 * Register a modal with the system
 */
export function registerModal(config: ModalConfig): void {
    modals.set(config.id, config);
    
    // Create modal element if it doesn't exist
    let modal = document.getElementById(config.id);
    if (!modal) {
        modal = createModalElement(config);
        document.body.appendChild(modal);
    }
    
    // Setup close handlers
    setupModalHandlers(config.id);
}

/**
 * Create modal HTML element
 */
function createModalElement(config: ModalConfig): HTMLElement {
    const modal = document.createElement('div');
    modal.id = config.id;
    modal.className = 'fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4 hidden';
    modal.setAttribute('role', 'dialog');
    modal.setAttribute('aria-modal', 'true');
    modal.setAttribute('aria-labelledby', `${config.id}-title`);
    
    modal.innerHTML = `
        <div class="bg-white rounded-xl shadow-xl w-full max-w-lg max-h-[90vh] overflow-y-auto">
            <div class="p-6">
                ${config.title ? `<h2 id="${config.id}-title" class="text-xl font-bold mb-4">${escapeHtml(config.title)}</h2>` : ''}
                <div id="${config.id}-content" class="modal-content">
                    ${typeof config.content === 'string' ? config.content : ''}
                </div>
            </div>
            <div class="p-4 border-t border-secondary/10 flex justify-end gap-2">
                <button type="button" class="px-4 py-2 bg-secondary/10 text-text rounded-lg hover:bg-secondary/20 transition-colors close-modal"
                        aria-label="Close modal">
                    Close
                </button>
            </div>
        </div>
    `;
    
    // Append content if it's an HTMLElement
    if (config.content instanceof HTMLElement) {
        const contentDiv = modal.querySelector(`#${config.id}-content`);
        if (contentDiv) {
            contentDiv.appendChild(config.content);
        }
    }
    
    return modal;
}

/**
 * Setup modal event handlers
 */
function setupModalHandlers(modalId: string): void {
    const modal = document.getElementById(modalId);
    if (!modal) return;

    const config = modals.get(modalId);
    
    // Close button handler
    const closeBtn = modal.querySelector('.close-modal');
    closeBtn?.addEventListener('click', () => closeModal(modalId));
    
    // Click outside to close
    modal.addEventListener('click', (e) => {
        if (e.target === modal) {
            closeModal(modalId);
        }
    });
    
    // Escape key to close
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && modal.classList.contains('flex')) {
            closeModal(modalId);
        }
    });
    
    // Call onOpen when modal is shown (if configured)
    const observer = new MutationObserver((mutations) => {
        mutations.forEach(mutation => {
            if (mutation.attributeName === 'class') {
                if (modal.classList.contains('flex')) {
                    config?.onOpen?.();
                }
            }
        });
    });
    observer.observe(modal, { attributes: true });
}

/**
 * Open a modal by ID
 */
export function openModal(modalId: string): void {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('hidden');
        modal.classList.add('flex');
        document.body.style.overflow = 'hidden';
        
        const config = modals.get(modalId);
        config?.onOpen?.();
    } else {
        console.warn(`Modal not found: ${modalId}`);
    }
}

/**
 * Close a modal by ID
 */
export function closeModal(modalId: string): void {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.add('hidden');
        modal.classList.remove('flex');
        document.body.style.overflow = '';
        
        const config = modals.get(modalId);
        config?.onClose?.();
    }
}

/**
 * Toggle modal visibility
 */
export function toggleModal(modalId: string): void {
    const modal = document.getElementById(modalId);
    if (modal) {
        if (modal.classList.contains('hidden')) {
            openModal(modalId);
        } else {
            closeModal(modalId);
        }
    }
}

/**
 * Check if a modal is open
 */
export function isModalOpen(modalId: string): boolean {
    const modal = document.getElementById(modalId);
    return modal ? !modal.classList.contains('hidden') : false;
}

/**
 * Initialize all quick links widgets on the page
 */
export function initAllQuickLinksWidgets(): void {
    const widgets = document.querySelectorAll('[data-quick-links-widget]');
    widgets.forEach(widget => {
        const widgetId = widget.dataset.quickLinksWidget;
        const items: QuickLinkItem[] = [];
        
        const links = widget.querySelectorAll<HTMLElement>('[data-quick-link]');
        links.forEach(link => {
            items.push({
                title: link.dataset.title || '',
                description: link.dataset.description || '',
                icon: link.dataset.icon || '',
                navigateUrl: link.dataset.navigateUrl || '',
                modalTarget: link.dataset.modalTarget,
                isModal: !!link.dataset.modalTarget
            });
        });
        
        initQuickLinksWidget({ widgetId, items });
    });
}

/**
 * Escape HTML to prevent XSS
 */
function escapeHtml(text: string): string {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
