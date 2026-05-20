class WorkspaceModalForms {
    constructor() {
        this.bindModal("workspaceCreateModal", ["closeWorkspaceCreateModal", "closeWorkspaceCreateModalSecondary"]);
        this.bindModal("workspaceEditModal", ["closeWorkspaceEditModal", "closeWorkspaceEditModalSecondary"]);
        this.bindModal("workspaceEditFromDetailsModal", ["closeWorkspaceEditFromDetailsModal", "closeWorkspaceEditFromDetailsModalSecondary"]);
        this.bindModal("workspaceAdminCreateModal", ["closeWorkspaceAdminCreateModal", "closeWorkspaceAdminCreateModalSecondary"]);
        this.bindModal("workspaceAdminEditModal", ["closeWorkspaceAdminEditModal", "closeWorkspaceAdminEditModalSecondary"]);
        this.bindOpeners();
        this.bindWorkspaceAdminValidation("workspaceAdminCreateForm", "workspaceAdminCreateEmailInput", "workspaceAdminCreatePhoneInput", "workspaceAdminCreateServerError");
        this.bindWorkspaceAdminValidation("workspaceAdminEditForm", "workspaceAdminEditEmailInput", "workspaceAdminEditPhoneInput", "workspaceAdminEditServerError");
        this.openWorkspaceAdminModalOnLoad();
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
        document.querySelectorAll<HTMLElement>("[data-open-workspace-create-modal='true']").forEach((element) => {
            element.addEventListener("click", (event) => {
                event.preventDefault();
                this.openById("workspaceCreateModal");
            });
        });

        document.querySelectorAll<HTMLElement>("[data-open-workspace-edit-modal='true']").forEach((element) => {
            element.addEventListener("click", (event) => {
                event.preventDefault();
                const workspaceId = element.dataset.workspaceId ?? "";
                const workspaceName = element.dataset.workspaceName ?? "";
                this.prepareWorkspaceEdit(workspaceId, workspaceName);
            });
        });

        const workspaceAdminCreateButton = document.getElementById("openWorkspaceAdminCreateModal");
        workspaceAdminCreateButton?.addEventListener("click", (event) => {
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
                const workspaceAdminId = element.dataset.workspaceAdminId ?? "";
                const workspaceAdminEmail = element.dataset.workspaceAdminEmail ?? "";
                const workspaceAdminUsername = element.dataset.workspaceAdminUsername ?? "";
                const workspaceAdminPhone = element.dataset.workspaceAdminPhone ?? "";
                this.prepareWorkspaceAdminEdit(workspaceAdminId, workspaceAdminEmail, workspaceAdminUsername, workspaceAdminPhone);
            });
        });
    }

    private openWorkspaceAdminModalOnLoad() {
        const root = document.querySelector<HTMLElement>("[data-open-workspace-admin-modal]");
        if (!root || root.dataset.openWorkspaceAdminModal !== "true") {
            return;
        }

        document.querySelector<HTMLElement>("[data-modal-key='workspace-admin-create']")?.click();
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

    private prepareWorkspaceEdit(workspaceId: string, workspaceName: string) {
        const editModalId = document.getElementById("workspaceEditModal") ? "workspaceEditModal" : "workspaceEditFromDetailsModal";
        const formId = editModalId === "workspaceEditModal" ? "workspaceEditForm" : "workspaceEditFromDetailsForm";
        const nameInputId = editModalId === "workspaceEditModal" ? "workspaceEditNameInput" : "workspaceEditFromDetailsNameInput";
        const idInputId = editModalId === "workspaceEditModal" ? "workspaceEditIdInput" : "workspaceEditFromDetailsIdInput";

        const form = document.getElementById(formId) as HTMLFormElement | null;
        if (!form) {
            return;
        }

        const actionTemplate = form.dataset.actionTemplate ?? "";
        if (actionTemplate) {
            form.action = actionTemplate.replace("__workspaceId__", workspaceId);
        }

        const nameInput = document.getElementById(nameInputId) as HTMLInputElement | null;
        if (nameInput) {
            nameInput.value = workspaceName;
        }

        const idInput = document.getElementById(idInputId) as HTMLInputElement | null;
        if (idInput) {
            idInput.value = workspaceId;
        }

        this.openById(editModalId);
    }

    private prepareWorkspaceAdminEdit(workspaceAdminId: string, workspaceAdminEmail: string, workspaceAdminUsername: string, workspaceAdminPhone: string) {
        const form = document.getElementById("workspaceAdminEditForm") as HTMLFormElement | null;
        if (!form) {
            return;
        }

        const actionTemplate = form.dataset.actionTemplate ?? "";
        if (actionTemplate) {
            form.action = actionTemplate.replace("__workspaceAdminId__", workspaceAdminId);
        }

        const adminIdInput = document.getElementById("workspaceAdminEditIdInput") as HTMLInputElement | null;
        if (adminIdInput) {
            adminIdInput.value = workspaceAdminId;
        }

        const adminEmailInput = document.getElementById("workspaceAdminEditEmailInput") as HTMLInputElement | null;
        if (adminEmailInput) {
            adminEmailInput.value = workspaceAdminEmail;
        }

        const adminUsernameInput = document.getElementById("workspaceAdminEditUsernameInput") as HTMLInputElement | null;
        if (adminUsernameInput) {
            adminUsernameInput.value = workspaceAdminUsername;
        }

        const adminPhoneInput = document.getElementById("workspaceAdminEditPhoneInput") as HTMLInputElement | null;
        if (adminPhoneInput) {
            adminPhoneInput.value = workspaceAdminPhone;
        }

        this.openById("workspaceAdminEditModal");
    }

    private bindWorkspaceAdminValidation(formId: string, emailId: string, phoneId: string, summaryId: string) {
        const form = document.getElementById(formId) as HTMLFormElement | null;
        if (!form) {
            return;
        }

        form.addEventListener("submit", async (event) => {
            event.preventDefault();
            const emailInput = document.getElementById(emailId) as HTMLInputElement | null;
            const phoneInput = document.getElementById(phoneId) as HTMLInputElement | null;
            const summary = document.getElementById(summaryId);

            this.clearWorkspaceAdminErrors(form, summary);

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
            this.renderWorkspaceAdminErrors(form, summary, payload?.errors);
        });
    }

    private clearWorkspaceAdminErrors(form: HTMLFormElement, summary: HTMLElement | null) {
        form.querySelectorAll<HTMLElement>("[data-valmsg-for]").forEach((element) => {
            element.textContent = "";
        });

        if (summary) {
            summary.textContent = "";
            summary.classList.add("hidden");
        }
    }

    private renderWorkspaceAdminErrors(
        form: HTMLFormElement,
        summary: HTMLElement | null,
        errors: Record<string, string[]> | undefined
    ) {
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
            if (!text) {
                return;
            }

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
            if (event.key !== "Escape") {
                return;
            }

            document.querySelectorAll<HTMLElement>(".fixed.inset-0.z-50").forEach((modal) => {
                if (!modal.classList.contains("hidden")) {
                    this.close(modal);
                }
            });
        });
    }

    private openById(modalId: string) {
        const modal = document.getElementById(modalId);
        if (!modal) {
            return;
        }

        modal.classList.remove("hidden");
        modal.classList.add("flex");
    }

    private close(modal: HTMLElement) {
        modal.classList.add("hidden");
        modal.classList.remove("flex");
    }
}

document.addEventListener("DOMContentLoaded", () => {
    new WorkspaceModalForms();
});
