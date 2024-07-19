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
import SecurityCell from "../Cells/securityCell.partial.obs";
import { IGridState } from "@Obsidian/Types/Controls/grid";

export default defineComponent({
    props: {
        ...standardColumnProps,

        name: {
            type: String as PropType<string>,
            default: "__security"
        },

        formatComponent: {
            type: Object as PropType<Component>,
            default: SecurityCell
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
            default: "security"
        },

        /**
         * The row item title to use when opening the security dialog. If a
         * plain string is provided it is the field name that contains the
         * item name. Otherwise it is a function that will be called with the
         * row and grid state and must return a string.
         */
        itemTitle: {
            type: [Function, String] as PropType<((row: Record<string, unknown>, grid: IGridState) => string) | string>,
            required: false
        },

        /**
         * The field to use to determine if the security button for a single
         * row should be disabled. If the value of this field is `true` then
         * the security button will be disabled.
         */
        disabledField: {
            type: String as PropType<string>,
            default: "isSecurityDisabled"
        }
    }
});
