class AdminManagementModals {
    constructor() {
        this.bindModal("converseyAdminCreateModal", ["closeConverseyAdminCreateModal", "closeConverseyAdminCreateModalSecondary"]);
        this.bindModal("converseyAdminEditModal", ["closeConverseyAdminEditModal", "closeConverseyAdminEditModalSecondary"]);
        this.bindOpeners();
        this.bindValidation("converseyAdminCreateForm", "converseyAdminCreateEmailInput", "converseyAdminCreatePhoneInput", "converseyAdminCreateServerError");
        this.bindValidation("converseyAdminEditForm", "converseyAdminEditEmailInput", "converseyAdminEditPhoneInput", "converseyAdminEditServerError");
        this.openAdminModalOnLoad();
        this.bindOneTimePasswordModal();
        this.bindEsc();
    }

    private bindModal(modalId: string, closeButtonIds: string[]) {
        const modal = document.getElementById(modalId);
        if (!modal) {
            return;
        }

        closeButtonIds.forEach((buttonId) => {
            document.getElementById(buttonId)?.addEventListener("click", () => this.close(modal));
        });

        modal.addEventListener("click", (event) => {
            if (event.target === modal) {
                this.close(modal);
            }
        });
    }

    private bindOpeners() {
        const createButton = document.getElementById("openConverseyAdminCreateModal");
        createButton?.addEventListener("click", (event) => {
            event.preventDefault();
            this.openById("converseyAdminCreateModal");
        });

        document.querySelectorAll<HTMLElement>("[data-modal-key='conversey-admin-create']").forEach((element) => {
            element.addEventListener("click", (event) => {
                event.preventDefault();
                this.openById("converseyAdminCreateModal");
            });
        });

        document.querySelectorAll<HTMLElement>("[data-open-conversey-admin-edit-modal='true']").forEach((element) => {
            element.addEventListener("click", (event) => {
                event.preventDefault();
                const adminId = element.dataset.converseyAdminId ?? "";
                const adminEmail = element.dataset.converseyAdminEmail ?? "";
                const adminUsername = element.dataset.converseyAdminUsername ?? "";
                const adminPhone = element.dataset.converseyAdminPhone ?? "";
                this.prepareEdit(adminId, adminEmail, adminUsername, adminPhone);
            });
        });
    }

    private openAdminModalOnLoad() {
        // Not used explicitly by queryparam right now, but left for future generic logic
    }

    private bindOneTimePasswordModal() {
        const modal = document.getElementById("oneTimePasswordModal");
        if (!modal) {
            return;
        }

        this.openById("oneTimePasswordModal");

        const copyButton = document.getElementById("copyOneTimePassword") as HTMLButtonElement | null;
        const passwordValue = document.getElementById("oneTimePasswordValue") as HTMLElement | null;
        const copiedMessage = document.getElementById("oneTimePasswordCopied");
        const closeButton = document.getElementById("closeOneTimePasswordModal");

        copyButton?.addEventListener("click", async () => {
            const password = passwordValue?.textContent ?? "";
            if (!password) {
                return;
            }

            try {
                await navigator.clipboard.writeText(password);
                copiedMessage?.classList.remove("hidden");
            } catch {
                copiedMessage?.classList.remove("hidden");
                copiedMessage!.textContent = "Copy failed. Select and copy manually.";
            }
        });

        closeButton?.addEventListener("click", () => {
            this.close(modal);
        });
    }

    private prepareEdit(adminId: string, adminEmail: string, adminUsername: string, adminPhone: string) {
        const form = document.getElementById("converseyAdminEditForm") as HTMLFormElement | null;
        if (!form) {
            return;
        }

        const actionTemplate = form.dataset.actionTemplate ?? "";
        if (actionTemplate) {
            form.action = actionTemplate.replace("__converseyAdminId__", adminId);
        }

        const idInput = document.getElementById("converseyAdminEditIdInput") as HTMLInputElement | null;
        if (idInput) idInput.value = adminId;

        const emailInput = document.getElementById("converseyAdminEditEmailInput") as HTMLInputElement | null;
        if (emailInput) emailInput.value = adminEmail;

        const usernameInput = document.getElementById("converseyAdminEditUsernameInput") as HTMLInputElement | null;
        if (usernameInput) usernameInput.value = adminUsername;

        const phoneInput = document.getElementById("converseyAdminEditPhoneInput") as HTMLInputElement | null;
        if (phoneInput) phoneInput.value = adminPhone;

        this.openById("converseyAdminEditModal");
    }

    private bindValidation(formId: string, emailId: string, phoneId: string, summaryId: string) {
        const form = document.getElementById(formId) as HTMLFormElement | null;
        if (!form) return;

        form.addEventListener("submit", async (event) => {
            event.preventDefault();
            const emailInput = document.getElementById(emailId) as HTMLInputElement | null;
            const phoneInput = document.getElementById(phoneId) as HTMLInputElement | null;
            const summary = document.getElementById(summaryId);

            this.clearErrors(form, summary);

            if (emailInput) {
                emailInput.setCustomValidity("");
                if (!emailInput.checkValidity()) {
                    emailInput.setCustomValidity("Enter valid email address.");
                }
            }

            if (phoneInput) {
                phoneInput.setCustomValidity("");
                const phoneValue = phoneInput.value.trim();
                const phoneRegex = /^[+]?[\d\s\-().]{7,20}$/;
                if (phoneValue.length > 0 && !phoneRegex.test(phoneValue)) {
                    phoneInput.setCustomValidity("Enter valid phone number.");
                }
            }

            if (!form.checkValidity()) {
                form.reportValidity();
                return;
            }

            const response = await fetch(form.action, {
                method: (form.method || "POST").toUpperCase(),
                body: new FormData(form),
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            if (response.ok) {
                const payload = await response.json().catch(() => null) as { redirectUrl?: string } | null;
                window.location.href = payload?.redirectUrl ?? window.location.href;
                return;
            }

            const payload = await response.json().catch(() => null) as { errors?: Record<string, string[]> } | null;
            this.renderErrors(form, summary, payload?.errors);
        });
    }

    private clearErrors(form: HTMLFormElement, summary: HTMLElement | null) {
        form.querySelectorAll<HTMLElement>("[data-valmsg-for]").forEach((element) => {
            element.textContent = "";
        });

        if (summary) {
            summary.textContent = "";
            summary.classList.add("hidden");
        }
    }

    private renderErrors(form: HTMLFormElement, summary: HTMLElement | null, errors: Record<string, string[]> | undefined) {
        if (!errors) {
            if (summary) {
                summary.textContent = "Something went wrong. Please try again.";
                summary.classList.remove("hidden");
            }
            return;
        }

        const generalErrors: string[] = [];

        Object.entries(errors).forEach(([key, messages]) => {
            const text = messages.join(" ").trim();
            if (!text) return;

            const fieldError = form.querySelector<HTMLElement>(`[data-valmsg-for='${key}']`);
            if (fieldError) {
                fieldError.textContent = text;
                return;
            }

            generalErrors.push(text);
        });

        if (generalErrors.length > 0 && summary) {
            summary.textContent = generalErrors.join(" ");
            summary.classList.remove("hidden");
        }
    }

    private bindEsc() {
        document.addEventListener("keydown", (event) => {
            if (event.key !== "Escape") return;
            document.querySelectorAll<HTMLElement>(".fixed.inset-0.z-50").forEach((modal) => {
                if (!modal.classList.contains("hidden")) {
                    this.close(modal);
                }
            });
        });
    }

    private openById(modalId: string) {
        const modal = document.getElementById(modalId);
        if (!modal) return;
        modal.classList.remove("hidden");
        modal.classList.add("flex");
    }

    private close(modal: HTMLElement) {
        modal.classList.add("hidden");
        modal.classList.remove("flex");
    }
}

document.addEventListener("DOMContentLoaded", () => {
    new AdminManagementModals();
});
