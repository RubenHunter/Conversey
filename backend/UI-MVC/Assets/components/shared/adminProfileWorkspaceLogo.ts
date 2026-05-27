class AdminProfileWorkspaceLogo {
    private readonly fileInput = document.getElementById("workspaceLogoFile") as HTMLInputElement | null
    private readonly preview = document.getElementById("workspaceLogoPreview")
    private readonly previewImg = document.getElementById("workspaceLogoImg") as HTMLImageElement | null
    private readonly hiddenUrl = document.getElementById("workspaceLogoUrl") as HTMLInputElement | null
    private readonly statusEl = document.getElementById("workspaceLogoStatus")
    private readonly errorEl = document.getElementById("workspaceLogoError")

    private readonly uploadUrl = "/admin/profile/workspace/upload-logo"

    constructor() {
        if (!this.fileInput || !this.preview || !this.hiddenUrl) {
            return
        }

        this.fileInput.addEventListener("change", () => {
            void this.handleFileChange()
        })
    }

    private async handleFileChange(): Promise<void> {
        const file = this.fileInput?.files?.[0]
        if (!file) return

        this.hideError()

        const token = this.getAntiForgeryToken()
        if (!token) {
            this.showError("Security token missing. Refresh page and retry.")
            return
        }

        const formData = new FormData()
        formData.append("imageFile", file)
        formData.append("__RequestVerificationToken", token)

        this.setStatus("Uploading image...")

        try {
            const response = await fetch(this.uploadUrl, {
                method: "POST",
                credentials: "same-origin",
                body: formData,
            })

            const result = await response.json().catch(() => null)

            if (!response.ok) {
                this.showError(result?.error ?? "Upload failed.")
                this.setStatus("")
                return
            }

            if (!result?.imageUrl) {
                this.showError("Upload succeeded but no image URL returned.")
                this.setStatus("")
                return
            }

            this.hiddenUrl!.value = result.imageUrl
            this.updatePreview(result.imageUrl)
            this.setStatus("Image uploaded.")
        } catch {
            this.showError("Image upload failed. Check connection and retry.")
            this.setStatus("")
        }
    }

    private updatePreview(imageUrl: string): void {
        if (!this.preview) return

        if (this.previewImg) {
            this.previewImg.src = imageUrl
        } else {
            this.preview.innerHTML = `<img src="${imageUrl}" alt="Workspace logo" class="w-full h-full object-cover" id="workspaceLogoImg" />`
        }
    }

    private getAntiForgeryToken(): string | null {
        return document.querySelector<HTMLInputElement>('input[name="__RequestVerificationToken"]')?.value ?? null
    }

    private setStatus(text: string): void {
        if (this.statusEl) {
            this.statusEl.textContent = text
        }
    }

    private showError(text: string): void {
        if (this.errorEl) {
            this.errorEl.textContent = text
            this.errorEl.classList.remove("hidden")
        }
    }

    private hideError(): void {
        if (this.errorEl) {
            this.errorEl.classList.add("hidden")
        }
    }
}

document.addEventListener("DOMContentLoaded", () => {
    if (document.getElementById("workspaceLogoFile")) {
        new AdminProfileWorkspaceLogo()
    }
})
