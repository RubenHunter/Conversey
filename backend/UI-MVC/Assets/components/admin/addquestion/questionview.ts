import {Question} from "../../../models/question";

export {questionView};

function questionView(question: Question, index: number, section?: string) {
    return `
<section class="flex items-center draggable-question gap-2 cursor-grab">
    <h3 class="leading-none grow">${question.text}</h3>
    <button class="icon-button trash-icon" onclick="removeAt(${section}, ${index})">Delete</button>
    <button class="icon-button pencil-icon" onclick="showEditQuestionView(${question}, ${index})">Edit</button>
    <button class="icon-button select-icon" onclick="selectAt(${index})">Select</button>
</section>`;
}