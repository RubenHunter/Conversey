interface TopicEntry {
    topicName: string;
    topicContext: string;
    maxBroadSelectionLoads: number;
}

class TopicManager {
    private readonly form!: HTMLFormElement;
    private readonly hiddenField!: HTMLInputElement;
    private readonly tableBody!: HTMLElement;
    private readonly modal!: HTMLElement;
    private readonly modalTitle!: HTMLElement;
    private readonly modalForm!: HTMLFormElement;
    private readonly topicNameInput!: HTMLInputElement;
    private readonly topicContextInput!: HTMLTextAreaElement;
    private readonly maxLoadsInput!: HTMLInputElement;
    private readonly maxLoadsBadge!: HTMLElement;
    private readonly saveBtn!: HTMLButtonElement;
    private readonly emptyRow: HTMLTableRowElement | null = null;

    private topics: TopicEntry[] = [];
    private editingIndex: number | null = null;
    private nextTempId = 1;
    private readonly isReadOnly: boolean;

    constructor() {
        const form = document.getElementById('create-project-step3-form') as HTMLFormElement | null;
        if (!form) return;

        this.form = form;
        const stepper = document.getElementById('dynamic-stepper');
        this.isReadOnly = stepper?.dataset.isReadonly === 'true';
        this.hiddenField = form.querySelector<HTMLInputElement>('input[name="CreateStep3ViewModel.TopicsJson"]')!;
        this.tableBody = document.getElementById('topicsTableBody')!;
        this.modal = document.getElementById('addTopicModal')!;
        this.modalTitle = document.getElementById('addTopicModalTitle')!;
        this.modalForm = document.getElementById('addTopicForm') as HTMLFormElement;
        this.topicNameInput = document.getElementById('modalTopicName') as HTMLInputElement;
        this.topicContextInput = document.getElementById('modalTopicContext') as HTMLTextAreaElement;
        this.maxLoadsInput = document.getElementById('modalTopicMaxLoads') as HTMLInputElement;
        this.maxLoadsBadge = document.getElementById('modalTopicMaxLoadsBadge')!;
        this.saveBtn = document.getElementById('saveAddTopicBtn') as HTMLButtonElement;
        this.emptyRow = this.tableBody.querySelector('tr') as HTMLTableRowElement | null;

        this.bindRangeSlider();
        this.bindModal();
        this.bindSaveButton();
        this.bindStepEnter();
        this.hydrateFromForm();
    }

    private bindRangeSlider(): void {
        const update = () => {
            this.maxLoadsBadge.textContent = this.maxLoadsInput.value;
        };
        this.maxLoadsInput.addEventListener('input', update);
        update();
    }

    private bindModal(): void {
        if (this.isReadOnly) return;
        document.querySelectorAll<HTMLElement>('[data-modal-key="add-topic"]').forEach((el) => {
            el.addEventListener('click', (event) => {
                event.preventDefault();
                this.openModal(null);
            });
        });

        document.getElementById('cancelAddTopicBtn')?.addEventListener('click', () => this.closeModal());
        this.modal.addEventListener('click', (event) => {
            if (event.target === this.modal) this.closeModal();
        });
    }

    private bindSaveButton(): void {
        if (this.isReadOnly) return;
        this.saveBtn.addEventListener('click', () => {
            if (!this.topicNameInput.reportValidity()) return;

            const topic: TopicEntry = {
                topicName: this.topicNameInput.value.trim(),
                topicContext: this.topicContextInput.value.trim(),
                maxBroadSelectionLoads: parseInt(this.maxLoadsInput.value, 10),
            };

            if (this.editingIndex !== null) {
                this.topics[this.editingIndex] = topic;
            } else {
                this.topics.push(topic);
            }

            this.refreshTable();
            this.syncToForm();
            this.closeModal();
        });
    }

    private bindStepEnter(): void {
        const container = document.getElementById('dynamic-stepper');
        container?.addEventListener('stepper:step-enter', ((event: CustomEvent) => {
            if (event.detail?.step === 3) {
                this.hydrateFromForm();
            }
        }) as EventListener);
    }

    private handleEdit(index: number): void {
        if (this.isReadOnly) return;
        this.openModal(index);
    }

    private handleDelete(index: number): void {
        if (this.isReadOnly) return;
        this.topics.splice(index, 1);
        this.refreshTable();
        this.syncToForm();
    }

    private hydrateFromForm(): void {
        this.topics = [];
        this.clearTableRows();
        const json = this.hiddenField.value.trim();
        if (!json || json === '[]') {
            this.showEmptyState();
            return;
        }
        try {
            const parsed = JSON.parse(json) as TopicEntry[];
            if (!Array.isArray(parsed)) {
                this.showEmptyState();
                return;
            }
            for (const entry of parsed) {
                const topic: TopicEntry = {
                    topicName: typeof entry.topicName === 'string' ? entry.topicName : '',
                    topicContext: typeof entry.topicContext === 'string' ? entry.topicContext : '',
                    maxBroadSelectionLoads: typeof entry.maxBroadSelectionLoads === 'number' ? entry.maxBroadSelectionLoads : 3,
                };
                this.topics.push(topic);
            }
            if (this.topics.length === 0) {
                this.showEmptyState();
            } else {
                this.refreshTable();
            }
        } catch {
            this.showEmptyState();
        }
    }

    private refreshTable(): void {
        this.clearTableRows();
        if (this.topics.length === 0) {
            this.showEmptyState();
            return;
        }
        this.hideEmptyState();
        for (let i = 0; i < this.topics.length; i++) {
            this.renderRow(this.topics[i], i);
        }
    }

    private renderRow(topic: TopicEntry, index: number): void {
        const rowId = `topic-row-${this.nextTempId++}`;
        const row = document.createElement('tr');
        row.id = rowId;
        row.className = 'transition-colors hover:bg-primary/5';

        const nameCell = document.createElement('td');
        nameCell.className = 'px-5 py-3.5 text-sm text-text/80';
        nameCell.textContent = topic.topicName;

        const contextCell = document.createElement('td');
        contextCell.className = 'px-5 py-3.5 text-sm text-text/80';
        contextCell.textContent = topic.topicContext || '-';

        const loadsCell = document.createElement('td');
        loadsCell.className = 'px-5 py-3.5 text-sm text-text/80';
        loadsCell.textContent = topic.maxBroadSelectionLoads.toString();

        const actionsCell = document.createElement('td');
        actionsCell.className = 'px-5 py-3.5 text-sm';

        const editBtn = document.createElement('button');
        editBtn.type = 'button';
        editBtn.className = 'font-semibold text-primary transition hover:text-primary/80';
        editBtn.textContent = 'Edit';
        if (this.isReadOnly) editBtn.disabled = true;
        editBtn.addEventListener('click', () => this.handleEdit(index));

        const divider = document.createElement('span');
        divider.className = 'mx-2 text-text/30';
        divider.textContent = '|';

        const deleteBtn = document.createElement('button');
        deleteBtn.type = 'button';
        deleteBtn.className = 'font-semibold text-accent transition hover:text-accent/80';
        deleteBtn.textContent = 'Delete';
        if (this.isReadOnly) deleteBtn.disabled = true;
        deleteBtn.addEventListener('click', () => this.handleDelete(index));

        actionsCell.appendChild(editBtn);
        actionsCell.appendChild(divider);
        actionsCell.appendChild(deleteBtn);

        row.appendChild(nameCell);
        row.appendChild(contextCell);
        row.appendChild(loadsCell);
        row.appendChild(actionsCell);
        this.tableBody.appendChild(row);
    }

    private syncToForm(): void {
        this.hiddenField.value = JSON.stringify(this.topics);
        this.form.dispatchEvent(new Event('input', { bubbles: true }));
    }

    private openModal(editIndex: number | null): void {
        if (this.isReadOnly) return;
        this.editingIndex = editIndex;

        if (editIndex !== null) {
            const topic = this.topics[editIndex];
            this.modalTitle.textContent = 'Edit Topic';
            this.saveBtn.textContent = 'Save Changes';
            this.topicNameInput.value = topic.topicName;
            this.topicContextInput.value = topic.topicContext;
            this.maxLoadsInput.value = topic.maxBroadSelectionLoads.toString();
            this.maxLoadsBadge.textContent = topic.maxBroadSelectionLoads.toString();
        } else {
            this.modalTitle.textContent = 'Add Topic';
            this.saveBtn.textContent = 'Add Topic';
            this.modalForm.reset();
            this.maxLoadsInput.value = '3';
            this.maxLoadsBadge.textContent = '3';
        }

        this.modal.classList.remove('hidden');
        this.modal.classList.add('flex');
        this.topicNameInput.focus();
    }

    private closeModal(): void {
        this.modal.classList.add('hidden');
        this.modal.classList.remove('flex');
    }

    private clearTableRows(): void {
        this.tableBody.querySelectorAll('tr[id^="topic-row-"]').forEach((row) => row.remove());
    }

    private hideEmptyState(): void {
        if (this.emptyRow) this.emptyRow.classList.add('hidden');
    }

    private showEmptyState(): void {
        if (this.emptyRow) this.emptyRow.classList.remove('hidden');
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new TopicManager();
});
