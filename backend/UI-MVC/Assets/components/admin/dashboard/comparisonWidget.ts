/**
 * Comparison Widget — radial circle comparison with toggle switch and hover synchronisation.
 */

interface ComparisonItem {
    label: string;
    value: number;
    color: string;
}

function colorClass(color: string, type: 'dot' | 'circle'): string {
    if (type === 'dot') {
        return color === 'secondary' ? 'bg-secondary' : color === 'accent' ? 'bg-accent' : 'bg-primary';
    }
    return color === 'secondary'
        ? 'bg-secondary/20 border-secondary'
        : color === 'accent'
            ? 'bg-accent/20 border-accent'
            : 'bg-primary/20 border-primary';
}

function layoutCircles(container: HTMLElement, items: ComparisonItem[]): void {
    if (!items.length) return;

    const w = container.clientWidth;
    const h = container.clientHeight;
    if (w === 0 || h === 0) return;

    const containerSize = Math.min(w, h);
    const maxValue = Math.max(...items.map(i => i.value), 1);
    const count = items.length;
    const angleStep = 360 / count;

    // Orbit radius: % of half-container size, adjusted for number of items
    const orbitRadius = containerSize * (count === 1 ? 0 : 0.28);

    // Max circle diameter fits inside the remaining space
    const maxDiameter = containerSize * 0.38;
    const minDiameter = containerSize * 0.14;

    const circleEls = container.querySelectorAll<HTMLElement>('.circle-item');

    circleEls.forEach(el => {
        const label = el.dataset.itemId!;
        const item = items.find(i => i.label === label);
        if (!item) {
            el.style.display = 'none';
            return;
        }

        el.style.display = '';
        const idx = items.indexOf(item);
        const angle = (angleStep * idx - 90) * (Math.PI / 180);
        const x = Math.cos(angle) * orbitRadius;
        const y = Math.sin(angle) * orbitRadius;

        const valueRatio = item.value / maxValue;
        const diameter = minDiameter + (maxDiameter - minDiameter) * Math.sqrt(valueRatio);

        el.style.left = '50%';
        el.style.top = '50%';
        el.style.transform = `translate(-50%, -50%) translate(${x}px, ${y}px)`;

        const inner = el.querySelector<HTMLElement>('div');
        if (inner) {
            inner.style.width = `${diameter}px`;
            inner.style.height = `${diameter}px`;
            // Remove old color classes, apply new ones
            inner.className = `rounded-full flex items-center justify-center shadow-sm border-2 transition-all duration-300 ${colorClass(item.color, 'circle')}`;
        }

        const span = el.querySelector<HTMLElement>('.circle-value');
        if (span) {
            span.textContent = String(item.value);
            span.style.fontSize = `${Math.max(10, diameter * 0.28)}px`;
        }

        el.dataset.value = String(item.value);
        el.dataset.color = item.color;
    });
}

function updateLegend(container: HTMLElement, items: ComparisonItem[]): void {
    const legend = container.closest('.comparison-widget')?.querySelector('.legend-container');
    if (!legend) return;

    const existing = legend.querySelectorAll<HTMLElement>('.legend-item');
    existing.forEach(el => {
        const label = el.dataset.itemId!;
        const item = items.find(i => i.label === label);
        if (item) {
            const dot = el.querySelector<HTMLElement>('.legend-dot');
            if (dot) dot.className = `w-2.5 h-2.5 rounded-full shrink-0 legend-dot ${colorClass(item.color, 'dot')}`;
        }
    });
}

function applyHover(widget: HTMLElement, activeLabel: string | null): void {
    const opacity = activeLabel ? '0.25' : '';
    widget.querySelectorAll<HTMLElement>('.circle-item').forEach(el => {
        el.style.opacity = activeLabel && el.dataset.itemId !== activeLabel ? opacity : '';
    });
    widget.querySelectorAll<HTMLElement>('.legend-item').forEach(el => {
        el.style.opacity = activeLabel && el.dataset.itemId !== activeLabel ? opacity : '';
    });
}

export function initComparisonWidget(widgetId: string): void {
    const widget = document.querySelector<HTMLElement>(`[data-comparison-widget="${widgetId}"]`);
    if (!widget) return;

    const container = widget.querySelector<HTMLElement>('.circle-container');
    if (!container) return;

    const primaryItems: ComparisonItem[] = JSON.parse(widget.dataset.items || '[]');
    const allItems: ComparisonItem[] = JSON.parse(widget.dataset.allItems || '[]');
    const hasToggle = allItems.length > 0;

    let activeItems = primaryItems;

    // Initial layout
    layoutCircles(container, activeItems);

    // Resize observer keeps circles correctly sized
    const ro = new ResizeObserver(() => layoutCircles(container, activeItems));
    ro.observe(container);

    // Toggle switch — primary / all
    if (hasToggle) {
        const toggleBtns = widget.querySelectorAll<HTMLElement>('.comparison-toggle');
        toggleBtns.forEach(btn => {
            btn.addEventListener('click', () => {
                const mode = btn.dataset.mode;
                activeItems = mode === 'all' ? allItems : primaryItems;

                toggleBtns.forEach(b => b.classList.toggle('active', b.dataset.mode === mode));

                layoutCircles(container, activeItems);
                updateLegend(container, activeItems);
            });
        });
    }

    // Hover synchronisation — legend ↔ circles
    const legendItems = widget.querySelectorAll<HTMLElement>('.legend-item');
    const circleItems = widget.querySelectorAll<HTMLElement>('.circle-item');

    legendItems.forEach(el => {
        el.addEventListener('mouseenter', () => applyHover(widget, el.dataset.itemId!));
        el.addEventListener('mouseleave', () => applyHover(widget, null));
    });
    circleItems.forEach(el => {
        el.addEventListener('mouseenter', () => applyHover(widget, el.dataset.itemId!));
        el.addEventListener('mouseleave', () => applyHover(widget, null));
    });
}

export function initAllComparisonWidgets(): void {
    document.querySelectorAll<HTMLElement>('[data-comparison-widget]').forEach(widget => {
        const id = widget.dataset.comparisonWidget;
        if (id) initComparisonWidget(id);
    });
}
