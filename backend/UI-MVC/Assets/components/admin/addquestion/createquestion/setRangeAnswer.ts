import {createQuestionComponent} from "../question.ts";
import {PagedModalComponent} from "../../../pagedModal.ts";
import {htmlToElement} from "../../../../utils/dom.ts";
import {rootQuestionList} from "../../../../modules/addQuestionPage.ts";
import {RangeQuestion} from "../../../../models/question.ts";


export type {SetRangeAnswerComponent};
export {createSetRangeAnswerComponent};

type SetRangeAnswerComponent = HTMLFormElement;

function createSetRangeAnswerComponent(
    modal: PagedModalComponent,
    question: RangeQuestion,
    onSave?: (updatedQuestion: RangeQuestion) => void
): SetRangeAnswerComponent {
    const component = htmlToElement<SetRangeAnswerComponent>(`
    <form method="dialog" class="question-modal">
            <header class="question-modal-header">
                <h2 class="question-modal-title">Add a Question</h2>
                <p class="question-modal-subtitle">Create a new question for your form</p>
            </header>
            <main class="question-modal-body">
                <label class="block text-sm font-medium text-zinc-700">
                    Min
                    <input type="number" min="0" step="1" value="0" name="min" class="question-modal-input">
                </label>
                <label class="block text-sm font-medium text-zinc-700">
                    Max
                    <input type="number" min="0" step="1" value="10" name="max" class="question-modal-input">
                </label>
            </main>
            <footer class="question-modal-footer">

                <button type="button" class="question-modal-btn" name="previous">
                    Previous
                </button>
    
                <button type="submit" class="question-modal-btn question-modal-btn-primary">
                    Create
                </button>
    
            </footer>
    </form>`);

    const previousButton = component.elements.namedItem('previous') as HTMLButtonElement;
    const minInput = component.elements.namedItem('min') as HTMLInputElement;
    const maxInput = component.elements.namedItem('max') as HTMLInputElement;
    previousButton.addEventListener('click', previous);

    component.addEventListener('submit', submit);

    return component;

    function submit(submit: SubmitEvent) {
        submit.preventDefault();
        const min: number = parseInt(minInput.value);
        const max: number = parseInt(maxInput.value);
        question.min = min;
        question.max = max;
        if (onSave) {
            onSave(question);
        } else {
            rootQuestionList.addElement(createQuestionComponent(question));
        }
        modal.destroy();
    }

    function previous() {
        modal.back();
    }
}
