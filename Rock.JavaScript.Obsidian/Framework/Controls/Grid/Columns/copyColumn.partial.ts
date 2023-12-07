import { standardColumnProps } from "@Obsidian/Core/Controls/grid";
import { Component, defineComponent, PropType } from "vue";
import CopyCell from "../Cells/copyCell.partial.obs";
import { ColumnDefinition, IGridState } from "@Obsidian/Types/Controls/grid";

function getValueFromField(row: Record<string, unknown>, column: ColumnDefinition): string {
    if (!column.field) {
        return "";
    }

    const value = row[column.field];

    if (typeof value === "string") {
        return value;
    }

    return "";
}

/**
 * Displays a copy button and places a string of text onto the browser
 * clipboard when the button is clicked. By default this will get a string
 * from the field specified by the `field` property.
 */
export default defineComponent({
    props: {
        ...standardColumnProps,

        name: {
            type: String as PropType<string>,
            default: "__copy"
        },

        formatComponent: {
            type: Object as PropType<Component>,
            default: CopyCell
        },

        valueToCopy: {
            type: Function as PropType<(row: Record<string, unknown>, column: ColumnDefinition, grid: IGridState) => string>,
            default: getValueFromField
        },

        headerClass: {
            type: String as PropType<string>,
            default: "grid-columncommand"
        },

        itemClass: {
            type: String as PropType<string>,
            default: "grid-columncommand"
        },

        width: {
            type: String as PropType<string>,
            default: "52px"
        }
    }
});
