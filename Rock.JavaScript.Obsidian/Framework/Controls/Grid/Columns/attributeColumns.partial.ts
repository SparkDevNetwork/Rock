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

import AttributeCell from "../Cells/attributeCell.partial.obs";
import PickExistingFilter from "../Filters/pickExistingFilter.partial.obs";
import { Component, defineComponent, PropType } from "vue";
import { AttributeFieldDefinitionBag } from "@Obsidian/ViewModels/Core/Grid/attributeFieldDefinitionBag";
import { ColumnDefinition, ColumnFilter, ExportValueFunction, FilterValueFunction, QuickFilterValueFunction, SortValueFunction } from "@Obsidian/Types/Controls/grid";
import { pickExistingFilterMatches } from "@Obsidian/Core/Controls/grid";
import TextSkeletonCell from "../Cells/textSkeletonCell.partial.obs";

/**
 * Gets the value to use for text-based operations on a cell of this column.
 * This includes exporting, sorting and filtering.
 *
 * @param row The row.
 * @param column The column.
 *
 * @returns A string value or undefined if the cell has no value.
 */
function getTextValue(row: Record<string, unknown>, column: ColumnDefinition): string | undefined {
    if (!column.field) {
        return undefined;
    }

    const value = row[column.field];

    if (!value || typeof value !== "object") {
        return undefined;
    }

    if (typeof value["text"] !== "string") {
        return undefined;
    }

    return value["text"];
}

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

        formatComponent: {
            type: Object as PropType<Component>,
            default: AttributeCell
        },

        /**
         * The function that will be called when exporting cells in this column.
         * If not provided then the text value from the column format template
         * will be used instead.
         */
        exportValue: {
            type: Function as PropType<ExportValueFunction>,
            default: getTextValue
        },

        /**
         * Specifies how to get the sort value to use when sorting by this column.
         * This will override the `sortField` setting. If a function is be provided
         * then it will be called with the row and the column definition and must
         * return either a string, number or undefined. If a string is provided
         * then it will be used as a Lava Template which will be passed the `row`
         * object used to calculate the value. If no `title` is specified then the
         * column will not be sortable.
         */
        sortValue: {
            type: Function as PropType<SortValueFunction>,
            default: getTextValue
        },

        /**
         * Overrides the default method of obtaining the value to use when matching
         * against the quick filter. If not specified then the value of of the row
         * in the `field` property will be used if it is a supported type. A
         * function may be specified which will be called with the row and column
         * definition and must return either a string or undefined. If a plain
         * string is specified then it will be used as a Lava Template which will
         * be passed the `row` object.
         */
        quickFilterValue: {
            type: Function as PropType<QuickFilterValueFunction>,
            default: getTextValue
        },

        /**
         * Specifies how to get the value to use when filtering by this column.
         * This is used on combination with the `filter` setting only. If a
         * function is be provided then it will be called with the row and the
         * column definition and must return a value recognized by the filter.
         * If a string is provided then it will be used as a Lava Template which
         * will be passed the `row` object used to calculate the value.
         */
        filterValue: {
            type: Function as PropType<FilterValueFunction>,
            default: getTextValue
        },

        /**
         * The skeleton that will be used while the grid contents are still
         * being loaded.
         */
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
