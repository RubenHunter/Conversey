/**
 * Health Check Widget — inline sequential provider check animation.
 * Triggered by clicking the [data-health-check] button inside the quick links widget.
 */

interface HealthResult {
    provider: string;
    status: 'ok' | 'error';
    latencyMs: number;
}

const DOT_IDLE    = '#e5e7eb'; // gray-200
const DOT_ACTIVE  = '#facc15'; // yellow-400
const DOT_OK      = '#4ade80'; // green-400
const DOT_ERROR   = '#f43f5e'; // rose-500

function sleep(ms: number): Promise<void> {
    return new Promise(r => setTimeout(r, ms));
}

function setDotColor(dot: HTMLElement, color: string): void {
    dot.style.backgroundColor = color;
}

export function initHealthCheck(): void {
    const btn = document.querySelector<HTMLElement>('[data-health-check="true"]');
    if (!btn) return;

    const widget = btn.closest<HTMLElement>('.quick-links-widget');
    if (!widget) return;

    const panel     = widget.querySelector<HTMLElement>('.health-check-panel');
    const list      = widget.querySelector<HTMLElement>('.quick-links-list');
    const dots      = Array.from(widget.querySelectorAll<HTMLElement>('.health-dot'));
    const statusEl  = widget.querySelector<HTMLElement>('.health-status');
    const closeBtn  = widget.querySelector<HTMLElement>('.health-close');

    if (!panel || !statusEl || dots.length < 3) return;

    btn.addEventListener('click', async () => {
        if (!panel.classList.contains('hidden')) return; // already running

        // Show panel, hide list
        list?.classList.add('hidden');
        panel.classList.remove('hidden');

        // Reset dots
        dots.forEach(d => setDotColor(d, DOT_IDLE));
        setDotColor(dots[0], DOT_ACTIVE);
        statusEl.textContent = 'Checking providers…';
        statusEl.className = 'health-status text-sm text-text/50 text-center';
        closeBtn?.classList.add('hidden');

        let results: HealthResult[] = [];
        try {
            const res = await fetch('/api/admin/ai/health', { credentials: 'include' });
            results = await res.json() as HealthResult[];
        } catch {
            statusEl.textContent = 'Could not reach health endpoint.';
            statusEl.className = 'health-status text-sm text-red-500 text-center';
            closeBtn?.classList.remove('hidden');
            return;
        }

        // Ensure we have 3 entries; pad with OK if fewer
        while (results.length < 3) {
            results.push({ provider: '—', status: 'ok', latencyMs: 0 });
        }

        for (let i = 0; i < 3; i++) {
            const result = results[i];
            statusEl.textContent = `Checking ${result.provider}…`;

            await sleep(750);

            setDotColor(dots[i], result.status === 'ok' ? DOT_OK : DOT_ERROR);

            if (i + 1 < 3) {
                setDotColor(dots[i + 1], DOT_ACTIVE);
            }
        }

        const allOk = results.every(r => r.status === 'ok');
        statusEl.textContent = allOk
            ? 'All systems operational'
            : `${results.filter(r => r.status !== 'ok').length} issue(s) detected`;
        statusEl.className = `health-status text-sm text-center font-semibold ${allOk ? 'text-green-600' : 'text-red-500'}`;

        closeBtn?.classList.remove('hidden');
    });

    closeBtn?.addEventListener('click', () => {
        panel.classList.add('hidden');
        list?.classList.remove('hidden');
        dots.forEach(d => setDotColor(d, DOT_IDLE));
        statusEl.textContent = '';
        closeBtn.classList.add('hidden');
    });
}
