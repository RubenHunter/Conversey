import {createDragAndDropListComponent, DragAndDropListComponent} from "../components/dragAndDropList.ts";
import {
    createQuestionComponent,
    createQuestionPlaceholderComponent,
    QuestionComponent
} from "../components/admin/addquestion/question.ts";
import {createSectionComponent, SectionComponent} from "../components/admin/addquestion/section.ts";
import {createPagedModalComponent} from "../components/pagedModal.ts";
import {createSetQuestionTypeComponent} from "../components/admin/addquestion/createquestion/setQuestionType.ts";
import {createModalComponent} from "../components/modal.ts";
import {createAddSectionComponent} from "../components/admin/addquestion/addSectionComponent.ts";
import {Question, QuestionType} from "../models/question.ts";

export {select, deselect, rootQuestionList, getListQuestions};

const rootQuestionElement: HTMLOListElement = document.getElementById('question-root') as HTMLOListElement;
const createQuestionButton: HTMLButtonElement = document.getElementById('create-question-button') as HTMLButtonElement;
const createSectionButton: HTMLButtonElement = document.getElementById('create-section-button') as HTMLButtonElement;
const deleteButton: HTMLButtonElement = document.getElementById('delete-button') as HTMLButtonElement;
const rootQuestionList: DragAndDropListComponent = createDragAndDropListComponent(createQuestionPlaceholderComponent());
rootQuestionElement.appendChild(rootQuestionList);
const selected: (QuestionComponent | SectionComponent)[] = [];

createQuestionButton.addEventListener('click', clickCreateQuestion);
createSectionButton.addEventListener('click', clickCreateSection);
deleteButton.addEventListener('click', clickDeleteButton);

const section = createSectionComponent('Bussen');
section.questions.addElement(createQuestionComponent({type: QuestionType.Open, text: "Wat vind je van het aantal bushaltes?", required: true}));
rootQuestionList.addElement(section);

function clickCreateQuestion() {
    const modal = createPagedModalComponent();
    modal.setPage(createSetQuestionTypeComponent(modal, {type: QuestionType.Open, text: "", required: true}));
    modal.show();
}

function select(element: QuestionComponent | SectionComponent) {
    selected.push(element);
}

function deselect(element: QuestionComponent | SectionComponent) {
    const index: number = selected.indexOf(element);
    if (index === -1) {
        return;
    }
    selected.splice(index, 1);
}

function clickCreateSection() {
    const modal = createModalComponent(
        createAddSectionComponent({
            onCancel() {
                modal.destroy()
            },
            onSubmit() {
                modal.destroy()
            }
        })
    );
    modal.show();
}

function getListQuestions(list: DragAndDropListComponent): Question[] {
    const questions: Question[] = [];
    for (const child of list.children) {
        const component: HTMLElement = child.firstElementChild! as HTMLElement;
        if (component instanceof HTMLDetailsElement) {
            const section: SectionComponent = component as SectionComponent;
            questions.push(...getListQuestions(section.questions));
        } else {
            const question: QuestionComponent = component as QuestionComponent;
            questions.push(question.question);
        }
    }
    return questions;
}

function clickDeleteButton() {
    for (const selectedElement of selected) {
        rootQuestionList.removeElement(selectedElement);
        deselect(selectedElement);
    }
}