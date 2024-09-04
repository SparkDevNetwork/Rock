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

import { Component, defineComponent, PropType } from "vue";
import { standardColumnProps } from "@Obsidian/Core/Controls/grid";
import ButtonCell from "../Cells/buttonCell.partial.obs";
import { IGridState } from "@Obsidian/Types/Controls/grid";

/**
 * A column that displays a single button with an icon on it.
 */
export default defineComponent({

    props: {
        ...standardColumnProps,

        /**
         * Called when the button has been clicked.
         */
        onClick: {
            type: Function as PropType<(key: string, grid: IGridState) => (void | Promise<void>)>,
            required: false
        },

        /**
         * The icon CSS class to use as the content for the button.
         */
        iconClass: {
            type: String as PropType<string>,
            required: true
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

        formatComponent: {
            type: Object as PropType<Component>,
            default: ButtonCell
        },

        columnType: {
            type: String as PropType<string>,
            default: "button"
        },
    },
});
