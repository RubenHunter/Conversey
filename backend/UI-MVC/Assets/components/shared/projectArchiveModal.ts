import {toCanvas} from "qrcode";

class ProjectArchiveModal {
    private modal = document.getElementById('archiveModal');
    private nameEl = document.getElementById('archiveItemName');
    private confirmBtn = document.getElementById('confirmArchive') as HTMLButtonElement | null;
    private copyModal = document.getElementById('copyModal');
    private copyNameEl = document.getElementById('copyItemName');
    private confirmCopyBtn = document.getElementById('confirmCopy') as HTMLButtonElement | null;
    private shareModal = document.getElementById('shareModal');
    private shareNameEl = document.getElementById('shareItemName');
    private shareStatusBadge = document.getElementById('shareStatusBadge');
    private shareAccessInfo = document.getElementById('shareAccessInfo');
    private shareUrlInput = document.getElementById('shareUrlInput') as HTMLInputElement | null;
    private copyShareUrlBtn = document.getElementById('copyShareUrl');
    private shareQrSection = document.getElementById('shareQrSection');
    private toggleQrBtn = document.getElementById('toggleQrBtn');
    private qrArrow = document.getElementById('qrArrow');
    private shareQrContainer = document.getElementById('shareQrContainer');
    private shareQrCanvas = document.getElementById('shareQrCanvas') as HTMLCanvasElement | null;
    private downloadQrBtn = document.getElementById('downloadQrBtn');

    private currentId: string | null = null;
    private qrGenerated = false;

    constructor() {
        this.bindButtons();
        this.bindModal();
        this.bindCopyModal();
        this.bindShareModal();
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

        document.querySelectorAll('button[data-share-id]').forEach(btn => {
            btn.addEventListener('click', (event) => {
                const el = event.currentTarget as HTMLButtonElement;
                this.currentId = el.dataset.shareId ?? null;
                if (this.shareNameEl) this.shareNameEl.textContent = el.dataset.shareName || '';
                const status = el.dataset.shareStatus ?? '';
                this.updateShareModal(status);
                this.openShare();
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

    private bindShareModal() {
        document.getElementById('cancelShare')?.addEventListener('click', () => this.closeShare());

        this.copyShareUrlBtn?.addEventListener('click', () => {
            if (!this.shareUrlInput) return;
            void navigator.clipboard.writeText(this.shareUrlInput.value);
            if (this.copyShareUrlBtn) {
                this.copyShareUrlBtn.textContent = 'Copied';
                setTimeout(() => {
                    if (this.copyShareUrlBtn) this.copyShareUrlBtn.textContent = 'Copy';
                }, 1500);
            }
        });

        this.toggleQrBtn?.addEventListener('click', () => {
            const collapsed = this.shareQrContainer?.classList.contains('grid-rows-[0fr]');
            if (collapsed) {
                this.shareQrContainer?.classList.replace('grid-rows-[0fr]', 'grid-rows-[1fr]');
                this.qrArrow?.classList.add('rotate-90');
            } else {
                this.shareQrContainer?.classList.replace('grid-rows-[1fr]', 'grid-rows-[0fr]');
                this.qrArrow?.classList.remove('rotate-90');
            }
        });

        this.downloadQrBtn?.addEventListener('click', () => {
            if (!this.shareQrCanvas) return;
            const link = document.createElement('a');
            link.download = `qr-${this.currentId ?? 'project'}.png`;
            link.href = this.shareQrCanvas.toDataURL('image/png');
            link.click();
        });

        this.shareModal?.addEventListener('click', (event) => {
            if (event.target === this.shareModal) this.closeShare();
        });
    }

    private updateShareModal(status: string) {
        const url = `${window.location.origin}/${this.currentId}`;
        if (this.shareUrlInput) this.shareUrlInput.value = url;

        if (this.shareStatusBadge) {
            this.shareStatusBadge.textContent = status;
            this.shareStatusBadge.className = 'text-[10px] uppercase tracking-widest font-bold px-2 py-1 rounded-full border ';

            switch (status) {
                case 'Active':
                    this.shareStatusBadge.classList.add('border-green-300', 'bg-green-50', 'text-green-700');
                    break;
                case 'Draft':
                    this.shareStatusBadge.classList.add('border-amber-300', 'bg-amber-50', 'text-amber-700');
                    break;
                case 'Archived':
                    this.shareStatusBadge.classList.add('border-gray-300', 'bg-gray-50', 'text-gray-500');
                    break;
                default:
                    this.shareStatusBadge.classList.add('border-secondary/20', 'bg-white/90', 'text-secondary');
            }
        }

        if (this.shareAccessInfo) {
            switch (status) {
                case 'Active':
                    this.shareAccessInfo.textContent = 'This survey is live. Anyone with the link can access it and submit responses.';
                    break;
                case 'Draft':
                    this.shareAccessInfo.textContent = 'This project is still a draft. Only workspace admins can see it. Publish the survey to make it publicly accessible.';
                    break;
                case 'Archived':
                    this.shareAccessInfo.textContent = 'This project has been archived and is no longer publicly accessible. Only workspace admins can view it.';
                    break;
                default:
                    this.shareAccessInfo.textContent = '';
            }
        }

        if (status === 'Active') {
            this.shareQrSection?.classList.remove('hidden');
            if (!this.qrGenerated && this.shareQrCanvas) {
                void this.generateQr(url);
            }
        } else {
            this.shareQrSection?.classList.add('hidden');
        }
    }
    
    private async generateQr(url: string) {
        if (!this.shareQrCanvas) return;
        toCanvas(this.shareQrCanvas, url, { width: 180, margin: 2 }, (error) => {
            if (!error) this.qrGenerated = true;
        });
    }

    private openShare() {
        this.shareModal?.classList.remove('hidden');
        this.shareModal?.classList.add('flex');
    }

    private closeShare() {
        this.shareModal?.classList.add('hidden');
        this.shareModal?.classList.remove('flex');
        this.shareQrContainer?.classList.replace('grid-rows-[1fr]', 'grid-rows-[0fr]');
        this.qrArrow?.classList.remove('rotate-90');
        this.currentId = null;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('archiveModal')) {
        new ProjectArchiveModal();
    }
});
