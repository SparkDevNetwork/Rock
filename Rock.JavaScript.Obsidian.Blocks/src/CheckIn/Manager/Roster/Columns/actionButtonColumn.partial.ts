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
import ActionButtonCell from "../Cells/actionButtonCell.partial.obs";
import GenericButtonSkeletonCell from "../Cells/genericButtonSkeletonCell.partial.obs";

/**
 * Displays a button cell for the present button.
 */
export default defineComponent({
    props: {
        ...standardColumnProps,

        name: {
            type: String as PropType<string>,
            default: "actionButton"
        },

        formatComponent: {
            type: Object as PropType<Component>,
            default: ActionButtonCell
        },

        skeletonComponent: {
            type: Object as PropType<Component>,
            default: GenericButtonSkeletonCell
        },

        columnType: {
            type: String as PropType<string>,
            default: "action-button"
        },

        headerClass: {
            type: String as PropType<string>,
            default: "grid-rostercommand"
        },

        itemClass: {
            type: String as PropType<string>,
            default: "grid-rostercommand"
        },

        width: {
            type: String as PropType<string>,
            default: "60px"
        },

        visiblePriority: {
            type: String as PropType<"xs" | "sm" | "md" | "lg" | "xl">,
            default: "sm"
        },
    }
});
