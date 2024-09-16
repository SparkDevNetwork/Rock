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

import PickExistingFilter from "../Filters/pickExistingFilter.partial.obs";
import { Component, defineComponent, PropType } from "vue";
import { AttributeFieldDefinitionBag } from "@Obsidian/ViewModels/Core/Grid/attributeFieldDefinitionBag";
import { ColumnFilter } from "@Obsidian/Types/Controls/grid";
import { pickExistingFilterMatches } from "@Obsidian/Core/Controls/grid";
import TextSkeletonCell from "../Cells/textSkeletonCell.partial.obs";

export default defineComponent({
    props: {
        attributes: {
            type: Array as PropType<AttributeFieldDefinitionBag[]>,
            default: []
        },

        __attributeColumns: {
            type: Boolean as PropType<boolean>,
            default: true
        },

        filter: {
            type: Object as PropType<ColumnFilter>,
            default: {
                component: PickExistingFilter,
                matches: pickExistingFilterMatches
            } as ColumnFilter
        },

        skeletonComponent: {
            type: Object as PropType<Component>,
            default: TextSkeletonCell
        },

        columnType: {
            type: String as PropType<string>,
            default: "attribute"
        }
    }
});
