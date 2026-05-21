import type { StepperHooks } from './universalStepper';
import { Stepper } from './universalStepper';

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

class ProjectDraftManager {
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
    private readonly step1NameWarning: HTMLElement | null;
    private readonly saveDraftBtn: HTMLButtonElement | null;

    private currentStep = 1;
    private previousDraftKey: string | null = null;
    private saveDraftFeedbackTimer: ReturnType<typeof setTimeout> | null = null;

    constructor(container: HTMLElement) {
        this.draftStoragePrefix = container.dataset.draftStoragePrefix || '';
        this.imageUploadUrl = container.dataset.imageUploadUrl || '';
        this.draftSaveUrl = container.dataset.draftSaveUrl || '';
        this.projectListUrl = container.dataset.projectListUrl || '/admin/projects';
        this.isCreatePage = container.dataset.isCreatePage === 'true';
        this.isCopyFlow = container.dataset.isCopyFlow === 'true';

        this.step1Form = container.querySelector('#create-project-step1-form');
        this.step1ImageFile = container.querySelector('#step1ImageFile');
        this.step1ImageUrl = container.querySelector('#step1ImageUrl');
        this.step1ImageUploadSignature = container.querySelector('#step1ImageUploadSignature');
        this.step1ImageUploadStatus = container.querySelector('#step1ImageUploadStatus');
        this.step1ImageUploadError = container.querySelector('#step1ImageUploadError');
        this.step1Slug = container.querySelector('input[name="CreateStep1ViewModel.Slug"]');
        this.step1Status = container.querySelector('select[name="CreateStep1ViewModel.Status"]');
        this.step1NameWarning = container.querySelector('#step1NameWarning');
        this.saveDraftBtn = container.querySelector('#saveDraftBtn');

        this.bindFormListeners();
        this.bindExitDraftModal();
        this.clearDraftIfNeeded();
        this.seedCopyDraft();
        this.hydrateDraft();
    }

    getStepperHooks(): StepperHooks {
        return {
            getInitialStep: () => this.readDraftSnapshot()?.currentStep ?? 1,
            onStepChange: (step: number) => {
                this.currentStep = step;
                this.persistDraft();
            },
            canAdvance: (currentStep: number) => {
                if (currentStep === 1) {
                    return this.validateAndUploadStep1();
                }
                return Promise.resolve(true);
            },
            onComplete: () => this.submitStep1Form(),
            onStepEnter: () => this.persistDraft(),
        };
    }

    restoreSavedStep(): number {
        return this.readDraftSnapshot()?.currentStep ?? 1;
    }

    private bindFormListeners() {
        this.step1Form?.addEventListener('input', () => this.persistDraft());
        this.step1Form?.addEventListener('change', () => this.persistDraft());
        this.saveDraftBtn?.addEventListener('click', async () => {
            this.persistDraft();
            await this.saveDraftToServer();
        });

        this.step1Form?.addEventListener('input', (event) => {
            const target = event.target as HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement | null;
            if (target?.name === 'CreateStep1ViewModel.Name') {
                this.setNameWarning('');
            }
        });

        if (this.step1Slug) {
            this.step1Slug.addEventListener('input', () => this.persistDraft());
        }
    }

    private async validateAndUploadStep1(): Promise<boolean> {
        if (!this.step1Form || !this.step1Form.reportValidity()) return false;
        return this.ensureStep1ImageUploaded();
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
                body: payload,
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
        this.clearLocalDraft();
        this.step1Form.requestSubmit();
    }

    private persistDraft() {
        if (!this.step1Form || !this.draftStoragePrefix) return;

        const key = this.getDraftStorageKey();
        if (this.previousDraftKey && this.previousDraftKey !== key) {
            localStorage.removeItem(this.previousDraftKey);
        }

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
            draftSynced: false,
        };

        localStorage.setItem(key, JSON.stringify(snapshot));
        localStorage.setItem(`${this.draftStoragePrefix}:latest`, key);
        this.previousDraftKey = key;
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

        if (saved.currentStep >= 1) {
            this.currentStep = saved.currentStep;
        }
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
                    draftSynced: typeof parsed.draftSynced === 'boolean' ? parsed.draftSynced : false,
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
        const existing = this.readDraftSnapshot();
        if (existing) return;
        this.clearLocalDraft();
    }

    private async saveDraftToServer() {
        if (!this.step1Form || !this.draftSaveUrl) return;

        const antiForgeryToken = this.step1Form.querySelector<HTMLInputElement>('input[name="__RequestVerificationToken"]')?.value;
        if (!antiForgeryToken) return;

        if (!this.step1Form.reportValidity()) return;

        this.setSaveDraftFeedback('Saving...', false);

        const payload = new FormData(this.step1Form);
        payload.set('__RequestVerificationToken', antiForgeryToken);

        const previousStatus = this.step1Status?.value ?? '';
        if (this.step1Status) {
            this.step1Status.value = 'Draft';
            payload.set(this.step1Status.name, 'Draft');
        }

        try {
            const response = await fetch(this.draftSaveUrl, {
                method: 'POST',
                credentials: 'same-origin',
                body: payload,
            });

            if (this.step1Status) {
                this.step1Status.value = previousStatus;
            }

            if (!response.ok) {
                await this.handleDraftSaveError(response);
                this.setSaveDraftFeedback('Save failed', true);
                return;
            }

            const data = (await response.json()) as { slug?: string };
            if (data?.slug && this.step1Slug) {
                this.step1Slug.value = data.slug;
            }

            this.setNameWarning('');
            this.clearLocalDraft();
            this.setSaveDraftFeedback('Draft saved!', false);
        } catch {
            this.setSaveDraftFeedback('Save failed', true);
        }
    }

    private setSaveDraftFeedback(message: string, isError: boolean) {
        if (!this.saveDraftBtn) return;
        if (this.saveDraftFeedbackTimer) {
            clearTimeout(this.saveDraftFeedbackTimer);
            this.saveDraftFeedbackTimer = null;
        }
        this.saveDraftBtn.textContent = message;
        this.saveDraftBtn.classList.toggle('text-error', isError);
        this.saveDraftBtn.classList.toggle('border-error', isError);
        this.saveDraftBtn.classList.toggle('text-success', !isError);
        this.saveDraftBtn.classList.toggle('border-success', !isError);
        this.saveDraftBtn.disabled = isError ? false : true;

        this.saveDraftFeedbackTimer = setTimeout(() => {
            if (!this.saveDraftBtn) return;
            this.saveDraftBtn.textContent = 'Save as draft';
            this.saveDraftBtn.classList.remove('text-error', 'border-error', 'text-success', 'border-success');
            this.saveDraftBtn.disabled = false;
        }, 2500);
    }

    private async handleDraftSaveError(response: Response) {
        if (response.status === 409) {
            this.setNameWarning('Project name already exists. Draft can save, but creation blocked until name unique.');
            return;
        }

        try {
            const body = (await response.json()) as { error?: string };
            if (body?.error) {
                this.setNameWarning(body.error);
            }
        } catch {
            return;
        }
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

function initProjectStepper() {
    const container = document.getElementById('dynamic-stepper');
    const hasProjectForm = document.getElementById('create-project-step1-form');
    if (!container || !hasProjectForm) return;

    const draftManager = new ProjectDraftManager(container);
    const hooks = draftManager.getStepperHooks();
    new Stepper('dynamic-stepper', hooks);
}

document.addEventListener('DOMContentLoaded', initProjectStepper);
