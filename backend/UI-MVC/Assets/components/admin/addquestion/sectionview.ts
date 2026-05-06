export {sectionView};

function sectionView(section: string) {
    return `
<details>
    <summary class="flex items-center draggable-question gap-2 cursor-grab">
        <h3 class="leading-none grow">${section}</h3>
        <span class="icon-button"></span>
        <button class="icon-button select-icon">Select</button>
    </summary>
    <ol class="w-full grid gap-2 mt-2">
    </ol>
</details>`;
}