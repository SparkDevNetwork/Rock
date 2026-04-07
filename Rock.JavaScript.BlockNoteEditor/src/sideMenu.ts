import type { Block, BlockNoteEditor } from "@blocknote/core";
import { createIconButton } from "./functions";
import { SideMenuExtension } from "@blocknote/core/extensions";

export class SideMenu {
    public readonly element: HTMLDivElement;

    private addButton: HTMLButtonElement;
    private actionButton: HTMLButtonElement;

    private block: Block<any, any, any> | null = null;

    constructor(editor: BlockNoteEditor, container: HTMLElement) {
        this.addButton = createIconButton('ti ti-plus');
        this.actionButton = createIconButton('ti ti-grip-vertical');
        this.actionButton.type = "button";
        this.actionButton.draggable = true;

        const sideMenuExtension = editor.getExtension(SideMenuExtension)!;

        this.addButton.addEventListener('click', () => {
            const anchorBlock = this.block!;
            /*const newBlocks = */editor.insertBlocks([{ type: "paragraph" }], anchorBlock, "after");

            // editor.setSelection(newBlocks[0], anchorBlock);
        });

        this.actionButton.addEventListener('dragstart', (event) => {
            sideMenuExtension.blockDragStart(event, this.block!);
        });

        this.actionButton.addEventListener('dragend', () => {
            sideMenuExtension.blockDragEnd();
        });

        this.actionButton.addEventListener('click', () => {
            console.log('Action button clicked');
        });

        this.element = document.createElement('div');
        this.element.classList.add("bn-side-menu");
        this.element.appendChild(this.addButton);
        this.element.appendChild(this.actionButton);

        editor.getExtension(SideMenuExtension)?.store.subscribe((state) => {
            this.block = editor.getTextCursorPosition().block;

            if (!state.currentVal?.show) {
                this.element.remove();
                return;
            }

            this.block = state.currentVal.block;
            const box = state.currentVal.referencePos;

            container.appendChild(this.element);

            const sideMenuRect = this.element.getBoundingClientRect();
            const containerRect = container.getBoundingClientRect();
            this.element.style.left = "0px";
            this.element.style.top = "0px";
            this.element.style.transform = `translate(${box.x - containerRect.left - sideMenuRect.width}px, ${box.y - containerRect.top}px)`;
        });
    }
}
