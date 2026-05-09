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
    // ── Gate dialog elements ──
    const gateBackdrop   = root.querySelector<HTMLDivElement>('#first-idea-contact-gate-backdrop')!
    const gateDialog     = root.querySelector<HTMLDivElement>('#first-idea-contact-gate-dialog')!
    const gateDenyBtn    = root.querySelector<HTMLButtonElement>('#first-idea-contact-gate-deny')!
    const gateAcceptBtn  = root.querySelector<HTMLButtonElement>('#first-idea-contact-gate-accept')!
    const gateRemember   = root.querySelector<HTMLInputElement>('#first-idea-contact-gate-remember')!

    // ── Full dialog elements ──
    const backdrop        = root.querySelector<HTMLDivElement>('#first-idea-contact-backdrop')!
    const dialog          = root.querySelector<HTMLDivElement>('#first-idea-contact-dialog')!
    const emailInput      = root.querySelector<HTMLInputElement>('#first-idea-contact-email')!
    const permissionInput = root.querySelector<HTMLInputElement>('#first-idea-contact-permission')!
    const rememberInput   = root.querySelector<HTMLInputElement>('#first-idea-contact-remember')!
    const acceptButton    = root.querySelector<HTMLButtonElement>('#first-idea-contact-accept')!
    const denyButton      = root.querySelector<HTMLButtonElement>('#first-idea-contact-deny')!

    let activeResolver: ((choice: FirstIdeaContactChoice | null) => void) | null = null
    let activePromise: Promise<FirstIdeaContactChoice | null> | null = null
    let isOpen = false

    function hasStoredDecision(): boolean {
        return readStoredConsent(storageKey) !== null
    }

    function getStoredChoice(): FirstIdeaContactChoice | null {
        const consent = readStoredConsent(storageKey)
        if (!consent) return null
        return consent.permissionGranted
            ? { email: consent.email, permissionGranted: true, remembered: consent.remembered }
            : null
    }

    function resolve(choice: FirstIdeaContactChoice | null): void {
        const resolver = activeResolver
        activeResolver = null
        activePromise = null
        isOpen = false
        closeAll()
        resolver?.(choice)
    }

    function closeAll(): void {
        gateDialog.hidden = true
        gateBackdrop.hidden = true
        dialog.hidden = true
        backdrop.hidden = true
    }

    function openGate(): void {
        gateRemember.checked = false
        gateDialog.hidden = false
        gateBackdrop.hidden = false
    }

    function closeGate(): void {
        gateDialog.hidden = true
        gateBackdrop.hidden = true
    }

    function openFull(): void {
        emailInput.value = ''
        permissionInput.checked = false
        rememberInput.checked = false
        acceptButton.disabled = true
        dialog.hidden = false
        backdrop.hidden = false
        requestAnimationFrame(() => emailInput.focus())
    }

    function updateAcceptState(): void {
        const email = emailInput.value.trim()
        acceptButton.disabled = email.length === 0 || !emailInput.validity.valid || !permissionInput.checked
    }

    // ── Gate events ──
    gateBackdrop.addEventListener('click', () => resolve(null))

    gateDenyBtn.addEventListener('click', () => {
        if (gateRemember.checked) {
            writeStoredConsent(storageKey, {
                email: '',
                permissionGranted: false,
                remembered: true,
                decidedAt: new Date().toISOString(),
            })
        }
        resolve(null)
    })

    gateAcceptBtn.addEventListener('click', () => {
        closeGate()
        openFull()
    })

    // ── Full dialog events ──
    backdrop.addEventListener('click', () => resolve(null))
    emailInput.addEventListener('input', updateAcceptState)
    permissionInput.addEventListener('change', updateAcceptState)

    acceptButton.addEventListener('click', () => {
        if (acceptButton.disabled) return
        writeStoredConsent(storageKey, {
            email: emailInput.value.trim(),
            permissionGranted: true,
            remembered: true,
            decidedAt: new Date().toISOString(),
        })
        resolve({
            email: emailInput.value.trim(),
            permissionGranted: true,
            remembered: true,
        })
    })

    denyButton.addEventListener('click', () => {
        if (rememberInput.checked) {
            writeStoredConsent(storageKey, {
                email: '',
                permissionGranted: false,
                remembered: true,
                decidedAt: new Date().toISOString(),
            })
        }
        resolve(null)
    })

    function open(): Promise<FirstIdeaContactChoice | null> {
        const storedChoice = getStoredChoice()
        if (storedChoice || hasStoredDecision()) {
            return Promise.resolve(storedChoice)
        }

        if (isOpen && activePromise) return activePromise

        isOpen = true
        activePromise = new Promise<FirstIdeaContactChoice | null>((r) => { activeResolver = r })
        openGate()
        return activePromise
    }

    return { open, hasStoredDecision }
}