import {createQuestionComponent} from "../question.ts";
import {PagedModalComponent} from "../../../pagedModal.ts";
import {htmlToElement} from "../../../../utils/dom.ts";
import {rootQuestionList} from "../../../../modules/addQuestionPage.ts";
import {RangeQuestion} from "../../../../models/question.ts";


export type {SetRangeAnswerComponent};
export {createSetRangeAnswerComponent};

type SetRangeAnswerComponent = HTMLFormElement;

function createSetRangeAnswerComponent(modal: PagedModalComponent, question: RangeQuestion): SetRangeAnswerComponent {
    const component = htmlToElement<SetRangeAnswerComponent>(`
    <form method="dialog" class="w-full bg-white rounded-2xl shadow-xl overflow-hidden">
    
            <header class="px-6 py-4 border-b border-gray-200">
                <h2 class="text-lg font-semibold text-gray-900">Add a Question</h2>
                <p class="text-sm text-gray-500">Create a new question for your form</p>
            </header>
            <main class="p-6 space-y-5">
                <label>
                    Min
                    <input type="number" min="0" step="1" value="0" name="min">
                </label>
                <label>
                    Max
                    <input type="number" min="0" step="1" value="10" name="max">
                </label>
            </main>
            <footer class="flex justify-end gap-3 px-6 py-4 border-t border-gray-200 bg-gray-50">

                <button type="button" class="px-4 py-2 text-sm rounded-lg border border-gray-300 hover:bg-gray-100" name="previous">
                    Previous
                </button>
    
                <button type="submit" class="px-4 py-2 text-sm rounded-lg bg-blue-600 text-white hover:bg-blue-700focus:outline-none focus:ring-2 focus:ring-blue-500">
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
        rootQuestionList.addElement(createQuestionComponent(question));
        modal.destroy();
    }

    function previous() {
        modal.back();
    }
}