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
import HtmlCell from "../Cells/htmlCell.partial.obs";
import HtmlSkeletonCell from "../Cells/htmlSkeletonCell.partial.obs";
import { standardColumnProps } from "@Obsidian/Core/Controls/grid";
import { ColumnDefinition, ExportValueFunction, FilterValueFunction, IGridState, QuickFilterValueFunction } from "@Obsidian/Types/Controls/grid";

/**
 * Gets the value to use when exporting a cell in this column.
 *
 * @param row The row that will be exported.
 * @param column The column that will be exported.
 *
 * @returns A number value or undefined if the cell has no value.
 */
function getExportValue(row: Record<string, unknown>, column: ColumnDefinition): string | undefined {
    if (!column.field) {
        return undefined;
    }

    const value = row[column.field];

    if (typeof value !== "string") {
        return undefined;
    }

    return value;
}

/**
 * Gets the value to use when performing filtering of a cell in this
 * column.
 *
 * @param _row The row that will be filtered.
 * @param _column The column that will be filtered.
 * @param _grid The state object that describes the grid.
 *
 * @returns A string value or undefined if the cell has no compatible value.
 */
function getFilterValue(_row: Record<string, unknown>, _column: ColumnDefinition, _grid: IGridState): undefined {
    // Disable filtering.
    return undefined;
}

/**
 * Gets the value to use when performing quick filtering of a cell in this
 * column.
 *
 * @param _row The row that will be filtered.
 * @param _column The column that will be filtered.
 *
 * @returns A string value or undefined if the cell has no compatible value.
 */
function getQuickFilterValue(_row: Record<string, unknown>, _column: ColumnDefinition): undefined {
    // Disable quick filtering.
    return undefined;
}

export default defineComponent({
    props: {
        ...standardColumnProps,

        formatComponent: {
            type: Object as PropType<Component>,
            default: HtmlCell
        },

        skeletonComponent: {
            type: Object as PropType<Component>,
            default: HtmlSkeletonCell
        },

        filterValue: {
            type: Object as PropType<FilterValueFunction>,
            default: getFilterValue
        },

        quickFilterValue: {
            type: Object as PropType<QuickFilterValueFunction>,
            default: getQuickFilterValue
        },

        exportValue: {
            type: Function as PropType<ExportValueFunction>,
            default: getExportValue
        },

        disableSort: {
            type: Boolean as PropType<boolean>,
            default: true
        },

        columnType: {
            type: String as PropType<string>,
            default: "html"
        },
    }
});