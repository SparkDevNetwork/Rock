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

// Import main Grid component.
import Grid from "./Grid/grid.partial.obs";

// Import column components.
import AttributeColumns from "./Grid/Columns/attributeColumns.partial";
import BooleanColumn from "./Grid/Columns/booleanColumn.partial";
import ButtonColumn from "./Grid/Columns/buttonColumn.partial";
import Column from "./Grid/Columns/column.partial";
import CurrencyColumn from "./Grid/Columns/currencyColumn.partial";
import DateColumn from "./Grid/Columns/dateColumn.partial";
import DateTimeColumn from "./Grid/Columns/dateTimeColumn.partial";
import DeleteColumn from "./Grid/Columns/deleteColumn.partial";
import DynamicColumns from "./Grid/Columns/dynamicColumns.partial";
import EditColumn from "./Grid/Columns/editColumn.partial";
import HighlightDetailColumn from "./Grid/Columns/highlightDetailColumn.partial";
import LabelColumn from "./Grid/Columns/labelColumn.partial";
import NumberBadgeColumn from "./Grid/Columns/numberBadgeColumn.partial";
import NumberColumn from "./Grid/Columns/numberColumn.partial";
import PersonColumn from "./Grid/Columns/personColumn.partial";
import ReorderColumn from "./Grid/Columns/reorderColumn.partial";
import SecurityColumn from "./Grid/Columns/securityColumn.partial";
import SelectColumn from "./Grid/Columns/selectColumn.partial";
import TextColumn from "./Grid/Columns/textColumn.partial";

// Import cell components.
import BooleanCell from "./Grid/Cells/booleanCell.partial.obs";
import ButtonCell from "./Grid/Cells/buttonCell.partial.obs";
import CurrencyCell from "./Grid/Cells/currencyCell.partial.obs";
import DateCell from "./Grid/Cells/dateCell.partial.obs";
import DateTimeCell from "./Grid/Cells/dateTimeCell.partial.obs";
import DeleteCell from "./Grid/Cells/deleteCell.partial.obs";
import EditCell from "./Grid/Cells/editCell.partial.obs";
import HighlightDetailCell from "./Grid/Cells/highlightDetailCell.partial.obs";
import LabelCell from "./Grid/Cells/labelCell.partial.obs";
import NumberBadgeCell from "./Grid/Cells/numberBadgeCell.partial.obs";
import NumberCell from "./Grid/Cells/numberCell.partial.obs";
import PersonCell from "./Grid/Cells/personCell.partial.obs";
import ReorderCell from "./Grid/Cells/reorderCell.partial.obs";
import SecurityCell from "./Grid/Cells/securityCell.partial.obs";
import SelectCell from "./Grid/Cells/selectCell.partial.obs";
import SelectHeaderCell from "./Grid/Cells/selectHeaderCell.partial.obs";
import TextCell from "./Grid/Cells/textCell.partial";

// Import skeleton cell components.
import CurrencySkeletonCell from "./Grid/Cells/currencySkeletonCell.partial.obs";
import DateSkeletonCell from "./Grid/Cells/dateSkeletonCell.partial.obs";
import DateTimeSkeletonCell from "./Grid/Cells/dateTimeSkeletonCell.partial.obs";
import NumberSkeletonCell from "./Grid/Cells/numberSkeletonCell.partial.obs";
import PersonSkeletonCell from "./Grid/Cells/personSkeletonCell.partial.obs";
import TextSkeletonCell from "./Grid/Cells/textSkeletonCell.partial.obs";

// Import filter components.
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
    ButtonColumn,
    Column,
    CurrencyColumn,
    DateColumn,
    DateTimeColumn,
    DeleteColumn,
    DynamicColumns,
    EditColumn,
    HighlightDetailColumn,
    LabelColumn,
    NumberBadgeColumn,
    NumberColumn,
    PersonColumn,
    ReorderColumn,
    SecurityColumn,
    SelectColumn,
    TextColumn
};

// Export cell components.
export {
    BooleanCell,
    ButtonCell,
    CurrencyCell,
    DateCell,
    DateTimeCell,
    DeleteCell,
    EditCell,
    HighlightDetailCell,
    LabelCell,
    NumberBadgeCell,
    NumberCell,
    PersonCell,
    ReorderCell,
    SecurityCell,
    SelectCell,
    SelectHeaderCell,
    TextCell
};

// Export skeleton cell components.
export {
    CurrencySkeletonCell,
    DateSkeletonCell,
    DateTimeSkeletonCell,
    NumberSkeletonCell,
    PersonSkeletonCell,
    TextSkeletonCell
};

// Export filter components.
export {
    BooleanFilter,
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
 * A column filter that can displays unique value and lets the individual
 * pick one or more values to use in filtering.
 */
export const pickExistingValueFilter: ColumnFilter = {
    component: PickExistingFilter,

    matches: pickExistingFilterMatches
};

/**
 * Default column filters by column type.
 */
export const defaultColumnFilters = {
    "boolean": booleanValueFilter,
    "date": dateValueFilter,
    "dateTime": dateValueFilter,
    "number": numberValueFilter,
    "text": textValueFilter,
};
