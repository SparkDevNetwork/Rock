declare module "@editorjs/checklist" {
    import { BlockTool, SanitizerConfig } from "@editorjs/editorjs";

    export interface ChecklistData {
        items: Array<ChecklistItem>;
    }

    export interface ChecklistItem {
        text: string;
        checked: boolean;
    }

    export default class Header implements BlockTool {
        sanitize?: SanitizerConfig | undefined;
        save(block: HTMLElement): ChecklistData;
        render(): HTMLElement;
    }
}
