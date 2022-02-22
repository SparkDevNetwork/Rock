import { BlockTool, BlockToolConstructorOptions, SanitizerConfig } from "@editorjs/editorjs";

export interface TableData {
    withHeadings: boolean;
    content: string[][];
}

export interface TableConfig {
    rows?: number;
    cols?: number;
}

export default class Table implements BlockTool {
    sanitize?: SanitizerConfig | undefined;

    constructor(config: BlockToolConstructorOptions<TableData, TableConfig>);

    save(block: HTMLElement): TableData;
    render(): HTMLElement;
}
