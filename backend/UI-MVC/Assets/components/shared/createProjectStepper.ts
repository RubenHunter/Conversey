import type { StepperHooks } from './universalStepper';
import { Stepper } from './universalStepper';
import { StepDraftManager } from './stepDraftManager';

interface MetaData {
    currentStep: number;
    slug: string;
}

interface PromptOverride {
    promptName: string;
    userPromptTemplate: string;
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
    themePrimary: 'CreateStep1ViewModel.ThemePrimary',
    themeSecondary: 'CreateStep1ViewModel.ThemeSecondary',
    themeAccent: 'CreateStep1ViewModel.ThemeAccent',
    themePreset: 'CreateStep1ViewModel.ThemePreset',
    themeFont: 'CreateStep1ViewModel.ThemeFont',
    minAge: 'CreateStep1ViewModel.MinAge',
    maxAge: 'CreateStep1ViewModel.MaxAge',
};

const STEP4_FIELD_MAP: Record<string, string> = {
    nudgingStrength: 'CreateStep1ViewModel.NudgingStrength',
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
    private readonly step1NameWarning: HTMLElement | null;
    private readonly saveDraftBtn: HTMLButtonElement | null;

    private readonly step1ThemePrimary: HTMLInputElement | null;
    private readonly step1ThemeSecondary: HTMLInputElement | null;
    private readonly step1ThemeAccent: HTMLInputElement | null;
    private readonly step1ThemePreset: HTMLInputElement | null;
    private readonly step1ThemeFont: HTMLInputElement | null;
    private readonly step1MinAge: HTMLInputElement | null;
    private readonly step1MaxAge: HTMLInputElement | null;
    private readonly step1MinAgeDisplay: HTMLElement | null;
    private readonly step1MaxAgeDisplay: HTMLElement | null;

    private readonly step4NudgingStrength: HTMLInputElement | null;
    private readonly step4NudgingDisplay: HTMLElement | null;
    private readonly step4PromptsJson: HTMLInputElement | null;
    private readonly step4Overrides: Map<string, string> = new Map();
    private readonly step5SaveDraftLink: HTMLButtonElement | null;
    private readonly step5SurveyLink: HTMLAnchorElement | null;
    private readonly step5PublishBtn: HTMLButtonElement | null;

    private currentStep = 1;
    private saveDraftFeedbackTimer: ReturnType<typeof setTimeout> | null = null;
    private uploadAbortController: AbortController | null = null;

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
        this.step1NameWarning = container.querySelector('#step1NameWarning');
        this.saveDraftBtn = container.querySelector('#saveDraftBtn');

        this.step1ThemePrimary = container.querySelector('#step1ThemePrimary');
        this.step1ThemeSecondary = container.querySelector('#step1ThemeSecondary');
        this.step1ThemeAccent = container.querySelector('#step1ThemeAccent');
        this.step1ThemePreset = container.querySelector('#step1ThemePreset');
        this.step1ThemeFont = container.querySelector('#step1ThemeFont');
        this.step1MinAge = container.querySelector('#step1MinAge');
        this.step1MaxAge = container.querySelector('#step1MaxAge');
        this.step1MinAgeDisplay = container.querySelector('#step1MinAgeDisplay');
        this.step1MaxAgeDisplay = container.querySelector('#step1MaxAgeDisplay');

        this.step4NudgingStrength = container.querySelector('#step4NudgingStrength');
        this.step4NudgingDisplay = container.querySelector('#step4NudgingDisplay');
        this.step4PromptsJson = container.querySelector('#step4PromptsJson');
        this.step5SaveDraftLink = container.querySelector('#step5SaveDraftLink');
        this.step5SurveyLink = container.querySelector('#step5SurveyLink');
        this.step5PublishBtn = container.querySelector('#step5PublishBtn');

        this.discoverForms(container);
        this.migrateOldDraft();
        this.bindFormListeners();
        this.bindSaveDraftButton();
        this.bindNameWarning();
        this.clearDraftIfNeeded();
        this.seedCopyDraft();
        this.hydrateActiveStep();
        this.bindExitDraftModal();
        this.bindStep1ImageDropzone(container);
        this.bindStep1ThemePicker(container);
        this.bindStep1FontPicker(container);
        this.bindStep1AgeSliders();
        this.bindStep4NudgingSlider();
        this.bindStep4AdvancedToggle();
        this.bindStep4PromptModal(container);
        this.bindPreviewModeToggle(container);
        this.bindStep5LaunchLinks();
        this.bindBackFab();

        // Native beforeunload removed. Custom _DraftExitModal handles exit confirmation.
    }

    getStepperHooks(): StepperHooks {
        return {
            getInitialStep: () => this.readMeta()?.currentStep ?? 1,
            onStepChange: (step: number) => {
                this.currentStep = step;
                this.saveMeta();
                this.updateBackFab(step);
            },
            canAdvance: (currentStep: number) => this.handleCanAdvance(currentStep),
            onComplete: () => this.handleComplete(),
            onStepEnter: (step: number) => {
                this.currentStep = step;
                this.stepManagers.get(step)?.hydrate();
                if (step === 1) this.refreshStep1UI();
                if (step === 4) this.refreshStep4UI();
                this.saveMeta();
            },
            onStepClick: async (targetStep: number, currentStep: number) => {
                if (targetStep < currentStep) {
                    this.navigateToStep(targetStep);
                    return;
                }

                const canAdvance = await this.handleCanAdvance(currentStep);
                if (canAdvance === false) return;
                this.navigateToStep(targetStep);
            }
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

        const step1Form = this.stepManagers.get(1)?.form;
        const interactionFormSelect = step1Form?.querySelector<HTMLSelectElement>('select[name="CreateStep1ViewModel.InteractionForm"]');
        interactionFormSelect?.addEventListener('change', () => {
            const previewToggle = document.getElementById('preview-mode-toggle');
            if (previewToggle) {
                if (interactionFormSelect.value === 'UserDefined' || interactionFormSelect.value === '0') {
                    previewToggle.classList.remove('hidden');
                    const currentMode = localStorage.getItem(`${this.draftStoragePrefix}:preview-mode`) ?? 'Chat';
                    previewToggle.querySelectorAll<HTMLButtonElement>('[data-preview-mode]').forEach(btn => {
                        const isActive = btn.dataset.previewMode === currentMode;
                        btn.classList.toggle('bg-text/10', isActive);
                        btn.classList.toggle('text-text', isActive);
                        btn.classList.toggle('text-text/40', !isActive);
                    });
                } else {
                    previewToggle.classList.add('hidden');
                }
            }
        });
    }

    private bindSaveDraftButton(): void {
        this.saveDraftBtn?.addEventListener('click', async () => {
            await this.ensureStep1ImageUploaded();
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

        step1Manager.hydrate();
        this.refreshStep1UI();

        if (!step1Manager.validate()) return;

        const imageOk = await this.ensureStep1ImageUploaded();
        if (!imageOk) return;

        this.injectStepFieldsIntoForm(2, step1Manager.form);
        this.injectStepFieldsIntoForm(3, step1Manager.form);
        this.injectStepFieldsIntoForm(4, step1Manager.form);

        this.clearAllLocalDrafts();
        step1Manager.form.requestSubmit();
    }

    private injectStepFieldsIntoForm(stepNum: number, targetForm: HTMLFormElement): void {
        const manager = this.stepManagers.get(stepNum);
        if (!manager) return;
        manager.persist();
        const fields = manager.getFieldMap();
        for (const [name, value] of Object.entries(fields)) {
            let input = targetForm.querySelector<HTMLInputElement>(`input[name="${name}"]`);
            if (!input) {
                input = document.createElement('input');
                input.type = 'hidden';
                input.name = name;
                targetForm.appendChild(input);
            }
            input.value = value;
        }
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
        if (meta.currentStep === 1) this.refreshStep1UI();
        this.updateBackFab(meta.currentStep);
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

        if (this.uploadAbortController) {
            this.uploadAbortController.abort();
        }
        this.uploadAbortController = new AbortController();

        const payload = new FormData();
        payload.append('imageFile', selectedFile);
        payload.append('__RequestVerificationToken', antiForgeryToken);

        try {
            const response = await fetch(this.imageUploadUrl, {
                method: 'POST',
                credentials: 'same-origin',
                body: payload,
                signal: this.uploadAbortController.signal,
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

            if (this.step1ImageUrl) {
                this.step1ImageUrl.value = responseBody.imageUrl;
                this.step1ImageUrl.dispatchEvent(new Event('input', { bubbles: true }));
            }
            if (this.step1ImageUploadSignature) this.step1ImageUploadSignature.value = currentSignature;

            if (this.step1ImageFile) this.step1ImageFile.value = '';

            this.setUploadStatus('Image uploaded.');
            return true;
        } catch (err) {
            if (err instanceof DOMException && err.name === 'AbortError') {
                this.setUploadStatus('');
                return false;
            }
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

        step1Manager.hydrate();
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

        try {
            const response = await fetch(this.draftSaveUrl, {
                method: 'POST',
                credentials: 'same-origin',
                body: payload,
            });

            if (!response.ok) {
                await this.handleDraftSaveError(response);
                this.setSaveDraftFeedback('Save failed', true);
                return;
            }

            const data = (await response.json()) as { slug?: string };
            if (data?.slug && this.step1Slug) {
                this.step1Slug.value = data.slug;
                this.saveMeta();
                this.updateStep5SurveyLink(data.slug);
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

            const step2Manager = this.stepManagers.get(2);
            if (step2Manager) {
                const qJson = parsed['questionsJson'];
                if (typeof qJson === 'string' && qJson && qJson !== '[]') {
                    localStorage.setItem(step2Manager.storageKey, JSON.stringify({
                        fields: { 'CreateStep2ViewModel.QuestionsJson': qJson },
                        draftSynced: false,
                    }));
                }
            }

            const step3Manager = this.stepManagers.get(3);
            if (step3Manager) {
                const tJson = parsed['topicsJson'];
                if (typeof tJson === 'string' && tJson && tJson !== '[]') {
                    localStorage.setItem(step3Manager.storageKey, JSON.stringify({
                        fields: { 'CreateStep3ViewModel.TopicsJson': tJson },
                        draftSynced: false,
                    }));
                }
            }

            const step4Manager = this.stepManagers.get(4);
            if (step4Manager) {
                const step4Fields: Record<string, string> = {};
                for (const [snapshotKey, formName] of Object.entries(STEP4_FIELD_MAP)) {
                    const value = parsed[snapshotKey];
                    if (typeof value === 'string') {
                        step4Fields[formName] = value;
                    } else if (value !== undefined && value !== null) {
                        step4Fields[formName] = String(value);
                    }
                }
                const pJson = parsed['promptsJson'];
                if (typeof pJson === 'string' && pJson && pJson !== '[]') {
                    step4Fields['CreateStep4ViewModel.PromptsJson'] = pJson;
                }
                if (Object.keys(step4Fields).length > 0) {
                    localStorage.setItem(step4Manager.storageKey, JSON.stringify({ fields: step4Fields, draftSynced: false }));
                }
            }

            this.saveMeta({ currentStep: 1, slug: '' });

            if (this.step1Slug) this.step1Slug.value = '';
        } catch {
            return;
        }
    }

    private clearDraftIfNeeded(): void {
        if (!this.isCreatePage || this.isCopyFlow) return;
        if (sessionStorage.getItem('recoverDraft') === '1') {
            sessionStorage.removeItem('recoverDraft');
            return;
        }
        const metaKey = `${this.draftStoragePrefix}:meta`;
        if (localStorage.getItem(metaKey)) return;
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

    // ── Step 1: image dropzone ──────────────────────────────────────────────

    private bindStep1ImageDropzone(container: HTMLElement): void {
        const dropzone = container.querySelector<HTMLElement>('#step1ImageDropzone');
        const uploadBtn = container.querySelector<HTMLButtonElement>('#step1ImageUploadBtn');
        const preview = container.querySelector<HTMLImageElement>('#step1ImagePreview');
        const placeholder = container.querySelector<HTMLElement>('#step1ImagePlaceholder');

        const showPreview = (src: string) => {
            if (!preview || !placeholder) return;
            preview.src = src;
            preview.classList.remove('hidden');
            placeholder.classList.add('hidden');
        };

        dropzone?.addEventListener('click', () => this.step1ImageFile?.click());
        uploadBtn?.addEventListener('click', (e) => {
            e.preventDefault();
            this.step1ImageFile?.click();
        });

        this.step1ImageFile?.addEventListener('change', () => {
            const file = this.step1ImageFile?.files?.[0];
            if (!file) return;
            const reader = new FileReader();
            reader.onload = (e) => {
                const result = e.target?.result;
                if (typeof result === 'string') showPreview(result);
            };
            reader.readAsDataURL(file);

            this.ensureStep1ImageUploaded();
        });

        const existingUrl = this.step1ImageUrl?.value.trim();
        if (existingUrl) showPreview(existingUrl);
    }

    // ── Step 1: theme picker ────────────────────────────────────────────────

    private bindStep1ThemePicker(container: HTMLElement): void {
        const modal = container.querySelector<HTMLElement>('#step1ThemePickerModal');
        const pickerPrimary = container.querySelector<HTMLInputElement>('#step1PickerPrimary');
        const pickerPrimaryHex = container.querySelector<HTMLInputElement>('#step1PickerPrimaryHex');
        const pickerSecondary = container.querySelector<HTMLInputElement>('#step1PickerSecondary');
        const pickerSecondaryHex = container.querySelector<HTMLInputElement>('#step1PickerSecondaryHex');
        const pickerAccent = container.querySelector<HTMLInputElement>('#step1PickerAccent');
        const pickerAccentHex = container.querySelector<HTMLInputElement>('#step1PickerAccentHex');
        const applyBtn = container.querySelector<HTMLButtonElement>('#step1PickerApply');
        const cancelBtn = container.querySelector<HTMLButtonElement>('#step1PickerCancel');

        const syncColorHex = (color: HTMLInputElement, hex: HTMLInputElement) => {
            color.addEventListener('input', () => { hex.value = color.value; });
            hex.addEventListener('input', () => {
                const v = hex.value.trim();
                if (/^#[0-9a-f]{6}$/i.test(v)) color.value = v;
            });
        };
        if (pickerPrimary && pickerPrimaryHex) syncColorHex(pickerPrimary, pickerPrimaryHex);
        if (pickerSecondary && pickerSecondaryHex) syncColorHex(pickerSecondary, pickerSecondaryHex);
        if (pickerAccent && pickerAccentHex) syncColorHex(pickerAccent, pickerAccentHex);

        const openPicker = (primary: string, secondary: string, accent: string) => {
            if (pickerPrimary) { pickerPrimary.value = primary; }
            if (pickerPrimaryHex) { pickerPrimaryHex.value = primary; }
            if (pickerSecondary) { pickerSecondary.value = secondary; }
            if (pickerSecondaryHex) { pickerSecondaryHex.value = secondary; }
            if (pickerAccent) { pickerAccent.value = accent; }
            if (pickerAccentHex) { pickerAccentHex.value = accent; }
            modal?.classList.remove('hidden');
            modal?.classList.add('flex');
        };

        const closePicker = () => {
            modal?.classList.add('hidden');
            modal?.classList.remove('flex');
        };

        container.querySelectorAll<HTMLElement>('.theme-row').forEach((row) => {
            row.addEventListener('click', (e) => {
                if ((e.target as HTMLElement).closest('.theme-row-plus')) {
                    e.stopPropagation();
                    openPicker(
                        row.dataset.primary ?? '#6c5ce7',
                        row.dataset.secondary ?? '#db99c8',
                        row.dataset.accent ?? '#cd6f88',
                    );
                    return;
                }
                this.applyThemePreset(
                    row.dataset.preset ?? 'default',
                    row.dataset.primary ?? '#6c5ce7',
                    row.dataset.secondary ?? '#db99c8',
                    row.dataset.accent ?? '#cd6f88',
                    container,
                );
            });
        });

        container.querySelector<HTMLButtonElement>('#step1CreateThemeBtn')?.addEventListener('click', () => {
            openPicker('#6c5ce7', '#db99c8', '#cd6f88');
        });

        applyBtn?.addEventListener('click', () => {
            const primary = pickerPrimary?.value ?? '#6c5ce7';
            const secondary = pickerSecondary?.value ?? '#db99c8';
            const accent = pickerAccent?.value ?? '#cd6f88';

            const customRow = container.querySelector<HTMLElement>('#step1CustomThemeRow');
            if (customRow) {
                customRow.dataset.primary = primary;
                customRow.dataset.secondary = secondary;
                customRow.dataset.accent = accent;
                const dots = customRow.querySelectorAll<HTMLElement>('.theme-dot');
                [primary, secondary, accent].forEach((c, i) => {
                    if (dots[i]) dots[i].style.background = c;
                });
                customRow.classList.remove('hidden');
            }

            this.applyThemePreset('custom', primary, secondary, accent, container);
            closePicker();
        });

        cancelBtn?.addEventListener('click', closePicker);
        modal?.addEventListener('click', (e) => { if (e.target === modal) closePicker(); });
    }

    private applyThemePreset(preset: string, primary: string, secondary: string, accent: string, container: HTMLElement): void {
        if (this.step1ThemePrimary) this.step1ThemePrimary.value = primary;
        if (this.step1ThemeSecondary) this.step1ThemeSecondary.value = secondary;
        if (this.step1ThemeAccent) this.step1ThemeAccent.value = accent;
        if (this.step1ThemePreset) this.step1ThemePreset.value = preset;

        // Trigger change so StepDraftManager persists the new values
        this.step1ThemePreset?.dispatchEvent(new Event('change', { bubbles: true }));

        container.querySelectorAll<HTMLElement>('.theme-row').forEach((row) => {
            const selected = row.dataset.preset === preset;
            row.classList.toggle('bg-text/5', selected);
            row.classList.toggle('ring-1', selected);
            row.classList.toggle('ring-inset', selected);
            row.classList.toggle('ring-text/20', selected);
        });
    }

    // ── Step 4: nudging slider ──────────────────────────────────────────────

    private bindStep4NudgingSlider(): void {
        this.step4NudgingStrength?.addEventListener('input', () => {
            if (this.step4NudgingDisplay && this.step4NudgingStrength) {
                this.step4NudgingDisplay.textContent = this.step4NudgingStrength.value;
            }
        });
    }

    private refreshStep4UI(): void {
        if (this.step4NudgingStrength && this.step4NudgingDisplay) {
            this.step4NudgingDisplay.textContent = this.step4NudgingStrength.value;
        }
        this.refreshStep4OverridesFromJson();
    }

    // ── Step 1: age sliders ─────────────────────────────────────────────────

    private bindStep1AgeSliders(): void {
        const update = (input: HTMLInputElement | null, display: HTMLElement | null) => {
            if (input && display) display.textContent = input.value;
        };

        this.step1MinAge?.addEventListener('input', () => {
            update(this.step1MinAge, this.step1MinAgeDisplay);
            if (this.step1MaxAge && this.step1MinAge &&
                parseInt(this.step1MinAge.value) > parseInt(this.step1MaxAge.value)) {
                this.step1MaxAge.value = this.step1MinAge.value;
                update(this.step1MaxAge, this.step1MaxAgeDisplay);
            }
        });

        this.step1MaxAge?.addEventListener('input', () => {
            update(this.step1MaxAge, this.step1MaxAgeDisplay);
            if (this.step1MinAge && this.step1MaxAge &&
                parseInt(this.step1MaxAge.value) < parseInt(this.step1MinAge.value)) {
                this.step1MinAge.value = this.step1MaxAge.value;
                update(this.step1MinAge, this.step1MinAgeDisplay);
            }
        });
    }

    // ── Step 1: refresh UI after hydration ──────────────────────────────────

    private refreshStep1UI(): void {
        if (this.step1MinAge && this.step1MinAgeDisplay) {
            this.step1MinAgeDisplay.textContent = this.step1MinAge.value;
        }
        if (this.step1MaxAge && this.step1MaxAgeDisplay) {
            this.step1MaxAgeDisplay.textContent = this.step1MaxAge.value;
        }

        const container = document.getElementById('dynamic-stepper');
        if (!container || !this.step1ThemePreset) return;

        const preset = this.step1ThemePreset.value || 'default';

        if (preset === 'custom') {
            const customRow = container.querySelector<HTMLElement>('#step1CustomThemeRow');
            if (customRow) {
                const p = this.step1ThemePrimary?.value ?? '#6c5ce7';
                const s = this.step1ThemeSecondary?.value ?? '#db99c8';
                const a = this.step1ThemeAccent?.value ?? '#cd6f88';
                customRow.dataset.primary = p;
                customRow.dataset.secondary = s;
                customRow.dataset.accent = a;
                const dots = customRow.querySelectorAll<HTMLElement>('.theme-dot');
                [p, s, a].forEach((c, i) => { if (dots[i]) dots[i].style.background = c; });
                customRow.classList.remove('hidden');
            }
        }

        container.querySelectorAll<HTMLElement>('.theme-row').forEach((row) => {
            const selected = row.dataset.preset === preset;
            row.classList.toggle('bg-text/5', selected);
            row.classList.toggle('ring-1', selected);
            row.classList.toggle('ring-inset', selected);
            row.classList.toggle('ring-text/20', selected);
        });

        const existingUrl = this.step1ImageUrl?.value.trim();
        if (existingUrl) {
            const preview = container.querySelector<HTMLImageElement>('#step1ImagePreview');
            const placeholder = container.querySelector<HTMLElement>('#step1ImagePlaceholder');
            if (preview && placeholder) {
                preview.src = existingUrl;
                preview.classList.remove('hidden');
                placeholder.classList.add('hidden');
            }
        }

        const savedFont = this.step1ThemeFont?.value ?? '';
        container.querySelectorAll<HTMLElement>('.font-option').forEach((btn) => {
            const selected = btn.dataset.font === savedFont;
            btn.classList.toggle('bg-text/5', selected);
            btn.classList.toggle('border-text/35', selected);
        });

        this.refreshStep4OverridesFromJson();
        this.updateStep5SurveyLink(this.step1Slug?.value ?? '');

        // Sync preview mode toggle visibility
        const previewToggle = document.getElementById('preview-mode-toggle');
        if (previewToggle) {
            const step1Form = this.stepManagers.get(1)?.form;
            const select = step1Form?.querySelector<HTMLSelectElement>('select[name="CreateStep1ViewModel.InteractionForm"]');
            if (select && (select.value === 'UserDefined' || select.value === '0')) {
                previewToggle.classList.remove('hidden');
                const currentMode = localStorage.getItem(`${this.draftStoragePrefix}:preview-mode`) ?? 'Chat';
                previewToggle.querySelectorAll<HTMLButtonElement>('[data-preview-mode]').forEach(btn => {
                    const isActive = btn.dataset.previewMode === currentMode;
                    btn.classList.toggle('bg-text/10', isActive);
                    btn.classList.toggle('text-text', isActive);
                    btn.classList.toggle('text-text/40', !isActive);
                });
            } else {
                previewToggle.classList.add('hidden');
            }
        }
    }

    private bindBackFab(): void {
        const backFab = document.getElementById('backFabBtn');
        if (!backFab) return;
        backFab.addEventListener('click', () => {
            document.getElementById('prevBtn')?.click();
        });
        this.updateBackFab(this.currentStep);
    }

    private updateBackFab(step: number): void {
        const backFab = document.getElementById('backFabBtn');
        if (!backFab) return;
        backFab.classList.toggle('hidden', step <= 1);
    }

    // ── Step 4: advanced settings toggle ───────────────────────────────────

    private bindStep4AdvancedToggle(): void {
        const toggle = document.getElementById('step4AdvancedToggle');
        const panel = document.getElementById('step4AdvancedPanel');
        const chevron = document.getElementById('step4AdvancedChevron');
        if (!toggle || !panel || !chevron) return;

        toggle.addEventListener('click', () => {
            const isOpen = panel.style.maxHeight !== '' && panel.style.maxHeight !== '0px';
            if (isOpen) {
                panel.style.maxHeight = '0px';
                chevron.style.transform = 'rotate(0deg)';
            } else {
                panel.style.maxHeight = `${panel.scrollHeight}px`;
                chevron.style.transform = 'rotate(180deg)';
            }
        });
    }

    // ── Step 4: prompt modal ────────────────────────────────────────────────

    private bindStep4PromptModal(container: HTMLElement): void {
        const modal = document.getElementById('step4PromptModal');
        const titleEl = document.getElementById('step4ModalTitle');
        const descEl = document.getElementById('step4ModalDescription');
        const textarea = document.getElementById('step4ModalTextarea') as HTMLTextAreaElement | null;
        const applyBtn = document.getElementById('step4ModalApply');
        const cancelBtn = document.getElementById('step4ModalCancel');
        const closeBtn = document.getElementById('step4ModalClose');
        const resetBtn = document.getElementById('step4ModalReset');
        if (!modal || !textarea) return;

        let activePromptName = '';
        let activeGlobalTemplate = '';

        const openModal = (name: string, description: string, globalTemplate: string) => {
            activePromptName = name;
            activeGlobalTemplate = globalTemplate;
            if (titleEl) titleEl.textContent = name;
            if (descEl) descEl.textContent = description;
            textarea.value = this.step4Overrides.get(name) ?? globalTemplate;
            modal.classList.remove('hidden');
            modal.classList.add('flex');
            setTimeout(() => textarea.focus(), 50);
        };

        const closeModal = () => {
            modal.classList.add('hidden');
            modal.classList.remove('flex');
        };

        container.addEventListener('click', (e) => {
            const btn = (e.target as HTMLElement).closest<HTMLButtonElement>('.prompt-edit-btn');
            if (!btn) return;
            openModal(
                btn.dataset.promptName ?? '',
                btn.dataset.promptDescription ?? '',
                btn.dataset.globalTemplate ?? '',
            );
        });

        applyBtn?.addEventListener('click', () => {
            if (!activePromptName) return;
            const val = textarea.value;
            if (val.trim()) {
                this.step4Overrides.set(activePromptName, val);
            } else {
                this.step4Overrides.delete(activePromptName);
            }
            this.persistStep4Overrides();
            this.updatePromptCardBadge(activePromptName, this.step4Overrides.has(activePromptName));
            closeModal();
        });

        resetBtn?.addEventListener('click', () => {
            textarea.value = activeGlobalTemplate;
        });

        cancelBtn?.addEventListener('click', closeModal);
        closeBtn?.addEventListener('click', closeModal);
        modal.addEventListener('click', (e) => { if (e.target === modal) closeModal(); });
    }

    private persistStep4Overrides(): void {
        if (!this.step4PromptsJson) return;
        const overrides: PromptOverride[] = [];
        for (const [name, template] of this.step4Overrides) {
            overrides.push({ promptName: name, userPromptTemplate: template });
        }
        this.step4PromptsJson.value = JSON.stringify(overrides);
        this.step4PromptsJson.dispatchEvent(new Event('change', { bubbles: true }));
    }

    private updatePromptCardBadge(promptName: string, hasOverride: boolean): void {
        const btn = document.querySelector<HTMLElement>(`.prompt-edit-btn[data-prompt-name="${CSS.escape(promptName)}"]`);
        if (!btn) return;
        const card = btn.closest<HTMLElement>('[data-name]');
        if (!card) return;
        let badge = card.querySelector<HTMLElement>('.prompt-override-badge');
        if (hasOverride && !badge) {
            badge = document.createElement('span');
            badge.className = 'prompt-override-badge flex-shrink-0 mt-0.5 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-primary/10 text-primary';
            badge.textContent = 'Customized';
            const nameEl = card.querySelector('h2');
            nameEl?.parentElement?.appendChild(badge);
        } else if (!hasOverride && badge) {
            badge.remove();
        }
    }

    private refreshStep4OverridesFromJson(): void {
        this.step4Overrides.clear();
        const raw = this.step4PromptsJson?.value;
        if (!raw || raw === '[]') return;
        try {
            const parsed = JSON.parse(raw) as Array<{ promptName?: string; userPromptTemplate?: string }>;
            for (const item of parsed) {
                if (item.promptName && item.userPromptTemplate) {
                    this.step4Overrides.set(item.promptName, item.userPromptTemplate);
                }
            }
            for (const [name] of this.step4Overrides) {
                this.updatePromptCardBadge(name, true);
            }
        } catch { /* ignore */ }
    }

    // ── Step 1: font picker ─────────────────────────────────────────────────

    private bindStep1FontPicker(container: HTMLElement): void {
        container.querySelectorAll<HTMLButtonElement>('.font-option').forEach((btn) => {
            btn.addEventListener('click', () => {
                const font = btn.dataset.font ?? '';
                if (this.step1ThemeFont) {
                    this.step1ThemeFont.value = font;
                    this.step1ThemeFont.dispatchEvent(new Event('change', { bubbles: true }));
                }
                container.querySelectorAll<HTMLElement>('.font-option').forEach((b) => {
                    const selected = b === btn;
                    b.classList.toggle('bg-text/5', selected);
                    b.classList.toggle('border-text/35', selected);
                });
            });
        });
    }

    // ── Preview mode toggle (UserDefined) ───────────────────────────────────

    private bindPreviewModeToggle(_container: HTMLElement): void {
        const toggle = document.getElementById('preview-mode-toggle')
        const chatBtn = toggle?.querySelector<HTMLButtonElement>('[data-preview-mode="Chat"]')
        const scrollBtn = toggle?.querySelector<HTMLButtonElement>('[data-preview-mode="Vertical_Scroll"]')
        const prefixedKey = `${this.draftStoragePrefix}:preview-mode`

        if (!toggle || !chatBtn || !scrollBtn) return

        const highlightBtn = (mode: string) => {
            const isChat = mode === 'Chat'
            chatBtn.classList.toggle('bg-text/10', isChat)
            chatBtn.classList.toggle('text-text', isChat)
            chatBtn.classList.toggle('text-text/40', !isChat)
            scrollBtn.classList.toggle('bg-text/10', !isChat)
            scrollBtn.classList.toggle('text-text', !isChat)
            scrollBtn.classList.toggle('text-text/40', isChat)
        }

        chatBtn.addEventListener('click', () => {
            localStorage.setItem(prefixedKey, 'Chat')
            highlightBtn('Chat')
        })

        scrollBtn.addEventListener('click', () => {
            localStorage.setItem(prefixedKey, 'Vertical_Scroll')
            highlightBtn('Vertical_Scroll')
        })

        // Init — read latest from localStorage
        highlightBtn(localStorage.getItem(prefixedKey) ?? 'Chat')
    }

    private bindStep5LaunchLinks(): void {
        if (!this.step5SaveDraftLink || !this.step5SurveyLink) return;

        this.step5SaveDraftLink.addEventListener('click', async () => {
            await this.ensureStep1ImageUploaded();
            this.persistCurrentStep();
            await this.saveDraftToServer();

            const slug = this.step1Slug?.value ?? '';
            if (slug) {
                window.open(this.buildSurveyLink(slug), '_blank');
            }
        });

        this.step5PublishBtn?.addEventListener('click', async () => {
            await this.handleComplete();
            const slug = this.step1Slug?.value ?? '';
            if (slug) {
                window.open(this.buildSurveyLink(slug), '_blank');
            }
        });

        const slug = this.step1Slug?.value ?? '';
        this.updateStep5SurveyLink(slug);
    }

    private updateStep5SurveyLink(slug: string): void {
        if (!this.step5SurveyLink) return;

        if (!slug) {
            this.step5SurveyLink.href = '#';
            this.step5SurveyLink.textContent = 'Survey link will appear after saving';
            this.step5SurveyLink.classList.add('pointer-events-none', 'opacity-60');
            return;
        }

        const link = this.buildSurveyLink(slug);
        this.step5SurveyLink.href = link;
        this.step5SurveyLink.textContent = link;
        this.step5SurveyLink.classList.remove('pointer-events-none', 'opacity-60');
    }

    private buildSurveyLink(slug: string): string {
        return `${window.location.origin}/${slug}`;
    }

    private navigateToStep(step: number): void {
        const stepper = document.getElementById('dynamic-stepper');
        if (!stepper) return;
        stepper.dispatchEvent(new CustomEvent('stepper:force-step', { detail: { step } }));
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
