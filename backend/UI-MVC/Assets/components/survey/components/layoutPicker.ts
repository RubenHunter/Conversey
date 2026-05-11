import { getSurveyStrings } from '../../../i18n/survey'
import { renderSurveyHeader } from './surveyHeader'

const chatExampleImage = '/images/chat_example.png'
const classicExampleImage = '/images/Screenshot 2026-05-11 105048.png'


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
}: ShowLayoutPickerParams): Promise<'chat' | 'classic'> {
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
                    <button class="layout-picker-side layout-picker-side--chat" data-layout="chat" type="button">
                        <div class="layout-picker-bg" style="background-image: url('${chatExampleImage}')"></div>
                        <div class="layout-picker-overlay"></div>
                        <div class="layout-picker-label">
                            <span class="layout-picker-label-title">${t.layoutPickerChat}</span>
                            <p class="layout-picker-label-desc">${t.layoutPickerChatDesc}</p>
                        </div>
                    </button>

                    <div class="layout-picker-divider max-[600px]:left-0 max-[600px]:right-0 max-[600px]:top-1/2 max-[600px]:bottom-auto max-[600px]:w-auto max-[600px]:h-0 max-[600px]:border-l-0 max-[600px]:border-t max-[600px]:border-t-[rgba(255,255,255,0.2)]">
                        <span class="layout-picker-or">or</span>
                    </div>

                    <button class="layout-picker-side layout-picker-side--classic" data-layout="classic" type="button">
                        <div class="layout-picker-bg layout-picker-bg--classic" style="background-image: url('${classicExampleImage}')"></div>
                        <div class="layout-picker-overlay"></div>
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
                const layout = btn.getAttribute('data-layout') as 'chat' | 'classic'
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