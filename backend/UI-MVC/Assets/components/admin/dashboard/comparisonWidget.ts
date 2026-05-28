/**
 * Comparison Widget — radial bubble comparison with toggle and hover sync.
 * Circles use solid brand fills sized proportionally to sqrt(value).
 */

interface ComparisonItem {
    label: string;
    value: number;
    color: string;
}

function colorHex(color: string): string {
    switch (color) {
        case 'secondary': return '#db99c8';
        case 'accent':    return '#cd6f88';
        default:          return '#6c5ce7';
    }
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

    const orbitRadius = containerSize * (count === 1 ? 0 : 0.28);
    const maxDiameter = containerSize * 0.42;
    const minDiameter = containerSize * 0.18;

    container.querySelectorAll<HTMLElement>('.circle-item').forEach(el => {
        const label = el.dataset.itemId!;
        const item = items.find(i => i.label === label);
        if (!item) { el.style.display = 'none'; return; }

        el.style.display = '';
        const idx = items.indexOf(item);
        const angle = (angleStep * idx - 90) * (Math.PI / 180);
        const x = Math.cos(angle) * orbitRadius;
        const y = Math.sin(angle) * orbitRadius;

        // sqrt scaling for perceptually fair area comparison
        const diameter = minDiameter + (maxDiameter - minDiameter) * Math.sqrt(item.value / maxValue);

        el.style.left = '50%';
        el.style.top = '50%';
        el.style.transform = `translate(-50%, -50%) translate(${x}px, ${y}px)`;

        const inner = el.querySelector<HTMLElement>('div');
        if (inner) {
            inner.style.width  = `${diameter}px`;
            inner.style.height = `${diameter}px`;
            inner.style.backgroundColor = colorHex(item.color);
            inner.style.color = '#fff';
            inner.className = 'rounded-full flex items-center justify-center shadow-md transition-all duration-300';
        }

        const span = el.querySelector<HTMLElement>('.circle-value');
        if (span) {
            span.textContent = String(item.value);
            span.style.fontSize = `${Math.max(10, diameter * 0.3)}px`;
            span.style.fontWeight = '600';
        }
    });
}

function updateLegend(widget: HTMLElement, items: ComparisonItem[]): void {
    const legend = widget.querySelector<HTMLElement>('.legend-container');
    if (!legend) return;

    // Rebuild legend items to reflect new dataset
    legend.innerHTML = items.map(item => `
        <div class="flex items-center gap-2 legend-item cursor-pointer" data-item-id="${item.label}">
            <div class="w-3.5 h-2 rounded shrink-0 legend-dot" style="background-color: ${colorHex(item.color)}"></div>
            <span class="text-xs text-text/70 truncate">${item.label}</span>
        </div>
    `).join('');

    // Re-attach hover handlers to new legend items
    const circleItems = widget.querySelectorAll<HTMLElement>('.circle-item');
    legend.querySelectorAll<HTMLElement>('.legend-item').forEach(el => {
        el.addEventListener('mouseenter', () => applyHover(widget, el.dataset.itemId!));
        el.addEventListener('mouseleave', () => applyHover(widget, null));
    });
    circleItems.forEach(el => {
        el.addEventListener('mouseenter', () => applyHover(widget, el.dataset.itemId!));
        el.addEventListener('mouseleave', () => applyHover(widget, null));
    });
}

function applyHover(widget: HTMLElement, activeLabel: string | null): void {
    widget.querySelectorAll<HTMLElement>('.circle-item').forEach(el => {
        el.style.opacity = activeLabel && el.dataset.itemId !== activeLabel ? '0.25' : '';
    });
    widget.querySelectorAll<HTMLElement>('.legend-item').forEach(el => {
        el.style.opacity = activeLabel && el.dataset.itemId !== activeLabel ? '0.25' : '';
    });
}

export function initComparisonWidget(widgetId: string): void {
    const widget = document.querySelector<HTMLElement>(`[data-comparison-widget="${widgetId}"]`);
    if (!widget) return;

    const container = widget.querySelector<HTMLElement>('.circle-container');
    if (!container) return;

    const primaryItems: ComparisonItem[] = JSON.parse(widget.dataset.items || '[]');
    const allItems: ComparisonItem[]     = JSON.parse(widget.dataset.allItems || '[]');
    const hasToggle = allItems.length > 0;

    let activeItems = primaryItems;

    layoutCircles(container, activeItems);

    const ro = new ResizeObserver(() => layoutCircles(container, activeItems));
    ro.observe(container);

    if (hasToggle) {
        const toggleBtns = widget.querySelectorAll<HTMLElement>('.comparison-toggle');
        toggleBtns.forEach(btn => {
            btn.addEventListener('click', () => {
                const mode = btn.dataset.mode;
                activeItems = mode === 'all' ? allItems : primaryItems;

                toggleBtns.forEach(b => b.classList.toggle('active', b.dataset.mode === mode));

                layoutCircles(container, activeItems);
                updateLegend(widget, activeItems);
            });
        });
    }

    // Hover sync — legend ↔ circles
    const legendItems  = widget.querySelectorAll<HTMLElement>('.legend-item');
    const circleItems  = widget.querySelectorAll<HTMLElement>('.circle-item');

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
