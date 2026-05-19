import { getLocale, setLocale, type SurveyLocale } from '../../i18n/survey'

// Event delegation — works even when dropdown is rendered after DOMContentLoaded
document.addEventListener('click', (e) => {
    const target = e.target as HTMLElement

    // Toggle handler
    const toggle = target.closest('.lang-toggle')
    if (toggle instanceof HTMLElement) {
        e.stopPropagation()
        const dd = toggle.closest('.lang-dropdown')
        if (!dd) return
        const menu = dd.querySelector('.lang-menu')
        if (!menu) return
        const isOpen = !menu.classList.contains('hidden')
        // Close all other dropdowns
        document.querySelectorAll('.lang-menu').forEach((m) => m.classList.add('hidden'))
        document.querySelectorAll('.lang-toggle').forEach((t) => t.setAttribute('aria-expanded', 'false'))
        if (!isOpen) {
            menu.classList.remove('hidden')
            toggle.setAttribute('aria-expanded', 'true')
        }
    }

    // Option handler
    const option = target.closest('.lang-option') as HTMLButtonElement | null
    if (option) {
        e.stopPropagation()
        const locale = option.dataset.lang as SurveyLocale | undefined
        if (!locale || locale === getLocale()) return
        setLocale(locale)
        window.location.reload()
    }

    // Close on outside click (if not clicking toggle or menu)
    if (!toggle && !option) {
        document.querySelectorAll('.lang-menu').forEach((m) => m.classList.add('hidden'))
        document.querySelectorAll('.lang-toggle').forEach((t) => t.setAttribute('aria-expanded', 'false'))
    }
})

export {}
