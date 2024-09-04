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
import RockFieldCell from "../Cells/rockFieldCell.partial.obs";
import { ColumnDefinition, ExportValueFunction } from "@Obsidian/Types/Controls/grid";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";

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

/**
 * Allows a single rock field value to be displayed. This is a special column and
 * should be considered internal to Rock and not used by plugins.
 *
 * @private
 */
export default defineComponent({
    props: {
        ...standardColumnProps,

        formatComponent: {
            type: Object as PropType<Component>,
            default: RockFieldCell
        },

        exportValue: {
            type: Function as PropType<ExportValueFunction>,
            default: getExportValue
        },

        /**
         * Defines the attribute that represents this column. It will be used
         * to format the value for display.
         */
        attribute: {
            type: Object as PropType<PublicAttributeBag>,
            required: true
        }
    }
});
