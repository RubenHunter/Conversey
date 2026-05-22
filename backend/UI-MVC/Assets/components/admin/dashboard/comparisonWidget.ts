/**
 * Comparison Widget Module
 * Handles interactivity for the radial circle comparison widget.
 * Provides hover effects and legend interactions.
 */

/**
 * Configuration for comparison widget items
 */
export interface ComparisonItem {
    label: string;
    value: number;
    color: string;
}

/**
 * Configuration for the comparison widget
 */
export interface ComparisonWidgetConfig {
    widgetId: string;
    items: ComparisonItem[];
}

/**
 * Initialize comparison widget interactivity
 */
export function initComparisonWidget(config: ComparisonWidgetConfig): void {
    const widget = document.querySelector(`[data-comparison-widget="${config.widgetId}"]`);
    if (!widget) {
        console.warn(`Comparison widget not found: ${config.widgetId}`);
        return;
    }

    const parent = widget.closest('.comparison-widget');
    if (!parent) return;

    // Get all circle items and legend items
    const circleItems = parent.querySelectorAll('.circle-item');
    const legendItems = parent.querySelectorAll('.legend-item');

    // Create a map of label to item for easy lookup
    const itemMap: Map<string, { circle: Element; legend: Element }> = new Map();
    
    legendItems.forEach(legend => {
        const label = legend.dataset.itemId;
        const circle = parent.querySelector(`.circle-item[data-item-id="${label}"]`);
        if (label && circle) {
            itemMap.set(label, { circle, legend });
        }
    });

    // Add hover effects to legend items
    legendItems.forEach(legend => {
        const label = legend.dataset.itemId;
        if (!label) return;

        legend.addEventListener('mouseenter', () => {
            const item = itemMap.get(label);
            if (item) {
                // Highlight legend
                legend.querySelector('.w-3')?.classList.remove('opacity-35');
                legend.querySelector('.w-3')?.classList.add('opacity-100');
                legend.querySelector('span')?.classList.remove('text-text/70');
                legend.querySelector('span')?.classList.add('text-text');
                
                // Scale up circle
                const circleDiv = item.circle.querySelector('div');
                if (circleDiv) {
                    circleDiv.classList.add('scale-105');
                }
            }
        });

        legend.addEventListener('mouseleave', () => {
            const item = itemMap.get(label);
            if (item) {
                // Restore legend
                legend.querySelector('.w-3')?.classList.add('opacity-35');
                legend.querySelector('.w-3')?.classList.remove('opacity-100');
                legend.querySelector('span')?.classList.add('text-text/70');
                legend.querySelector('span')?.classList.remove('text-text');
                
                // Restore circle
                const circleDiv = item.circle.querySelector('div');
                if (circleDiv) {
                    circleDiv.classList.remove('scale-105');
                }
            }
        });

        // Add click to filter (optional - could highlight only selected items)
        legend.addEventListener('click', () => {
            // Toggle visibility of the item
            const item = itemMap.get(label);
            if (item) {
                const circleDiv = item.circle.querySelector('div');
                if (circleDiv) {
                    const isHidden = circleDiv.classList.contains('opacity-0');
                    if (isHidden) {
                        circleDiv.classList.remove('opacity-0');
                        legend.classList.remove('opacity-30');
                    } else {
                        circleDiv.classList.add('opacity-0');
                        legend.classList.add('opacity-30');
                    }
                }
            }
        });
    });

    // Add hover effects to circle items
    circleItems.forEach(circle => {
        const label = circle.dataset.itemId;
        if (!label) return;

        circle.addEventListener('mouseenter', () => {
            const item = itemMap.get(label);
            if (item) {
                // Highlight legend
                const legend = item.legend;
                legend.querySelector('.w-3')?.classList.remove('opacity-35');
                legend.querySelector('.w-3')?.classList.add('opacity-100');
                legend.querySelector('span')?.classList.remove('text-text/70');
                legend.querySelector('span')?.classList.add('text-text');
                
                // Scale up circle
                const circleDiv = circle.querySelector('div');
                if (circleDiv) {
                    circleDiv.classList.add('scale-105');
                }
            }
        });

        circle.addEventListener('mouseleave', () => {
            const item = itemMap.get(label);
            if (item) {
                // Restore legend
                const legend = item.legend;
                legend.querySelector('.w-3')?.classList.add('opacity-35');
                legend.querySelector('.w-3')?.classList.remove('opacity-100');
                legend.querySelector('span')?.classList.add('text-text/70');
                legend.querySelector('span')?.classList.remove('text-text');
                
                // Restore circle
                const circleDiv = circle.querySelector('div');
                if (circleDiv) {
                    circleDiv.classList.remove('scale-105');
                }
            }
        });

        circle.addEventListener('click', () => {
            // Toggle visibility
            const circleDiv = circle.querySelector('div');
            if (circleDiv) {
                const isHidden = circleDiv.classList.contains('opacity-0');
                if (isHidden) {
                    circleDiv.classList.remove('opacity-0');
                } else {
                    circleDiv.classList.add('opacity-0');
                }
            }
            
            const item = itemMap.get(label);
            if (item) {
                const legend = item.legend;
                const isHidden = circle.querySelector('div')?.classList.contains('opacity-0');
                if (isHidden) {
                    legend.classList.add('opacity-30');
                } else {
                    legend.classList.remove('opacity-30');
                }
            }
        });
    });
}

/**
 * Initialize all comparison widgets on the page
 */
export function initAllComparisonWidgets(): void {
    const widgets = document.querySelectorAll('[data-comparison-widget]');
    widgets.forEach(widget => {
        const widgetId = widget.dataset.comparisonWidget;
        if (widgetId) {
            // Extract items from the legend
            const legendItems = widget.querySelectorAll('.legend-item');
            const items: ComparisonItem[] = [];
            
            legendItems.forEach(legend => {
                const label = legend.dataset.itemId;
                const valueEl = widget.querySelector(`.circle-item[data-item-id="${label}"] .font-semibold`);
                const value = parseInt(valueEl?.textContent || '0');

                if (label) {
                    items.push({ label, value, color: 'primary' });
                }
            });
            
            initComparisonWidget({ widgetId, items });
        }
    });
}
