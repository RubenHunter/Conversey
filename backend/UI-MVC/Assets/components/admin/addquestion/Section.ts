import {createQuestionPlaceholderComponent} from "./Question.ts";
import {createDragAndDropListComponent, DragAndDropListComponent} from "../../DragAndDropList.ts";
import {htmlToElement} from "../../../utils/dom.ts";

export type {SectionComponent};
export {createSectionComponent};

type SectionComponent = HTMLDetailsElement & {
    questions: DragAndDropListComponent;
};

function createSectionComponent(name: string): SectionComponent {
    const element = htmlToElement<SectionComponent>(`
    <details class="group rounded-xl border border-zinc-200 bg-white shadow-sm">
    <summary class="flex cursor-pointer items-center gap-3 p-4 list-none">
        <img src="/Assets/drag.svg" alt="drag-icon" class="icon"/>
    
        <h3 class="grow font-medium">${name}</h3>
    
        <img src="/Assets/triangle_up.svg" class="icon transition duration-200 group-open:rotate-180" alt=""/>
    
        <input type="checkbox">
    </summary>
    </details>`);

    element.questions = createDragAndDropListComponent(createQuestionPlaceholderComponent());

    element.appendChild(element.questions);

    return element;
}