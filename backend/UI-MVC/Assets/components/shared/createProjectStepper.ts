import type { StepperHooks } from './universalStepper';
import { Stepper } from './universalStepper';
import { StepDraftManager } from './stepDraftManager';

interface MetaData {
    currentStep: number;
    slug: string;
}

interface ImageUploadResponse {
    imageUrl?: unknown;
    error?: unknown;
}

const STEP1_FIELD_MAP: Record<string, string> = {
    name: 'CreateStep1ViewModel.Name',
    description: 'CreateStep1ViewModel.Description',
    interactionForm: 'CreateStep1ViewModel.InteractionForm',
    startDate: 'CreateStep1ViewModel.StartDate',
    endDate: 'CreateStep1ViewModel.EndDate',
    imageUrl: 'CreateStep1ViewModel.ImageUrl',
    imageUploadSignature: 'step1ImageUploadSignature',
    nudgingStrength: 'CreateStep1ViewModel.NudgingStrength',
    status: 'CreateStep1ViewModel.Status',
};

class ProjectDraftManager {
    private readonly draftStoragePrefix: string;
    private readonly imageUploadUrl: string;
    private readonly draftSaveUrl: string;
    private readonly projectListUrl: string;
    private readonly isCreatePage: boolean;
    private readonly isCopyFlow: boolean;

    private readonly stepManagers = new Map<number, StepDraftManager>();

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
    private saveDraftFeedbackTimer: ReturnType<typeof setTimeout> | null = null;

    constructor(container: HTMLElement) {
        this.draftStoragePrefix = container.dataset.draftStoragePrefix || '';
        this.imageUploadUrl = container.dataset.imageUploadUrl || '';
        this.draftSaveUrl = container.dataset.draftSaveUrl || '';
        this.projectListUrl = container.dataset.projectListUrl || '/admin/projects';
        this.isCreatePage = container.dataset.isCreatePage === 'true';
        this.isCopyFlow = container.dataset.isCopyFlow === 'true';

        this.step1ImageFile = container.querySelector('#step1ImageFile');
        this.step1ImageUrl = container.querySelector('#step1ImageUrl');
        this.step1ImageUploadSignature = container.querySelector('#step1ImageUploadSignature');
        this.step1ImageUploadStatus = container.querySelector('#step1ImageUploadStatus');
        this.step1ImageUploadError = container.querySelector('#step1ImageUploadError');
        this.step1Slug = container.querySelector('input[name="CreateStep1ViewModel.Slug"]');
        this.step1Status = container.querySelector('select[name="CreateStep1ViewModel.Status"]');
        this.step1NameWarning = container.querySelector('#step1NameWarning');
        this.saveDraftBtn = container.querySelector('#saveDraftBtn');

        this.discoverForms(container);
        this.migrateOldDraft();
        this.bindFormListeners();
        this.bindSaveDraftButton();
        this.bindNameWarning();
        this.clearDraftIfNeeded();
        this.seedCopyDraft();
        this.hydrateActiveStep();
        this.bindExitDraftModal();
    }

    getStepperHooks(): StepperHooks {
        return {
            getInitialStep: () => this.readMeta()?.currentStep ?? 1,
            onStepChange: (step: number) => {
                this.currentStep = step;
                this.saveMeta();
            },
            canAdvance: (currentStep: number) => this.handleCanAdvance(currentStep),
            onComplete: () => this.handleComplete(),
            onStepEnter: (step: number) => {
                this.currentStep = step;
                this.stepManagers.get(step)?.hydrate();
                this.saveMeta();
            },
        };
    }

    private discoverForms(container: HTMLElement): void {
        const forms = container.querySelectorAll<HTMLFormElement>('form[id^="create-project-step"]');
        for (const form of forms) {
            const match = form.id.match(/^create-project-step(\d+)-form$/);
            if (!match) continue;
            const stepNum = parseInt(match[1], 10);

            const beforeSave = stepNum === 1
                ? () => this.ensureStep1ImageUploaded()
                : undefined;

            const manager = new StepDraftManager(stepNum, form, this.draftStoragePrefix, beforeSave);
            this.stepManagers.set(stepNum, manager);
        }
    }

    private bindFormListeners(): void {
        for (const [, manager] of this.stepManagers) {
            manager.form.addEventListener('input', () => {
                manager.persist();
                this.saveMeta();
            });
            manager.form.addEventListener('change', () => {
                manager.persist();
                this.saveMeta();
            });
        }
    }

    private bindSaveDraftButton(): void {
        this.saveDraftBtn?.addEventListener('click', async () => {
            this.persistCurrentStep();
            await this.saveDraftToServer();
        });
    }

    private bindNameWarning(): void {
        const step1Form = this.stepManagers.get(1)?.form;
        step1Form?.addEventListener('input', (event) => {
            const target = event.target as HTMLElement | null;
            if (target instanceof HTMLInputElement && target.name === 'CreateStep1ViewModel.Name') {
                this.setNameWarning('');
            }
        });
    }

    private async handleCanAdvance(currentStep: number): Promise<boolean> {
        const manager = this.stepManagers.get(currentStep);
        if (!manager) return true;

        if (!manager.validate()) return false;

        return manager.runBeforeSave();
    }

    private async handleComplete(): Promise<void> {
        const step1Manager = this.stepManagers.get(1);
        if (!step1Manager) return;

        if (!step1Manager.validate()) return;

        const imageOk = await this.ensureStep1ImageUploaded();
        if (!imageOk) return;

        this.clearAllLocalDrafts();
        step1Manager.form.requestSubmit();
    }

    private persistCurrentStep(): void {
        this.stepManagers.get(this.currentStep)?.persist();
        this.saveMeta();
    }

    private hydrateActiveStep(): void {
        const meta = this.readMeta();
        if (!meta) return;

        this.currentStep = meta.currentStep;
        this.stepManagers.get(meta.currentStep)?.hydrate();
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

        if (!this.imageUploadUrl) {
            this.setUploadError('Image upload endpoint missing.');
            return false;
        }

        const step1Form = this.stepManagers.get(1)?.form;
        const antiForgeryToken = step1Form
            ?.querySelector<HTMLInputElement>('input[name="__RequestVerificationToken"]')?.value;
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

    private async saveDraftToServer(): Promise<void> {
        const step1Manager = this.stepManagers.get(1);
        if (!step1Manager || !this.draftSaveUrl) return;

        const antiForgeryToken = step1Manager.form
            .querySelector<HTMLInputElement>('input[name="__RequestVerificationToken"]')?.value;
        if (!antiForgeryToken) return;

        if (!step1Manager.form.reportValidity()) return;

        this.setSaveDraftFeedback('Saving...', false);

        const payload = new FormData();
        payload.set('__RequestVerificationToken', antiForgeryToken);

        for (const [, manager] of this.stepManagers) {
            const fields = manager.getFieldMap();
            for (const [name, value] of Object.entries(fields)) {
                payload.set(name, value);
            }
        }

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
                this.saveMeta();
            }

            for (const [, manager] of this.stepManagers) {
                manager.markSynced();
            }

            this.setNameWarning('');
            this.setSaveDraftFeedback('Draft saved!', false);
        } catch {
            this.setSaveDraftFeedback('Save failed', true);
        }
    }

    private bindExitDraftModal(): void {
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
            this.clearAllLocalDrafts();
            window.location.href = this.projectListUrl;
        });

        cancelBtn?.addEventListener('click', () => closeModal());
        modal.addEventListener('click', (event) => {
            if (event.target === modal) closeModal();
        });

        document.addEventListener('click', (event) => {
            const target = event.target as HTMLElement | null;
            if (!target) return;
            if (target.closest('#dynamic-stepper')) return;
            if (target.closest('a[href]')) {
                if (!this.shouldBlockExit()) return;
                event.preventDefault();
                openModal();
            }
        }, { capture: true });
    }

    private shouldBlockExit(): boolean {
        if (!this.draftStoragePrefix) return false;
        for (const [, manager] of this.stepManagers) {
            if (manager.hasUnsynced()) return true;
        }
        return false;
    }

    private clearAllLocalDrafts(): void {
        if (!this.draftStoragePrefix) return;
        for (const [, manager] of this.stepManagers) {
            manager.clear();
        }
        localStorage.removeItem(`${this.draftStoragePrefix}:meta`);
    }

    private seedCopyDraft(): void {
        const payloadEl = document.getElementById('copyDraftPayload');
        if (!payloadEl || !this.draftStoragePrefix) return;
        const payload = payloadEl.getAttribute('data-payload');
        if (!payload) return;

        try {
            const parsed = JSON.parse(payload) as Record<string, unknown>;
            const step1Manager = this.stepManagers.get(1);
            if (!step1Manager) return;

            const fields: Record<string, string> = {};
            for (const [snapshotKey, formName] of Object.entries(STEP1_FIELD_MAP)) {
                const value = parsed[snapshotKey];
                if (typeof value === 'string') {
                    fields[formName] = value;
                } else if (value !== undefined && value !== null) {
                    fields[formName] = String(value);
                }
            }

            localStorage.setItem(step1Manager.storageKey, JSON.stringify({ fields, draftSynced: false }));
            this.saveMeta({ currentStep: 1, slug: '' });

            if (this.step1Slug) this.step1Slug.value = '';
        } catch {
            return;
        }
    }

    private clearDraftIfNeeded(): void {
        if (!this.isCreatePage || this.isCopyFlow) return;
        const meta = this.readMeta();
        if (meta) return;
        this.clearAllLocalDrafts();
    }

    private migrateOldDraft(): void {
        if (!this.draftStoragePrefix) return;

        const latestKey = localStorage.getItem(`${this.draftStoragePrefix}:latest`);
        const oldKeys = [
            latestKey,
            `${this.draftStoragePrefix}:draft`,
        ].filter((k): k is string => k !== null);

        for (const oldKey of oldKeys) {
            const raw = localStorage.getItem(oldKey);
            if (!raw) continue;

            try {
                const parsed = JSON.parse(raw) as Record<string, unknown>;
                if (!parsed || typeof parsed !== 'object') continue;

                const fields: Record<string, string> = {};
                for (const [snapshotKey, formName] of Object.entries(STEP1_FIELD_MAP)) {
                    const value = parsed[snapshotKey];
                    if (typeof value === 'string') {
                        fields[formName] = value;
                    } else if (value !== undefined && value !== null) {
                        fields[formName] = String(value);
                    }
                }

                const step1Manager = this.stepManagers.get(1);
                if (!step1Manager) break;

                localStorage.setItem(
                    step1Manager.storageKey,
                    JSON.stringify({ fields, draftSynced: false }),
                );

                const currentStep = typeof parsed.currentStep === 'number' ? parsed.currentStep : 1;
                const slug = typeof parsed.slug === 'string' ? parsed.slug : '';
                this.saveMeta({ currentStep, slug });
                break;
            } catch {
                continue;
            }
        }

        localStorage.removeItem(`${this.draftStoragePrefix}:draft`);
        localStorage.removeItem(`${this.draftStoragePrefix}:latest`);
    }

    private readMeta(): MetaData | null {
        const raw = localStorage.getItem(`${this.draftStoragePrefix}:meta`);
        if (!raw) return null;
        try {
            const parsed = JSON.parse(raw) as Partial<MetaData>;
            if (!parsed || typeof parsed !== 'object') return null;
            return {
                currentStep: typeof parsed.currentStep === 'number' ? parsed.currentStep : 1,
                slug: typeof parsed.slug === 'string' ? parsed.slug : '',
            };
        } catch {
            return null;
        }
    }

    private saveMeta(overrides?: Partial<MetaData>): void {
        const current = this.readMeta() ?? { currentStep: 1, slug: '' };
        const meta: MetaData = {
            currentStep: overrides?.currentStep ?? this.currentStep,
            slug: overrides?.slug ?? this.step1Slug?.value ?? current.slug,
        };
        localStorage.setItem(`${this.draftStoragePrefix}:meta`, JSON.stringify(meta));
    }

    private setSaveDraftFeedback(message: string, isError: boolean): void {
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

    private async handleDraftSaveError(response: Response): Promise<void> {
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

    private createFileSignature(file: File): string {
        return `${file.name}:${file.size}:${file.lastModified}`;
    }

    private setUploadError(message: string): void {
        if (this.step1ImageUploadError) this.step1ImageUploadError.textContent = message;
    }

    private setUploadStatus(message: string): void {
        if (this.step1ImageUploadStatus) this.step1ImageUploadStatus.textContent = message;
    }

    private setNameWarning(message: string): void {
        if (this.step1NameWarning) this.step1NameWarning.textContent = message;
    }
}

function initProjectStepper(): void {
    const container = document.getElementById('dynamic-stepper');
    const hasProjectForm = container?.querySelector('form[id^="create-project-step"]') !== null;
    if (!container || !hasProjectForm) return;

    const draftManager = new ProjectDraftManager(container);
    const hooks = draftManager.getStepperHooks();
    new Stepper('dynamic-stepper', hooks);
}

document.addEventListener('DOMContentLoaded', initProjectStepper);
