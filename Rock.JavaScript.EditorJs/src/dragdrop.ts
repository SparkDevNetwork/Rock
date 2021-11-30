import EditorDragDrop from "editorjs-drag-drop";
import EditorJS from "@editorjs/editorjs";

/**
 * Custom subclass that fixes a bug with drag/drop when only one block.
 */
export class DragDrop extends EditorDragDrop {
    constructor(editor: EditorJS) {
        super(editor);

        this.setDragEndListener();
    }

    /**
     * Sets the drag end events listener. This clears our drag operation
     * so we don't fire during an external drag even (like an image being
     * dragged into the editor).
     */
    protected setDragEndListener() {
        if (!this.readOnly) {
            const settingsButton = this.holder.querySelector('.ce-toolbar__settings-btn');

            settingsButton!.setAttribute('draggable', 'true');
            settingsButton!.addEventListener('dragend', () => {
                this.startBlock = null;
            });
        }
    }

    /**
     * Gets the real drop target element.
     * 
     * @param target The target of the drop operation.
     * @returns The real drop target or null if invalid.
     */
    protected getDropTarget(target: HTMLElement) {
        const block = super.getDropTarget(target);

        /* Bug in editor.js when trying to move block when there is only one
         * block. */
        if (block !== null && super.getTargetPosition(block) === this.startBlock) {
            return null;
        }

        return block;
    }

    /**
     * Moves the dragged element to the drop position.
     *
     * @see {@link https://editorjs.io/blocks#move}
     */
    protected moveBlocks() {
        /* Ensure this drop operation is happening for our drag operation. */
        if (this.startBlock != null && this.endBlock != null) {
            super.moveBlocks();
        }
    }
}
