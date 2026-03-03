export interface ScrollNav {
    update(currentIndex: number, totalCount: number): void
    destroy(): void
}

export function renderScrollNav(
    onNavigate: (direction: 'up' | 'down') => void,
): ScrollNav {
    const nav = document.createElement('div')
    nav.className = 'fixed bottom-6 right-6 flex flex-col gap-2 z-50'
    nav.id = 'scroll-nav'

    nav.innerHTML = `
        <button
            id="scroll-up"
            class="w-12 h-12 rounded-full flex items-center justify-center transition-all"
            style="background-color: var(--color-primary); color: var(--color-text-on-primary); border: none; cursor: pointer; box-shadow: var(--shadow-lg);"
            aria-label="Previous question"
        >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/>
            </svg>
        </button>
        <button
            id="scroll-down"
            class="w-12 h-12 rounded-full flex items-center justify-center transition-all"
            style="background-color: var(--color-primary); color: var(--color-text-on-primary); border: none; cursor: pointer; box-shadow: var(--shadow-lg);"
            aria-label="Next question"
        >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/>
            </svg>
        </button>
    `

    const upBtn = nav.querySelector<HTMLButtonElement>('#scroll-up')!
    const downBtn = nav.querySelector<HTMLButtonElement>('#scroll-down')!

    upBtn.addEventListener('click', () => onNavigate('up'))
    downBtn.addEventListener('click', () => onNavigate('down'))

    document.body.appendChild(nav)

    function setDisabled(btn: HTMLButtonElement, disabled: boolean): void {
        if (disabled) {
            btn.style.opacity = '0.3'
            btn.style.cursor = 'not-allowed'
            btn.disabled = true
        } else {
            btn.style.opacity = '1'
            btn.style.cursor = 'pointer'
            btn.disabled = false
        }
    }

    return {
        update(currentIndex: number, totalCount: number): void {
            setDisabled(upBtn, currentIndex <= 0)
            setDisabled(downBtn, currentIndex >= totalCount - 1)
        },
        destroy(): void {
            nav.remove()
        },
    }
}

