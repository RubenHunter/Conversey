class WorkspaceModalForms {
    private readonly imageUploadInFlight = new Map<HTMLFormElement, Promise<string | null>>();

    constructor() {
        this.bindModal("workspaceCreateModal", ["closeWorkspaceCreateModal", "closeWorkspaceCreateModalSecondary"]);
        this.bindModal("workspaceEditModal", ["closeWorkspaceEditModal", "closeWorkspaceEditModalSecondary"]);
        this.bindModal("workspaceEditFromDetailsModal", ["closeWorkspaceEditFromDetailsModal", "closeWorkspaceEditFromDetailsModalSecondary"]);
        this.bindModal("workspaceAdminCreateModal", ["closeWorkspaceAdminCreateModal", "closeWorkspaceAdminCreateModalSecondary"]);
        this.bindModal("workspaceAdminEditModal", ["closeWorkspaceAdminEditModal", "closeWorkspaceAdminEditModalSecondary"]);
        this.bindOpeners();
        this.bindWorkspaceImageUploads();
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
                const workspaceImageUrl = element.dataset.workspaceImageUrl ?? "";
                this.prepareWorkspaceEdit(workspaceId, workspaceName, workspaceImageUrl);
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

    private bindWorkspaceImageUploads() {
        document.querySelectorAll<HTMLFormElement>("form[data-image-upload-url]").forEach((form) => {
            const fileInput = form.querySelector<HTMLInputElement>("[data-workspace-image-file]");
            if (!fileInput) {
                return;
            }

            fileInput.addEventListener("change", () => {
                void this.uploadWorkspaceImageIfNeeded(form, fileInput);
            });

            form.addEventListener("submit", async (event) => {
                const selectedFile = fileInput.files?.[0];
                if (!selectedFile) {
                    return;
                }

                const signatureInput = form.querySelector<HTMLInputElement>("[data-workspace-image-signature]");
                const imageUrlInput = form.querySelector<HTMLInputElement>("[data-workspace-image-url]");
                const existingSignature = signatureInput?.value ?? "";
                const existingUrl = imageUrlInput?.value.trim() ?? "";
                const currentSignature = this.createFileSignature(selectedFile);

                if (currentSignature === existingSignature && existingUrl.length > 0) {
                    return;
                }

                event.preventDefault();

                const imageUrl = await this.uploadWorkspaceImageIfNeeded(form, fileInput);
                if (imageUrl === null) {
                    return;
                }

                form.submit();
            });
        });
    }

    private async uploadWorkspaceImageIfNeeded(form: HTMLFormElement, fileInput: HTMLInputElement): Promise<string | null> {
        const existing = this.imageUploadInFlight.get(form);
        if (existing) {
            return existing;
        }

        const task = this.executeWorkspaceImageUpload(form, fileInput);
        this.imageUploadInFlight.set(form, task);

        try {
            return await task;
        } finally {
            this.imageUploadInFlight.delete(form);
        }
    }

    private async executeWorkspaceImageUpload(form: HTMLFormElement, fileInput: HTMLInputElement): Promise<string | null> {
        const selectedFile = fileInput.files?.[0];
        const imageUrlInput = form.querySelector<HTMLInputElement>("[data-workspace-image-url]");
        const signatureInput = form.querySelector<HTMLInputElement>("[data-workspace-image-signature]");
        const statusEl = form.querySelector<HTMLElement>("[data-workspace-image-status]");
        const errorEl = form.querySelector<HTMLElement>("[data-workspace-image-error]");

        if (!selectedFile) {
            if (errorEl) errorEl.textContent = "";
            if (statusEl) statusEl.textContent = "";
            return imageUrlInput?.value.trim() ?? "";
        }

        const uploadUrl = form.dataset.imageUploadUrl ?? "";
        if (!uploadUrl) {
            if (errorEl) errorEl.textContent = "Image upload endpoint missing.";
            return null;
        }

        const antiForgeryToken = form.querySelector<HTMLInputElement>('input[name="__RequestVerificationToken"]')?.value;
        if (!antiForgeryToken) {
            if (errorEl) errorEl.textContent = "Security token missing. Refresh page and retry.";
            return null;
        }

        const signature = this.createFileSignature(selectedFile);
        const existingSignature = signatureInput?.value ?? "";
        const existingUrl = imageUrlInput?.value.trim() ?? "";
        if (signature === existingSignature && existingUrl.length > 0) {
            if (errorEl) errorEl.textContent = "";
            if (statusEl) statusEl.textContent = "Image already uploaded.";
            return existingUrl;
        }

        if (errorEl) errorEl.textContent = "";
        if (statusEl) statusEl.textContent = "Uploading image...";

        const payload = new FormData();
        payload.append("imageFile", selectedFile);
        payload.append("__RequestVerificationToken", antiForgeryToken);

        try {
            const response = await fetch(uploadUrl, {
                method: "POST",
                credentials: "same-origin",
                body: payload
            });

            const result = await this.parseUploadResponse(response);
            if (!response.ok) {
                if (errorEl) errorEl.textContent = result.errorMessage;
                if (statusEl) statusEl.textContent = "";
                return null;
            }

            if (!result.imageUrl) {
                if (errorEl) errorEl.textContent = "Upload succeeded but no image URL returned.";
                if (statusEl) statusEl.textContent = "";
                return null;
            }

            if (imageUrlInput) imageUrlInput.value = result.imageUrl;
            if (signatureInput) signatureInput.value = signature;
            if (statusEl) statusEl.textContent = "Image uploaded.";
            return result.imageUrl;
        } catch {
            if (errorEl) errorEl.textContent = "Image upload failed. Check connection and retry.";
            if (statusEl) statusEl.textContent = "";
            return null;
        }
    }

    private async parseUploadResponse(response: Response): Promise<{ imageUrl: string; errorMessage: string }> {
        let parsed: { imageUrl?: unknown; error?: unknown } | null = null;
        try {
            parsed = (await response.json()) as { imageUrl?: unknown; error?: unknown };
        } catch {
            parsed = null;
        }

        const imageUrl = parsed && typeof parsed.imageUrl === "string" ? parsed.imageUrl : "";
        const errorMessage = parsed && typeof parsed.error === "string"
            ? parsed.error
            : `Image upload failed (${response.status}).`;

        return { imageUrl, errorMessage };
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

    private prepareWorkspaceEdit(workspaceId: string, workspaceName: string, workspaceImageUrl: string) {
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

        const imageUrlInput = form.querySelector<HTMLInputElement>("[data-workspace-image-url]");
        if (imageUrlInput) {
            imageUrlInput.value = workspaceImageUrl;
        }

        const signatureInput = form.querySelector<HTMLInputElement>("[data-workspace-image-signature]");
        if (signatureInput) {
            signatureInput.value = "";
        }

        const statusEl = form.querySelector<HTMLElement>("[data-workspace-image-status]");
        if (statusEl) {
            statusEl.textContent = workspaceImageUrl ? "Current image loaded." : "";
        }

        const errorEl = form.querySelector<HTMLElement>("[data-workspace-image-error]");
        if (errorEl) {
            errorEl.textContent = "";
        }

        this.openById(editModalId);
    }

    private createFileSignature(file: File): string {
        return `${file.name}:${file.size}:${file.lastModified}`;
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
                const payload = await response.json().catch(() => null) as { redirectUrl?: string; emailSent?: boolean } | null;
                if (payload?.emailSent && summary) {
                    summary.textContent = "Email with login instructions sent successfully.";
                    summary.classList.remove("hidden");
                    summary.classList.add("text-emerald-600", "bg-emerald-50", "rounded-xl", "px-3", "py-2");
                }
                setTimeout(() => {
                    window.location.href = payload?.redirectUrl ?? window.location.href;
                }, payload?.emailSent ? 1500 : 0);
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
