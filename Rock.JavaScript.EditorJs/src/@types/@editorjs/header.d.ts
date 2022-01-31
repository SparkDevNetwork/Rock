declare module "@editorjs/header" {
    import { BlockTool, SanitizerConfig } from "@editorjs/editorjs";

    export interface HeaderData {
        level: number;

        text: string;
    }

    export default class Header implements BlockTool {
        sanitize?: SanitizerConfig | undefined;
        save(block: HTMLElement): HeaderData;
        render(): HTMLElement;
    }
}
