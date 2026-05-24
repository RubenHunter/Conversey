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
            <img src="/Assets/drag.svg" alt="drag-icon" class="icon"/>
        
            <h3 class="grow font-medium">${name}</h3>
        
            <img src="/Assets/triangle_up.svg" class="icon transition duration-200 group-open:rotate-180" alt=""/>
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