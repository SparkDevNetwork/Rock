declare module "@editorjs/warning" {
    import { BlockTool, SanitizerConfig } from "@editorjs/editorjs";

    export interface WarningData {
        title: string;
        message: string;
    }

    export interface WarningConfig {
        titlePlaceholder?: string;
        warningPlaceholder?: string;
    }

    export default class Warning implements BlockTool {
        sanitize?: SanitizerConfig | undefined;

        save(block: HTMLElement): WarningData;
        render(): HTMLElement;
    }
}
