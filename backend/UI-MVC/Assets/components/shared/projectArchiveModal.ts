class ProjectArchiveModal {
    private modal = document.getElementById('archiveModal');
    private nameEl = document.getElementById('archiveItemName');
    private confirmBtn = document.getElementById('confirmArchive') as HTMLButtonElement | null;
    private copyModal = document.getElementById('copyModal');
    private copyNameEl = document.getElementById('copyItemName');
    private confirmCopyBtn = document.getElementById('confirmCopy') as HTMLButtonElement | null;

    private currentId: string | null = null;

    constructor() {
        this.bindButtons();
        this.bindModal();
        this.bindCopyModal();
    }

    private bindButtons() {
        document.querySelectorAll('button[data-archive-id]').forEach(btn => {
            btn.addEventListener('click', (event) => {
                const el = event.currentTarget as HTMLButtonElement;
                this.currentId = el.dataset.archiveId ?? null;
                if (this.nameEl) this.nameEl.textContent = el.dataset.archiveName || 'this project';
                this.open();
            });
        });

        document.querySelectorAll('button[data-copy-id]').forEach(btn => {
            btn.addEventListener('click', (event) => {
                const el = event.currentTarget as HTMLButtonElement;
                this.currentId = el.dataset.copyId ?? null;
                if (this.copyNameEl) this.copyNameEl.textContent = el.dataset.copyName || 'this project';
                this.openCopy();
            });
        });
    }

    private bindModal() {
        document.getElementById('cancelArchive')?.addEventListener('click', () => this.close());

        this.confirmBtn?.addEventListener('click', async () => {
            if (!this.currentId) return;
            await this.archive(this.currentId);
        });

        this.modal?.addEventListener('click', (event) => {
            if (event.target === this.modal) this.close();
        });
    }

    private bindCopyModal() {
        document.getElementById('cancelCopy')?.addEventListener('click', () => this.closeCopy());

        this.confirmCopyBtn?.addEventListener('click', () => {
            if (!this.currentId) return;
            window.location.href = `/admin/projects/new?copy=${this.currentId}`;
        });

        this.copyModal?.addEventListener('click', (event) => {
            if (event.target === this.copyModal) this.closeCopy();
        });
    }

    private async archive(id: string) {
        const token = (document.querySelector(
            "input[name='__RequestVerificationToken']"
        ) as HTMLInputElement)?.value;

        if (!token) return;

        const response = await fetch(`/admin/projects/${id}/archive`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            }
        });

        if (!response.ok) return;

        window.location.reload();
    }

    private open() {
        this.modal?.classList.remove('hidden');
        this.modal?.classList.add('flex');
    }

    private close() {
        this.modal?.classList.add('hidden');
        this.modal?.classList.remove('flex');
        this.currentId = null;
    }

    private openCopy() {
        this.copyModal?.classList.remove('hidden');
        this.copyModal?.classList.add('flex');
    }

    private closeCopy() {
        this.copyModal?.classList.add('hidden');
        this.copyModal?.classList.remove('flex');
        this.currentId = null;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('archiveModal')) {
        new ProjectArchiveModal();
    }
});
