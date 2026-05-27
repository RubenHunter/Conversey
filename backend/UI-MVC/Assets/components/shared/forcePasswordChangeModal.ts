class ForcePasswordChangeModal {
    private readonly root = document.querySelector<HTMLElement>("[data-force-password-change]");
    private readonly modal = document.getElementById("forceChangePasswordModal");
    private readonly form = document.getElementById("forceChangePasswordForm") as HTMLFormElement | null;
    private readonly error = document.getElementById("forcePasswordError");
    private readonly checklist = document.getElementById("forcePasswordChecklist");
    private readonly submitButton = this.form?.querySelector<HTMLButtonElement>('button[type="submit"]');

    constructor() {
        if (!this.root || this.root.dataset.forcePasswordChange !== "true") {
            return;
        }

        if (!this.modal || !this.form || !this.error) {
            return;
        }

        this.open();
        this.bindEvents();
    }

    private bindEvents() {
        const newPasswordInput = document.getElementById("forceNewPassword") as HTMLInputElement | null;
        newPasswordInput?.addEventListener("input", (event) => {
            const value = (event.target as HTMLInputElement | null)?.value ?? "";
            this.updateChecklist(value);
        });

        this.form?.addEventListener("submit", (event) => {
            event.preventDefault();
            void this.submitForm();
        });
    }

    private open() {
        this.modal?.classList.remove("hidden");
        this.modal?.classList.add("flex");
        this.updateChecklist("");
    }

    private async submitForm() {
        if (!this.form) {
            return;
        }

        const newPassword = (document.getElementById("forceNewPassword") as HTMLInputElement | null)?.value ?? "";
        const confirmPassword = (document.getElementById("forceConfirmPassword") as HTMLInputElement | null)?.value ?? "";

        if (!newPassword || !confirmPassword) {
            this.showError("Fill in all password fields.");
            return;
        }

        if (newPassword !== confirmPassword) {
            this.showError("New password and confirmation do not match.");
            return;
        }

        this.error?.classList.add("hidden");

        try {
            this.setSubmitting(true);
            const response = await fetch(this.form.action, {
                method: "POST",
                headers: {
                    "RequestVerificationToken": this.getToken() ?? ""
                },
                body: new FormData(this.form)
            });

            if (!response.ok) {
                const payload = await response.json().catch(() => null);
                const message = this.resolveErrorMessage(payload);
                this.showError(message);
                return;
            }

            window.location.reload();
        } catch {
            this.showError("Password update failed. Try again.");
        } finally {
            this.setSubmitting(false);
        }
    }

    private showError(message: string) {
        if (!this.error) {
            return;
        }

        this.error.textContent = message;
        this.error.classList.remove("hidden");
    }

    private getToken(): string | null {
        return this.form?.querySelector<HTMLInputElement>("input[name='__RequestVerificationToken']")?.value ?? null;
    }

    private resolveErrorMessage(payload: any): string {
        if (!payload) {
            return "Password update failed.";
        }

        if (typeof payload.message === "string" && payload.message.trim().length > 0) {
            return payload.message;
        }

        if (Array.isArray(payload.errors) && payload.errors.length > 0) {
            return payload.errors.join(" ");
        }

        if (payload.errors && typeof payload.errors === "object") {
            const messages = Object.values(payload.errors).flat().filter((value) => typeof value === "string");
            if (messages.length > 0) {
                return messages.join(" ");
            }
        }

        return "Password update failed.";
    }

    private updateChecklist(password: string) {
        if (!this.checklist) {
            return;
        }

        const checks = {
            length: password.length >= 6,
            lowercase: /[a-z]/.test(password),
            uppercase: /[A-Z]/.test(password),
            digit: /\d/.test(password),
            symbol: /[^A-Za-z0-9]/.test(password)
        };

        Object.entries(checks).forEach(([key, met]) => {
            const item = this.checklist?.querySelector<HTMLLIElement>(`li[data-requirement='${key}']`);
            if (!item) {
                return;
            }

            if (met) {
                item.classList.remove("text-accent");
                item.classList.add("text-text/70", "line-through");
            } else {
                item.classList.add("text-accent");
                item.classList.remove("text-text/70", "line-through");
            }
        });
    }

    private setSubmitting(isSubmitting: boolean) {
        if (this.submitButton) {
            this.submitButton.disabled = isSubmitting;
        }
    }
}

document.addEventListener("DOMContentLoaded", () => {
    new ForcePasswordChangeModal();
});
