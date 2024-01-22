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
import EditCell from "../Cells/editCell.partial.obs";
import { IGridState } from "@Obsidian/Types/Controls/grid";

/**
 * Shows an edit button that will call the click handler when clicked.
 */
export default defineComponent({
    props: {
        ...standardColumnProps,

        name: {
            type: String as PropType<string>,
            default: "__edit"
        },

        formatComponent: {
            type: Object as PropType<Component>,
            default: EditCell
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

        /**
        * An optional callback that will be used to determine if the delete
        * button is disabled for the specified row.
        */
        rowDisabled: {
            type: Function as PropType<((row: Record<string, unknown>, grid: IGridState) => boolean)>,
            required: false
        },

        /**
         * Called when the edit button has been clicked. If a Promise is
         * returned then the button will remain disabled until the Promise is
         * resolved.
         */
        onClick: {
            type: Function as PropType<((key: string) => void) | ((key: string) => Promise<void>)>,
            required: false
        }
    }
});
