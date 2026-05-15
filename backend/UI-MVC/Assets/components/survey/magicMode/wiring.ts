import { createMagicModeModal, type MagicModeModalController } from './magicModeModal'

export interface MagicModeWiringOptions {
    getQuestionText: () => string
    onResult: (finalText: string) => void
}

export function wireMagicModeButton(
    button: HTMLElement,
    options: MagicModeWiringOptions
): MagicModeModalController {
    const modal = createMagicModeModal()
    button.addEventListener('click', () => {
        modal.open(options.getQuestionText(), options.onResult)
    })
    return modal
}
