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
    }
});
