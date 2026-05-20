import {createSectionComponent} from "./Section.ts";
import {htmlToElement} from "../../../utils/dom.ts";
import {rootQuestionList} from "../../../modules/AddQuestionPage.ts";

export {createAddSectionComponent}
export type {AddSectionComponent}

type AddSectionComponent = HTMLFormElement;

type AddSectionComponentOptions = {
    onCancel: () => void;
    onSubmit: () => void;
};

function createAddSectionComponent(options: AddSectionComponentOptions): AddSectionComponent {
    const component = htmlToElement<AddSectionComponent>(`
    <form method="dialog" class="w-full bg-white rounded-2xl shadow-xl overflow-hidden">
    
            <header class="px-6 py-4 border-b border-gray-200">
                <h2 class="text-lg font-semibold text-gray-900">Add section</h2>
                <p class="text-sm text-gray-500">Add a section to group questions.</p>
            </header>
            <main class="p-6 space-y-5">
                <input type="text" name="text" required class="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-smfocus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500">
            </main>
            <footer class="flex justify-end gap-3 px-6 py-4 border-t border-gray-200 bg-gray-50">

                <button type="button" command="close" commandfor="dialog" class="px-4 py-2 text-sm rounded-lg border border-gray-300 hover:bg-gray-100" name="cancel">
                    Cancel
                </button>
    
                <button type="submit" class="px-4 py-2 text-sm rounded-lg bg-blue-600 text-white hover:bg-blue-700focus:outline-none focus:ring-2 focus:ring-blue-500">
                    Create
                </button>
    
            </footer>
    </form>`);

    const cancelButton = component.elements.namedItem('cancel') as HTMLButtonElement;
    const textInput = component.elements.namedItem('text') as HTMLInputElement;

    component.addEventListener('submit', submit);
    cancelButton.addEventListener('click', options.onCancel);


    return component;
    function submit(submit: SubmitEvent) {
        submit.preventDefault();
        rootQuestionList.addElement(createSectionComponent(textInput.value));
        options.onSubmit();
    }
}