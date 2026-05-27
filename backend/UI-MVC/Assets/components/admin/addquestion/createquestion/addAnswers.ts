import {PagedModalComponent} from "../../../pagedModal.ts";
import {htmlToElement} from "../../../../utils/dom.ts";
import {createDragAndDropListComponent} from "../../../dragAndDropList.ts";
import {AnswerComponent, createAnswerComponent} from "./answerComponent.ts";
import {createQuestionComponent} from "../question.ts";
import {rootQuestionList} from "../../../../modules/addQuestionPage.ts";
import {Answer, FixedQuestion} from "../../../../models/question.ts";

type AddAnswersComponent = HTMLFormElement;

export {createAddAnswersComponent};

function createAddAnswersComponent(
    dialog: PagedModalComponent,
    question: FixedQuestion,
    onSave?: (updatedQuestion: FixedQuestion) => void
): AddAnswersComponent {
    const component = htmlToElement<AddAnswersComponent>(`
    <form method="dialog" class="question-modal">
            <header class="question-modal-header">
                <h2 class="question-modal-title">Add answers</h2>
                <p class="question-modal-subtitle">Add possible answers to the question.</p>
            </header>
            <main class="question-modal-body">
                <button type="button" class="question-modal-btn" name="add-answer">Add answer</button>
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

    const mainContainer = component.querySelector('main') as HTMLElement;
    const answerList = createDragAndDropListComponent(htmlToElement(`<div class="space-y-2"></div>`));
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
        if (onSave) {
            onSave(question);
        } else {
            rootQuestionList.addElement(createQuestionComponent(question))
        }
    }

    function previous() {
        dialog.back();
    }
}
