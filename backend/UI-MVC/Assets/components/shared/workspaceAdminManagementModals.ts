class WorkspaceAdminManagementModals {
    constructor() {
        this.bindModal("workspaceAdminCreateModal", ["closeWorkspaceAdminCreateModal", "closeWorkspaceAdminCreateModalSecondary"]);
        this.bindModal("workspaceAdminEditModal", ["closeWorkspaceAdminEditModal", "closeWorkspaceAdminEditModalSecondary"]);
        this.bindOpeners();
        this.bindValidation("workspaceAdminCreateForm", "workspaceAdminCreateEmailInput", "workspaceAdminCreatePhoneInput", "workspaceAdminCreateServerError");
        this.bindValidation("workspaceAdminEditForm", "workspaceAdminEditEmailInput", "workspaceAdminEditPhoneInput", "workspaceAdminEditServerError");
        this.openAdminModalOnLoad();
        this.bindOneTimePasswordModal();
        this.bindEsc();
    }

    private bindModal(modalId: string, closeButtonIds: string[]) {
        const modal = document.getElementById(modalId);
        if (!modal) return;

        closeButtonIds.forEach((buttonId) => {
            document.getElementById(buttonId)?.addEventListener("click", () => this.close(modal));
        });

        modal.addEventListener("click", (event) => {
            if (event.target === modal) this.close(modal);
        });
    }

    private bindOpeners() {
        const createButton = document.getElementById("openWorkspaceAdminCreateModal");
        createButton?.addEventListener("click", (event) => {
            event.preventDefault();
            this.openById("workspaceAdminCreateModal");
        });

        document.querySelectorAll<HTMLElement>("[data-modal-key='workspace-admin-create']").forEach((element) => {
            element.addEventListener("click", (event) => {
                event.preventDefault();
                this.openById("workspaceAdminCreateModal");
            });
        });

        document.querySelectorAll<HTMLElement>("[data-open-workspace-admin-edit-modal='true']").forEach((element) => {
            element.addEventListener("click", (event) => {
                event.preventDefault();
                const adminId = element.dataset.workspaceAdminId ?? "";
                const adminEmail = element.dataset.workspaceAdminEmail ?? "";
                const adminUsername = element.dataset.workspaceAdminUsername ?? "";
                const adminPhone = element.dataset.workspaceAdminPhone ?? "";
                this.prepareEdit(adminId, adminEmail, adminUsername, adminPhone);
            });
        });
    }

    private openAdminModalOnLoad() {}

    private bindOneTimePasswordModal() {
        const modal = document.getElementById("oneTimePasswordModal");
        if (!modal) return;

        this.openById("oneTimePasswordModal");

        const copyButton = document.getElementById("copyOneTimePassword") as HTMLButtonElement | null;
        const passwordValue = document.getElementById("oneTimePasswordValue") as HTMLElement | null;
        const copiedMessage = document.getElementById("oneTimePasswordCopied");
        const closeButton = document.getElementById("closeOneTimePasswordModal");

        copyButton?.addEventListener("click", async () => {
            const password = passwordValue?.textContent ?? "";
            if (!password) return;
            try {
                await navigator.clipboard.writeText(password);
                copiedMessage?.classList.remove("hidden");
            } catch {
                copiedMessage?.classList.remove("hidden");
                copiedMessage!.textContent = "Copy failed. Select and copy manually.";
            }
        });

        closeButton?.addEventListener("click", () => this.close(modal));
    }

    private prepareEdit(adminId: string, adminEmail: string, adminUsername: string, adminPhone: string) {
        const form = document.getElementById("workspaceAdminEditForm") as HTMLFormElement | null;
        if (!form) return;

        const actionBase = "/admin/workspace/admins/edit/" + adminId;
        form.action = actionBase;

        const idInput = document.getElementById("workspaceAdminEditIdInput") as HTMLInputElement | null;
        if (idInput) idInput.value = adminId;

        const emailInput = document.getElementById("workspaceAdminEditEmailInput") as HTMLInputElement | null;
        if (emailInput) emailInput.value = adminEmail;

        const usernameInput = document.getElementById("workspaceAdminEditUsernameInput") as HTMLInputElement | null;
        if (usernameInput) usernameInput.value = adminUsername;

        const phoneInput = document.getElementById("workspaceAdminEditPhoneInput") as HTMLInputElement | null;
        if (phoneInput) phoneInput.value = adminPhone;

        this.openById("workspaceAdminEditModal");
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
                if (!modal.classList.contains("hidden")) this.close(modal);
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
    new WorkspaceAdminManagementModals();
});
