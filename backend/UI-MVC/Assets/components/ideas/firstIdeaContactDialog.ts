interface CreateFirstIdeaContactDialogParams {
    root: ParentNode
    storageKey: string
}

interface StoredContactConsent {
    email: string
    permissionGranted: boolean
    remembered: boolean
    decidedAt: string
}

interface FirstIdeaContactChoice {
    email: string
    permissionGranted: boolean
    remembered: boolean
}

function readStoredConsent(storageKey: string): StoredContactConsent | null {
    try {
        const raw = window.localStorage.getItem(storageKey)
        if (!raw) return null
        const parsed = JSON.parse(raw) as Partial<StoredContactConsent> | null
        if (!parsed || typeof parsed !== 'object') return null
        if (typeof parsed.email !== 'string') return null
        if (typeof parsed.permissionGranted !== 'boolean') return null
        if (typeof parsed.remembered !== 'boolean') return null
        if (typeof parsed.decidedAt !== 'string') return null

        return {
            email: parsed.email,
            permissionGranted: parsed.permissionGranted,
            remembered: parsed.remembered,
            decidedAt: parsed.decidedAt,
        }
    } catch {
        return null
    }
}

function writeStoredConsent(storageKey: string, consent: StoredContactConsent): void {
    try {
        window.localStorage.setItem(storageKey, JSON.stringify(consent))
    } catch {
        // Ignore storage failures so the dialog still works in restricted environments.
    }
}

export function createFirstIdeaContactDialogController({ root, storageKey }: CreateFirstIdeaContactDialogParams): {
    open(): Promise<FirstIdeaContactChoice | null>
    hasStoredDecision(): boolean
} {
    const backdrop = root.querySelector<HTMLDivElement>('#first-idea-contact-backdrop')!
    const dialog = root.querySelector<HTMLDivElement>('#first-idea-contact-dialog')!
    const emailInput = root.querySelector<HTMLInputElement>('#first-idea-contact-email')!
    const permissionInput = root.querySelector<HTMLInputElement>('#first-idea-contact-permission')!
    const rememberChoiceInput = root.querySelector<HTMLInputElement>('#first-idea-contact-remember')!
    const acceptButton = root.querySelector<HTMLButtonElement>('#first-idea-contact-accept')!
    const denyButton = root.querySelector<HTMLButtonElement>('#first-idea-contact-deny')!

    let isOpen = false
    let activeResolver: ((choice: FirstIdeaContactChoice | null) => void) | null = null
    let activePromise: Promise<FirstIdeaContactChoice | null> | null = null

    function getStoredChoice(): FirstIdeaContactChoice | null {
        const consent = readStoredConsent(storageKey)
        if (!consent) return null
        return consent.permissionGranted
            ? {
                email: consent.email,
                permissionGranted: true,
                remembered: consent.remembered,
            }
            : null
    }

    function hasStoredDecision(): boolean {
        return readStoredConsent(storageKey) !== null
    }

    function close(): void {
        dialog.hidden = true
        backdrop.hidden = true
        isOpen = false
    }

    function resolve(choice: FirstIdeaContactChoice | null): void {
        const resolver = activeResolver
        activeResolver = null
        activePromise = null
        close()
        resolver?.(choice)
    }

    function updateAcceptState(): void {
        const email = emailInput.value.trim()
        acceptButton.disabled = email.length === 0 || !emailInput.validity.valid || !permissionInput.checked
    }

    function persistAcceptedChoice(): void {
        const consent: StoredContactConsent = {
            email: emailInput.value.trim(),
            permissionGranted: permissionInput.checked,
            remembered: true,
            decidedAt: new Date().toISOString(),
        }
        writeStoredConsent(storageKey, consent)
    }

    function persistDeniedChoice(): void {
        if (!rememberChoiceInput.checked) return

        const consent: StoredContactConsent = {
            email: '',
            permissionGranted: false,
            remembered: true,
            decidedAt: new Date().toISOString(),
        }
        writeStoredConsent(storageKey, consent)
    }

    function open(): Promise<FirstIdeaContactChoice | null> {
        const storedChoice = getStoredChoice()
        if (storedChoice || hasStoredDecision()) {
            return Promise.resolve(storedChoice)
        }

        if (isOpen && activePromise) {
            return activePromise
        }

        isOpen = true
        emailInput.value = ''
        permissionInput.checked = false
        rememberChoiceInput.checked = false
        acceptButton.disabled = true
        dialog.hidden = false
        backdrop.hidden = false

        activePromise = new Promise<FirstIdeaContactChoice | null>((resolvePromise) => {
            activeResolver = resolvePromise
        })

        requestAnimationFrame(() => {
            emailInput.focus()
        })

        return activePromise
    }

    backdrop.addEventListener('click', () => {
        resolve(null)
    })

    emailInput.addEventListener('input', updateAcceptState)
    permissionInput.addEventListener('change', updateAcceptState)

    acceptButton.addEventListener('click', () => {
        if (acceptButton.disabled) return
        persistAcceptedChoice()
        resolve({
            email: emailInput.value.trim(),
            permissionGranted: true,
            remembered: true,
        })
    })

    denyButton.addEventListener('click', () => {
        persistDeniedChoice()
        resolve(null)
    })

    return {
        open,
        hasStoredDecision,
    }
}





