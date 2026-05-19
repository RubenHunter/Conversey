class AdminProfilePasswordModal {
    private readonly modal = document.getElementById("changePasswordModal");
    private readonly openButton = document.getElementById("openChangePasswordModal");
    private readonly cancelButton = document.getElementById("cancelChangePassword");
    private readonly form = document.getElementById("changePasswordForm") as HTMLFormElement | null;
    private readonly info = document.getElementById("passwordModalInfo");
    private readonly error = document.getElementById("passwordModalError");

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
            this.info!.textContent = "Password form ready. Backend endpoint not connected yet.";
            this.info!.classList.remove("hidden");
            this.close();
        });
    }

    private open() {
        this.error?.classList.add("hidden");
        this.form?.reset();
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
}

document.addEventListener("DOMContentLoaded", () => {
    if (document.getElementById("changePasswordModal")) {
        new AdminProfilePasswordModal();
    }
});
