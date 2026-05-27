import {createDragAndDropListComponent, DragAndDropListComponent} from "../components/dragAndDropList.ts";
import {
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
const selectAllCheckbox: HTMLInputElement | null = rootQuestionElement.querySelector('header input[type="checkbox"]');
const rootQuestionList: DragAndDropListComponent = createDragAndDropListComponent(createQuestionPlaceholderComponent());
rootQuestionElement.appendChild(rootQuestionList);
const selected: (QuestionComponent | SectionComponent)[] = [];

createQuestionButton.addEventListener('click', clickCreateQuestion);
createSectionButton.addEventListener('click', clickCreateSection);
deleteButton.addEventListener('click', clickDeleteButton);
selectAllCheckbox?.addEventListener('change', toggleSelectAll);

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
    while (selected.length > 0) {
        const selectedElement = selected[0];
        rootQuestionList.removeElement(selectedElement);
        deselect(selectedElement);
    }
}

function toggleSelectAll() {
    if (!selectAllCheckbox) return;

    const check = selectAllCheckbox.checked;
    for (const child of rootQuestionList.children) {
        const component = child.firstElementChild as HTMLElement | null;
        if (!component) continue;

        if (component instanceof HTMLDetailsElement) {
            const section = component as SectionComponent;
            setSectionChecked(section, check);
        } else {
            const checkbox = component.querySelector('input[type="checkbox"]') as HTMLInputElement | null;
            if (checkbox) {
                checkbox.checked = check;
                checkbox.dispatchEvent(new Event('change', { bubbles: true }));
            }
        }
    }
}

function setSectionChecked(section: SectionComponent, check: boolean) {
    const sectionToggle = section.querySelector('summary input[type="checkbox"]') as HTMLInputElement | null;
    if (sectionToggle) {
        sectionToggle.checked = check;
        sectionToggle.dispatchEvent(new Event('change', { bubbles: true }));
    }

    for (const child of section.questions.children) {
        const component = child.firstElementChild as HTMLElement | null;
        if (!component) continue;
        const checkbox = component.querySelector('input[type="checkbox"]') as HTMLInputElement | null;
        if (checkbox) {
            checkbox.checked = check;
            checkbox.dispatchEvent(new Event('change', { bubbles: true }));
        }
    }
}
