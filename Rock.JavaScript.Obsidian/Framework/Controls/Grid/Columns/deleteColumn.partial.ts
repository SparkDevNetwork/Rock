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
import DeleteCell from "../Cells/deleteCell.partial.obs";
import { IGridState } from "@Obsidian/Types/Controls/grid";

export default defineComponent({
    props: {
        ...standardColumnProps,

        name: {
            type: String as PropType<string>,
            default: "__delete"
        },

        formatComponent: {
            type: Object as PropType<Component>,
            default: DeleteCell
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

        columnType: {
            type: String as PropType<string>,
            default: "delete"
        },

        /**
         * Disables the normal confirmation message displayed before calling
         * the click handler.
         */
        disableConfirmation: {
            type: Boolean as PropType<boolean>,
            default: false
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
         * Called when the delete button has been clicked and the confirmation
         * has been approved.
         */
        onClick: {
            type: Function as PropType<(key: string, grid: IGridState) => (void | Promise<void>)>,
            required: false
        }
    }
});
