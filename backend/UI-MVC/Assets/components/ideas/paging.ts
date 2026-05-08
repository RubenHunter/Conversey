export function getVisibleLimit(batchSize: number, extraLoadsUsed: number): number {
    return batchSize * (1 + extraLoadsUsed)
}

export function hasMoreIdeasToLoad(
    visibleIdeasCacheLength: number,
    visibleLimit: number,
    extraLoadsUsed: number,
    maxExtraLoads: number,
): boolean {
    return visibleIdeasCacheLength > visibleLimit && extraLoadsUsed < maxExtraLoads
}

export function resetPaging(_extraLoadsUsed: number, _isLoadingMoreIdeas: boolean, _autoLoadArmed: boolean): { extraLoadsUsed: number; isLoadingMoreIdeas: boolean; autoLoadArmed: boolean } {
    return { extraLoadsUsed: 0, isLoadingMoreIdeas: false, autoLoadArmed: true }
}

export function updateLoadMoreButton(params: {
    btn: HTMLButtonElement
    textEl: HTMLSpanElement
    isLoading: boolean
    hasMore: boolean
    loadingText: string
    loadMoreText: string
    listEl: HTMLElement
}): void {
    const { btn, textEl, isLoading, hasMore, loadingText, loadMoreText: moreText, listEl } = params
    const wasLoading = btn.classList.contains('ideas-load-more--loading')

    btn.hidden = !hasMore
    btn.disabled = isLoading || !hasMore
    btn.classList.toggle('ideas-load-more--loading', isLoading)
    btn.setAttribute('aria-busy', String(isLoading))
    textEl.textContent = isLoading ? loadingText : moreText

    listEl.classList.toggle('ideas-list--has-more', hasMore)

    // Force SVG animation restart
    if (isLoading && !wasLoading) {
        const ringFill = btn.querySelector<SVGCircleElement>('.ideas-load-more-ring-fill')
        if (ringFill) {
            ringFill.style.animation = 'none'
            void ringFill.getBoundingClientRect()
            ringFill.style.animation = ''
        }
    }

    if (btn.parentElement !== listEl) {
        listEl.appendChild(btn)
    }
}
