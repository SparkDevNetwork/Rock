declare module "@editorjs/embed" {
    import { BlockTool, BlockToolConstructorOptions, SanitizerConfig } from "@editorjs/editorjs";

    export interface EmbedData {
        service: string;
        source: string;
        embed: string;
        width: number;
        height: number;
        caption: string;
    }

    export interface EmbedConfig {
        services?: Record<string, object | boolean>;
    }

    export default class Embed implements BlockTool {
        sanitize?: SanitizerConfig | undefined;

        constructor(config: BlockToolConstructorOptions<EmbedData, EmbedConfig>);

        static prepare(config: { config: EmbedConfig }): void;

        save(block: HTMLElement): EmbedData;
        render(): HTMLElement;
    }
}
