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

import Grid from "./Grid/grid.partial.obs";

import AttributeColumns from "./Grid/Columns/attributeColumns.partial";
import BooleanColumn from "./Grid/Columns/booleanColumn.partial";
import Column from "./Grid/Columns/column.partial";
import CurrencyColumn from "./Grid/Columns/currencyColumn.partial";
import DateColumn from "./Grid/Columns/dateColumn.partial";
import DateTimeColumn from "./Grid/Columns/dateTimeColumn.partial";
import DeleteColumn from "./Grid/Columns/deleteColumn.partial";
import EditColumn from "./Grid/Columns/editColumn.partial";
import LabelColumn from "./Grid/Columns/labelColumn.partial";
import NumberBadgeColumn from "./Grid/Columns/numberBadgeColumn.partial";
import NumberColumn from "./Grid/Columns/numberColumn.partial";
import PersonColumn from "./Grid/Columns/personColumn.partial";
import ReorderColumn from "./Grid/Columns/reorderColumn.partial";
import SecurityColumn from "./Grid/Columns/securityColumn.partial";
import SelectColumn from "./Grid/Columns/selectColumn.partial";
import TextColumn from "./Grid/Columns/textColumn.partial";
import CopyColumn from "./Grid/Columns/copyColumn.partial";
import ButtonColumn from "./Grid/Columns/buttonColumn.partial";

import BooleanCell from "./Grid/Cells/booleanCell.partial.obs";
import CurrencyCell from "./Grid/Cells/currencyCell.partial.obs";
import DateCell from "./Grid/Cells/dateCell.partial.obs";
import DateTimeCell from "./Grid/Cells/dateTimeCell.partial.obs";
import DeleteCell from "./Grid/Cells/deleteCell.partial.obs";
import EditCell from "./Grid/Cells/editCell.partial.obs";
import LabelCell from "./Grid/Cells/labelCell.partial.obs";
import NumberBadgeCell from "./Grid/Cells/numberBadgeCell.partial.obs";
import NumberCell from "./Grid/Cells/numberCell.partial.obs";
import PersonCell from "./Grid/Cells/personCell.partial.obs";
import ReorderCell from "./Grid/Cells/reorderCell.partial.obs";
import SecurityCell from "./Grid/Cells/securityCell.partial.obs";
import SelectCell from "./Grid/Cells/selectCell.partial.obs";
import SelectHeaderCell from "./Grid/Cells/selectHeaderCell.partial.obs";
import TextCell from "./Grid/Cells/textCell.partial";
import CopyCell from "./Grid/Cells/copyCell.partial.obs";
import ButtonCell from "./Grid/Cells/buttonCell.partial.obs";

import BooleanFilter from "./Grid/Filters/booleanFilter.partial.obs";
import DateFilter from "./Grid/Filters/dateFilter.partial.obs";
import NumberFilter from "./Grid/Filters/numberFilter.partial.obs";
import PickExistingFilter from "./Grid/Filters/pickExistingFilter.partial.obs";
import TextFilter from "./Grid/Filters/textFilter.partial.obs";

import { booleanFilterMatches, dateFilterMatches, numberFilterMatches, pickExistingFilterMatches, textFilterMatches } from "@Obsidian/Core/Controls/grid";
import { ColumnFilter } from "@Obsidian/Types/Controls/grid";

// Export main Grid component.
export default Grid;

// Export column components.
export {
    AttributeColumns,
    BooleanColumn,
    Column,
    CurrencyColumn,
    DateColumn,
    DateTimeColumn,
    DeleteColumn,
    EditColumn,
    LabelColumn,
    NumberBadgeColumn,
    NumberColumn,
    PersonColumn,
    ReorderColumn,
    SecurityColumn,
    SelectColumn,
    TextColumn,
    CopyColumn,
    ButtonColumn
};

// Export cell components.
export {
    BooleanCell,
    CurrencyCell,
    DateCell,
    DateTimeCell,
    DeleteCell,
    EditCell,
    LabelCell,
    NumberBadgeCell,
    NumberCell,
    PersonCell,
    ReorderCell,
    SecurityCell,
    SelectCell,
    SelectHeaderCell,
    TextCell,
    CopyCell,
    ButtonCell
};

// Export filter components.
export {
    DateFilter,
    NumberFilter,
    PickExistingFilter,
    TextFilter
};

/** A column filter that can be used with boolean values. */
export const booleanValueFilter: ColumnFilter = {
    component: BooleanFilter,

    matches: booleanFilterMatches
};

/** A column filter that can be used with date values. */
export const dateValueFilter: ColumnFilter = {
    component: DateFilter,

    matches: dateFilterMatches
};

/** A column filter that can be used with numeric values. */
export const numberValueFilter: ColumnFilter = {
    component: NumberFilter,

    matches: numberFilterMatches
};

/** A column filter that performs simple substring matching. */
export const textValueFilter: ColumnFilter = {
     component: TextFilter,

     matches: textFilterMatches
};

/**
 * A column filter that can displays unique value and let's the individual
 * pick one or more values to use in filtering.
 */
export const pickExistingValueFilter: ColumnFilter = {
    component: PickExistingFilter,

    matches: pickExistingFilterMatches
};
