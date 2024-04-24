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
import NumberBadgeCell from "../Cells/numberBadgeCell.partial.obs";
import NumberBadgeSkeletonCell from "../Cells/numberBadgeSkeletonCell.partial.obs";
import { ColumnDefinition, ExportValueFunction } from "@Obsidian/Types/Controls/grid";

/**
 * Gets the value to use when exporting a cell of this column.
 *
 * @param row The row that will be exported.
 * @param column The column that will be exported.
 *
 * @returns A number value or undefined if the cell has no value.
 */
function getExportValue(row: Record<string, unknown>, column: ColumnDefinition): number | undefined {
    if (!column.field) {
        return undefined;
    }

    const value = row[column.field];

    if (typeof value !== "number") {
        return undefined;
    }

    return value;
}

export default defineComponent({
    props: {
        ...standardColumnProps,

        formatComponent: {
            type: Object as PropType<Component>,
            default: NumberBadgeCell
        },

        skeletonComponent: {
            type: Object as PropType<Component>,
            default: NumberBadgeSkeletonCell
        },

        exportValue: {
            type: Function as PropType<ExportValueFunction>,
            default: getExportValue
        },

        columnType: {
            type: String as PropType<string>,
            default: "numberBadge"
        },

        /**
         * A value greater than or equal to `hideMinimum` and less than or
         * equal to `hideMaximum` will be hidden.
         */
        hideMinimum: {
            type: Number as PropType<number>,
            required: false
        },

        /**
         * A value greater than or equal to `hideMinimum` and less than or
         * equal to `hideMaximum` will be hidden.
         */
        hideMaximum: {
            type: Number as PropType<number>,
            required: false
        },

        /**
         * A value greater than or equal to `infoMinimum` and less than or
         * equal to `infoMaximum` will draw the badge with the info color
         * scheme. This will take precedence over the hide properties.
         */
        infoMinimum: {
            type: Number as PropType<number>,
            required: false
        },

        /**
         * A value greater than or equal to `infoMinimum` and less than or
         * equal to `infoMaximum` will draw the badge with the info color
         * scheme. This will take precedence over the hide properties.
         */
        infoMaximum: {
            type: Number as PropType<number>,
            required: false
        },

        /**
         * A value greater than or equal to `successMinimum` and less than or
         * equal to `successMaximum` will draw the badge with the success color
         * scheme. This will take precedence over the info properties.
         */
        successMinimum: {
            type: Number as PropType<number>,
            required: false
        },

        /**
         * A value greater than or equal to `successMinimum` and less than or
         * equal to `successMaximum` will draw the badge with the success color
         * scheme. This will take precedence over the info properties.
         */
        successMaximum: {
            type: Number as PropType<number>,
            required: false
        },

        /**
         * A value greater than or equal to `warningMinimum` and less than or
         * equal to `warningMaximum` will draw the badge with the warning color
         * scheme. This will take precedence over the success properties.
         */
        warningMinimum: {
            type: Number as PropType<number>,
            required: false
        },

        /**
         * A value greater than or equal to `warningMinimum` and less than or
         * equal to `warningMaximum` will draw the badge with the warning color
         * scheme. This will take precedence over the success properties.
         */
        warningMaximum: {
            type: Number as PropType<number>,
            required: false
        },

        /**
         * A value greater than or equal to `dangerMinimum` and less than or
         * equal to `dangerMaximum` will draw the badge with the danger color
         * scheme. This will take precedence over the warning properties.
         */
        dangerMinimum: {
            type: Number as PropType<number>,
            required: false
        },

        /**
         * A value greater than or equal to `dangerMinimum` and less than or
         * equal to `dangerMaximum` will draw the badge with the danger color
         * scheme. This will take precedence over the warning properties.
         */
        dangerMaximum: {
            type: Number as PropType<number>,
            required: false
        }
    }
});
