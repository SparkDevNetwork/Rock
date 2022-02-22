declare module '@editorjs/code' {
    import { BlockTool, SanitizerConfig } from "@editorjs/editorjs";

    export interface CodeConfig {
        placeholder?: string;
    }

    export interface CodeData {
        code: string;
    }

    export default class Code implements BlockTool {
        constructor(config: { data: CodeData, config: CodeConfig, api: any, readOnly: boolean });

        sanitize?: SanitizerConfig | undefined;
        get data(): CodeData;
        set data(data: CodeData);

        save(codeWrapper: HTMLDivElement): CodeData;
        render(): HTMLElement;
    }
}

