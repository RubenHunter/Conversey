interface StepDraftData {
    fields: Record<string, string>;
    draftSynced: boolean;
}

function notifyPreviewIframes(storageKey: string): void {
    const iframes = document.querySelectorAll<HTMLIFrameElement>('iframe')
    for (const iframe of iframes) {
        iframe.contentWindow?.postMessage(
            { type: 'draft-changed', storageKey },
            window.location.origin,
        )
    }
}

export class StepDraftManager {
    readonly storageKey: string;

    constructor(
        readonly stepNum: number,
        readonly form: HTMLFormElement,
        storagePrefix: string,
        private readonly beforeSave?: () => Promise<boolean>,
    ) {
        this.storageKey = `${storagePrefix}:step:${stepNum}`;
    }

    persist(): void {
        const fields = this.collectFields();
        localStorage.setItem(this.storageKey, JSON.stringify({ fields, draftSynced: false }));
        notifyPreviewIframes(this.storageKey);
    }

    hydrate(): void {
        const data = this.readData();
        if (!data) return;
        this.populateFields(data.fields);
    }

    clear(): void {
        localStorage.removeItem(this.storageKey);
    }

    validate(): boolean {
        return this.form.reportValidity();
    }

    hasUnsynced(): boolean {
        const data = this.readData();
        if (!data) return false;
        const hasContent = this.collectFieldsHasContent(data.fields);
        return hasContent && !data.draftSynced;
    }

    markSynced(): void {
        const data = this.readData();
        if (!data) return;
        data.draftSynced = true;
        localStorage.setItem(this.storageKey, JSON.stringify(data));
    }

    getFieldMap(): Record<string, string> {
        return this.readData()?.fields ?? {};
    }

    async runBeforeSave(): Promise<boolean> {
        return this.beforeSave?.() ?? true;
    }

    private readData(): StepDraftData | null {
        const raw = localStorage.getItem(this.storageKey);
        if (!raw) return null;
        try {
            const parsed = JSON.parse(raw) as Partial<StepDraftData>;
            if (!parsed || typeof parsed !== 'object') return null;
            if (typeof parsed.fields !== 'object' || parsed.fields === null) return null;
            return {
                fields: parsed.fields as Record<string, string>,
                draftSynced: typeof parsed.draftSynced === 'boolean' ? parsed.draftSynced : false,
            };
        } catch {
            return null;
        }
    }

    private collectFields(): Record<string, string> {
        const fields: Record<string, string> = {};
        for (const element of this.form.elements) {
            const named = element as Element & { name?: string; value?: string; type?: string; checked?: boolean };
            if (!named.name) continue;
            if (named.name === '__RequestVerificationToken') continue;

            if (element instanceof HTMLFieldSetElement) continue;
            if (element instanceof HTMLButtonElement) continue;

            if (element instanceof HTMLInputElement) {
                if (element.type === 'file' || element.type === 'submit') continue;
                if (element.type === 'radio' || element.type === 'checkbox') {
                    fields[element.name] = element.checked ? (element.value || 'on') : '';
                    continue;
                }
            }

            if ('value' in element && typeof element.value === 'string') {
                fields[named.name] = element.value;
            }
        }
        return fields;
    }

    private collectFieldsHasContent(fields: Record<string, string>): boolean {
        for (const value of Object.values(fields)) {
            if (value.trim().length > 0) return true;
        }
        return false;
    }

    private populateFields(fields: Record<string, string>): void {
        for (const [name, value] of Object.entries(fields)) {
            const element = this.form.elements.namedItem(name);
            if (!element) continue;

            if (element instanceof HTMLInputElement && (element.type === 'radio' || element.type === 'checkbox')) {
                element.checked = value === element.value || value === 'on';
                continue;
            }

            if (element instanceof HTMLInputElement
                || element instanceof HTMLTextAreaElement
                || element instanceof HTMLSelectElement) {
                element.value = value;
            }
        }
    }
}
