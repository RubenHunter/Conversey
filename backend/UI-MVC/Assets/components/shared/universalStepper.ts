export interface StepperHooks {
    setup?: (reset: () => void) => void;
    getInitialStep?: () => number;
    onStepChange?: (step: number) => void;
    onStepEnter?: (step: number) => void;
    canAdvance?: (currentStep: number) => Promise<boolean>;
    onComplete?: () => void;
}

export class Stepper {
    private currentStep: number = 1;
    private readonly totalSteps: number;
    private readonly entityName: string;
    private readonly hooks: StepperHooks;

    private readonly container: HTMLElement;
    private readonly nextBtn: HTMLButtonElement;
    private readonly prevBtn: HTMLButtonElement;
    private readonly progressLine: HTMLElement;
    private readonly trackContainer: HTMLElement;

    constructor(containerId: string, hooks?: StepperHooks) {
        this.hooks = hooks ?? {};

        this.container = document.getElementById(containerId)!;
        this.totalSteps = parseInt(this.container.dataset.totalSteps || '1', 10);
        this.entityName = this.container.dataset.entity || 'Item';

        this.nextBtn = this.container.querySelector('#nextBtn')!;
        this.prevBtn = this.container.querySelector('#prevBtn')!;
        this.progressLine = this.container.querySelector('#progress-line')!;
        this.trackContainer = this.container.querySelector('[class*="relative"][class*="flex"]')!;

        this.init();
    }

    private init() {
        this.currentStep = this.clampStep(this.hooks.getInitialStep?.() ?? 1);

        this.nextBtn.addEventListener('click', () => void this.handleNext());
        this.prevBtn.addEventListener('click', () => void this.handlePrev());

        this.hooks.setup?.(() => this.reset());

        setTimeout(() => this.positionTrack(), 0);
        this.updateUI();
        this.dispatchStepEnter(this.currentStep);
    }

    reset() {
        this.goToStep(1);
    }

    private async handleNext() {
        if (this.currentStep === this.totalSteps) {
            this.hooks.onComplete?.();
            return;
        }

        const next = this.currentStep + 1;
        const canAdvance = await this.hooks.canAdvance?.(this.currentStep);
        if (canAdvance === false) return;

        this.goToStep(next);
    }

    private handlePrev() {
        if (this.currentStep <= 1) return;
        this.goToStep(this.currentStep - 1);
    }

    private goToStep(step: number) {
        if (step < 1 || step > this.totalSteps) return;
        this.currentStep = step;
        this.updateUI();
        this.hooks.onStepChange?.(step);
        this.dispatchStepEnter(step);
    }

    private dispatchStepEnter(step: number) {
        this.hooks.onStepEnter?.(step);
        this.container.dispatchEvent(
            new CustomEvent('stepper:step-enter', { detail: { step }, bubbles: true })
        );
    }

    private updateUI() {
        const progressPercent = ((this.currentStep - 1) / (this.totalSteps - 1)) * 100;
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

    private clampStep(step: number): number {
        return Math.min(Math.max(Math.floor(step), 1), this.totalSteps);
    }
}
