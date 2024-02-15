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

/**
 * This is a special placeholder column that informs the grid where to place
 * dynamic columns that will hold the entity attribute values.
 */
export default defineComponent({
    props: {
        /**
         * A collection of objects that define the attributes that will be
         * displayed in the grid.
         */
        attributes: {
            type: Array as PropType<AttributeFieldDefinitionBag[]>,
            default: []
        },

        /**
         * A special placeholder used by the grid to detect that this is the
         * column to be replaced with the actual attribute columns.
         */
        __attributeColumns: {
            type: Boolean as PropType<boolean>,
            default: true
        },

        /**
         * The filter component to used for filtering these attributes.
         */
        filter: {
            type: Object as PropType<ColumnFilter | null>,
            default: {
                component: PickExistingFilter,
                matches: pickExistingFilterMatches
            } as ColumnFilter
        },

        /**
         * The skeleton that will be used while the grid contents are still
         * being loaded.
         */
        skeletonComponent: {
            type: Object as PropType<Component>,
            default: TextSkeletonCell
        }
    }
});
