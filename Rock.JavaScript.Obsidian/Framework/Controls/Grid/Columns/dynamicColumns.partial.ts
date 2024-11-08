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
import { ColumnFilter, FilterValueFunction } from "@Obsidian/Types/Controls/grid";
import { DynamicFieldDefinitionBag } from "@Obsidian/ViewModels/Core/Grid/dynamicFieldDefinitionBag";

export default defineComponent({
    props: {
        /**
         * The dynamic field definitions to describe the dynamic columns that
         * should be added to this grid, as well as an optional column filter
         * per field definition.
         */
        dynamicFields: {
            type: Array as PropType<(DynamicFieldDefinitionBag & { filter?: ColumnFilter, filterValue?: FilterValueFunction | string })[]>,
            default: [],
            required: true
        },

        /**
         * Leave this value as is for dynamic columns to work.
         */
        __dynamicColumns: {
            type: Boolean as PropType<boolean>,
            default: true,
            required: true
        },

        /**
         * The dynamic column components that are available to be created within
         * this grid.
         */
        columnComponents: {
            type: Object as PropType<Record<string, Component>>,
            required: false
        },

        /**
         * The default column component to use if a dynamic field's specified
         * column type is not found in the provided `columnComponents`.
         */
        defaultColumnComponent: {
            type: Object as PropType<Component>,
            required: true
        },

        /**
         * Leave this value as is for dynamic columns to work.
         */
        columnType: {
            type: String as PropType<string>,
            default: "dynamic",
            required: true
        }
    }
});
