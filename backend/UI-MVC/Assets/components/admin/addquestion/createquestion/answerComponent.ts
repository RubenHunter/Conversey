import {createDraggableComponent, Draggable} from "../../../dragAndDropList.ts";
import {htmlToElement} from "../../../../utils/dom.ts";
import {Answer} from "../../../../models/question.ts";

export {createAnswerComponent};
export type {AnswerComponent};

type AnswerComponent = Draggable & {
    answer: Answer;
    edit(): void;
};

function createAnswerComponent(answer: Answer): AnswerComponent {
    const component = createDraggableComponent(
        htmlToElement(`<article class="bg-red-600"></article>`)
    ) as AnswerComponent;
    
    component.answer = answer;
    component.edit = edit;
    component.addEventListener('dragstart', startDrag);
    component.addEventListener('dragend', endDrag);
    component.addEventListener('dblclick', edit);
    
    
    updateText();
    
    return component;

    function startDrag() {
        component.classList.add("opacity-50");
    }

    function endDrag() {
        component.classList.remove("opacity-50");
    }

    function edit() {
        const input = htmlToElement<HTMLInputElement>(`<input type="text" class="input" value="${answer.text}"/>`);
        input.addEventListener('change', () => setText(input.value));
        component.replaceChildren(input);
        input.focus();
    }

    function setText(input: string) {
        answer.text = input;
        updateText();
    }
    
    function updateText() {
        component.innerText = answer.text;
    }
}