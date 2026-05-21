import {createDragAndDropListComponent, DragAndDropListComponent} from "../components/DragAndDropList.ts";
import {createQuestionComponent, createQuestionPlaceholderComponent} from "../components/admin/addquestion/Question.ts";
import {createSectionComponent} from "../components/admin/addquestion/Section.ts";
import {createPagedModalComponent} from "../components/PagedModal.ts";
import {createSetQuestionTypeComponent} from "../components/admin/addquestion/createquestion/SetQuestionType.ts";
import {createModalComponent} from "../components/Modal.ts";
import {createAddSectionComponent} from "../components/admin/addquestion/AddSectionComponent.ts";
import {createOpenQuestion} from "../models/Question.ts";

const rootQuestionElement: HTMLOListElement = document.getElementById('question-root') as HTMLOListElement;
const createQuestionButton: HTMLButtonElement = document.getElementById('create-question-button') as HTMLButtonElement;
const createSectionButton: HTMLButtonElement = document.getElementById('create-section-button') as HTMLButtonElement;
const deleteButton: HTMLButtonElement = document.getElementById('delete-button') as HTMLButtonElement;

export const rootQuestionList: DragAndDropListComponent = createDragAndDropListComponent(createQuestionPlaceholderComponent());
rootQuestionElement.appendChild(rootQuestionList);


createQuestionButton.addEventListener('click', clickCreateQuestion);
createSectionButton.addEventListener('click', clickCreateSection);
deleteButton.addEventListener('click', clickDeleteButton);

const section = createSectionComponent('Bussen');
section.questions.addElement(createQuestionComponent(createOpenQuestion("Wat vind je van het aantal bushaltes?")));
rootQuestionList.addElement(section);

function clickCreateQuestion() {
    const modal = createPagedModalComponent();
    modal.setPage(createSetQuestionTypeComponent(modal));
    modal.show();
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

}