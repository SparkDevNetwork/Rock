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
import PersonCell from "../Cells/personCell.partial.obs";
import PersonSkeletonCell from "../Cells/personSkeletonCell.partial.obs";
import { ColumnDefinition, ExportValueFunction, QuickFilterValueFunction, SortValueFunction } from "@Obsidian/Types/Controls/grid";
import { PersonFieldBag } from "@Obsidian/ViewModels/Core/Grid/personFieldBag";

/**
 * Gets the value to use when displaying a cell of this column.
 *
 * @param row The row that will be displayed.
 * @param column The column that will be displayed.
 *
 * @returns A string value or undefined if the cell has no value.
 */
function getDisplayedValue(row: Record<string, unknown>, column: ColumnDefinition): string | undefined {
    if (!column.field) {
        return undefined;
    }

    const value = row[column.field] as PersonFieldBag | undefined;

    if (!value || typeof value !== "object") {
        return undefined;
    }

    if (column.props.showLastNameFirst === true) {
        return `${value.lastName ?? ""}, ${value.nickName ?? ""}`;
    }

    return `${value.nickName ?? ""} ${value.lastName ?? ""}`;
}

/**
 * Displays a cell as a Person. This has a few options to control the
 * formatting, but generally follows a standard layout so different grids with
 * person columns all look the same.
 */
export default defineComponent({
    props: {
        ...standardColumnProps,

        formatComponent: {
            type: Object as PropType<Component>,
            default: PersonCell
        },

        skeletonComponent: {
            type: Object as PropType<Component>,
            default: PersonSkeletonCell
        },

        quickFilterValue: {
            type: Function as PropType<QuickFilterValueFunction>,
            default: getDisplayedValue
        },

        exportValue: {
            type: Function as PropType<ExportValueFunction>,
            default: getDisplayedValue
        },

        sortValue: {
            type: Function as PropType<SortValueFunction>,
            default: getDisplayedValue
        },

        /**
         * If `true` then the avatar image will not be shown.
         */
        hideAvatar: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /**
         * Normally the column will display the nick name first, such as
         * "Ted Decker". If this is set to `true` then it will show the last
         * name first, such as "Decker, Ted".
         */
        showLastNameFirst: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /**
         * If `true` then the person will be displayed as a hyperlink to the
         * person detail page. No checking is performed if the current
         * individual has access to that page.
         */
        showAsLink: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        /**
         * If true, when hovering over the avatar will show a popover with some
         * additional information about the person.
         */
        enableHoverInfo: {
            type: Boolean,
            default: false
        },

        columnType: {
            type: String as PropType<string>,
            default: "person"
        },

        detailField: {
            type: [String, Boolean] as PropType<string | false>,
            required: false
        }
    }
});
