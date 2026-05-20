interface StepDraftSnapshot {
    currentStep: number;
    name: string;
    description: string;
    interactionForm: string;
    startDate: string;
    endDate: string;
    imageUrl: string;
    imageUploadSignature: string;
    nudgingStrength: string;
    status: string;
    slug: string;
    draftSynced: boolean;
}

interface ImageUploadResponse {
    imageUrl?: unknown;
    error?: unknown;
}

class CreateProjectStepper {
    private currentStep = 1;
    private isTransitioning = false;
    private allowNext = false;
    private isRestoring = false;

    private readonly container: HTMLElement;
    private readonly nextBtn: HTMLButtonElement;
    private readonly prevBtn: HTMLButtonElement;
    private readonly saveDraftBtn: HTMLButtonElement | null;
    private readonly totalSteps: number;
    private readonly draftStoragePrefix: string;
    private readonly imageUploadUrl: string;
    private readonly draftSaveUrl: string;
    private readonly projectListUrl: string;
    private readonly isCreatePage: boolean;
    private readonly isCopyFlow: boolean;

    private readonly step1Form: HTMLFormElement | null;
    private readonly step1ImageFile: HTMLInputElement | null;
    private readonly step1ImageUrl: HTMLInputElement | null;
    private readonly step1ImageUploadSignature: HTMLInputElement | null;
    private readonly step1ImageUploadStatus: HTMLElement | null;
    private readonly step1ImageUploadError: HTMLElement | null;
    private readonly step1Slug: HTMLInputElement | null;
    private readonly step1Status: HTMLSelectElement | null;
    private readonly step1NudgingStrength: HTMLInputElement | null;
    private readonly step1NameWarning: HTMLElement | null;

    constructor(containerId: string) {
        this.container = document.getElementById(containerId)!;
        this.totalSteps = parseInt(this.container.dataset.totalSteps || '1', 10);
        this.draftStoragePrefix = this.container.dataset.draftStoragePrefix || '';
        this.imageUploadUrl = this.container.dataset.imageUploadUrl || '';
        this.draftSaveUrl = this.container.dataset.draftSaveUrl || '';
        this.projectListUrl = this.container.dataset.projectListUrl || '/admin/projects';
        this.isCreatePage = this.container.dataset.isCreatePage === 'true';
        this.isCopyFlow = this.container.dataset.isCopyFlow === 'true';

        this.nextBtn = this.container.querySelector('#nextBtn')!;
        this.prevBtn = this.container.querySelector('#prevBtn')!;
        this.saveDraftBtn = this.container.querySelector('#saveDraftBtn');

        this.step1Form = this.container.querySelector('#create-project-step1-form');
        this.step1ImageFile = this.container.querySelector('#step1ImageFile');
        this.step1ImageUrl = this.container.querySelector('#step1ImageUrl');
        this.step1ImageUploadSignature = this.container.querySelector('#step1ImageUploadSignature');
        this.step1ImageUploadStatus = this.container.querySelector('#step1ImageUploadStatus');
        this.step1ImageUploadError = this.container.querySelector('#step1ImageUploadError');
        this.step1Slug = this.container.querySelector('input[name="CreateStep1ViewModel.Slug"]');
        this.step1Status = this.container.querySelector('select[name="CreateStep1ViewModel.Status"]');
        this.step1NudgingStrength = this.container.querySelector('input[name="CreateStep1ViewModel.NudgingStrength"]');
        this.step1NameWarning = this.container.querySelector('#step1NameWarning');

        this.init();
    }

    private init() {
        this.nextBtn.addEventListener('click', (event) => {
            void this.handleNextClick(event);
        }, { capture: true });

        this.prevBtn.addEventListener('click', () => {
            this.persistDraft();
        });

        this.saveDraftBtn?.addEventListener('click', async () => {
            this.persistDraft();
            await this.saveDraftToServer();
        });

        this.step1Form?.addEventListener('input', () => this.persistDraft());
        this.step1Form?.addEventListener('change', () => this.persistDraft());

        this.step1Form?.addEventListener('input', (event) => {
            const target = event.target as HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement | null;
            if (target?.name === 'CreateStep1ViewModel.Name') {
                this.setNameWarning('');
            }
        });

        this.bindExitDraftModal();

        this.container.addEventListener('stepper:step-enter', (event) => {
            const detail = (event as CustomEvent<{ step?: number }>).detail;
            if (typeof detail?.step === 'number') {
                this.currentStep = detail.step;
            }
            this.persistDraft();
        });

        this.clearDraftIfNeeded();
        this.hydrateDraft();
        this.seedCopyDraft();
        this.restoreSavedStep();
    }

    private async handleNextClick(event: Event) {
        if (this.allowNext) {
            this.allowNext = false;
            return;
        }

        if (this.isRestoring) {
            return;
        }

        if (this.isTransitioning) {
            event.preventDefault();
            event.stopImmediatePropagation();
            return;
        }

        if (this.currentStep === this.totalSteps) {
            event.preventDefault();
            event.stopImmediatePropagation();
            this.submitStep1Form();
            return;
        }

        if (this.currentStep !== 1) {
            return;
        }

        if (!this.step1Form || !this.step1Form.reportValidity()) {
            event.preventDefault();
            event.stopImmediatePropagation();
            return;
        }

        event.preventDefault();
        event.stopImmediatePropagation();

        this.isTransitioning = true;
        const canProceed = await this.ensureStep1ImageUploaded();
        this.isTransitioning = false;

        if (!canProceed) return;

        this.allowNext = true;
        this.nextBtn.click();
    }

    private async ensureStep1ImageUploaded(): Promise<boolean> {
        const selectedFile = this.step1ImageFile?.files?.[0];
        if (!selectedFile) return true;

        const currentSignature = this.createFileSignature(selectedFile);
        const existingSignature = this.step1ImageUploadSignature?.value ?? '';
        const existingImageUrl = this.step1ImageUrl?.value.trim() ?? '';
        if (existingSignature === currentSignature && existingImageUrl.length > 0) {
            return true;
        }

        if (!this.imageUploadUrl || !this.step1Form) {
            this.setUploadError('Image upload endpoint missing.');
            return false;
        }

        const antiForgeryToken = this.step1Form.querySelector<HTMLInputElement>('input[name="__RequestVerificationToken"]')?.value;
        if (!antiForgeryToken) {
            this.setUploadError('Security token missing. Refresh page and retry.');
            return false;
        }

        this.setUploadError('');
        this.setUploadStatus('Uploading image...');

        const payload = new FormData();
        payload.append('imageFile', selectedFile);
        payload.append('__RequestVerificationToken', antiForgeryToken);

        try {
            const response = await fetch(this.imageUploadUrl, {
                method: 'POST',
                credentials: 'same-origin',
                body: payload
            });

            const responseBody = await this.parseUploadResponse(response);
            if (!response.ok) {
                this.setUploadError(responseBody.errorMessage);
                this.setUploadStatus('');
                return false;
            }

            if (!responseBody.imageUrl) {
                this.setUploadError('Upload succeeded but no image URL returned.');
                this.setUploadStatus('');
                return false;
            }

            if (this.step1ImageUrl) this.step1ImageUrl.value = responseBody.imageUrl;
            if (this.step1ImageUploadSignature) this.step1ImageUploadSignature.value = currentSignature;

            this.setUploadStatus('Image uploaded.');
            return true;
        } catch {
            this.setUploadError('Image upload failed. Check connection and retry.');
            this.setUploadStatus('');
            return false;
        }
    }

    private async parseUploadResponse(response: Response): Promise<{ imageUrl: string; errorMessage: string }> {
        let parsed: ImageUploadResponse | null = null;
        try {
            parsed = (await response.json()) as ImageUploadResponse;
        } catch {
            parsed = null;
        }

        const imageUrl = parsed && typeof parsed.imageUrl === 'string' ? parsed.imageUrl : '';
        const errorMessage = parsed && typeof parsed.error === 'string'
            ? parsed.error
            : `Image upload failed (${response.status}).`;

        return { imageUrl, errorMessage };
    }

    private submitStep1Form() {
        if (!this.step1Form) return;
        if (!this.step1Form.reportValidity()) return;
        this.persistDraft();
        this.step1Form.requestSubmit();
    }

    private persistDraft() {
        if (!this.step1Form || !this.draftStoragePrefix) return;

        const key = this.getDraftStorageKey();
        const snapshot: StepDraftSnapshot = {
            currentStep: this.currentStep,
            name: this.getFieldValue('CreateStep1ViewModel.Name'),
            description: this.getFieldValue('CreateStep1ViewModel.Description'),
            interactionForm: this.getFieldValue('CreateStep1ViewModel.InteractionForm'),
            startDate: this.getFieldValue('CreateStep1ViewModel.StartDate'),
            endDate: this.getFieldValue('CreateStep1ViewModel.EndDate'),
            imageUrl: this.step1ImageUrl?.value ?? '',
            imageUploadSignature: this.step1ImageUploadSignature?.value ?? '',
            nudgingStrength: this.getFieldValue('CreateStep1ViewModel.NudgingStrength'),
            status: this.getFieldValue('CreateStep1ViewModel.Status'),
            slug: this.step1Slug?.value ?? '',
            draftSynced: false
        };

        localStorage.setItem(key, JSON.stringify(snapshot));
        localStorage.setItem(`${this.draftStoragePrefix}:latest`, key);
    }

    private hydrateDraft() {
        if (!this.step1Form || !this.draftStoragePrefix) return;

        const saved = this.readDraftSnapshot();
        if (!saved) return;

        this.setFieldValue('CreateStep1ViewModel.Name', saved.name);
        this.setFieldValue('CreateStep1ViewModel.Description', saved.description);
        this.setFieldValue('CreateStep1ViewModel.InteractionForm', saved.interactionForm);
        this.setFieldValue('CreateStep1ViewModel.StartDate', saved.startDate);
        this.setFieldValue('CreateStep1ViewModel.EndDate', saved.endDate);
        this.setFieldValue('CreateStep1ViewModel.NudgingStrength', saved.nudgingStrength);
        this.setFieldValue('CreateStep1ViewModel.Status', saved.status);
        if (this.step1Slug) this.step1Slug.value = saved.slug;

        if (this.step1ImageUrl) this.step1ImageUrl.value = saved.imageUrl;
        if (this.step1ImageUploadSignature) this.step1ImageUploadSignature.value = saved.imageUploadSignature;
        if (saved.imageUrl.trim().length > 0) this.setUploadStatus('Loaded saved draft image.');

        if (saved.currentStep >= 1 && saved.currentStep <= this.totalSteps) {
            this.currentStep = saved.currentStep;
        }
    }

    private restoreSavedStep() {
        if (this.currentStep <= 1) return;
        this.isRestoring = true;
        for (let step = 1; step < this.currentStep; step += 1) {
            this.allowNext = true;
            this.nextBtn.click();
        }
        this.isRestoring = false;
    }

    private readDraftSnapshot(): StepDraftSnapshot | null {
        const latestKey = localStorage.getItem(`${this.draftStoragePrefix}:latest`);
        const keys = this.getDraftCandidateKeys(latestKey);

        for (const key of keys) {
            const raw = localStorage.getItem(key);
            if (!raw) continue;

            try {
                const parsed = JSON.parse(raw) as Partial<StepDraftSnapshot>;
                if (typeof parsed !== 'object' || parsed === null) continue;

                return {
                    currentStep: typeof parsed.currentStep === 'number' ? parsed.currentStep : 1,
                    name: typeof parsed.name === 'string' ? parsed.name : '',
                    description: typeof parsed.description === 'string' ? parsed.description : '',
                    interactionForm: typeof parsed.interactionForm === 'string' ? parsed.interactionForm : '',
                    startDate: typeof parsed.startDate === 'string' ? parsed.startDate : '',
                    endDate: typeof parsed.endDate === 'string' ? parsed.endDate : '',
                    imageUrl: typeof parsed.imageUrl === 'string' ? parsed.imageUrl : '',
                    imageUploadSignature: typeof parsed.imageUploadSignature === 'string' ? parsed.imageUploadSignature : '',
                    nudgingStrength: typeof parsed.nudgingStrength === 'string' ? parsed.nudgingStrength : '',
                    status: typeof parsed.status === 'string' ? parsed.status : '',
                    slug: typeof parsed.slug === 'string' ? parsed.slug : '',
                    draftSynced: typeof parsed.draftSynced === 'boolean' ? parsed.draftSynced : false
                };
            } catch {
                continue;
            }
        }

        return null;
    }

    private getDraftCandidateKeys(latestKey: string | null): string[] {
        const activeKey = this.getDraftStorageKey();
        const draftKey = `${this.draftStoragePrefix}:draft`;
        const keys = [activeKey, draftKey];
        if (latestKey) keys.unshift(latestKey);
        return [...new Set(keys)];
    }

    private getDraftStorageKey(): string {
        const slugValue = this.step1Slug?.value.trim() ?? '';
        if (slugValue.length > 0) {
            return `${this.draftStoragePrefix}:${slugValue}`;
        }

        const name = this.getFieldValue('CreateStep1ViewModel.Name');
        const slug = this.slugify(name);
        return `${this.draftStoragePrefix}:${slug || 'draft'}`;
    }

    private async saveDraftToServer() {
        if (!this.step1Form || !this.draftSaveUrl) return;

        const antiForgeryToken = this.step1Form.querySelector<HTMLInputElement>('input[name="__RequestVerificationToken"]')?.value;
        if (!antiForgeryToken) return;

        if (!this.step1Form.reportValidity()) return;

        const payload = new FormData(this.step1Form);
        payload.set('__RequestVerificationToken', antiForgeryToken);

        const previousStatus = this.step1Status?.value ?? '';
        if (this.step1Status) {
            this.step1Status.value = 'Draft';
            payload.set(this.step1Status.name, 'Draft');
        }

        const response = await fetch(this.draftSaveUrl, {
            method: 'POST',
            credentials: 'same-origin',
            body: payload
        });

        if (this.step1Status) {
            this.step1Status.value = previousStatus;
        }

        if (!response.ok) {
            await this.handleDraftSaveError(response);
            return;
        }

        const data = await response.json() as { slug?: string };
        if (data?.slug && this.step1Slug) {
            this.step1Slug.value = data.slug;
        }

        this.setNameWarning('');

        this.persistDraftWithSynced(true);
    }

    private persistDraftWithSynced(synced: boolean) {
        if (!this.step1Form || !this.draftStoragePrefix) return;

        const key = this.getDraftStorageKey();
        const snapshot = this.readDraftSnapshot() ?? {
            currentStep: this.currentStep,
            name: this.getFieldValue('CreateStep1ViewModel.Name'),
            description: this.getFieldValue('CreateStep1ViewModel.Description'),
            interactionForm: this.getFieldValue('CreateStep1ViewModel.InteractionForm'),
            startDate: this.getFieldValue('CreateStep1ViewModel.StartDate'),
            endDate: this.getFieldValue('CreateStep1ViewModel.EndDate'),
            imageUrl: this.step1ImageUrl?.value ?? '',
            imageUploadSignature: this.step1ImageUploadSignature?.value ?? '',
            nudgingStrength: this.getFieldValue('CreateStep1ViewModel.NudgingStrength'),
            status: this.getFieldValue('CreateStep1ViewModel.Status'),
            slug: this.step1Slug?.value ?? '',
            draftSynced: false
        };

        snapshot.draftSynced = synced;
        localStorage.setItem(key, JSON.stringify(snapshot));
        localStorage.setItem(`${this.draftStoragePrefix}:latest`, key);
    }

    private async handleDraftSaveError(response: Response) {
        if (response.status === 409) {
            this.setNameWarning('Project name already exists. Draft can save, but creation blocked until name unique.');
            return;
        }

        try {
            const body = await response.json() as { error?: string };
            if (body?.error) {
                this.setNameWarning(body.error);
            }
        } catch {
            return;
        }
    }

    private bindExitDraftModal() {
        const modal = document.getElementById('draftExitModal');
        if (!modal) return;

        const openModal = () => {
            modal.classList.remove('hidden');
            modal.classList.add('flex');
        };

        const closeModal = () => {
            modal.classList.add('hidden');
            modal.classList.remove('flex');
        };

        const saveBtn = document.getElementById('draftExitSave');
        const clearBtn = document.getElementById('draftExitClear');
        const cancelBtn = document.getElementById('draftExitCancel');

        saveBtn?.addEventListener('click', async () => {
            await this.saveDraftToServer();
            window.location.href = this.projectListUrl;
        });

        clearBtn?.addEventListener('click', () => {
            this.clearLocalDraft();
            window.location.href = this.projectListUrl;
        });

        cancelBtn?.addEventListener('click', () => closeModal());
        modal.addEventListener('click', (event) => {
            if (event.target === modal) closeModal();
        });

        document.addEventListener('click', (event) => {
            const target = event.target as HTMLElement | null;
            if (!target) return;
            if (target.closest('a[href]')) {
                if (!this.shouldBlockExit()) return;
                event.preventDefault();
                openModal();
            }
        }, { capture: true });
    }

    private shouldBlockExit(): boolean {
        if (!this.step1Form || !this.draftStoragePrefix) return false;
        const snapshot = this.readDraftSnapshot();
        if (!snapshot) return false;
        const hasContent = snapshot.name.trim().length > 0 || snapshot.description.trim().length > 0;
        return hasContent && !snapshot.draftSynced;
    }

    private clearLocalDraft() {
        if (!this.draftStoragePrefix) return;
        const latestKey = localStorage.getItem(`${this.draftStoragePrefix}:latest`);
        const keys = this.getDraftCandidateKeys(latestKey);
        for (const key of keys) {
            localStorage.removeItem(key);
        }
        localStorage.removeItem(`${this.draftStoragePrefix}:latest`);
    }

    private seedCopyDraft() {
        const payloadEl = document.getElementById('copyDraftPayload');
        if (!payloadEl || !this.draftStoragePrefix) return;
        const payload = payloadEl.getAttribute('data-payload');
        if (!payload) return;
        try {
            const parsed = JSON.parse(payload) as StepDraftSnapshot;
            const key = `${this.draftStoragePrefix}:draft`;
            localStorage.setItem(key, JSON.stringify(parsed));
            localStorage.setItem(`${this.draftStoragePrefix}:latest`, key);
            if (this.step1Slug) this.step1Slug.value = '';
        } catch {
            return;
        }
    }

    private clearDraftIfNeeded() {
        if (!this.isCreatePage || this.isCopyFlow) return;
        this.clearLocalDraft();
    }

    private slugify(value: string): string {
        return value
            .trim()
            .toLowerCase()
            .replace(/\s+/g, '-')
            .replace(/[^a-z0-9_-]/g, '');
    }

    private getFieldValue(name: string): string {
        const field = this.step1Form?.elements.namedItem(name);
        if (field instanceof HTMLInputElement || field instanceof HTMLTextAreaElement || field instanceof HTMLSelectElement) {
            return field.value ?? '';
        }
        return '';
    }

    private setFieldValue(name: string, value: string) {
        const field = this.step1Form?.elements.namedItem(name);
        if (field instanceof HTMLInputElement || field instanceof HTMLTextAreaElement || field instanceof HTMLSelectElement) {
            field.value = value;
        }
    }

    private createFileSignature(file: File): string {
        return `${file.name}:${file.size}:${file.lastModified}`;
    }

    private setUploadError(message: string) {
        if (this.step1ImageUploadError) this.step1ImageUploadError.textContent = message;
    }

    private setUploadStatus(message: string) {
        if (this.step1ImageUploadStatus) this.step1ImageUploadStatus.textContent = message;
    }

    private setNameWarning(message: string) {
        if (this.step1NameWarning) this.step1NameWarning.textContent = message;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('dynamic-stepper') && document.getElementById('create-project-step1-form')) {
        new CreateProjectStepper('dynamic-stepper');
    }
});
