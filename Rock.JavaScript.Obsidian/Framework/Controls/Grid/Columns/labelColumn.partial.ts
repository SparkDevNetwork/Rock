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
import LabelCell from "../Cells/labelCell.partial.obs";
import LabelSkeletonCell from "../Cells/labelSkeletonCell.partial.obs";
import { ColumnDefinition, ExportValueFunction, FilterValueFunction, QuickFilterValueFunction, SortValueFunction } from "@Obsidian/Types/Controls/grid";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

/**
 * Gets the value to use when quick filtering or exporting a cell of this
 * column.
 *
 * @param row The row that will be filtered or exported.
 * @param column The column that will be filtered or exported.
 *
 * @returns A string value or undefined if the cell has no value.
 */
function getTextValue(row: Record<string, unknown>, column: ColumnDefinition): string | undefined {
    if (!column.field) {
        return undefined;
    }

    const value = row[column.field];

    // Check if it is a ListItemBag value type.
    if (typeof value === "object") {
        if (value === null || value["text"] === null || value["text"] === undefined) {
            return "";
        }

        return `${(value as ListItemBag).text}`;
    }
    else if (typeof value === "number" || typeof value === "string") {
        const textSource = column.props["textSource"] as Record<string | number, string>;

        if (textSource && value in textSource) {
            return textSource[value];
        }
    }

    return `${value}`;
}

export default defineComponent({
    props: {
        ...standardColumnProps,

        formatComponent: {
            type: Object as PropType<Component>,
            default: LabelCell
        },

        skeletonComponent: {
            type: Object as PropType<Component>,
            default: LabelSkeletonCell
        },

        quickFilterValue: {
            type: Object as PropType<QuickFilterValueFunction | string>,
            default: getTextValue
        },

        filterValue: {
            type: Object as PropType<FilterValueFunction | string>,
            default: getTextValue
        },

        sortValue: {
            type: Function as PropType<SortValueFunction>,
            default: getTextValue
        },

        exportValue: {
            type: Function as PropType<ExportValueFunction>,
            default: getTextValue
        },

        columnType: {
            type: String as PropType<string>,
            default: "label"
        },

        /**
         * The lookup table to use to translate the raw value into a string.
         * This can be used, for example, to translate an enum into its text
         * format. This table is used before the classSource and colorSource
         * tables are used.
         */
        textSource: {
            type: Object as PropType<Record<string | number, string>>,
            required: false
        },

        /**
         * The lookup table to use when applying a custom label type tag to
         * the badge. The key is the text value of the field. The value is
         * a standard label suffix such as `primary` or `danger`.
         */
        classSource: {
            type: Object as PropType<Record<string, string>>,
            required: false
        },

        /**
         * The lookup table to use when applying a custom background color to
         * the badge. The key is the text value of the field. The value is
         * a standard CSS color designation.
         */
        colorSource: {
            type: Object as PropType<Record<string, string>>,
            required: false
        }
    }
});
