interface PageElements {
    tableId: string;
    rowSelector: string;
    expandRowSelector: string;
    expandRowTag: string;
    rowDataAttr: string;
    filterFormId: string;
    exportBtnId: string;
    exportMenuId: string;
    exportLinkClass: string;
    pageSizeId: string;
    paginationInfoId: string;
    paginationId: string;
    prevBtnId: string;
    nextBtnId: string;
    pageNumsId: string;
    pageNumClass: string;
    exportType: string;
    renderPageFn: string;
    type: string;
}

const CONFIGS: Record<string, PageElements> = {
    idea: {
        type: 'idea',
        tableId: 'ideas-table',
        rowSelector: 'tr.idea-row',
        expandRowSelector: '.idea-expand-row',
        expandRowTag: 'idea-expand-row',
        rowDataAttr: 'data-idea-id',
        filterFormId: 'ideas-filter-form',
        exportBtnId: 'export-dropdown-btn-ideas',
        exportMenuId: 'export-dropdown-menu-ideas',
        exportLinkClass: 'export-ideas-link',
        pageSizeId: 'ideas-page-size',
        paginationInfoId: 'ideas-pagination-info',
        paginationId: 'ideas-pagination',
        prevBtnId: 'ideas-prev',
        nextBtnId: 'ideas-next',
        pageNumsId: 'ideas-page-nums',
        pageNumClass: 'ideas-page-num',
        exportType: 'ideas-only',
        renderPageFn: '_ideasRenderPage',
    },
    answer: {
        type: 'answer',
        tableId: 'answers-table',
        rowSelector: 'tr.answer-row',
        expandRowSelector: '.answer-expand-row',
        expandRowTag: 'answer-expand-row',
        rowDataAttr: 'data-answer-id',
        filterFormId: 'answers-filter-form',
        exportBtnId: 'export-dropdown-btn-answers',
        exportMenuId: 'export-dropdown-menu-answers',
        exportLinkClass: 'export-answers-link',
        pageSizeId: 'answers-page-size',
        paginationInfoId: 'answers-pagination-info',
        paginationId: 'answers-pagination',
        prevBtnId: 'answers-prev',
        nextBtnId: 'answers-next',
        pageNumsId: 'answers-page-nums',
        pageNumClass: 'answers-page-num',
        exportType: 'answers-only',
        renderPageFn: '_answersRenderPage',
    },
};

function initChevrons(el: PageElements): void {
    document.querySelectorAll(el.rowSelector).forEach((row) => {
        row.addEventListener('click', function () {
            const rowId = row.getAttribute(el.rowDataAttr);
            if (!rowId) return;
            const expandRow = document.querySelector(
                el.expandRowSelector + '[' + el.rowDataAttr + '="' + rowId + '"]'
            ) as HTMLElement | null;
            const chevron = row.querySelector('.chevron') as HTMLElement | null;
            if (expandRow) {
                const isExpanded = !expandRow.classList.contains('hidden');
                expandRow.classList.toggle('hidden');
                if (chevron) {
                    if (isExpanded) chevron.classList.remove('rotate-90');
                    else chevron.classList.add('rotate-90');
                }
            }
        });
    });
}

function initExport(el: PageElements): void {
    const exportBtn = document.getElementById(el.exportBtnId);
    const exportMenu = document.getElementById(el.exportMenuId);
    if (!exportBtn || !exportMenu) return;

    exportBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        exportMenu.classList.toggle('hidden');
    });
    document.addEventListener('click', () => exportMenu.classList.add('hidden'));
    exportMenu.addEventListener('click', (e) => e.stopPropagation());

    document.querySelectorAll('.' + el.exportLinkClass).forEach((link) => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const filterAware = (link as HTMLElement).dataset.exportFiltered === 'true';
            const wsSlug =
                document.querySelector('[data-workspace-slug]')?.getAttribute('data-workspace-slug') ||
                window.location.hostname.split('.')[0];
            const params = new URLSearchParams();
            params.set('workspaceId', wsSlug);
            params.set('type', el.exportType);
            if (filterAware) {
                const formEl = document.getElementById(el.filterFormId) as HTMLFormElement | null;
                if (formEl) {
                    const fd = new FormData(formEl);
                    for (const pair of fd.entries()) {
                        if (pair[1] && typeof pair[1] === 'string' && pair[0] !== '')
                            params.set(pair[0], pair[1]);
                    }
                }
            }
            window.location.href = '/api/admin/analytics/export?' + params.toString();
            exportMenu.classList.add('hidden');
        });
    });
}

function initSort(el: PageElements): void {
    const table = document.getElementById(el.tableId);
    if (!table) return;
    const headers = table.querySelectorAll<HTMLElement>('th[data-sort]');
    let currentSort: { col: string | null; asc: boolean } = { col: null, asc: true };

    headers.forEach((th) => {
        th.addEventListener('click', () => {
            const col = th.getAttribute('data-sort');
            if (!col) return;
            const asc = currentSort.col === col ? !currentSort.asc : true;
            currentSort = { col, asc };

            headers.forEach((h) => {
                h.removeAttribute('data-sort-dir');
            });
            th.setAttribute('data-sort-dir', asc ? 'asc' : 'desc');

            const tbody = table.querySelector('tbody');
            if (!tbody) return;
            const rows = Array.from(tbody.querySelectorAll<HTMLElement>(el.rowSelector));
            rows.sort((a, b) => {
                const aVal = (a.querySelector('[data-col="' + col + '"]')?.textContent || '')
                    .trim()
                    .toLowerCase();
                const bVal = (b.querySelector('[data-col="' + col + '"]')?.textContent || '')
                    .trim()
                    .toLowerCase();
                if (col === 'id') {
                    const aNum = parseInt(aVal.replace('#', ''));
                    const bNum = parseInt(bVal.replace('#', ''));
                    return asc ? aNum - bNum : bNum - aNum;
                }
                if (col === 'comments') {
                    return asc ? parseInt(aVal) - parseInt(bVal) : parseInt(bVal) - parseInt(aVal);
                }
                if (col === 'date') {
                    return asc ? aVal.localeCompare(bVal) : bVal.localeCompare(aVal);
                }
                return asc ? aVal.localeCompare(bVal) : bVal.localeCompare(aVal);
            });

            const expandRows = tbody.querySelectorAll(el.expandRowSelector);
            const expandMap: Record<string, Element> = {};
            expandRows.forEach((r) => {
                expandMap[r.getAttribute(el.rowDataAttr) || ''] = r;
            });

            rows.forEach((row) => {
                tbody.appendChild(row);
                const expandRow = expandMap[row.getAttribute(el.rowDataAttr) || ''];
                if (expandRow) tbody.appendChild(expandRow);
            });

            const renderFn = (window as any)[el.renderPageFn];
            if (typeof renderFn === 'function') renderFn();
        });
    });
}

function initPagination(el: PageElements): void {
    const pageSizeSelect = document.getElementById(el.pageSizeId) as HTMLSelectElement | null;
    const infoEl = document.getElementById(el.paginationInfoId);
    const paginationEl = document.getElementById(el.paginationId);
    const prevBtn = document.getElementById(el.prevBtnId) as HTMLButtonElement | null;
    const nextBtn = document.getElementById(el.nextBtnId) as HTMLButtonElement | null;
    const pageNumsEl = document.getElementById(el.pageNumsId);
    const tbody = document.querySelector('#' + el.tableId + ' tbody');
    if (!pageSizeSelect || !tbody || !infoEl || !paginationEl || !prevBtn || !nextBtn || !pageNumsEl)
        return;

    let currentPage = 1;
    let pageSize = parseInt(pageSizeSelect.value);
    let totalPages = 0;

    function getRows(): NodeListOf<HTMLElement> {
        return tbody!.querySelectorAll(el.rowSelector);
    }

    function renderPage(): void {
        const rows = getRows();
        const totalItems = rows.length;
        pageSize = parseInt(pageSizeSelect!.value);
        totalPages = pageSize === -1 ? 1 : Math.ceil(totalItems / pageSize);
        if (currentPage > totalPages) currentPage = totalPages;

        const start = pageSize === -1 ? 0 : (currentPage - 1) * pageSize;
        const end = pageSize === -1 ? totalItems : Math.min(start + pageSize, totalItems);
        const pageRowIds = new Set<string>();
        for (let i = start; i < end; i++) {
            const id = rows[i].getAttribute(el.rowDataAttr);
            if (id) pageRowIds.add(id);
        }

        rows.forEach((row) => {
            const id = row.getAttribute(el.rowDataAttr);
            const expandRow = tbody!.querySelector(
                el.expandRowSelector + '[' + el.rowDataAttr + '="' + id + '"]'
            ) as HTMLElement | null;
            if (id && pageRowIds.has(id)) {
                row.style.display = '';
                if (expandRow) expandRow.style.display = '';
            } else {
                row.style.display = 'none';
                if (expandRow) {
                    expandRow.classList.add('hidden');
                    expandRow.style.display = 'none';
                }
            }
        });

        infoEl!.textContent =
            'Showing ' +
            (totalItems === 0 ? 0 : start + 1) +
            '-' +
            end +
            ' of ' +
            totalItems +
            ' ' +
            (el.type === 'idea' ? 'ideas' : 'answers');

        if (totalPages <= 1) {
            paginationEl!.style.display = 'none';
        } else {
            paginationEl!.style.display = '';
        }

        prevBtn!.disabled = currentPage <= 1;
        nextBtn!.disabled = currentPage >= totalPages;

        const maxPages = Math.min(totalPages, 7);
        let startPage = Math.max(1, currentPage - 3);
        const endPage = Math.min(totalPages, startPage + maxPages - 1);
        if (endPage - startPage < maxPages - 1) startPage = Math.max(1, endPage - maxPages + 1);

        let pageNumsHtml = '';
        for (let p = startPage; p <= endPage; p++) {
            if (p === currentPage) {
                pageNumsHtml +=
                    '<span class="px-2 py-1 rounded bg-primary text-white text-xs font-semibold">' +
                    p +
                    '</span>';
            } else {
                pageNumsHtml +=
                    '<button class="' +
                    el.pageNumClass +
                    ' px-2 py-1 rounded border border-secondary/20 text-xs hover:bg-secondary/5" data-page="' +
                    p +
                    '">' +
                    p +
                    '</button>';
            }
        }
        pageNumsEl!.innerHTML = pageNumsHtml;

        pageNumsEl!.querySelectorAll('.' + el.pageNumClass).forEach((btn) => {
            btn.addEventListener('click', function () {
                currentPage = parseInt(btn.getAttribute('data-page') || '1');
                renderPage();
            });
        });
    }

    pageSizeSelect.addEventListener('change', () => {
        currentPage = 1;
        renderPage();
    });

    prevBtn.addEventListener('click', () => {
        if (currentPage > 1) {
            currentPage--;
            renderPage();
        }
    });

    nextBtn.addEventListener('click', () => {
        if (currentPage < totalPages) {
            currentPage++;
            renderPage();
        }
    });

    (window as any)[el.renderPageFn] = renderPage;
    renderPage();
}

const TYPE_TABLE_MAP: Record<string, keyof typeof CONFIGS> = {
    'ideas-table': 'idea',
    'answers-table': 'answer',
};

function autoInit(): void {
    for (const [tableId, type] of Object.entries(TYPE_TABLE_MAP)) {
        if (document.getElementById(tableId)) {
            const el = CONFIGS[type];
            if (!el) return;
            initChevrons(el);
            initExport(el);
            initPagination(el);
            initSort(el);
            return;
        }
    }
}

autoInit();
