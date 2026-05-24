import {createQuestionPlaceholderComponent} from "./question.ts";
import {createDragAndDropListComponent, DragAndDropListComponent} from "../../dragAndDropList.ts";
import {htmlToElement} from "../../../utils/dom.ts";
import {deselect, select} from "../../../modules/addQuestionPage.ts";

export type {SectionComponent};
export {createSectionComponent};

type SectionComponent = HTMLDetailsElement & {
    questions: DragAndDropListComponent;
};

function createSectionComponent(name: string): SectionComponent {
    const selectButton = htmlToElement<HTMLInputElement>(`<input type="checkbox">`);
    selectButton.addEventListener("change", selectChange);
    const component = htmlToElement<SectionComponent>(`
    <details class="group rounded-xl border border-zinc-200 bg-white shadow-sm">
        <summary class="flex cursor-pointer items-center gap-3 p-4 list-none">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 15 24" fill="currentColor" class="icon">
                <circle cx="3" cy="3" r="3"/>
                <circle cx="12" cy="3" r="3"/>
                <circle cx="3" cy="12" r="3"/>
                <circle cx="12" cy="12" r="3"/>
                <circle cx="3" cy="21" r="3"/>
                <circle cx="12" cy="21" r="3"/>
            </svg>
            
       
            <h3 class="grow font-medium">${name}</h3>
        
            <svg xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 16 16" class="icon transition duration-200 group-open:rotate-180">
              <path fill-rule="evenodd" d="M7.022 1.566a1.13 1.13 0 0 1 1.96 0l6.857 11.667c.457.778-.092 1.767-.98 1.767H1.144c-.889 0-1.437-.99-.98-1.767z"/>
            </svg>
        </summary>
    </details>`);
    
    component.querySelector('summary')?.appendChild(selectButton);
    

    component.questions = createDragAndDropListComponent(createQuestionPlaceholderComponent());

    component.appendChild(component.questions);

    return component;

    function selectChange() {
        if (selectButton.checked) {
            select(component);
        } else {
            deselect(component);
        }
    }
}