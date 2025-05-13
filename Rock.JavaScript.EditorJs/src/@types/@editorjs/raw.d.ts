declare module "@editorjs/raw" {
    import { BlockTool, SanitizerConfig, ToolboxConfig } from "@editorjs/editorjs";

    export interface RawData {
        text: string;
    }

    export default class Raw implements BlockTool {
        constructor(config: any);

        sanitize?: SanitizerConfig | undefined;
        save(block: HTMLElement): RawData;
        render(): HTMLElement;

        static get toolbox(): ToolboxConfig;
    }
}
