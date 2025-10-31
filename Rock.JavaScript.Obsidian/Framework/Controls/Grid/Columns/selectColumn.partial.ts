// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

import { standardColumnProps } from "@Obsidian/Core/Controls/grid";
import { Component, defineComponent, PropType } from "vue";
import SelectCell from "../Cells/selectCell.partial.obs";
import SelectHeaderCell from "../Cells/selectHeaderCell.partial.obs";
import { IGridState } from "@Obsidian/Types/Controls/grid";

/**
 * Displays a checkbox that can be used to select the row for bulk operations
 * performed by grid actions.
 */
export default defineComponent({
    props: {
        ...standardColumnProps,

        name: {
            type: String as PropType<string>,
            default: "__select"
        },

        formatComponent: {
            type: Object as PropType<Component>,
            default: SelectCell
        },

        headerComponent: {
            type: Object as PropType<Component>,
            default: SelectHeaderCell
        },

        headerClass: {
            type: String as PropType<string>,
            default: "grid-select-field"
        },

        itemClass: {
            type: String as PropType<string>,
            default: "grid-select-field"
        },

        width: {
            type: String as PropType<string>,
            default: "56px"
        },

        columnType: {
            type: String as PropType<string>,
            default: "select"
        },

        // #region Row Exclusion Props

        /**
         * A function that determines if the row should be excluded from having a Select checkbox.
         * Should return true to exclude the row, false otherwise.
         */
        rowDisabled: {
            type: Function as PropType<((row: Record<string, unknown>, grid: IGridState) => boolean)>,
            default: undefined,
            required: false
        },

        /**
         * If true, the "select all" checkbox in the header will be available even if
         * rowDisabled is defined.
         *
         * This is only useful if your rows are sorted in a way that being able to select all
         * selectable rows on the current page is meaningful.
         */
        forceAllowSelectAll: {
            type: Boolean as PropType<boolean>,
            default: false,
            required: false
        }

        // #endregion Row Exclusion Props
    },
});
