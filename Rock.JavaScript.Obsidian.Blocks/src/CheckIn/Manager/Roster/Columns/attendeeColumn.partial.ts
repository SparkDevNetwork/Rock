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
import AttendeeCell from "../Cells/attendeeCell.partial.obs";
import AttendeeSkeletonCell from "../Cells/attendeeSkeletonCell.partial.obs";
import { ColumnDefinition, ExportValueFunction, QuickFilterValueFunction, SortValueFunction } from "@Obsidian/Types/Controls/grid";
import { RosterAttendeeBag } from "@Obsidian/ViewModels/Blocks/CheckIn/Manager/Roster/rosterAttendeeBag";

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

    const value = row[column.field] as RosterAttendeeBag | undefined;

    if (!value || typeof value !== "object") {
        return undefined;
    }

    return value.fullName ?? "";
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
            default: AttendeeCell
        },

        skeletonComponent: {
            type: Object as PropType<Component>,
            default: AttendeeSkeletonCell
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

        columnType: {
            type: String as PropType<string>,
            default: "attendee"
        },
    }
});
