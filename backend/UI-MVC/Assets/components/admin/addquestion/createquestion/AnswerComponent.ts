import {createDraggableComponent, Draggable} from "../../../DragAndDropList.ts";
import {htmlToElement} from "../../../../utils/dom.ts";

export {createAnswerComponent};
export type {AnswerComponent};

type AnswerComponent = Draggable & {
    text: string;
    edit(): void;
};

function createAnswerComponent(): AnswerComponent {
    const component = createDraggableComponent(
        htmlToElement(`<article class="bg-red-600"></article>`)
    ) as AnswerComponent;

    component.edit = edit;
    component.addEventListener('dragstart', startDrag);
    component.addEventListener('dragend', endDrag);
    return component;

    function startDrag() {
        component.classList.add("opacity-50");
    }

    function endDrag() {
        component.classList.remove("opacity-50");
    }

    function edit() {
        const input = htmlToElement<HTMLInputElement>(`<input type="text" class="input"/>`);
        input.addEventListener('change', () => setText(input.value));
        component.replaceChildren(input);
        input.focus();
    }

    function setText(input: string) {
        component.text = input;
        component.innerText = input;
    }
}