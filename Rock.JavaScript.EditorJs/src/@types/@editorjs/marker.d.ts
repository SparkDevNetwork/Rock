declare module "@editorjs/marker" {
    import { InlineTool, SanitizerConfig } from "@editorjs/editorjs";

    export default class Marker implements InlineTool {
        shortcut?: string;

        surround(range: Range): void;
        checkState(selection: Selection): boolean;
        render(): HTMLElement;
    }
}
