import {createDraggableComponent, Draggable} from "../../dragAndDropList.ts";
import {Question, QuestionType} from "../../../models/question.ts";
import {htmlToElement} from "../../../utils/dom.ts";
import {deselect, rootQuestionList, select} from "../../../modules/addQuestionPage.ts";
import {createPagedModalComponent} from "../../pagedModal.ts";
import {createSetQuestionTypeComponent} from "./createquestion/setQuestionType.ts";

export {createQuestionComponent, createQuestionPlaceholderComponent};
export type {QuestionComponent, QuestionPlaceholderComponent};

type QuestionComponent = Draggable & {
    question: Question;
};

function createQuestionComponent(question: Question): QuestionComponent {
    const deleteButton = htmlToElement<HTMLButtonElement>(
        `<button type="button" class="question-card-action question-card-action-danger" aria-label="Delete question">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16" class="icon">
                    <path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5m2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5m3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0z"/>
                    <path d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4zM2.5 3h11V2h-11z"/>
                </svg>
            </button>`
    );
    deleteButton.addEventListener('click', destroy);

    const editButton = htmlToElement<HTMLButtonElement>(
        `<button type="button" class="question-card-action" aria-label="Edit question">
                <svg xmlns="http://www.w3.org/2000/svg" fill="" viewBox="0 0 16 16" class="icon">
                    <path d="M12.854.146a.5.5 0 0 0-.707 0L10.5 1.793 14.207 5.5l1.647-1.646a.5.5 0 0 0 0-.708zm.646 6.061L9.793 2.5 3.293 9H3.5a.5.5 0 0 1 .5.5v.5h.5a.5.5 0 0 1 .5.5v.5h.5a.5.5 0 0 1 .5.5v.5h.5a.5.5 0 0 1 .5.5v.207zm-7.468 7.468A.5.5 0 0 1 6 13.5V13h-.5a.5.5 0 0 1-.5-.5V12h-.5a.5.5 0 0 1-.5-.5V11h-.5a.5.5 0 0 1-.5-.5V10h-.5a.5.5 0 0 1-.175-.032l-.179.178a.5.5 0 0 0-.11.168l-2 5a.5.5 0 0 0 .65.65l5-2a.5.5 0 0 0 .168-.11z"/>
                </svg>
            </button>`
    );

    const selectButton = htmlToElement<HTMLInputElement>(
        `<input type="checkbox" aria-label="Select a question" />`
    );
    selectButton.addEventListener('change', selectChange);

    const component: QuestionComponent = createDraggableComponent(htmlToElement<QuestionComponent>(`
<article class="question question-card">
    <div class="question-card-handle">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 15 24" fill="currentColor" class="icon">
            <circle cx="3" cy="3" r="3"/>
            <circle cx="12" cy="3" r="3"/>
            <circle cx="3" cy="12" r="3"/>
            <circle cx="12" cy="12" r="3"/>
            <circle cx="3" cy="21" r="3"/>
            <circle cx="12" cy="21" r="3"/>
        </svg>
    </div>

    <img src="${getQuestionIconSrc(question.type)}" alt="question-type-icon" class="question-card-type-icon"/>

    <div class="question-card-body">
        <h3 class="question-card-title">
            ${question.text}
        </h3>
        <div class="question-card-meta">
            <span class="question-card-pill">${question.type}</span>
            ${question.required ? '<span class="question-card-pill question-card-required">Required</span>' : ''}
        </div>
    </div>

    <menu class="question-card-menu">
    </menu>
</article>`)) as QuestionComponent;
    
    component.question = question;

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
        const editableQuestion: Question = structuredClone(question);
        modal.setPage(createSetQuestionTypeComponent(modal, editableQuestion, (updatedQuestion) => {
            component.question = updatedQuestion;
            updateComponentContent(component, updatedQuestion);
        }));
        modal.show();
    }
}

function updateComponentContent(component: QuestionComponent, question: Question): void {
    const title = component.querySelector('.question-card-title');
    if (title) {
        title.textContent = question.text;
    }


    const meta = component.querySelector('.question-card-meta');
    if (meta) {
        meta.innerHTML = `
            <span class="question-card-pill">${question.type}</span>
            ${question.required ? '<span class="question-card-pill question-card-required">Required</span>' : ''}
        `;
    }

    const icon = component.querySelector('.question-card-type-icon') as HTMLImageElement | null;
    if (icon) {
        icon.src = getQuestionIconSrc(question.type);
    }
}

function getQuestionIconSrc(type: QuestionType): string {
    switch (type) {
        case QuestionType.Open:
            return '/Assets/multiple_choice.svg';
        case QuestionType.MultipleChoice:
            return '/Assets/multiple_choice.svg';
        case QuestionType.SingleChoice:
            return '/Assets/multiple_choice.svg';
        case QuestionType.Scale:
            return '/Assets/multiple_choice.svg';
        default:
            return '/Assets/multiple_choice.svg';
    }
}


type QuestionPlaceholderComponent = HTMLElement;

function createQuestionPlaceholderComponent(): QuestionPlaceholderComponent {

    return htmlToElement<QuestionPlaceholderComponent>(`
<div class="flex items-center gap-3 rounded-xl border border-zinc-200 bg-white p-4 shadow-sm h-2">
</div>`);
}
