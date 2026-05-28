class PasswordVisibilityToggle {
    private readonly toggleButtons = Array.from(document.querySelectorAll<HTMLButtonElement>("[data-password-toggle]"));

    constructor() {
        if (this.toggleButtons.length === 0) {
            return;
        }

        this.bindEvents();
    }

    private bindEvents() {
        this.toggleButtons.forEach((button) => {
            button.addEventListener("click", () => this.toggle(button));
        });
    }

    private toggle(button: HTMLButtonElement) {
        const targetId = button.dataset.passwordToggle;
        if (!targetId) {
            return;
        }

        const input = document.getElementById(targetId) as HTMLInputElement | null;
        if (!input) {
            return;
        }

        const isPassword = input.type === "password";
        input.type = isPassword ? "text" : "password";
        button.setAttribute("aria-label", isPassword ? "Hide password" : "Show password");

        const showIcon = button.querySelector<SVGElement>("[data-password-icon='show']");
        const hideIcon = button.querySelector<SVGElement>("[data-password-icon='hide']");

        showIcon?.classList.toggle("hidden", isPassword);
        hideIcon?.classList.toggle("hidden", !isPassword);
    }
}

document.addEventListener("DOMContentLoaded", () => {
    new PasswordVisibilityToggle();
});
