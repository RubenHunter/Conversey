export function renderCompletedPage(container: HTMLElement): void {
    container.innerHTML = `
        <div class="flex flex-col items-center justify-center min-h-dvh px-6 py-10">
            <div class="w-20 h-20 rounded-full flex items-center justify-center mb-6"
                 style="background-color: var(--color-success-bg);">
                <svg class="w-10 h-10" style="color: var(--color-success);" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
                </svg>
            </div>
            <h1 class="text-2xl font-bold text-center mb-3" style="color: var(--color-text);">
                Thank You!
            </h1>
            <p class="text-center mb-8 leading-relaxed" style="color: var(--color-text-secondary); font-size: var(--font-size-base);">
                Your survey responses have been submitted successfully. Your input helps us make a real difference.
            </p>
            <button
                id="btn-to-ideas"
                class="w-full py-4 px-6 rounded-xl font-semibold text-lg transition-all active:scale-[0.98]"
                style="background-color: var(--color-secondary); color: var(--color-text-on-primary); border: none; cursor: pointer; box-shadow: var(--shadow-md);">
                Continue to Ideas
            </button>
        </div>
    `

    // Placeholder: "Continue to Ideas" button - will navigate to ideas phase later
    const ideasBtn = container.querySelector<HTMLButtonElement>('#btn-to-ideas')
    ideasBtn?.addEventListener('click', () => {
        alert('Ideas phase coming soon!')
    })
}

