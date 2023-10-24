import { standardColumnProps } from "@Obsidian/Core/Controls/grid";
import { Component, defineComponent, PropType } from "vue";
import CopyCell from "../Cells/copyCell.partial.obs";
import { IGridState } from "@Obsidian/Types/Controls/grid";

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
        },

        onClick: {
            type: Function as PropType<(key: string, grid: IGridState) => (void | Promise<void>)>,
            required: false
        }
    }
});
