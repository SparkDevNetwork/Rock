declare module "@editorjs/nested-list" {
    import { BlockTool, BlockToolConstructorOptions, BlockToolData, SanitizerConfig } from "@editorjs/editorjs";

    export interface NestedListItem {
        content: string;
        items: Array<NestedListItem>;
    }

    export interface NestedListData {
        style: "ordered" | "unordered";
        items: Array<NestedListItem>;
    }

    export default class NestedList implements BlockTool {
        sanitize?: SanitizerConfig | undefined;

        constructor(config: BlockToolConstructorOptions<NestedListData>);
        save(block: HTMLElement): NestedListData;
        render(): HTMLElement;
    }
}
