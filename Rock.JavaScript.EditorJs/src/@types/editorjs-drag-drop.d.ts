declare module "editorjs-drag-drop" {
    import EditorJS from "@editorjs/editorjs";

    export default class DragDrop {
        protected startBlock: number | null;
        protected endBlock: number | null;
        protected readOnly: boolean;
        protected holder: HTMLElement;

        constructor(editor: EditorJS);

        protected getDropTarget(target: HTMLElement): HTMLElement | null;
        protected getTargetPosition(target: HTMLElement): number;
        protected moveBlocks(): void;
    }
}
