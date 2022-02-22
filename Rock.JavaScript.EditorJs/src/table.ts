import { BlockToolConstructorOptions } from "@editorjs/editorjs";
import EditorTable, { TableData, TableConfig as EditorTableConfig } from "./vendor/table/src/plugin"

interface TableConfig extends EditorTableConfig {
    /** True if a new table should have headings enabled. */
    defaultHeadings?: boolean;
}

/**
 * Custom implementation of the Table block.
 */
export class Table extends EditorTable {
    constructor(config: BlockToolConstructorOptions<TableData, TableConfig>) {
        config.data = config.data || {};
        if (config.data.withHeadings === undefined && config.data.content === undefined) {
            config.data.withHeadings = config.config?.defaultHeadings ?? false;
        }

        super(config);
    }

    /**
     * Checks if the block's save data should be included in the structured
     * content stream.
     *
     * @param savedData The data that was returned by the save method.
     * @returns true if the block's contents should be included in the save data.
     */
    public validate(savedData: TableData) {
        return savedData.content.length > 0;
    }
}
