import { rootQuestionList, getListQuestions } from '../../modules/addQuestionPage.ts';
import { createQuestionComponent } from '../admin/addquestion/question.ts';
import type { Question } from '../../models/question.ts';

function getHiddenField(): HTMLInputElement | null {
    return document.querySelector<HTMLInputElement>(
        '#create-project-step2-form input[name="CreateStep2ViewModel.QuestionsJson"]'
    );
}

function getForm(): HTMLFormElement | null {
    return document.getElementById('create-project-step2-form') as HTMLFormElement | null;
}

function syncToForm(): void {
    const field = getHiddenField();
    const form = getForm();
    if (!field || !form) return;

    const questions = getListQuestions(rootQuestionList);
    field.value = JSON.stringify(questions);
    console.log(field.value);
    form.dispatchEvent(new Event('input', { bubbles: true }));
}

let observer: MutationObserver | null = null;

function connectObserver(): void {
    observer = new MutationObserver(syncToForm);
    observer.observe(rootQuestionList, { childList: true });
}

function hydrateFromForm(): void {
    observer?.disconnect();

    while (rootQuestionList.firstElementChild) {
        rootQuestionList.firstElementChild.remove();
    }

    const json = getHiddenField()?.value.trim() ?? '';
    if (json && json !== '[]') {
        try {
            const questions = JSON.parse(json) as Question[];
            if (Array.isArray(questions)) {
                for (const q of questions) {
                    rootQuestionList.addElement(createQuestionComponent(q));
                }
            }
        } catch {
            // ignore malformed JSON
        }
    }

    connectObserver();
    syncToForm();
}

document.addEventListener('DOMContentLoaded', () => {
    const stepper = document.getElementById('dynamic-stepper');
    stepper?.addEventListener('stepper:step-enter', ((event: CustomEvent) => {
        if (event.detail?.step === 2) {
            hydrateFromForm();
        }
    }) as EventListener);

    hydrateFromForm();
});
