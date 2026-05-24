import {PagedModalComponent} from "../../../pagedModal.ts";
import {htmlToElement} from "../../../../utils/dom.ts";
import {createDragAndDropListComponent} from "../../../dragAndDropList.ts";
import {AnswerComponent, createAnswerComponent} from "./answerComponent.ts";
import {createQuestionComponent} from "../question.ts";
import {rootQuestionList} from "../../../../modules/addQuestionPage.ts";
import {Answer, FixedQuestion} from "../../../../models/question.ts";

type AddAnswersComponent = HTMLFormElement;

export {createAddAnswersComponent};

function createAddAnswersComponent(dialog: PagedModalComponent, question: FixedQuestion): AddAnswersComponent {
    const component = htmlToElement<AddAnswersComponent>(`
    <form method="dialog" class="w-full bg-white rounded-2xl shadow-xl overflow-hidden">
    
            <header class="px-6 py-4 border-b border-gray-200">
                <h2 class="text-lg font-semibold text-gray-900">Add answers</h2>
                <p class="text-sm text-gray-500">Add possible answers to the question.</p>
            </header>
            <main class="p-6 space-y-5">
                <button type="button" name="add-answer">+</button>
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

    const mainContainer = component.querySelector('main') as HTMLElement;
    const answerList = createDragAndDropListComponent(htmlToElement(`<div class="flex items-center rounded-xl border border-zinc-200 bg-white shadow-sm h-1"></div>`));
    mainContainer.appendChild(answerList);
    const addAnswerButton = component.elements.namedItem('add-answer') as HTMLButtonElement;
    const previousButton = component.elements.namedItem('previous') as HTMLButtonElement;

    addAnswerButton.addEventListener('click', clickAddAnswer);

    component.addEventListener('submit', submit);
    previousButton.addEventListener('click', previous);

    for (const answer of question.possibleAnswers ?? []) {
        addAnswer(answer);
    }

    return component;

    function clickAddAnswer() {
        const answerComponent = addAnswer({text: ""});
        answerComponent.edit();
    }
    
    function addAnswer(answer: Answer): AnswerComponent {
        const answerComponent = createAnswerComponent(answer);
        answerList.addElement(answerComponent);
        return answerComponent;
    }

    function submit() {
        const answers: Answer[] = [];
        for (const answerListItem of answerList.children) {
            const answerComponent: AnswerComponent = answerListItem.firstChild as AnswerComponent;
            answers.push(answerComponent.answer);
        }
        question.possibleAnswers = answers;
        rootQuestionList.addElement(createQuestionComponent(question))
    }

    function previous() {
        dialog.back();
    }
}