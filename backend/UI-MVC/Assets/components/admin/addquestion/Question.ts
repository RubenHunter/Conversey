import {createDraggableComponent, Draggable} from "../../DragAndDropList.ts";
import {Question} from "../../../models/Question.ts";
import {htmlToElement} from "../../../utils/dom.ts";
import {deselect, rootQuestionList, select} from "../../../modules/AddQuestionPage.ts";
import {createPagedModalComponent} from "../../PagedModal.ts";
import {createSetQuestionTypeComponent} from "./createquestion/SetQuestionType.ts";

export {createQuestionComponent, createQuestionPlaceholderComponent};
export type {QuestionComponent, QuestionPlaceholderComponent};

type QuestionComponent = Draggable;

function createQuestionComponent(question: Question): QuestionComponent {
    
    const deleteButton = htmlToElement<HTMLButtonElement>(
        `<button class="icon-btn">
                <img src="/Assets/trash.svg" alt="trash-icon" class="icon"/>
            </button>`
    );
    deleteButton.addEventListener('click', destroy);

    const editButton = htmlToElement<HTMLButtonElement>(
        `<button class="icon-btn">
                <img src="/Assets/pencil.svg" alt="pencil-icon" class="icon"/>
            </button>`
    );

    const selectButton = htmlToElement<HTMLInputElement>(
        `<input type="checkbox" aria-label="Select a question" />`
    );
    selectButton.addEventListener('change', selectChange);

    const component: QuestionComponent = createDraggableComponent(htmlToElement<QuestionComponent>(`
<article class="question flex items-center gap-3 rounded-xl border border-zinc-200 bg-white p-4 shadow-sm hover:bg-zinc-50">
    <img src="/Assets/drag.svg" alt="drag-icon" class="icon"/>
    
    <img src="/Assets/multiple_choice.svg" alt="single-choice-icon" class="icon w-6 h-6"/>

    <h3 class="grow font-medium">
        ${question.text}
    </h3>

    <menu class="flex gap-1">
    </menu>
</article>`)) as QuestionComponent;

    editButton.addEventListener('click', edit);
    
    function addListElement(parent: HTMLElement, child: HTMLElement) {
        let li = document.createElement("li");
        li.appendChild(child);
        parent.appendChild(li);
    }
    
    const actionMenu = component.querySelector('menu') as HTMLMenuElement;
    addListElement(actionMenu, deleteButton);
    addListElement(actionMenu, editButton);
    addListElement(actionMenu, selectButton);

    component.addEventListener('dragstart', startDrag);
    component.addEventListener('dragend', endDrag);

    return component;

    function startDrag() {
        component.classList.add("opacity-50");
    }

    function endDrag() {
        component.classList.remove("opacity-50");
    }
    
    function destroy() {
        rootQuestionList.removeElement(component);
    }
    
    function selectChange() {
        if (selectButton.checked) {
            select(component);
        } else {
            deselect(component);
        }
    }
    
    function edit() {
        const modal = createPagedModalComponent();
        modal.setPage(createSetQuestionTypeComponent(modal, question));
        modal.show();
    }
}


type QuestionPlaceholderComponent = HTMLElement;

function createQuestionPlaceholderComponent(): QuestionPlaceholderComponent {

    return htmlToElement<QuestionPlaceholderComponent>(`
<div class="flex items-center gap-3 rounded-xl border border-zinc-200 bg-white p-4 shadow-sm h-2">
</div>`);
}