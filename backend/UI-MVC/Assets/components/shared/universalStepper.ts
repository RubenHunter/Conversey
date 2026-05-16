export class Stepper {
    private currentStep: number = 1;
    private totalSteps: number;
    private entityName: string;

    private container: HTMLElement;
    private nextBtn: HTMLButtonElement;
    private prevBtn: HTMLButtonElement;
    private progressLine: HTMLElement;
    private trackContainer: HTMLElement;

    constructor(containerId: string) {
        this.container = document.getElementById(containerId)!;
        this.totalSteps = parseInt(this.container.dataset.totalSteps || "1");
        this.entityName = this.container.dataset.entity || "Item";

        this.nextBtn = this.container.querySelector('#nextBtn')!;
        this.prevBtn = this.container.querySelector('#prevBtn')!;
        this.progressLine = this.container.querySelector('#progress-line')!;
        this.trackContainer = this.container.querySelector('[class*="relative"][class*="flex"]')!;

        this.init();
    }

    private init() {
        this.nextBtn.addEventListener('click', () => {
            if (this.currentStep === this.totalSteps) {
                const saveBtn = document.getElementById('saveProviderBtn') as HTMLButtonElement | null;
                if (saveBtn) {
                    saveBtn.click();
                }
                return;
            }
            this.goToStep(this.currentStep + 1);
        });
        this.prevBtn.addEventListener('click', () => this.goToStep(this.currentStep - 1));

        // Wait for DOM to render, then calculate and position track
        setTimeout(() => this.positionTrack(), 0);
        this.updateUI();
    }

    private positionTrack() {
        const steps = this.container.querySelectorAll('.step-indicator');
        if (steps.length < 2) return;

        const firstStep = steps[0] as HTMLElement;
        const lastStep = steps[steps.length - 1] as HTMLElement;

        const firstRect = firstStep.getBoundingClientRect();
        const lastRect = lastStep.getBoundingClientRect();
        const containerRect = this.trackContainer.getBoundingClientRect();

        // Calculate offsets relative to container
        const leftOffset = firstRect.left - containerRect.left + firstRect.width / 2;
        const rightOffset = containerRect.right - lastRect.right + lastRect.width / 2;
        const trackWidth = containerRect.width - leftOffset - rightOffset;

        // Update track positioning
        const track = this.progressLine.parentElement!;
        track.style.left = `${leftOffset}px`;
        track.style.right = `${rightOffset}px`;
        track.style.width = `${trackWidth}px`;
    }

    private goToStep(step: number) {
        if (step < 1 || step > this.totalSteps) return;
        this.currentStep = step;
        this.updateUI();
        this.container.dispatchEvent(new CustomEvent('stepper:step-enter', { detail: { step }, bubbles: true }));
    }

    private updateUI() {
        // 1. Update Progress Line
        const progressPercent = ((this.currentStep - 1) / (this.totalSteps - 1)) * 100;
        this.progressLine.style.width = `${progressPercent}%`;

        // 2. Update Circles & Content
        this.container.querySelectorAll('.step-indicator').forEach((el, idx) => {
            const stepIdx = idx + 1;
            const circle = el.querySelector('.step-circle')!;
            const dot = el.querySelector('.step-dot')!;
            const pane = document.getElementById(`step-content-${stepIdx}`)!;

            // Reset classes
            circle.className = "step-circle w-7 h-7 rounded-full border-2 flex items-center justify-center transition-all duration-300";
            dot.classList.add('scale-0');
            pane.classList.add('hidden');

            if (stepIdx < this.currentStep) {
                // Completed
                circle.classList.add('bg-primary', 'border-primary');
            } else if (stepIdx === this.currentStep) {
                // Current
                circle.classList.add('bg-background', 'border-primary', 'ring-4', 'ring-primary/10');
                dot.classList.remove('scale-0');
                dot.classList.add('bg-primary');
                pane.classList.remove('hidden');
            } else {
                // Future
                circle.classList.add('bg-background', 'border-text/30');
            }
        });

        // 3. Button Logic
        this.prevBtn.classList.toggle('hidden', this.currentStep === 1);

        if (this.currentStep === this.totalSteps) {
            this.nextBtn.innerText = `Create ${this.entityName}`;
            this.nextBtn.classList.replace('bg-text', 'bg-primary');
        } else {
            this.nextBtn.innerText = 'Next >';
            this.nextBtn.classList.replace('bg-primary', 'bg-text');
        }
    }
}

// Initialize on load
document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('dynamic-stepper')) {
        new Stepper('dynamic-stepper');
    }
});