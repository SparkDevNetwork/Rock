declare module "@editorjs/quote" {
    import { BlockTool, SanitizerConfig } from "@editorjs/editorjs";

    export interface QuoteData {
        text: string;
        caption?: string;
        alignment?: "left" | "center";
    }

    export default class Quote implements BlockTool {
        sanitize?: SanitizerConfig | undefined;
        save(block: HTMLElement): QuoteData;
        render(): HTMLElement;
    }
}
