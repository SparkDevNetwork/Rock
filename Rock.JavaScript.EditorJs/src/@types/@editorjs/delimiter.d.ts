declare module "@editorjs/delimiter" {
    import { BlockTool, SanitizerConfig } from "@editorjs/editorjs";

    export interface DelimiterData {
    }

    export default class Delimiter implements BlockTool {
        sanitize?: SanitizerConfig | undefined;

        save(block: HTMLElement): DelimiterData;
        render(): HTMLElement;
    }
}
