import { getSurveyStrings } from "../../i18n/survey";

const t = getSurveyStrings();

class DeleteModal {
    private modal = document.getElementById("deleteModal")!;
    private nameEl = document.getElementById("deleteItemName")!;
    private confirmBtn = document.getElementById("confirmDelete") as HTMLButtonElement;
    private toast = document.getElementById("toast")!;

    private currentId: string | null = null;
    private endpoint: string | null = null;

    constructor() {
        this.bindButtons();
        this.bindModal();
    }

    private bindButtons() {
        document.querySelectorAll("button[data-delete-id]").forEach(btn => {
            btn.addEventListener("click", (e) => {
                const el = e.currentTarget as HTMLButtonElement;

                this.currentId = el.dataset.deleteId!;
                this.endpoint = el.dataset.deleteEndpoint;

                const name = el.dataset.deleteName || "this item";

                this.nameEl.textContent = name;

                this.open();
            });
        });
    }

    private bindModal() {
        document.getElementById("cancelDelete")!.addEventListener("click", () => {
            this.close();
        });

        this.confirmBtn.addEventListener("click", async () => {
            if (!this.currentId || !this.endpoint) return;

            await this.deleteItem(this.endpoint, this.currentId);
        });

        this.modal.addEventListener("click", (e) => {
            if (e.target === this.modal) this.close();
        });
    }

    private async deleteItem(endpoint: string, id: string) {
        const token = (document.querySelector(
            "input[name='__RequestVerificationToken']"
        ) as HTMLInputElement).value;

        try {
            const res = await fetch(endpoint, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token
                },
                body: JSON.stringify(id)
            });

            if (!res.ok) {
                this.showToast(await res.text() || t.deleteFailed, false);
                return;
            }

            // remove row if exists (table case)
            document.querySelector(`[data-row-id="${id}"]`)
                ?.closest("tr")
                ?.remove();

            this.showToast(t.deletedSuccessfully, true);
            this.close();

        } catch {
            this.showToast(t.networkError, false);
        }
    }

    private open() {
        this.modal.classList.remove("hidden");
        this.modal.classList.add("flex");
    }

    private close() {
        this.modal.classList.add("hidden");
        this.modal.classList.remove("flex");
        this.currentId = null;
        this.endpoint = null;
    }

    private showToast(message: string, success: boolean) {
        this.toast.textContent = message;

        this.toast.className =
            `fixed bottom-5 right-5 rounded-md px-4 py-2 text-white shadow-lg ` +
            (success ? "bg-green-600" : "bg-red-600");

        this.toast.classList.remove("hidden");

        setTimeout(() => this.toast.classList.add("hidden"), 3000);
    }
}

document.addEventListener("DOMContentLoaded", () => {
    if (document.getElementById("deleteModal")) {
        new DeleteModal();
    }
});