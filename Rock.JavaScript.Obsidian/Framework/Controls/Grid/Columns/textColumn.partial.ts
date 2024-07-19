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
import TextCell from "../Cells/textCell.partial";
import TextSkeletonCell from "../Cells/textSkeletonCell.partial.obs";
import { ColumnDefinition, ExportValueFunction } from "@Obsidian/Types/Controls/grid";

/**
 * Gets the value to use when quick filtering a cell of this column.
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

        exportValue: {
            type: Function as PropType<ExportValueFunction>,
            default: getExportValue
        },

        columnType: {
            type: String as PropType<string>,
            default: "text"
        },
    }
});
