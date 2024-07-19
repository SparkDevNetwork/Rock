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
import ReorderCell from "../Cells/reorderCell.partial.obs";

/**
 * Displays a standard re-order cell that can be used by the individual to drag
 * and drop the row to change the order in the list.
 */
export default defineComponent({
    props: {
        ...standardColumnProps,

        name: {
            type: String as PropType<string>,
            default: "__reorder"
        },

        formatComponent: {
            type: Object as PropType<Component>,
            default: ReorderCell
        },

        headerClass: {
            type: String as PropType<string>,
            default: "grid-columnreorder"
        },

        itemClass: {
            type: String as PropType<string>,
            default: "grid-columnreorder"
        },

        width: {
            type: String as PropType<string>,
            default: "52px"
        },

        columnType: {
            type: String as PropType<string>,
            default: "reorder"
        },

        /**
         * Called when the order of an item has changed. The first parameter
         * is the row item that was moved. The second parameter is the row item
         * it was dropped in front of or `null` if it was dropped at the end of
         * the grid. The grid rows will already be updated when this is called
         * so you only need to handle any additional logic, such as updating
         * the database.
         *
         * If `false` is returned then the the re-order operation is
         * aborted. If a Promise is returned then the grid will wait until it
         * is resolved before allowing another re-order operation.
         */
        onOrderChanged: {
            type: Function as PropType<(item: Record<string, unknown>, beforeItem: Record<string, unknown> | null) => void | Promise<void> | boolean | Promise<boolean>>,
            required: false
        }
    }
});
