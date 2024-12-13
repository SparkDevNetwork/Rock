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
import { Component, PropType, defineComponent } from "vue";
import HighlightDetailCell from "../Cells/highlightDetailCell.partial.obs";
import HighlightDetailSkeletonCell from "../Cells/highlightDetailSkeletonCell.partial.obs";
import { ColumnDefinition, ExportValueFunction, IGridState, QuickFilterValueFunction } from "@Obsidian/Types/Controls/grid";
import { extractText } from "@Obsidian/Utility/component";

/**
 * Gets the value to use when exporting a cell of this column.
 *
 * @param row The row that will be filtered.
 * @param column The column that will be filtered.
 *
 * @returns A string value or undefined if the cell has no value.
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
 * Gets the value to use when quick filtering a cell of this column.
 *
 * @param row The row that will be filtered.
 * @param column The column that will be filtered.
 * @param grid The grid state for this cell.
 *
 * @returns A string value or undefined if the cell has no value.
 */
function getQuickFilterValue(row: Record<string, unknown>, column: ColumnDefinition, grid: IGridState): string | undefined {
    if (!column.field) {
        return undefined;
    }

    const value = row[column.field] ?? "";

    if (typeof value !== "string") {
        return undefined;
    }

    if (typeof column.props.detailField === "string") {
        const detailValue = row[column.props.detailField];

        if (typeof detailValue === "string") {
            return `${value} ${detailValue}`;
        }
    }
    else if (column.slots.detailFormat) {
        const component = column.slots.detailFormat;
        const cellProps = {
            column,
            row,
            grid
        };

        const detailValue = extractText(component, cellProps);

        return `${value} ${detailValue}`;
    }

    return value;
}

export default defineComponent({
    props: {
        ...standardColumnProps,

        formatComponent: {
            type: Object as PropType<Component>,
            default: HighlightDetailCell
        },

        skeletonComponent: {
            type: Object as PropType<Component>,
            default: HighlightDetailSkeletonCell
        },

        quickFilterValue: {
            type: Function as PropType<QuickFilterValueFunction>,
            default: getQuickFilterValue
        },

        exportValue: {
            type: Function as PropType<ExportValueFunction>,
            default: getExportValue
        },

        columnType: {
            type: String as PropType<string>,
            default: "text"
        },

        /** The field to use to populate the detail line. */
        detailField: {
            type: String as PropType<string>,
            required: false
        },
    }
});
