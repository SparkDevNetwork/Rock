import { createIconButton } from "./functions";
import { SideMenuExtension, SuggestionMenu } from "@blocknote/core/extensions";
import type { RockBlock, RockBlockNoteEditor } from "./schema";

export class SideMenu {
    public readonly element: HTMLDivElement;

    private addButton: HTMLButtonElement;
    private actionButton: HTMLButtonElement;

    private block: RockBlock | null = null;

    constructor(editor: RockBlockNoteEditor, container: HTMLElement) {
        this.addButton = createIconButton('ti ti-plus');
        this.addButton.type = "button";
        this.actionButton = createIconButton('ti ti-grip-vertical');
        this.actionButton.type = "button";
        this.actionButton.draggable = true;

        const sideMenuExtension = editor.getExtension(SideMenuExtension)!;
        const suggestionMenuExtension = editor.getExtension(SuggestionMenu)!;

        this.addButton.addEventListener('click', () => {
            if (!this.block) {
                return;
            }

            const blockContent = this.block.content;
            const isBlockEmpty = blockContent !== undefined
                && Array.isArray(blockContent)
                && blockContent.length === 0;

            // If the current block is empty, open the suggestion menu for
            // the current block. Otherwise, insert a new paragraph block and
            // open the suggestion menu for that new block.
            if (isBlockEmpty) {
                editor.setTextCursorPosition(this.block);
                suggestionMenuExtension.openSuggestionMenu("/");
            }
            else {
                const newBlock = editor.insertBlocks([{ type: "paragraph" }], this.block, "after")[0];

                editor.setTextCursorPosition(newBlock);
                suggestionMenuExtension.openSuggestionMenu("/");
            }
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

            this.block = state.currentVal.block as RockBlock;
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
