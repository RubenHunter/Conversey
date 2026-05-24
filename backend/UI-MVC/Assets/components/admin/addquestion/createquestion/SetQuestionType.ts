import {createAddAnswersComponent} from "./AddAnswers.ts";
import {createSetRangeAnswerComponent} from "./SetRangeAnswer.ts";
import {PagedModalComponent} from "../../../PagedModal.ts";
import {QuestionType} from "../../../../models/AddQuestion.ts";
import {htmlToElement} from "../../../../utils/dom.ts";

export {createSetQuestionTypeComponent};

type SetQuestionTypeComponent = HTMLFormElement;

function createSetQuestionTypeComponent(modal: PagedModalComponent): SetQuestionTypeComponent {

    const options:string = Object.values(QuestionType)
        .map(t => `<option value="${t}">${t}</option>`)
        .join();

    const component = htmlToElement<SetQuestionTypeComponent>(`
    <form method="dialog" class="w-full bg-white rounded-2xl shadow-xl overflow-hidden">
    
            <header class="px-6 py-4 border-b border-gray-200">
                <h2 class="text-lg font-semibold text-gray-900">Add a Question</h2>
                <p class="text-sm text-gray-500">Create a new question for your form</p>
            </header>
            <main class="p-6 space-y-5">
                <label class="block text-sm font-medium text-gray-700">
                    Question
                    <input name="question-text" type="text" required class="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-smfocus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"/>
                </label>
            
                <label class="block text-sm font-medium text-gray-700">
                    Type
                    <select name="question-type" class="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500">
                        ${options}
                    </select>
                </label>
            </main>
            <footer class="flex justify-end gap-3 px-6 py-4 border-t border-gray-200 bg-gray-50">

                <button type="button" command="close" commandfor="dialog" class="px-4 py-2 text-sm rounded-lg border border-gray-300 hover:bg-gray-100" name="cancel">
                    Cancel
                </button>
    
                <button type="submit" class="px-4 py-2 text-sm rounded-lg bg-blue-600 text-white hover:bg-blue-700focus:outline-none focus:ring-2 focus:ring-blue-500">
                    Next
                </button>
    
            </footer>
    </form>`);

    const submitButton = component.querySelector('button[type="submit"]')!;
    const cancelButton = component.elements.namedItem('cancel')! as HTMLSelectElement;
    const selectElement = component.elements.namedItem('question-type')! as HTMLSelectElement;
    const questionInput = component.elements.namedItem('question-text')! as HTMLInputElement;
    selectElement.addEventListener('change', updateSubmitButton);
    cancelButton.addEventListener('cancel', cancel);

    component.addEventListener('submit', submit);

    let lastType: QuestionType;
    updateSubmitButton();


    return component;

    function submit(submit: SubmitEvent) {
        submit.preventDefault();

        const typeValue = selectElement.value as QuestionType;
        const question = questionInput.value;

        let single = true;
        switch (typeValue) {
            case QuestionType.MultipleChoice:
                single = false;
            case QuestionType.SingleChoice:
                modal.setPage(createAddAnswersComponent(modal, question, single))
                break;
            case QuestionType.Open:
                console.log('submit');
                break;
            case QuestionType.Range:
                modal.setPage(createSetRangeAnswerComponent(modal, question))
                break;
        }
    }

    function updateSubmitButton() {
        const newType = selectElement.value as QuestionType;
        if (lastType == QuestionType.Open && newType != QuestionType.Open) {
            submitButton.innerHTML = 'Next';
        } else if (lastType != QuestionType.Open && newType == QuestionType.Open) {
            submitButton.innerHTML = 'Create';
        }
        lastType = newType;
    }

    function cancel() {
        modal.close();
    }
}