import {createModalComponent, type ModalComponent} from "./modal.ts";

export type {PagedModalComponent};

export {createPagedModalComponent};

type PagedModalComponent = ModalComponent & {
    setPage(element: HTMLElement): void;
    back(): void;
};

function createPagedModalComponent(): PagedModalComponent {
    const component: PagedModalComponent = createModalComponent() as PagedModalComponent;

    component.setPage = setPage;
    let currentPage: HTMLElement;
    let lastPage: HTMLElement;

    component.back = back;

    return component;

    function setPage(element: HTMLElement) {
        if (currentPage != undefined) {
            lastPage = currentPage;
        }
        currentPage = element;
        component.replaceChildren(element);
    }

    function back() {
        setPage(lastPage);
    }
}