import {htmlToElement} from "../utils/dom.ts";

export type {DragAndDropListComponent, Draggable};
export {createDragAndDropListComponent, createDraggableComponent};

let draggingElement: Draggable | undefined;

type DragAndDropListComponent = HTMLOListElement & {
    addElement(element: Draggable): void;
    removeElement(element: Draggable): void;
};

type Draggable = HTMLElement;

const DATA_TAG = 'drag-element';

function createDraggableComponent(element: HTMLElement): Draggable {
    const component: Draggable = element as Draggable;
    component.draggable = true;

    component.addEventListener('dragstart', (dragStart: DragEvent) => {
        dragStart.dataTransfer!.effectAllowed = "move";
        dragStart.dataTransfer!.setData(DATA_TAG, "");
    });



    return component;
}

function createDragAndDropListComponent(dragPlaceholder: HTMLElement): DragAndDropListComponent {
    const component = htmlToElement<DragAndDropListComponent>(`<ol class="space-y-2 pb-4"></ol>`);

    component.addEventListener('dragover', (drag: DragEvent) => {
        if (drag.dataTransfer!.types.includes(DATA_TAG)) {
            drag.preventDefault();
            drag.stopPropagation();
            movePlaceholder(drag)
        }
    });

    component.addEventListener('dragleave', (drag: DragEvent) => {
        if (component.contains(drag.relatedTarget! as Node)) return;
        if (dragPlaceholder.isConnected) {
            removePlaceholder()
        }
    });

    component.addEventListener('drop', (drag: DragEvent) => {
        drag.preventDefault();
        drag.stopPropagation();


        draggingElement!.parentElement?.remove();
        addElement(draggingElement!);
        removePlaceholder()
        draggingElement = undefined;

    });

    component.addElement = addElement;
    component.removeElement = removeElement;

    return component;

    function addElement(element: Draggable) {
        element.addEventListener('dragstart', () => {
            draggingElement = element;
        });

        const listElement = createListItem(element);
        if (dragPlaceholder.isConnected) {
            component.insertBefore(listElement, dragPlaceholder.parentElement);
        } else {
            component.appendChild(listElement);
        }
    }

    function removeElement(element: Draggable) {
        element.parentElement!.remove();
    }

    function movePlaceholder(event: DragEvent) {
        if (!event.dataTransfer?.types.includes(DATA_TAG) || !draggingElement) {
            return;
        }

        event.preventDefault();

        let existingPlaceholderItem: HTMLElement | undefined;

        if (dragPlaceholder.isConnected) {
            existingPlaceholderItem = dragPlaceholder.parentElement!;

            const rect = existingPlaceholderItem.getBoundingClientRect();
            if (rect.top <= event.clientY && rect.bottom >= event.clientY) {
                return;
            }
        }

        for (const questionListItem of component.children) {
            if (questionListItem.getBoundingClientRect().bottom >= event.clientY) {
                if (questionListItem === existingPlaceholderItem) return;

                existingPlaceholderItem?.remove();

                if (
                    questionListItem.firstChild === draggingElement ||
                    questionListItem.previousElementSibling === draggingElement
                ) {
                    return;
                }

                const newPlaceholderItem = createListItem(dragPlaceholder);

                component.insertBefore(newPlaceholderItem, questionListItem);
                return;
            }
        }

        if (existingPlaceholderItem) {
            existingPlaceholderItem.remove();
        }

        if (component.lastElementChild === draggingElement) return;

        const finalPlaceholderItem = createListItem(dragPlaceholder);
        component.append(finalPlaceholderItem);
    }

    function createListItem(child: HTMLElement): HTMLElement {
        const listItem = document.createElement('li');
        listItem.appendChild(child);
        return listItem;
    }

    function removePlaceholder() {
        dragPlaceholder!.parentElement!.remove();
    }
}