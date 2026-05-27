import {createAddAnswersComponent} from "./addAnswers.ts";
import {createSetRangeAnswerComponent} from "./setRangeAnswer.ts";
import {PagedModalComponent} from "../../../pagedModal.ts";
import {FixedQuestion, Question, QuestionType, RangeQuestion} from "../../../../models/question.ts";
import {htmlToElement} from "../../../../utils/dom.ts";
import {rootQuestionList} from "../../../../modules/addQuestionPage.ts";
import {createQuestionComponent} from "../question.ts";

export {createSetQuestionTypeComponent};

type SetQuestionTypeComponent = HTMLFormElement;

function createSetQuestionTypeComponent(
    modal: PagedModalComponent,
    question: Question,
    onSave?: (updatedQuestion: Question) => void
): SetQuestionTypeComponent {

    const options:string = Object.values(QuestionType)
        .map(t => `<option value="${t}">${t}</option>`)
        .join();

    const component = htmlToElement<SetQuestionTypeComponent>(`
    <form method="dialog" class="question-modal">
            <header class="question-modal-header">
                <h2 class="question-modal-title">Add a Question</h2>
                <p class="question-modal-subtitle">Create a new question for your form</p>
            </header>
            <main class="question-modal-body">
                <label class="block text-sm font-medium text-zinc-700">
                    Question
                    <input name="question-text" type="text" required class="question-modal-input"/>
                </label>
            
                <label class="block text-sm font-medium text-zinc-700">
                    Type
                    <select name="question-type" class="question-modal-select">
                        ${options}
                    </select>
                </label>
                
                <label class="flex items-center text-sm font-medium text-zinc-700">
                    Required
                    <input name="question-required" type="checkbox" class="question-modal-checkbox">
                </label>
            </main>
            <footer class="question-modal-footer">

                <button type="button" command="close" commandfor="dialog" class="question-modal-btn" name="cancel">
                    Cancel
                </button>
    
                <button type="submit" class="question-modal-btn question-modal-btn-primary">
                    Next
                </button>
    
            </footer>
    </form>`);

    const submitButton = component.querySelector('button[type="submit"]')!;
    const cancelButton = component.elements.namedItem('cancel')! as HTMLSelectElement;
    const selectElement = component.elements.namedItem('question-type')! as HTMLSelectElement;
    const questionInput = component.elements.namedItem('question-text')! as HTMLInputElement;
    const requiredCheckbox = component.elements.namedItem('question-required')! as HTMLInputElement;
    
    selectElement.value = question.type;
    questionInput.value = question.text;
    requiredCheckbox.checked = question.required;
    
    selectElement.addEventListener('change', setType);
    cancelButton.addEventListener('click', cancel);
    component.addEventListener('submit', submit);
    requiredCheckbox.addEventListener('change', changeRequired);

    updateSubmitButton();


    return component;

    function submit(submit: SubmitEvent) {
        submit.preventDefault();

        question.text = questionInput.value;
        switch (question.type) {
            case QuestionType.MultipleChoice:
            case QuestionType.SingleChoice:
                modal.setPage(createAddAnswersComponent(modal, question as FixedQuestion, onSave))
                break;
            case QuestionType.Open:
                if (onSave) {
                    onSave(question);
                } else {
                    rootQuestionList.addElement(createQuestionComponent(question));
                }
                modal.destroy();
                break;
            case QuestionType.Scale:
                modal.setPage(createSetRangeAnswerComponent(modal, question as RangeQuestion, onSave))
                break;
        }
    }
    
    function setType() {
        question.type = selectElement.value as QuestionType;
    }

    function updateSubmitButton() {
        let text: string;
        switch (question.type) {
            case QuestionType.Open:
                text = 'Create';
                break;
            default:
                text = 'Next';
        }
        submitButton.innerHTML = text;
    }
    
    function changeRequired() {
        question.required = requiredCheckbox.checked;
    }

    function cancel() {
        modal.destroy();
    }
}
