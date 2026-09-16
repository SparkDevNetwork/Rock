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
import { TextCell, TextSkeletonCell } from "@Obsidian/Controls/grid";
import { Component, defineComponent, PropType } from "vue";
import { ColumnDefinition, ExportValueFunction, FilterValueFunction, SortValueFunction } from "@Obsidian/Types/Controls/grid";

/**
 * Recovers the natural numeric value of a cell so the column can sort and filter
 * numerically. The merge preview emits numbers as pre-formatted strings (to preserve
 * the legacy grid's separator-free display, including decimal scale such as "100.00"),
 * so the numeric value has to be parsed back out of that string for the grid's numeric
 * comparisons.
 *
 * @param row The row whose value is being inspected.
 * @param column The column the value belongs to.
 *
 * @returns The numeric value, or undefined when the cell has no parseable number.
 */
function getNumericValue(row: Record<string, unknown>, column: ColumnDefinition): number | undefined {
    if (!column.field) {
        return undefined;
    }

    const value = row[column.field];

    if (typeof value === "number") {
        return value;
    }

    if (typeof value !== "string" || value.length === 0) {
        return undefined;
    }

    const numericValue = Number(value);
    return isNaN(numericValue) ? undefined : numericValue;
}

/**
 * Returns the cell's raw display string for export so the exported value matches
 * exactly what is shown in the grid.
 *
 * @param row The row that will be exported.
 * @param column The column that will be exported.
 *
 * @returns The string value, or undefined when the cell has no string value.
 */
function getExportValue(row: Record<string, unknown>, column: ColumnDefinition): string | undefined {
    if (!column.field) {
        return undefined;
    }

    const value = row[column.field];

    return typeof value === "string" ? value : undefined;
}

/**
 * A number column that renders its value as raw text (no thousands separators,
 * preserving the server-formatted string) while still sorting and filtering
 * numerically. The merge preview grid uses this so numeric columns behave like
 * numbers without the locale formatting that the standard number column would apply.
 */
export default defineComponent({
    props: {
        ...standardColumnProps,

        formatComponent: {
            type: Object as PropType<Component>,
            default: TextCell
        },

        skeletonComponent: {
            type: Object as PropType<Component>,
            default: TextSkeletonCell
        },

        sortValue: {
            type: Function as PropType<SortValueFunction>,
            default: getNumericValue
        },

        filterValue: {
            type: Function as PropType<FilterValueFunction>,
            default: getNumericValue
        },

        exportValue: {
            type: Function as PropType<ExportValueFunction>,
            default: getExportValue
        },

        columnType: {
            type: String as PropType<string>,
            default: "number"
        },
    }
});
