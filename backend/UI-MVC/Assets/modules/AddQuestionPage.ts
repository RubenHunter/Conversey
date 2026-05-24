import {createDragAndDropListComponent, DragAndDropListComponent} from "../components/DragAndDropList.ts";
import {
    createQuestionComponent,
    createQuestionPlaceholderComponent,
    QuestionComponent
} from "../components/admin/addquestion/Question.ts";
import {createSectionComponent, SectionComponent} from "../components/admin/addquestion/Section.ts";
import {createPagedModalComponent} from "../components/PagedModal.ts";
import {createSetQuestionTypeComponent} from "../components/admin/addquestion/createquestion/SetQuestionType.ts";
import {createModalComponent} from "../components/Modal.ts";
import {createAddSectionComponent} from "../components/admin/addquestion/AddSectionComponent.ts";
import {QuestionType} from "../models/Question.ts";

export {select, deselect, rootQuestionList};

const rootQuestionElement: HTMLOListElement = document.getElementById('question-root') as HTMLOListElement;
const createQuestionButton: HTMLButtonElement = document.getElementById('create-question-button') as HTMLButtonElement;
const createSectionButton: HTMLButtonElement = document.getElementById('create-section-button') as HTMLButtonElement;
const deleteButton: HTMLButtonElement = document.getElementById('delete-button') as HTMLButtonElement;
console.log('load')
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
    console.log('click')
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

function clickDeleteButton() {
    for (const selectedElement of selected) {
        rootQuestionList.removeElement(selectedElement);
        deselect(selectedElement);
    }
}