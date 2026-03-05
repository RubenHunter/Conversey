export interface ScrollNav {
    update(currentIndex: number, totalCount: number): void
    destroy(): void
}

export function renderScrollNav(
    onNavigate: (direction: 'up' | 'down') => void,
): ScrollNav {
    const nav = document.createElement('div')
    nav.className = 'survey-scroll-nav'
    nav.id = 'scroll-nav'

    nav.innerHTML = `
        <button
            id="scroll-up"
            class="survey-scroll-btn"
            aria-label="Previous question"
        >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/>
            </svg>
        </button>
        <button
            id="scroll-down"
            class="survey-scroll-btn"
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
        btn.disabled = disabled
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
