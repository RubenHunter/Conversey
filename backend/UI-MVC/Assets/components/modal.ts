import {htmlToElement} from "../utils/dom.ts";

export type {ModalComponent};

export {createModalComponent};

type ModalComponent = HTMLDialogElement & {
    show(): void,
    destroy(): void,
};

function createModalComponent(element?: HTMLElement): ModalComponent {
    const component: ModalComponent = htmlToElement(`
        <dialog closedby="any" id="dialog" class="rounded-2xl p-0 w-full max-w-lg backdrop:bg-black/50 m-auto">
        </dialog>`);

    if (element != undefined) {
        component.appendChild(element);
    }

    component.show = show;
    component.destroy = destroy;

    return component;

    function show() {
        document.body.appendChild(component);
        component.showModal();
    }

    function destroy() {
        component.close();
        component.remove();
    }
}