import {createDraggableComponent, Draggable} from "../../DragAndDropList.ts";
import {Question} from "../../../models/Question.ts";
import {htmlToElement} from "../../../utils/dom.ts";

export {createQuestionComponent, createQuestionPlaceholderComponent};
export type {QuestionComponent, QuestionPlaceholderComponent};

type QuestionComponent = Draggable;

function createQuestionComponent(question: Question): QuestionComponent {

    const component: QuestionComponent = createDraggableComponent(htmlToElement<QuestionComponent>(`
<article class="question flex items-center gap-3 rounded-xl border border-zinc-200 bg-white p-4 shadow-sm hover:bg-zinc-50">
    <img src="/Assets/drag.svg" alt="drag-icon" class="icon"/>
    
    <img src="/Assets/multiple_choice.svg" alt="single-choice-icon" class="icon w-6 h-6"/>

    <h3 class="grow font-medium">
        ${question.text}
    </h3>

    <menu class="flex gap-1">
        <li>
            <button class="icon-btn">
                <img src="/Assets/trash.svg" alt="trash-icon" class="icon"/>
            </button>
        </li>

        <li>
            <button class="icon-btn">
                <img src="/Assets/pencil.svg" alt="pencil-icon" class="icon"/>
            </button>
        </li>

        <li class="flex items-center">
            <label>
                <input type="checkbox" aria-label="Select a question" />
            </label>
        </li>
    </menu>
</article>`));

    component.addEventListener('dragstart', startDrag);
    component.addEventListener('dragend', endDrag);

    return component;

    function startDrag() {
        component.classList.add("opacity-50");
    }

    function endDrag() {
        component.classList.remove("opacity-50");
    }
}


type QuestionPlaceholderComponent = HTMLElement;

function createQuestionPlaceholderComponent(): QuestionPlaceholderComponent {

    const component: QuestionPlaceholderComponent = htmlToElement<QuestionPlaceholderComponent>(`
<div class="flex items-center gap-3 rounded-xl border border-zinc-200 bg-white p-4 shadow-sm h-2">
</div>`);
    return component;
}