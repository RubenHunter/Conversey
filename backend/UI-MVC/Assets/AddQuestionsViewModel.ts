import {questionView} from "./components/admin/addquestion/questionview";
import {Question, QuestionType} from "./models/question";
import {sectionView} from "./components/admin/addquestion/sectionview";

export {removeAt, selectAt}

document.getElementById("add-question-button")
.addEventListener("click", showAddQuestionPopup);
document.getElementById("add-section-button")
    .addEventListener("click", showAddSectionPopup);
document.getElementById("delete-button")
    .addEventListener("click", deleteSelected);

const questionListElement = document.getElementById("question-list");
const addQuestionDialogElement = document.getElementById("add-question-dialog") as HTMLDialogElement;
addQuestionDialogElement.children[0].addEventListener("submit", submitAddQuestionForm);
const addQuestionTextElement = document.getElementById("add-question-text") as HTMLInputElement;
const addQuestionTypeElement = document.getElementById("add-question-type") as HTMLSelectElement;
const addSectionDialogElement = document.getElementById("add-section-dialog") as HTMLDialogElement;
addSectionDialogElement.children[0].addEventListener("submit", submitAddSectionForm);
const addSectionTextElement = document.getElementById("add-section-text") as HTMLInputElement;

function deleteSelected() {
    for (const i of selected) {
        removeAt(null, i);
    }
}

let selected: Array<number>;

function addLast(element: Question | string) {
    let view: string;
    if ((typeof element) === "string") {
        view = sectionView(element)
    } else if ((typeof element) === "object") {
        view = questionView(element, questionListElement.children.length)
    }
    questionListElement.insertAdjacentHTML('beforeend', view)
}

function selectAt(index: number) {
    selected.push(index);
}

function removeAt(section?: string, index?: number) {
    let element = questionListElement;
    if (section !== undefined) {
        const foundSection = element.querySelector(section).parentElement.parentElement;
        if (index == undefined) {
            element.removeChild(foundSection);
            return;
        }
        element = foundSection;
    }
    element.children[index].remove();
}

function showAddSectionPopup() {
    addSectionDialogElement.show();
    
}

function showAddQuestionPopup() {
   addQuestionDialogElement.show();
}

function submitAddSectionForm(submit: SubmitEvent) {
    submit.preventDefault();
    addLast(addSectionTextElement.value);
}

function submitAddQuestionForm(submit: SubmitEvent) {
    submit.preventDefault();
    const q: Question = {
        id: 0,
        projectId: 0,
        text: addQuestionTextElement.value,
        type: QuestionType[addQuestionTypeElement.value],
        isRequired: false,
    };
    addLast(q);
}