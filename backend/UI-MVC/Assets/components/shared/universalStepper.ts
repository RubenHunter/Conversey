interface StepDraftSnapshot {
    currentStep: number;
    name: string;
    description: string;
    interactionForm: string;
    startDate: string;
    endDate: string;
    imageUrl: string;
    imageUploadSignature: string;
}

interface ImageUploadResponse {
    imageUrl?: unknown;
    error?: unknown;
}

export class Stepper {
    private currentStep = 1;
    private readonly totalSteps: number;
    private readonly entityName: string;
    private readonly draftStoragePrefix: string;
    private readonly imageUploadUrl: string;

    private readonly container: HTMLElement;
    private readonly nextBtn: HTMLButtonElement;
    private readonly prevBtn: HTMLButtonElement;
    private readonly saveDraftBtn: HTMLButtonElement | null;
    private readonly progressLine: HTMLElement;
    private readonly trackContainer: HTMLElement;

    private readonly step1Form: HTMLFormElement | null;
    private readonly step1ImageFile: HTMLInputElement | null;
    private readonly step1ImageUrl: HTMLInputElement | null;
    private readonly step1ImageUploadSignature: HTMLInputElement | null;
    private readonly step1ImageUploadStatus: HTMLElement | null;
    private readonly step1ImageUploadError: HTMLElement | null;

    private isTransitioning = false;

    constructor(containerId: string) {
        this.container = document.getElementById(containerId)!;
        this.totalSteps = parseInt(this.container.dataset.totalSteps || '1', 10);
        this.entityName = this.container.dataset.entity || 'Item';
        this.draftStoragePrefix = this.container.dataset.draftStoragePrefix || '';
        this.imageUploadUrl = this.container.dataset.imageUploadUrl || '';

        this.nextBtn = this.container.querySelector('#nextBtn')!;
        this.prevBtn = this.container.querySelector('#prevBtn')!;
        this.saveDraftBtn = this.container.querySelector('#saveDraftBtn');
        this.progressLine = this.container.querySelector('#progress-line')!;
        this.trackContainer = this.container.querySelector('[class*="relative"][class*="flex"]')!;

        this.step1Form = this.container.querySelector('#create-project-step1-form');
        this.step1ImageFile = this.container.querySelector('#step1ImageFile');
        this.step1ImageUrl = this.container.querySelector('#step1ImageUrl');
        this.step1ImageUploadSignature = this.container.querySelector('#step1ImageUploadSignature');
        this.step1ImageUploadStatus = this.container.querySelector('#step1ImageUploadStatus');
        this.step1ImageUploadError = this.container.querySelector('#step1ImageUploadError');

        this.init();
    }

    private init() {
        this.nextBtn.addEventListener('click', () => {
            void this.handleNextClick();
        });
        this.prevBtn.addEventListener('click', () => {
            void this.goToStep(this.currentStep - 1);
        });
        this.saveDraftBtn?.addEventListener('click', () => this.persistDraft());

        this.hydrateDraft();

        setTimeout(() => this.positionTrack(), 0);
        this.updateUI();
    }

    private async handleNextClick() {
        if (this.currentStep === this.totalSteps) {
            this.submitStep1Form();
            return;
        }

        await this.goToStep(this.currentStep + 1);
    }

    private async goToStep(step: number) {
        if (step < 1 || step > this.totalSteps || this.isTransitioning) return;

        this.isTransitioning = true;
        try {
            if (step > this.currentStep) {
                const canProceed = await this.beforeForwardStep(step);
                if (!canProceed) return;
            }

            this.currentStep = step;
            this.updateUI();
            this.persistDraft();
        } finally {
            this.isTransitioning = false;
        }
    }

    private async beforeForwardStep(targetStep: number): Promise<boolean> {
        if (this.currentStep !== 1 || targetStep <= 1) return true;
        if (!this.step1Form) return true;
        if (!this.step1Form.reportValidity()) return false;
        return await this.ensureStep1ImageUploaded();
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

    private positionTrack() {
        const steps = this.container.querySelectorAll('.step-indicator');
        if (steps.length < 2) return;

        const firstStep = steps[0] as HTMLElement;
        const lastStep = steps[steps.length - 1] as HTMLElement;

        const firstRect = firstStep.getBoundingClientRect();
        const lastRect = lastStep.getBoundingClientRect();
        const containerRect = this.trackContainer.getBoundingClientRect();

        const leftOffset = firstRect.left - containerRect.left + firstRect.width / 2;
        const rightOffset = containerRect.right - lastRect.right + lastRect.width / 2;
        const trackWidth = containerRect.width - leftOffset - rightOffset;

        const track = this.progressLine.parentElement!;
        track.style.left = `${leftOffset}px`;
        track.style.right = `${rightOffset}px`;
        track.style.width = `${trackWidth}px`;
    }

    private updateUI() {
        const progressPercent = this.totalSteps <= 1
            ? 100
            : ((this.currentStep - 1) / (this.totalSteps - 1)) * 100;
        this.progressLine.style.width = `${progressPercent}%`;

        this.container.querySelectorAll('.step-indicator').forEach((el, idx) => {
            const stepIdx = idx + 1;
            const circle = el.querySelector('.step-circle')!;
            const dot = el.querySelector('.step-dot')!;
            const pane = document.getElementById(`step-content-${stepIdx}`)!;

            circle.className = 'step-circle w-7 h-7 rounded-full border-2 flex items-center justify-center transition-all duration-300';
            dot.classList.add('scale-0');
            pane.classList.add('hidden');

            if (stepIdx < this.currentStep) {
                circle.classList.add('bg-primary', 'border-primary');
            } else if (stepIdx === this.currentStep) {
                circle.classList.add('bg-background', 'border-primary', 'ring-4', 'ring-primary/10');
                dot.classList.remove('scale-0');
                dot.classList.add('bg-primary');
                pane.classList.remove('hidden');
            } else {
                circle.classList.add('bg-background', 'border-text/30');
            }
        });

        this.prevBtn.classList.toggle('hidden', this.currentStep === 1);

        if (this.currentStep === this.totalSteps) {
            this.nextBtn.innerText = `Create ${this.entityName}`;
            this.nextBtn.classList.replace('bg-text', 'bg-primary');
        } else {
            this.nextBtn.innerText = 'Next >';
            this.nextBtn.classList.replace('bg-primary', 'bg-text');
        }
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
            imageUploadSignature: this.step1ImageUploadSignature?.value ?? ''
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

        if (this.step1ImageUrl) this.step1ImageUrl.value = saved.imageUrl;
        if (this.step1ImageUploadSignature) this.step1ImageUploadSignature.value = saved.imageUploadSignature;
        if (saved.imageUrl.trim().length > 0) this.setUploadStatus('Loaded saved draft image.');

        if (saved.currentStep >= 1 && saved.currentStep <= this.totalSteps) {
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
                    imageUploadSignature: typeof parsed.imageUploadSignature === 'string' ? parsed.imageUploadSignature : ''
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
        const name = this.getFieldValue('CreateStep1ViewModel.Name');
        const slug = this.slugify(name);
        return `${this.draftStoragePrefix}:${slug || 'draft'}`;
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
}

document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('dynamic-stepper')) {
        new Stepper('dynamic-stepper');
    }
});
