export function submitFormPreserveFilters(form: HTMLFormElement): void {
    const dateFrom =
        (form.querySelector('input[name="dateFrom"]') as HTMLInputElement)?.value || '';
    const dateTo =
        (form.querySelector('input[name="dateTo"]') as HTMLInputElement)?.value || '';
    const topicId =
        (form.querySelector('select[name="topicId"]') as HTMLSelectElement)?.value || '';
    const status =
        (form.querySelector('select[name="status"]') as HTMLSelectElement)?.value || '';

    if (dateFrom) {
        const df = document.createElement('input');
        df.type = 'hidden';
        df.name = 'dateFrom';
        df.value = dateFrom;
        form.appendChild(df);
    }
    if (dateTo) {
        const dt = document.createElement('input');
        dt.type = 'hidden';
        dt.name = 'dateTo';
        dt.value = dateTo;
        form.appendChild(dt);
    }
    if (topicId) {
        const ti = document.createElement('input');
        ti.type = 'hidden';
        ti.name = 'topicId';
        ti.value = topicId;
        form.appendChild(ti);
    }
    if (status) {
        const si = document.createElement('input');
        si.type = 'hidden';
        si.name = 'status';
        si.value = status;
        form.appendChild(si);
    }
    form.submit();
}
