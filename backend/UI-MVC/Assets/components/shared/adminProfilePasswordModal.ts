class AdminProfilePasswordModal {
    private readonly modal = document.getElementById("changePasswordModal");
    private readonly openButton = document.getElementById("openChangePasswordModal");
    private readonly cancelButton = document.getElementById("cancelChangePassword");
    private readonly form = document.getElementById("changePasswordForm") as HTMLFormElement | null;
    private readonly info = document.getElementById("passwordModalInfo");
    private readonly error = document.getElementById("passwordModalError");
    private readonly submitButton = this.form?.querySelector<HTMLButtonElement>('button[type="submit"]');
    private readonly newPasswordInput = document.getElementById("newPassword") as HTMLInputElement | null;
    private readonly checklist = document.getElementById("passwordChecklist");

    constructor() {
        if (!this.modal || !this.openButton || !this.cancelButton || !this.form || !this.info || !this.error) {
            return;
        }

        this.bindEvents();
    }

    private bindEvents() {
        this.openButton?.addEventListener("click", () => this.open());
        this.cancelButton?.addEventListener("click", () => this.close());

        this.modal?.addEventListener("click", (event) => {
            if (event.target === this.modal) {
                this.close();
            }
        });

        document.addEventListener("keydown", (event) => {
            if (event.key === "Escape" && this.modal && !this.modal.classList.contains("hidden")) {
                this.close();
            }
        });

        this.form?.addEventListener("submit", (event) => {
            event.preventDefault();

            void this.submitForm();
        });

        this.newPasswordInput?.addEventListener("input", (event) => {
            const value = (event.target as HTMLInputElement | null)?.value ?? "";
            this.updateChecklist(value);
        });
    }

    private async submitForm() {
        if (!this.form) {
            return;
        }

        const currentPassword = (document.getElementById("currentPassword") as HTMLInputElement | null)?.value ?? "";
        const newPassword = (document.getElementById("newPassword") as HTMLInputElement | null)?.value ?? "";
        const confirmPassword = (document.getElementById("confirmPassword") as HTMLInputElement | null)?.value ?? "";

        if (!currentPassword || !newPassword || !confirmPassword) {
            this.showError("Fill in all password fields.");
            return;
        }

        if (newPassword !== confirmPassword) {
            this.showError("New password and confirmation do not match.");
            return;
        }

        this.error?.classList.add("hidden");
        this.info?.classList.add("hidden");

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
                const errorMessage = this.resolveErrorMessage(payload);
                this.showError(errorMessage);
                return;
            }

            this.info!.textContent = "Password updated.";
            this.info!.classList.remove("hidden");
            this.close();
        } catch {
            this.showError("Password update failed. Try again.");
        } finally {
            this.setSubmitting(false);
        }
    }

    private open() {
        this.error?.classList.add("hidden");
        this.form?.reset();
        this.info?.classList.add("hidden");
        this.updateChecklist("");
        this.modal?.classList.remove("hidden");
        this.modal?.classList.add("flex");
    }

    private close() {
        this.modal?.classList.add("hidden");
        this.modal?.classList.remove("flex");
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

    private setSubmitting(isSubmitting: boolean) {
        if (this.submitButton) {
            this.submitButton.disabled = isSubmitting;
        }
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
}

document.addEventListener("DOMContentLoaded", () => {
    if (document.getElementById("changePasswordModal")) {
        new AdminProfilePasswordModal();
    }
});
