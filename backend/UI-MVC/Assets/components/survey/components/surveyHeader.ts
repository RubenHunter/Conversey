import { getOrganizationBranding, renderOrganizationBadge } from '../../shared/organizationBranding'
import { getLocale, type SurveyLocale } from '../../../i18n/survey'

interface RenderSurveyHeaderParams {
    organizationName: string
    organizationSlug: string
    organizationLogo?: string
}

const LANG_LABELS: Record<SurveyLocale, string> = {
    nl: 'NL',
    en: 'EN',
    fr: 'FR',
}

export function renderSurveyHeader({ organizationName, organizationSlug, organizationLogo }: RenderSurveyHeaderParams): string {
    const { badge, displayName, logoUrl } = getOrganizationBranding(organizationName, organizationSlug, organizationLogo)
    const currentLang = getLocale()
    const langOptions = (['nl', 'en', 'fr'] as SurveyLocale[])
        .map((locale) => {
            const selected = locale === currentLang ? ' class="lang-option--selected"' : ''
            return `<button class="lang-option" data-lang="${locale}"${selected}>${LANG_LABELS[locale]}</button>`
        })
        .join('')

    return `
        <div class="survey-topbar">
            <div class="survey-topbar-left">
                <div class="survey-topbar-logo"><img src="/Assets/Conversey_logo.png" alt="Conversey" /></div>
                <div class="survey-topbar-logo-title max-[440px]:hidden">CONVERSEY</div>
            </div>
            <div class="survey-topbar-right">
                <div class="lang-dropdown">
                    <button class="lang-toggle" aria-label="Change language" aria-haspopup="true" aria-expanded="false">
                        <svg class="lang-globe-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                            <circle cx="12" cy="12" r="10"/>
                            <path d="M2 12h20M12 2a15.3 15.3 0 014 10 15.3 15.3 0 01-4 10 15.3 15.3 0 01-4-10A15.3 15.3 0 0112 2z"/>
                        </svg>
                        <span>${LANG_LABELS[currentLang]}</span>
                        <span class="lang-chevron" aria-hidden="true">▾</span>
                    </button>
                    <div class="lang-menu hidden" role="menu">${langOptions}</div>
                </div>
                <div class="survey-topbar-brand">
                    ${renderOrganizationBadge(badge, logoUrl)}
                    <div class="survey-topbar-name max-[340px]:hidden">${displayName}</div>
                </div>
            </div>
        </div>
    `
}

interface CreateSurveyHeaderControllerParams {
    root: HTMLElement
}

interface SurveyHeaderController {
    updateProgress(current: number, total: number): void
    getProgressBar(): HTMLElement | null
}

export function createSurveyHeaderController({
    root,
}: CreateSurveyHeaderControllerParams): SurveyHeaderController {
    const progressBar = root.querySelector<HTMLElement>('#progress-bar')
    const progressBadge = root.querySelector<HTMLElement>('#progress-badge')

    function updateProgress(current: number, total: number): void {
        if (progressBar) {
            const percentage = total > 0 ? (current / total) * 100 : 0
            progressBar.style.width = `${percentage}%`
        }
        if (progressBadge) {
            progressBadge.textContent = `${current} / ${total}`
        }
    }

    return {
        updateProgress,
        getProgressBar: () => progressBar || null,
    }
}
