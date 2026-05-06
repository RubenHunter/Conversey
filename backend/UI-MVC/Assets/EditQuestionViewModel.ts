import {Question, QuestionType} from "./models/question";

export {showEditQuestionView};

const editDialogElement = document.getElementById("edit-question-dialog") as HTMLDialogElement;
const editQuestionTextElement = document.getElementById("edit-question-text") as HTMLInputElement;
const editQuestionTypeElement = document.getElementById("edit-question-type") as HTMLSelectElement;

editDialogElement.children[0].addEventListener("submit", submitEditQuestion)

let editingQuestion: Question;
let editingIndex: number;

function submitEditQuestion(submit: SubmitEvent) {
    if (editingQuestion == undefined || editingIndex == undefined) {
        return;
    }
    
    submit.preventDefault();
    editingQuestion.text = editQuestionTextElement.value;
    editingQuestion.type = QuestionType[editQuestionTypeElement.selectedOptions[editQuestionTypeElement.selectedIndex].text];
    editDialogElement.close();
}

function showEditQuestionView(question: Question, index: number) {
    editDialogElement.show();
    editQuestionTextElement.textContent = question.text;
    editQuestionTypeElement.selectedIndex = question.type;
    editingQuestion = question;
    editingIndex = index;
}