declare module "@editorjs/inline-code"
{
    import { InlineTool } from "@editorjs/editorjs";

    export default class InlineCode implements InlineTool {
        shortcut?: string | undefined;
        surround(range: Range): void;
        checkState(selection: Selection): boolean;
        render(): HTMLElement;
    }
}
