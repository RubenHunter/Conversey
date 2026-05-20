import { getSurveyStrings } from '../../../i18n/survey'
import { renderSurveyHeader } from './surveyHeader'
import { InteractionType } from '../../../models/project'

const CHAT_BUBBLE_ICON = `<svg class="layout-picker-icon" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
  <path d="M10 14h38a6 6 0 016 6v24a6 6 0 01-6 6H30l-8 8-2-8H10a6 6 0 01-6-6V20a6 6 0 016-6z" fill="white" fill-opacity="0.95" stroke="white" stroke-width="2" stroke-linejoin="round"/>
  <ellipse cx="32" cy="24" rx="8" ry="1.5" fill="color-mix(in srgb, var(--color-primary) 25%, transparent)" opacity="0.7"/>
  <circle cx="32" cy="32" r="3" fill="color-mix(in srgb, var(--color-primary) 25%, transparent)" opacity="0.7"/>
</svg>`

const CLASSIC_SCROLL_ICON = `<svg class="layout-picker-icon" viewBox="0 0 64 64" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
  <rect x="14" y="8" width="36" height="48" rx="5" fill="white" fill-opacity="0.95" stroke="white" stroke-width="2"/>
  <rect x="21" y="16" width="22" height="6" rx="1.5" fill="color-mix(in srgb, var(--color-primary) 25%, transparent)" opacity="0.7"/>
  <rect x="21" y="26" width="22" height="2" rx="1" fill="color-mix(in srgb, var(--color-primary) 18%, transparent)" opacity="0.7"/>
  <rect x="21" y="32" width="22" height="2" rx="1" fill="color-mix(in srgb, var(--color-primary) 18%, transparent)" opacity="0.7"/>
  <rect x="21" y="38" width="15" height="2" rx="1" fill="color-mix(in srgb, var(--color-primary) 18%, transparent)" opacity="0.7"/>
  <rect x="21" y="44" width="22" height="2" rx="1" fill="color-mix(in srgb, var(--color-primary) 18%, transparent)" opacity="0.5"/>
</svg>`

const chatExampleImage = new URL('../../../chat_example.png', import.meta.url).href
const classicExampleImage = new URL('../../../classic_example.png', import.meta.url).href


interface ShowLayoutPickerParams {
    container: HTMLElement
    storageKey: string
    organizationName: string
    organizationSlug: string
}

export async function showLayoutPicker({
    container,
    storageKey,
    organizationName,
    organizationSlug,
}: ShowLayoutPickerParams): Promise<typeof InteractionType.Chat | typeof InteractionType.VerticalScroll> {
    const t = getSurveyStrings()
    const headerHTML = renderSurveyHeader({ organizationName, organizationSlug })
    

    return new Promise((resolve) => {
        container.innerHTML = `
            <div class="layout-picker-wrap">
                ${headerHTML}

                <div class="layout-picker-header">
                    <h1 class="layout-picker-title">${t.layoutPickerTitle}</h1>
                </div>

                <div class="layout-picker-body max-[600px]:flex-col">
                    <button class="layout-picker-side layout-picker-side--chat" data-layout="${InteractionType.Chat}" type="button">
                        <div class="layout-picker-bg" style="background-image: url('${chatExampleImage}')"></div>
                        <div class="layout-picker-overlay"></div>
                        <div class="layout-picker-icon-wrap">${CHAT_BUBBLE_ICON}</div>
                        <div class="layout-picker-label">
                            <span class="layout-picker-label-title">${t.layoutPickerChat}</span>
                            <p class="layout-picker-label-desc">${t.layoutPickerChatDesc}</p>
                        </div>
                    </button>

                    <div class="layout-picker-divider max-[600px]:left-0 max-[600px]:right-0 max-[600px]:top-1/2 max-[600px]:bottom-auto max-[600px]:w-auto max-[600px]:h-0 max-[600px]:border-l-0 max-[600px]:border-t max-[600px]:border-t-[rgba(255,255,255,0.2)]">
                        <span class="layout-picker-or">or</span>
                    </div>

                    <button class="layout-picker-side layout-picker-side--classic" data-layout="${InteractionType.VerticalScroll}" type="button">
                        <div class="layout-picker-bg" style="background-image: url('${classicExampleImage}')"></div>
                        <div class="layout-picker-overlay"></div>
                        <div class="layout-picker-icon-wrap">${CLASSIC_SCROLL_ICON}</div>
                        <div class="layout-picker-label">
                            <span class="layout-picker-label-title">${t.layoutPickerClassic}</span>
                            <p class="layout-picker-label-desc">${t.layoutPickerClassicDesc}</p>
                        </div>
                    </button>
                </div>

                <div class="layout-picker-footer">
                    <label class="layout-picker-toggle-wrap">
                        <div class="layout-picker-toggle-track" id="layout-picker-toggle-track">
                            <div class="layout-picker-toggle-thumb"></div>
                        </div>
                        <input type="checkbox" id="layout-picker-remember" style="display:none">
                        <span class="layout-picker-toggle-label">${t.layoutPickerSave}</span>
                    </label>
                </div>
            </div>`

        const track = container.querySelector<HTMLElement>('#layout-picker-toggle-track')
        const checkbox = container.querySelector<HTMLInputElement>('#layout-picker-remember')

        const syncToggleState = () => {
            track?.classList.toggle('layout-picker-toggle--on', checkbox?.checked ?? false)
        }

        checkbox?.addEventListener('change', syncToggleState)
        syncToggleState()

        container.querySelectorAll<HTMLButtonElement>('[data-layout]').forEach((btn) => {
            btn.addEventListener('click', () => {
                const layout = btn.getAttribute('data-layout')
                if (layout !== InteractionType.Chat && layout !== InteractionType.VerticalScroll) return
                const remember = checkbox?.checked ?? false
                try {
                    if (remember) {
                        localStorage.setItem(storageKey, layout)
                    } else {
                        localStorage.removeItem(storageKey)
                    }
                } catch {
                    // Ignore storage failures and continue with the selected layout.
                }
                resolve(layout)
            })
        })
    })
}