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

import { Component, createElementVNode, defineComponent, PropType, reactive, toRaw, unref, VNode, watch, WatchStopHandle } from "vue";
import { NumberFilterMethod } from "@Obsidian/Enums/Core/Grid/numberFilterMethod";
import { DateFilterMethod } from "@Obsidian/Enums/Core/Grid/dateFilterMethod";
import { PickExistingFilterMethod } from "@Obsidian/Enums/Core/Grid/pickExistingFilterMethod";
import { TextFilterMethod } from "@Obsidian/Enums/Core/Grid/textFilterMethod";
import { ColumnFilter, ColumnDefinition, IGridState, StandardFilterProps, StandardCellProps, IGridCache, IGridRowCache, ColumnSort, SortValueFunction, FilterValueFunction, QuickFilterValueFunction, StandardColumnProps, StandardHeaderCellProps, EntitySetOptions, ExportValueFunction, StandardSkeletonCellProps, GridLength, BooleanSearchBag } from "@Obsidian/Types/Controls/grid";
import { ICancellationToken } from "@Obsidian/Utility/cancellation";
import { extractText, getVNodeProp, getVNodeProps } from "@Obsidian/Utility/component";
import { DayOfWeek, RockDateTime } from "@Obsidian/Utility/rockDateTime";
import { resolveMergeFields } from "@Obsidian/Utility/lava";
import { deepEqual } from "@Obsidian/Utility/util";
import { AttributeFieldDefinitionBag } from "@Obsidian/ViewModels/Core/Grid/attributeFieldDefinitionBag";
import { DynamicFieldDefinitionBag } from "@Obsidian/ViewModels/Core/Grid/dynamicFieldDefinitionBag";
import { GridDefinitionBag } from "@Obsidian/ViewModels/Core/Grid/gridDefinitionBag";
import { GridEntitySetBag } from "@Obsidian/ViewModels/Core/Grid/gridEntitySetBag";
import { GridEntitySetItemBag } from "@Obsidian/ViewModels/Core/Grid/gridEntitySetItemBag";
import { Guid } from "@Obsidian/Types";
import mitt, { Emitter } from "mitt";
import { CustomColumnDefinitionBag } from "@Obsidian/ViewModels/Core/Grid/customColumnDefinitionBag";
import { ColumnPositionAnchor } from "@Obsidian/Enums/Core/Grid/columnPositionAnchor";
import { BooleanFilterMethod } from "@Obsidian/Enums/Core/Grid/booleanFilterMethod";
import { getValueFromPath } from "@Obsidian/Utility/objectUtils";

// #region Internal Types

/**
 * Defines the grid event names and the parameter they are passed.
 */
type GridEvents = {
    /** Called when the {@link IGridState.rows} value has changed. */
    rowsChanged: IGridState;

    /** Called when the {@link IGridState.filteredRows} value has changed. */
    filteredRowsChanged: IGridState;

    /** Called when the {@link IGridState.sortedRows} value has changed. */
    sortedRowsChanged: IGridState;

    /** Called when the {@link IGridState.visibleColumns} value has changed. */
    visibleColumnsChanged: IGridState;

    /** Called when the {@link IGridState.selectedKeys} value has changed. */
    selectedKeysChanged: IGridState;

    /** Called when the {@link IGridState.isFiltered} value has changed. */
    isFilteredChanged: IGridState;

    /** Called when the {@link IGridState.isSorted} value has changed. */
    isSortedChanged: IGridState;
};

// #endregion

// #region Keys

/**
 * The standard action URLs to use when performing grid actions.
 */
export const GridActionUrlKey = {
    /** The URL to use when sending a communication. */
    Communicate: "communicate",

    /** The URL to use when merging Person records. */
    MergePerson: "mergePerson",

    /** The URL to use when merging Business records. */
    MergeBusiness: "mergeBusiness",

    /** The URL to use when performing a bulk update. */
    BulkUpdate: "bulkUpdate",

    /** The URL to use when launching a workflow for each record. */
    LaunchWorkflow: "launchWorkflow",

    /** The URL to use to start a merge template request. */
    MergeTemplate: "mergeTemplate"
} as const;

// #endregion

// #region Standard Component Props

/**
 * Defines the standard properties available on all columns.
 */
export const standardColumnProps: StandardColumnProps = {
    name: {
        type: String as PropType<string>,
        default: ""
    },

    title: {
        type: String as PropType<string>,
        required: false
    },

    field: {
        type: String as PropType<string>,
        required: false
    },

    quickFilterValue: {
        type: Object as PropType<QuickFilterValueFunction | string>,
        required: false
    },

    sortField: {
        type: String as PropType<string>,
        required: false
    },

    sortValue: {
        type: Object as PropType<(SortValueFunction | string)>,
        required: false
    },

    filter: {
        type: Object as PropType<ColumnFilter>,
        required: false
    },

    filterValue: {
        type: Object as PropType<(FilterValueFunction | string)>,
        required: false
    },

    exportValue: {
        type: Function as PropType<ExportValueFunction>,
        required: false
    },

    columnType: {
        type: String as PropType<string>,
        required: false
    },

    headerClass: {
        type: String as PropType<string>,
        required: false
    },

    itemClass: {
        type: String as PropType<string>,
        required: false
    },

    formatComponent: {
        type: Object as PropType<Component>,
        required: false
    },

    headerComponent: {
        type: Object as PropType<Component>,
        required: false
    },

    skeletonComponent: {
        type: Object as PropType<Component>,
        required: false
    },

    hideOnScreen: {
        type: Boolean as PropType<boolean>,
        required: false
    },

    excludeFromExport: {
        type: Boolean as PropType<boolean>,
        required: false
    },

    visiblePriority: {
        type: String as PropType<"xs" | "sm" | "md" | "lg" | "xl">,
        default: "xs"
    },

    width: {
        type: String as PropType<string>,
        required: false
    },

    disableSort: {
        type: Boolean as PropType<boolean>,
        default: false
    },
};

/** The standard properties available on header cells. */
export const standardHeaderCellProps: StandardHeaderCellProps = {
    column: {
        type: Object as PropType<ColumnDefinition>,
        required: true
    },

    grid: {
        type: Object as PropType<IGridState>,
        required: true
    }
};

/** The standard properties available on cells. */
export const standardCellProps: StandardCellProps = {
    column: {
        type: Object as PropType<ColumnDefinition>,
        required: true
    },

    row: {
        type: Object as PropType<Record<string, unknown>>,
        required: true
    },

    grid: {
        type: Object as PropType<IGridState>,
        required: true
    }
};

/** The standard properties available on skeleton cells. */
export const standardSkeletonCellProps: StandardSkeletonCellProps = {
    column: {
        type: Object as PropType<ColumnDefinition>,
        required: true
    },

    grid: {
        type: Object as PropType<IGridState>,
        required: true
    }
};

/**
 * The standard properties that are made available to column filter
 * components.
 */
export const standardFilterProps: StandardFilterProps = {
    modelValue: {
        type: Object as PropType<unknown>,
        required: false
    },

    column: {
        type: Object as PropType<ColumnDefinition>,
        required: true
    },

    grid: {
        type: Object as PropType<IGridState>,
        required: true
    }
};

// #endregion

// #region Filter Matches Functions

/**
 * The text column filter that performs a comparison of `haystack` and
 * the value and comparison type inside `needle` to see if it matches.
 *
 * @private This is used internally by Rock and should not be used directly.
 *
 * @param needle The filter value defined in the UI component.
 * @param haystack The filter value from the row that must match the `needle`.
 *
 * @returns True if `haystack` matches the `needle` and should be included in the results.
 */
export function textFilterMatches(needle: unknown, haystack: unknown): boolean {
    if (!needle || typeof needle !== "object" || typeof needle["method"] !== "number" || typeof needle["value"] !== "string") {
        return false;
    }

    // Allow undefined values and number values, but everything else is
    // considered a non-match.
    if (haystack !== undefined && typeof haystack !== "string") {
        return false;
    }

    const haystackValue = haystack?.toLowerCase() ?? "";
    const needleValue = needle["value"].toLowerCase();

    if (needle["method"] === TextFilterMethod.Equals) {
        return haystackValue === needleValue;
    }
    else if (needle["method"] === TextFilterMethod.DoesNotEqual) {
        return haystackValue !== needleValue;
    }
    else if (needle["method"] === TextFilterMethod.Contains) {
        return haystackValue.includes(needleValue);
    }
    else if (needle["method"] === TextFilterMethod.DoesNotContain) {
        return !haystackValue.includes(needleValue);
    }
    else if (needle["method"] === TextFilterMethod.StartsWith) {
        return haystackValue.indexOf(needleValue) === 0;
    }
    else if (needle["method"] === TextFilterMethod.EndsWith) {
        return haystackValue.lastIndexOf(needleValue) === haystackValue.length - needleValue.length;
    }
    else {
        return false;
    }
}

/**
 * The boolean column filter that performs a comparison of `haystack` and
 * the comparison type inside `needle` to see if it matches.
 *
 * @private This is used internally by Rock and should not be used directly.
 *
 * @param needle The filter value defined in the UI component.
 * @param haystack The filter value from the row that must match the `needle`.
 *
 * @returns True if `haystack` matches the `needle` and should be included in the results.
 */
export function booleanFilterMatches(needle: unknown, haystack: unknown): boolean {
    if (!needle || typeof needle !== "object" || typeof needle["method"] !== "number") {
        return false;
    }

    const needleBag = needle as BooleanSearchBag;

    if (needleBag.method === BooleanFilterMethod.Yes && haystack === true) {
        return true;
    }

    return false;
}

/**
 * The column filter compares the `needle` against the `haystack` to see if
 * they match. This is a deep equality check so if they are arrays or objects
 * then all child objects and properties must match exactly.
 *
 * @private This is used internally by Rock and should not be used directly.
 *
 * @param needle The filter value defined in the UI component.
 * @param haystack The filter value from the row that must match the `needle`.
 *
 * @returns True if `haystack` matches the `needle` and should be included in the results.
 */
export function pickExistingFilterMatches(needle: unknown, haystack: unknown): boolean {
    if (!needle || typeof needle !== "object" || typeof needle["method"] !== "number" || !Array.isArray(needle["value"])) {
        return false;
    }

    if (needle["value"].length === 0) {
        return true;
    }

    if (needle["method"] === PickExistingFilterMethod.Any) {
        return needle["value"].some(n => deepEqual(n, haystack, true));
    }
    else if (needle["method"] === PickExistingFilterMethod.Exclude) {
        return !needle["value"].some(n => deepEqual(n, haystack, true));
    }
    else {
        return false;
    }
}

/**
 * The number column filter that performs a comparison of `haystack` and
 * the value and comparison type inside `needle` to see if it matches.
 *
 * @private This is used internally by Rock and should not be used directly.
 *
 * @param needle The filter value defined in the UI component.
 * @param haystack The filter value from the row that must match the `needle`.
 *
 * @returns True if `haystack` matches the `needle` and should be included in the results.
 */
export function numberFilterMatches(needle: unknown, haystack: unknown, column: ColumnDefinition, grid: IGridState): boolean {
    if (!needle || typeof needle !== "object") {
        return false;
    }

    // Allow undefined values and number values, but everything else is
    // considered a non-match.
    if (haystack !== undefined && typeof haystack !== "number") {
        return false;
    }

    if (needle["method"] === NumberFilterMethod.Equals) {
        return haystack === needle["value"];
    }
    else if (needle["method"] === NumberFilterMethod.DoesNotEqual) {
        return haystack !== needle["value"];
    }

    // All the remaining comparison types require a value.
    if (haystack === undefined) {
        return false;
    }

    if (needle["method"] === NumberFilterMethod.GreaterThan) {
        return haystack > needle["value"];
    }
    else if (needle["method"] === NumberFilterMethod.GreaterThanOrEqual) {
        return haystack >= needle["value"];
    }
    else if (needle["method"] === NumberFilterMethod.LessThan) {
        return haystack < needle["value"];
    }
    else if (needle["method"] === NumberFilterMethod.LessThanOrEqual) {
        return haystack <= needle["value"];
    }
    else if (needle["method"] === NumberFilterMethod.Between) {
        if (typeof needle["value"] !== "number" || typeof needle["secondValue"] !== "number") {
            return false;
        }

        return haystack >= needle["value"] && haystack <= needle["secondValue"];
    }
    else if (needle["method"] === NumberFilterMethod.TopN) {
        const nCount = needle["value"];

        if (typeof nCount !== "number" || nCount <= 0) {
            return false;
        }

        const cacheKey = grid.getColumnCacheKey(column, "number-filter", `top-${nCount}`);
        const topn = grid.cache.getOrAdd(cacheKey, () => {
            return calculateColumnTopNRowValue(nCount, column, grid);
        });

        return haystack >= topn;
    }
    else if (needle["method"] === NumberFilterMethod.AboveAverage) {
        const cacheKey = grid.getColumnCacheKey(column, "number-filter", "average");
        const average = grid.cache.getOrAdd(cacheKey, () => {
            return calculateColumnAverageValue(column, grid);
        });

        return haystack > average;
    }
    else if (needle["method"] === NumberFilterMethod.BelowAverage) {
        const cacheKey = grid.getColumnCacheKey(column, "number-filter", "average");
        const average = grid.cache.getOrAdd(cacheKey, () => {
            return calculateColumnAverageValue(column, grid);
        });

        return haystack < average;
    }
    else {
        return false;
    }
}

/**
 * The date column filter that performs a comparison of `haystack` and
 * the value and comparison type inside `needle` to see if it matches.
 *
 * @private This is used internally by Rock and should not be used directly.
 *
 * @param needle The filter value defined in the UI component.
 * @param haystack The filter value from the row that must match the `needle`.
 *
 * @returns True if `haystack` matches the `needle` and should be included in the results.
 */
export function dateFilterMatches(needle: unknown, haystack: unknown): boolean {
    if (!needle || typeof needle !== "object") {
        return false;
    }

    // Allow undefined values and number values, but everything else is
    // considered a non-match.
    if (haystack !== undefined && typeof haystack !== "string") {
        return false;
    }

    const needleFirstDate = RockDateTime.parseISO(needle["value"] ?? "")?.date.toMilliseconds() ?? 0;
    const needleSecondDate = RockDateTime.parseISO(needle["secondValue"] ?? "")?.date.toMilliseconds() ?? 0;
    const haystackDate = RockDateTime.parseISO(haystack ?? "")?.date.toMilliseconds() ?? 0;
    const today = RockDateTime.now().date;

    if (needle["method"] === DateFilterMethod.Equals) {
        return haystackDate === needleFirstDate;
    }
    else if (needle["method"] === DateFilterMethod.DoesNotEqual) {
        return haystackDate !== needleFirstDate;
    }

    // All the remaining comparison types require a value.
    if (haystackDate === 0) {
        return false;
    }

    if (needle["method"] === DateFilterMethod.Before) {
        return haystackDate < needleFirstDate;
    }
    else if (needle["method"] === DateFilterMethod.After) {
        return haystackDate > needleFirstDate;
    }
    else if (needle["method"] === DateFilterMethod.Between) {
        return haystackDate >= needleFirstDate && haystackDate <= needleSecondDate;
    }
    // SUn = 0, Mon = 1, Tue = 2, Wed = 3
    // Start = Tue = 2
    else if (needle["method"] === DateFilterMethod.ThisWeek) {
        const firstDayOfWeek = getStartOfWeek(today);
        const lastDayOfWeek = firstDayOfWeek.addDays(6);

        return haystackDate >= firstDayOfWeek.toMilliseconds()
            && haystackDate <= lastDayOfWeek.toMilliseconds();
    }
    else if (needle["method"] === DateFilterMethod.LastWeek) {
        const firstDayOfWeek = getStartOfWeek(today).addDays(-7);
        const lastDayOfWeek = firstDayOfWeek.addDays(6);

        return haystackDate >= firstDayOfWeek.toMilliseconds()
            && haystackDate <= lastDayOfWeek.toMilliseconds();
    }
    else if (needle["method"] === DateFilterMethod.NextWeek) {
        const firstDayOfWeek = getStartOfWeek(today).addDays(7);
        const lastDayOfWeek = firstDayOfWeek.addDays(6);

        return haystackDate >= firstDayOfWeek.toMilliseconds()
            && haystackDate <= lastDayOfWeek.toMilliseconds();
    }
    else if (needle["method"] === DateFilterMethod.ThisMonth) {
        const firstDayOfMonth = today.addDays(-(today.day - 1));
        const lastDayOfMonth = firstDayOfMonth.addMonths(1).addDays(-1);

        return haystackDate >= firstDayOfMonth.toMilliseconds()
            && haystackDate <= lastDayOfMonth.toMilliseconds();
    }
    else if (needle["method"] === DateFilterMethod.LastMonth) {
        const firstDayOfMonth = today.addDays(-(today.day - 1)).addMonths(-1);
        const lastDayOfMonth = firstDayOfMonth.addMonths(1).addDays(-1);

        return haystackDate >= firstDayOfMonth.toMilliseconds()
            && haystackDate <= lastDayOfMonth.toMilliseconds();
    }
    else if (needle["method"] === DateFilterMethod.NextMonth) {
        const firstDayOfMonth = today.addDays(-(today.day - 1)).addMonths(1);
        const lastDayOfMonth = firstDayOfMonth.addMonths(1).addDays(-1);

        return haystackDate >= firstDayOfMonth.toMilliseconds()
            && haystackDate <= lastDayOfMonth.toMilliseconds();
    }
    else if (needle["method"] === DateFilterMethod.ThisYear) {
        const firstDayOfYear = today.addDays(-(today.dayOfYear - 1));
        const lastDayOfYear = firstDayOfYear.addYears(1).addDays(-1);

        return haystackDate >= firstDayOfYear.toMilliseconds()
            && haystackDate <= lastDayOfYear.toMilliseconds();
    }
    else if (needle["method"] === DateFilterMethod.LastYear) {
        const firstDayOfYear = today.addDays(-(today.dayOfYear - 1)).addYears(-1);
        const lastDayOfYear = firstDayOfYear.addYears(1).addDays(-1);

        return haystackDate >= firstDayOfYear.toMilliseconds()
            && haystackDate <= lastDayOfYear.toMilliseconds();
    }
    else if (needle["method"] === DateFilterMethod.NextYear) {
        const firstDayOfYear = today.addDays(-(today.dayOfYear - 1)).addYears(1);
        const lastDayOfYear = firstDayOfYear.addYears(1).addDays(-1);

        return haystackDate >= firstDayOfYear.toMilliseconds()
            && haystackDate <= lastDayOfYear.toMilliseconds();
    }
    else if (needle["method"] === DateFilterMethod.YearToDate) {
        const firstDayOfYear = today.addDays(-(today.dayOfYear - 1));

        return haystackDate >= firstDayOfYear.toMilliseconds()
            && haystackDate <= today.toMilliseconds();
    }
    else {
        return false;
    }
}

// #endregion

// #region Entity Sets

/**
 * Gets the entity set bag that can be send to the server to create an entity
 * set representing the selected items in the grid.
 *
 * @param grid The grid state that will be used as the source data.
 * @param keyFields The fields to use for the entity keys. This is only used when
 * populating the item entityKey value. It is not used to detect selection state.
 * If multiple keys are specified then an {@link GridEntitySetItemBag} will be
 * created for each key.
 * @param options The options that describe how the entity set should be generated.
 *
 * @returns A new instance of {@link GridEntitySetBag} that contains the data.
 */
export async function getEntitySetBag(grid: IGridState, keyFields: string[], options?: EntitySetOptions): Promise<GridEntitySetBag> {
    const selectedKeys = grid.selectedKeys;
    const entitySetItemLookup: Record<string, GridEntitySetItemBag> = {};
    let itemOrder = 0;
    const entitySetBag: GridEntitySetBag = {
        entityTypeKey: options?.entityTypeGuid ?? grid.entityTypeGuid,
        items: []
    };

    function processRow(row: Record<string, unknown>): void {
        const rowKey = grid.getRowKey(row);

        // If we have any selected keys but the row isn't one of them then
        // skip it.
        if (selectedKeys.length > 0) {
            if (!rowKey || !selectedKeys.includes(rowKey)) {
                return;
            }
        }

        const entityKeyValues: (string | undefined)[] = [];
        const mergeValues: Record<string, unknown> = {};

        // Search each of the key fields we were told to check and look
        // for any entity keys.
        for (const key of keyFields) {
            const keyValue = getValueFromPath(row, key);

            if (typeof keyValue === "number" && keyValue !== 0) {
                entityKeyValues.push(keyValue.toString());
            }
            else if (typeof keyValue === "string" && keyValue !== "") {
                // For compatibility with legacy grid, check if we can split
                // the string on the normal seperators.
                const keyValues = keyValue.replace(/[\s|,;]+/, ",").split(",");

                for (const kv of keyValues) {
                    if (kv !== "" && !entityKeyValues.includes(kv)) {
                        entityKeyValues.push(kv);
                    }
                }
            }
        }

        if (keyFields.length === 0) {
            // We just want the merge fields, so put in a bogus key.
            entityKeyValues.push(undefined);
        }

        // Get any additional merge values requested.
        if (options?.mergeFields) {
            for (const mergeKey of Object.keys(options.mergeFields)) {
                mergeValues[options.mergeFields[mergeKey]] = toRaw(row[mergeKey]);
            }
        }

        // Get any additional merge column values requested.
        if (options?.mergeColumns) {
            for (const mergeKey of Object.keys(options.mergeColumns)) {
                const column = grid.columns.find(c => c.name === mergeKey);

                if (column) {
                    if (options?.purpose === "export") {
                        mergeValues[options.mergeColumns[mergeKey]] = column.exportValue(row, column, grid);
                    }
                    else {
                        const cellProps = {
                            column,
                            row,
                            grid
                        };

                        mergeValues[options.mergeColumns[mergeKey]] = extractText(column.formatComponent, cellProps);
                    }
                }
            }
        }

        // Get any custom merge values that should be added.
        if (options?.additionalMergeFieldsFactory) {
            const additionalValues = options.additionalMergeFieldsFactory(row, grid);

            for (const key of Object.keys(additionalValues)) {
                mergeValues[key] = additionalValues[key];
            }
        }

        // Create (or update) all the entity set item bags for the entity
        // keys that we found in this row.
        for (const entityKey of entityKeyValues) {
            let item = entityKey ? entitySetItemLookup[entityKey] : undefined;

            if (!item) {
                item = {
                    entityKey: entityKey,
                    order: itemOrder++,
                    additionalMergeValues: { ...mergeValues }
                };

                entitySetBag.items?.push(item);

                if (entityKey) {
                    entitySetItemLookup[entityKey] = item;
                }
            }

            if (options?.purpose === "communication") {
                // We do something special when building an entity set
                // for use in a communication. Each person can only exist
                // in the set once, but might have different merge values
                // that came from different rows. So a special key of
                // "AdditionalFields" is used which is an array that contains
                // the merge values of each row this entity showed up in.
                if (!item.additionalMergeValues) {
                    item.additionalMergeValues = {};
                }

                let rows = item.additionalMergeValues["AdditionalFields"] as Record<string, unknown>[];

                if (!rows) {
                    rows = item.additionalMergeValues["AdditionalFields"] = [];
                }

                rows.push({ ...mergeValues });
            }
        }
    }

    // Because we might be dealing with large data sets and might be pulling
    // formatted data from components, use a worker so the UI doesn't freeze.
    const worker = new BackgroundItemsFunctionWorker(grid.sortedRows, processRow);

    await worker.run();

    return entitySetBag;
}

// #endregion

// #region Functions

/**
 * Gets a new date from the passed date whose date portion is the start
 * of the week. The time value is not modified.
 *
 * @param date The original date to be used in the calculation.
 *
 * @returns A new date object whose date portion matches the start of the week.
 */
function getStartOfWeek(date: RockDateTime): RockDateTime {
    const weekStartsOn = DayOfWeek.Monday;
    let targetDate: RockDateTime = date;

    while (targetDate.dayOfWeek !== weekStartsOn) {
        targetDate = targetDate.addDays(-1);
    }

    return targetDate;
}

/**
 * Calculates the average numerical value across all rows in a grid column.
 *
 * @param column The column whose values should be considered for the average.
 * @param grid The grid that provides all the rows.
 *
 * @returns A number that represents the average value or `0` if no rows with numeric values existed.
 */
export function calculateColumnAverageValue(column: ColumnDefinition, grid: IGridState): number {
    let count = 0;
    let total = 0;

    for (const row of grid.rows) {
        const rowValue = column.filterValue(row, column, grid);

        if (typeof rowValue === "number") {
            total += rowValue;
            count++;
        }
    }

    return count === 0 ? 0 : total / count;
}

/**
 * Calculates top Nth numeric row value for a column. If the cell values are
 * `1, 2, 3, 4, 4, 5, 5` and `rowCount` is 3 then this will return
 * the value `4`. Which means the final row count displayed will be 4 because
 * there are 4 rows with values >= 4.
 *
 * @param column The column whose values should be considered for the average.
 * @param grid The grid that provides all the rows.
 *
 * @returns A number that represents the average value or `0` if no rows with numeric values existed.
 */
export function calculateColumnTopNRowValue(rowCount: number, column: ColumnDefinition, grid: IGridState): number {
    const values: number[] = [];

    for (const row of grid.rows) {
        const rowValue = column.filterValue(row, column, grid);

        if (typeof rowValue === "number") {
            values.push(rowValue);
        }
    }

    if (values.length === 0) {
        return 0;
    }

    // Sort in descending order.
    values.sort((a, b) => b - a);

    if (rowCount <= values.length) {
        return values[rowCount - 1];
    }
    else {
        return values[values.length - 1];
    }
}

/**
 * Gets the value from the row cache or creates the value and stores it in
 * cache. This is a tiny helper function to simplify the process of
 * implementing cache when constructing column definitions.
 *
 * @param row The row whose value will be cached.
 * @param column The column that the value will be associated with.
 * @param key The key that identifies the value to be cached.
 * @param grid The grid that will be used for caching.
 * @param factory The function that will return the value if it is not already in cache.
 * @returns Either the value from cache or the value returned by `factory`.
 */
function getOrAddRowCacheValue<T>(row: Record<string, unknown>, column: ColumnDefinition, key: string, grid: IGridState, factory: (() => T)): T {
    const finalKey = grid.getColumnCacheKey(column, "grid", key);

    return grid.rowCache.getOrAdd<T>(row, finalKey, () => factory());
}

/**
 * Builds the column definitions for the attributes defined on the node.
 *
 * @param columns The array of columns that the new attribute columns will be appended to.
 * @param node The node that defines the attribute fields.
 */
function buildAttributeColumns(columns: ColumnDefinition[], node: VNode): void {
    const attributes = getVNodeProp<AttributeFieldDefinitionBag[]>(node, "attributes");
    const filter = getVNodeProp<ColumnFilter>(node, "filter");
    const skeletonComponent = getVNodeProp<Component>(node, "skeletonComponent");

    if (!attributes) {
        return;
    }

    for (const attribute of attributes) {
        if (!attribute.name) {
            continue;
        }

        columns.push({
            name: attribute.name,
            title: attribute.title ?? undefined,
            field: attribute.name,
            sortValue: (r, c) => c.field ? String(r[c.field]) : undefined,
            quickFilterValue: (r, c, g) => getOrAddRowCacheValue(r, c, "quickFilterValue", g, () => c.field ? String(r[c.field]) : undefined),
            filter,
            filterValue: (r, c) => c.field ? String(r[c.field]) : undefined,
            exportValue: (r, c) => c.field ? String(r[c.field]) : undefined,
            formatComponent: defaultCell,
            condensedComponent: defaultCell,
            skeletonComponent,
            hideOnScreen: false,
            excludeFromExport: false,
            visiblePriority: "md",
            width: {
                value: 10,
                unitType: "%"
            },
            disableSort: false,
            props: {},
            slots: {},
            data: {}
        });
    }
}

/**
 * Builds the column definitions for the dynamic columns defined on the node.
 *
 * @param columns The array of columns that the new dynamic columns will be appended to.
 * @param node The node that defines the dynamic fields.
 */
function buildDynamicColumns(columns: ColumnDefinition[], node: VNode): void {
    const dynamicFields = getVNodeProp<(DynamicFieldDefinitionBag & { filter?: ColumnFilter, filterValue?: FilterValueFunction | string })[]>(node, "dynamicFields");
    if (!dynamicFields) {
        return;
    }

    const columnComponents = getVNodeProp<Record<string, Component>>(node, "columnComponents") ?? {};
    const defaultColumnComponent = getVNodeProp<Component>(node, "defaultColumnComponent");
    if (!defaultColumnComponent) {
        return;
    }

    for (const dynamicField of dynamicFields) {
        if (!dynamicField.name) {
            continue;
        }

        const columnComponent = columnComponents?.[dynamicField.columnType ?? ""]
            ?? defaultColumnComponent;

        const vNode = createElementVNode(columnComponent, {
            name: dynamicField.name,
            title: dynamicField.title,
            field: dynamicField.name,
            width: dynamicField.width,
            filter: dynamicField.filter,
            filterValue: dynamicField.filterValue ?? columnComponent["filterValue"],
            hideOnScreen: dynamicField.hideOnScreen,
            excludeFromExport: dynamicField.excludeFromExport,
            visiblePriority: dynamicField.visiblePriority
        });

        if (dynamicField.fieldProperties) {
            Object.entries(dynamicField.fieldProperties).forEach(([key, value]) => {
                if (value && vNode.props) {
                    vNode.props[key] = value;
                }
            });
        }

        const columnDefinition = buildColumn(dynamicField.name, vNode);
        columns.push(columnDefinition);
    }
}

/**
 * Builds and inserts the column definitions for the array of custom columns.
 *
 * @param columns The array of column definitions to be updated.
 * @param customColumns The array of custom columns that the new columns derived from.
 *
 * @private This function is private and should not be exported.
 */
function insertCustomColumns(columns: ColumnDefinition[], customColumns: CustomColumnDefinitionBag[]): void {
    for (const customColumn of customColumns) {
        if (!customColumn.fieldName) {
            continue;
        }

        const columnDefinition: ColumnDefinition = {
            name: customColumn.fieldName,
            title: customColumn.headerText ?? undefined,
            field: customColumn.fieldName,
            sortValue: (r, c) => c.field ? String(r[c.field]) : undefined,
            quickFilterValue: (r, c, g) => getOrAddRowCacheValue(r, c, "quickFilterValue", g, () => c.field ? String(r[c.field]) : undefined),
            filter: undefined, // TODO: Fill this in somehow.
            filterValue: (r, c) => c.field ? String(r[c.field]) : undefined,
            exportValue: (r, c) => c.field ? String(r[c.field]) : undefined,
            formatComponent: htmlCell,
            condensedComponent: htmlCell,
            columnType: "",
            headerClass: customColumn.headerClass ?? undefined,
            itemClass: customColumn.itemClass ?? undefined,
            hideOnScreen: false,
            excludeFromExport: false,
            visiblePriority: "md",
            width: {
                value: 10,
                unitType: "%"
            },
            disableSort: false,
            props: {},
            slots: {},
            data: {}
        };

        // Get the offset position, clamp it between 0 and the size of the array.
        let offset = Math.max(0, customColumn.positionOffset);
        offset = Math.min(columns.length, offset);

        if (customColumn.anchor === ColumnPositionAnchor.FirstColumn) {
            columns.splice(offset, 0, columnDefinition);
        }
        else if (customColumn.anchor === ColumnPositionAnchor.LastColumn) {
            columns.splice(columns.length - offset, 0, columnDefinition);
        }
    }
}

/**
 * Builds a new column definition from the information provided.
 *
 * @param name The name of the column.
 * @param node The node that contains all the details about the column.
 *
 * @returns A new object that represents the column.
 */
function buildColumn(name: string, node: VNode): ColumnDefinition {
    const field = getVNodeProp<string>(node, "field");
    const title = getVNodeProp<string>(node, "title");
    const formatTemplate = node.children?.["format"] as Component | undefined;
    const formatComponent = formatTemplate ?? getVNodeProp<Component>(node, "formatComponent") ?? defaultCell;
    const condensedTemplate = node.children?.["condensed"] as Component | undefined;
    const condensedComponent = condensedTemplate ?? getVNodeProp<Component>(node, "condensedComponent") ?? formatComponent;
    const headerTemplate = node.children?.["header"] as Component | undefined;
    const headerComponent = headerTemplate ?? getVNodeProp<Component>(node, "headerComponent");
    const skeletonTemplate = node.children?.["skeleton"] as Component | undefined;
    const skeletonComponent = skeletonTemplate ?? getVNodeProp<Component>(node, "skeletonComponent");
    const exportTemplate = node.children?.["export"] as Component | undefined;
    const filter = getVNodeProp<ColumnFilter>(node, "filter");
    const headerClass = getVNodeProp<string>(node, "headerClass");
    const itemClass = getVNodeProp<string>(node, "itemClass");
    const columnType = getVNodeProp<string>(node, "columnType");
    const hideOnScreen = getVNodeProp<boolean>(node, "hideOnScreen") === true || getVNodeProp<string>(node, "hideOnScreen") === "";
    const excludeFromExport = getVNodeProp<boolean>(node, "excludeFromExport") === true || getVNodeProp<string>(node, "excludeFromExport") === "";
    const visiblePriority = getVNodeProp<"xs" | "sm" | "md" | "lg" | "xl">(node, "visiblePriority") || "xs";
    const width = getVNodeProp<string>(node, "width");
    const disableSort = getVNodeProp<boolean>(node, "disableSort") || false;
    const filterPrependComponent = node.children?.["filterPrepend"] as Component | undefined;

    // Get the function that will provide the sort value.
    let sortValue = getVNodeProp<SortValueFunction | string>(node, "sortValue");

    if (!sortValue) {
        const sortField = getVNodeProp<string>(node, "sortField") || field;

        if (sortField) {
            sortValue = (r) => {
                const v = r[sortField];

                if (typeof v === "string" || typeof v === "number") {
                    return v;
                }
                else {
                    return String(r[sortField]);
                }
            };
        }
        else {
            sortValue = undefined;
        }
    }
    else if (typeof sortValue === "string") {
        const template = sortValue;

        sortValue = (row): string | undefined => {
            return resolveMergeFields(template, { row });
        };
    }

    // Get the function that will provide the quick filter value.
    let quickFilterValue = getVNodeProp<QuickFilterValueFunction | string>(node, "quickFilterValue");

    if (!quickFilterValue) {
        // One was not provided, so generate a common use one.
        quickFilterValue = (r, c): string | undefined => {
            if (!c.field) {
                return undefined;
            }

            const v = r[c.field];

            if (typeof v === "string") {
                return v;
            }
            else if (typeof v === "number") {
                return v.toString();
            }
            else {
                return undefined;
            }
        };
    }
    else if (typeof quickFilterValue === "string") {
        const template = quickFilterValue;

        quickFilterValue = (row): string | undefined => {
            return resolveMergeFields(template, { row });
        };
    }

    // Get the function that will provide the column filter value.
    let filterValue = getVNodeProp<FilterValueFunction | string>(node, "filterValue");

    if (filterValue === undefined) {
        // One wasn't provided, so do our best to infer what it should be.
        filterValue = (r, c): string | number | boolean | undefined => {
            if (!c.field) {
                return undefined;
            }

            const v = r[c.field];

            if (typeof v === "string" || typeof v === "number" || typeof v === "boolean") {
                return v;
            }
            else {
                return undefined;
            }
        };
    }
    else if (typeof filterValue === "string") {
        const template = filterValue;

        filterValue = (row): string => {
            return resolveMergeFields(template, { row });
        };
    }

    // Get the function that will provide the export value.
    let exportValue = getVNodeProp<ExportValueFunction>(node, "exportValue");

    if (!exportValue) {
        const component = exportTemplate ?? formatComponent;
        const fn: ExportValueFunction = (r, c, g) => {
            const cellProps = {
                column: c,
                row: r,
                grid: g
            };

            return extractText(component, cellProps);
        };

        exportValue = fn;
    }

    // Convert all the value functions into cached ones.
    const sortValueFactory = sortValue;
    sortValue = (r, c, g) => {
        return sortValueFactory !== undefined
            ? getOrAddRowCacheValue(r, c, "sortValue", g, () => sortValueFactory(r, c, g))
            : undefined;
    };

    const filterValueFactory = filterValue;
    filterValue = (r, c, g) => {
        return getOrAddRowCacheValue(r, c, "filterValue", g, () => filterValueFactory(r, c, g));
    };

    const quickFilterValueFactory = quickFilterValue;
    quickFilterValue = (r, c, g) => {
        return getOrAddRowCacheValue(r, c, "quickFilterValue", g, () => quickFilterValueFactory(r, c, g));
    };

    // Build the final column definition.
    const column: ColumnDefinition = {
        name,
        title,
        field,
        formatComponent,
        condensedComponent,
        headerComponent,
        skeletonComponent,
        filterPrependComponent,
        filter,
        sortValue,
        disableSort,
        filterValue,
        quickFilterValue,
        exportValue,
        hideOnScreen,
        excludeFromExport,
        visiblePriority,
        width: parseGridLength(width),
        columnType,
        headerClass,
        itemClass,
        props: getVNodeProps(node),
        slots: node.children as Record<string, Component> ?? {},
        data: {}
    };

    return column;
}

/**
 * Builds the column definitions from the array of virtual nodes found inside
 * of a component.
 *
 * @param columnNodes The virtual nodes that contain the definitions of the columns.
 *
 * @returns An array of {@link ColumnDefinition} objects.
 */
export function getColumnDefinitions(columnNodes: VNode[]): ColumnDefinition[] {
    const columns: ColumnDefinition[] = [];

    for (const node of columnNodes) {
        const name = getVNodeProp<string>(node, "name");

        // Check if this node is the special AttributeColumns or DynamicColumns node.
        if (!name) {
            if (getVNodeProp<boolean>(node, "__attributeColumns") === true) {
                buildAttributeColumns(columns, node);
            }
            else if (getVNodeProp<boolean>(node, "__dynamicColumns") === true) {
                buildDynamicColumns(columns, node);
            }

            continue;
        }

        // Build the final column definition.
        const column = buildColumn(name, node);
        columns.push(column);
    }

    return columns;
}

/**
 * Gets the key to use on the internal cache object to load the cached data
 * for the specified row.
 *
 * @param row The row whose identifier key is needed.
 *
 * @returns The identifier key of the row or `undefined` if it could not be determined.
 */
export function getRowKey(row: Record<string, unknown>, itemIdKey?: string): string | undefined {
    if (!itemIdKey) {
        return undefined;
    }

    const rowKey = getValueFromPath(row, itemIdKey);

    if (typeof rowKey === "string") {
        return rowKey;
    }
    else if (typeof rowKey === "number") {
        return `${rowKey}`;
    }
    else {
        return undefined;
    }
}

/**
 * Parses the width string into a well formed {@link GridLength} object
 * that can be worked with more easily than the raw string.
 *
 * @param width The width that should be parsed into a grid length.
 *
 * @returns A {@link GridLength} object that describes the width.
 */
function parseGridLength(width: string | undefined): GridLength {
    if (!width) {
        return {
            value: 10,
            unitType: "%"
        };
    }

    const value = parseInt(width);

    if (width.endsWith("%")) {
        return {
            value: isNaN(value) ? 10 : value,
            unitType: "%"
        };
    }

    // Default to pixels.
    return {
        value: isNaN(value) ? 10 : value,
        unitType: "px"
    };
}

/**
 * Gets the custom styles to apply to cells of a specific column.
 *
 * @param column The column whose cell styles are being requested.
 *
 * @returns An object that contains the custom styles to apply to cells of this column.
 */
export function getColumnStyles(column: ColumnDefinition): Record<string, string> {
    const styles: Record<string, string> = {};

    if (column.width.unitType === "px") {
        styles.flex = `0 0 ${column.width.value}px`;
    }
    else {
        styles.flex = `1 1 ${column.width.value}%`;
    }

    return styles;
}

// #endregion

// #region Classes

/**
 * Default implementation used for caching data with Grid.
 *
 * @private This class is meant for internal use only.
 */
export class GridCache implements IGridCache {
    /** The private cache data storage. */
    private cacheData: Record<string, unknown> = {};

    // #region IGridCache Implementation

    public clear(): void {
        this.cacheData = {};
    }

    public remove(key: string): void {
        if (key in this.cacheData) {
            delete this.cacheData[key];
        }
    }

    public get<T = unknown>(key: string): T | undefined {
        if (key in this.cacheData) {
            return <T>this.cacheData[key];
        }
        else {
            return undefined;
        }
    }

    public getOrAdd<T = unknown>(key: string, factory: () => T): T;
    public getOrAdd<T = unknown>(key: string, factory: () => T | undefined): T | undefined;
    public getOrAdd<T = unknown>(key: string, factory: () => T | undefined): T | undefined {
        if (key in this.cacheData) {
            return <T>this.cacheData[key];
        }
        else {
            const value = factory();

            if (value !== undefined) {
                this.cacheData[key] = value;
            }

            return value;
        }
    }

    public addOrReplace<T = unknown>(key: string, value: T): T {
        this.cacheData[key] = value;

        return value;
    }

    // #endregion
}

/**
 * Default implementation used for caching grid row data.
 *
 * @private This class is meant for internal use only.
 */
export class GridRowCache implements IGridRowCache {
    /** The internal cache object used to find the cached row data. */
    private cache: IGridCache = new GridCache();

    /** The key name to use on the row objects to find the row identifier. */
    private rowItemKey?: string;

    /**
     * Creates a new grid row cache object that provides caching for each row.
     * This is used by other parts of the grid to cache expensive calculations
     * that pertain to a single row.
     *
     * @param itemIdKey The key name to use on the row objects to find the row identifier.
     */
    public constructor(itemIdKey: string | undefined) {
        this.rowItemKey = itemIdKey;
    }

    /**
     * Gets the key to use on the internal cache object to load the cached data
     * for the specified row.
     *
     * @param row The row whose identifier key is needed.
     *
     * @returns The identifier key of the row or `undefined` if it could not be determined.
     */
    private getRowKey(row: Record<string, unknown>): string | undefined {
        return getRowKey(row, this.rowItemKey);
    }

    /**
     * Sets the key that will be used when accessing a row to determine its
     * unique identifier in the grid. This will also clear all cached data.
     *
     * @param itemKey The key name to use on the row objects to find the row identifier.
     */
    public setRowItemKey(itemKey: string | undefined): void {
        if (this.rowItemKey !== itemKey) {
            this.rowItemKey = itemKey;
            this.clear();
        }
    }

    /**
     * Removes the cached values for a row.
     *
     * @param rowKey The key that identifies the row.
     * @param key The key inside the row cache to be removed or `undefined` to remove all cached data for the row.
     */
    public removeByRowKey(rowKey: string, key: string | undefined): void {
        const cacheRow = this.cache.get<GridCache>(rowKey);

        if (!cacheRow) {
            return;
        }

        if (!key) {
            cacheRow.clear();
        }
        else {
            cacheRow.remove(key);
        }
    }

    // #region IGridRowCache Implementation

    public clear(): void {
        this.cache.clear();
    }

    public remove(row: Record<string, unknown>): void;
    public remove(row: Record<string, unknown>, key: string): void;
    public remove(row: Record<string, unknown>, key?: string): void {
        const rowKey = this.getRowKey(row);

        if (!rowKey) {
            return;
        }

        this.removeByRowKey(rowKey, key);
    }

    public get<T = unknown>(row: Record<string, unknown>, key: string): T | undefined {
        const rowKey = this.getRowKey(row);

        if (!rowKey) {
            return undefined;
        }

        return this.cache.getOrAdd(rowKey, () => new GridCache()).get<T>(key);
    }

    public getOrAdd<T = unknown>(row: Record<string, unknown>, key: string, factory: () => T): T;
    public getOrAdd<T = unknown>(row: Record<string, unknown>, key: string, factory: () => T | undefined): T | undefined;
    public getOrAdd<T = unknown>(row: Record<string, unknown>, key: string, factory: () => T | undefined): T | undefined {
        const rowKey = this.getRowKey(row);

        if (!rowKey) {
            return factory();
        }

        return this.cache.getOrAdd(rowKey, () => new GridCache()).getOrAdd<T>(key, factory);
    }

    public addOrReplace<T = unknown>(row: Record<string, unknown>, key: string, value: T): T {
        const rowKey = this.getRowKey(row);

        if (!rowKey) {
            return value;
        }

        return this.cache.getOrAdd(rowKey, () => new GridCache()).addOrReplace<T>(key, value);
    }

    // #endregion
}

/**
 * Helper class for running tasks in the background without tying up the UI
 * thread. This will run on the UI thread, but in small chunks so that it does
 * not lock the UI. This is about the best we can do, but works fairly well.
 */
abstract class BackgroundWorker {
    /**
     * The number of milliseconds between runs when the idle callback is
     * not available.
     */
    private interval: number = 50;

    /**
     * The maximum number of milliseconds to run a single iteration if no
     * duration is provided by the browser. 50ms seems to be what browsers
     * use for the idle callback, so use the same value.
     */
    private intervalRunDuration: number = 50;

    /** Determines if the worker has been started already. */
    private started: boolean = false;

    /** Determines if the worker has been requested to cancel. */
    private cancelled: boolean = false;

    /** The callback function to trigger the promise to resolve. */
    private resolvePromise!: () => void;

    /** The callback funciton to trigger the promise to reject with an error. */
    private rejectPromise!: (error: Error) => void;

    /**
     * Creates a new instance of {@link BackgroundWorker}.
     *
     * @param cancellationToken An optional cancellation token that will instruct this worker to stop processing.
     */
    constructor(cancellationToken?: ICancellationToken) {
        if (cancellationToken) {
            cancellationToken.onCancellationRequested(() => this.cancel());
        }
    }

    /**
     * Starts the worker and begins processing in the background.
     *
     * @returns A promise that indicates when the worker has finished.
     */
    public run(): Promise<void> {
        if (this.started) {
            throw new Error("Can't start background worker that is already started.");
        }

        this.started = true;
        this.queueNext();

        return new Promise((resolve, reject) => {
            this.resolvePromise = resolve;
            this.rejectPromise = reject;
        });
    }

    /**
     * Requests that the worker cancel it's processing. This will not be
     * immediate but will be acted upon at the next processing cycle.
     */
    public cancel(): void {
        this.cancelled = true;
    }

    /**
     * Queues up the next processing cycle. If requestIdleCallback is available
     * then it will be used, otherwise we will fallback to setTimeout.
     */
    private queueNext(): void {
        if (window.requestIdleCallback !== undefined) {
            window.requestIdleCallback(deadline => this.processInternal(deadline), {
                timeout: this.interval
            });
        }
        else {
            setTimeout(() => this.processInternal(undefined), this.interval);
        }
    }

    /**
     * Handles a single processing pass. This handles cancellation and tracking
     * when this cycle should stop processing.
     *
     * @param deadline Special instructions from the browser on when to stop processing.
     */
    private processInternal(deadline: IdleDeadline | undefined): void {
        if (this.cancelled) {
            return this.rejectPromise(new Error("Cancellation requested."));
        }

        try {
            let hasMore: boolean;

            if (deadline && !deadline.didTimeout) {
                hasMore = this.process(() => {
                    return deadline.timeRemaining() <= 0;
                });
            }
            else {
                const timeoutAt = window.performance.now() + this.intervalRunDuration;
                hasMore = this.process(() => window.performance.now() >= timeoutAt);
            }

            if (hasMore) {
                this.queueNext();
            }
            else {
                this.resolvePromise();
            }
        }
        catch (error) {
            this.rejectPromise(error instanceof Error ? error : new Error(String(error)));
        }
    }

    /**
     * Called periodically to process for a short period of time. Call the
     * {@link didTimeout} function to determine if it is safe to continue.
     *
     * @param didTimeout A function that returns `true` when processing should be suspended.
     *
     * @returns A boolean that indicates if processing should continue. If there is no more data to process, then `false` should be returned.
     */
    protected abstract process(didTimeout: () => boolean): boolean;
}

/**
 * A helper class to process a set of items in the background. A user-defined
 * callback will be called for each item in the array, pausing between
 * invocations whenever the time limit has been reached.
 */
class BackgroundItemsFunctionWorker<T> extends BackgroundWorker {
    /** The worker function to call for each item. */
    private workerFunction: (item: T) => void;

    /** The array of items to be processed. */
    private items: readonly T[];

    /** The index of the next item to be processed. */
    private itemIndex: number = 0;

    /**
     * Creates a new worker that will process the set of {@link items} with
     * the {@link workerFunction} callback.
     *
     * @param items The array of items to be processed.
     * @param workerFunction The function to be called for each item in the array.
     */
    constructor(items: readonly T[], workerFunction: ((item: T) => void)) {
        super();

        this.workerFunction = workerFunction;
        this.items = items;
    }

    protected override process(didTimeout: () => boolean): boolean {
        while (this.itemIndex < this.items.length && !didTimeout()) {
            this.workerFunction(this.items[this.itemIndex++]);
        }

        return this.itemIndex < this.items.length;
    }
}

/**
 * Walks the row data in the grid and slowly triggers all the row values so
 * that they get cached into memory for fast access later.
 */
class BackgroundGridRowCacheWorker extends BackgroundWorker {
    /** The next row index to be processed. */
    private rowIndex: number = 0;

    private readonly grid: GridState;

    constructor(grid: GridState) {
        super();

        this.grid = grid;
    }

    protected override process(didTimeout: () => boolean): boolean {
        while (this.rowIndex < this.grid.rows.length && !didTimeout()) {
            const row = this.grid.rows[this.rowIndex++];

            for (const column of this.grid.columns) {
                if (column.name.startsWith("__")) {
                    continue;
                }

                column.sortValue?.(row, column, this.grid);
                column.filterValue(row, column, this.grid);
                column.quickFilterValue(row, column, this.grid);
            }
        }

        return this.rowIndex < this.grid.rows.length;
    }
}

/**
 * Default implementation of the grid state for internal use.
 *
 * @private The is an internal class that should not be used by plugins.
 */
export class GridState implements IGridState {
    // #region Properties

    private internalColumns: ReadonlyArray<ColumnDefinition> = [];
    private internalRows: Record<string, unknown>[] = [];
    private internalFilteredRows: ReadonlyArray<Record<string, unknown>> = [];
    private internalSortedRows: ReadonlyArray<Record<string, unknown>> = [];
    private internalVisibleColumns: ReadonlyArray<ColumnDefinition>;
    private internalSelectedKeys: string[] = [];
    private internalIsFiltered: boolean = false;
    private internalIsSorted: boolean = false;

    /** The definition data we were created with. */
    private gridDefinition?: GridDefinitionBag;

    /** This tracks the state of each row when operating in reactive mode. */
    private rowReactiveTracker: Record<string, string> = {};

    /** The handle that can be used to stop watching row changes. */
    private internalRowsWatcher?: WatchStopHandle;

    /** Determines if we are monitoring for changes to the row data. */
    private liveUpdates: boolean;

    /** The key to get the unique identifier of each row. */
    private itemKey?: string;

    /** The current quick filter value that will be used to filter the rows. */
    private quickFilter: string = "";

    /**
     * The currently applied per-column filters that will be used to filter
     * the rows.
     */
    private columnFilters: Record<string, unknown | undefined> = {};

    /** The current column being used to sort the rows. */
    private columnSort?: ColumnSort;

    /** The event emitter for all the grid events. */
    private readonly emitter: Emitter<GridEvents> = mitt<GridEvents>();

    /** A background worker that will populate all the row cache data. */
    private populateRowCacheWorker: BackgroundGridRowCacheWorker | null = null;

    // #endregion

    // #region Constructors

    /**
     * Creates a new instance of the GridState for use with the Grid component.
     *
     * @param columns The columns to initialize the Grid with.
     * @param gridDefinition The definition data that defines some of the structure of the grid.
     * @param liveUpdates If true then the grid will monitor for live updates to rows.
     * @param itemTerm The word or phrase that describes each row.
     * @param entityTypeGuid The unique identifier of the entity type this grid represents, or `undefined`.
     */
    constructor(columns: ColumnDefinition[], gridDefinition: GridDefinitionBag | undefined, liveUpdates: boolean, itemTerm: string, entityTypeGuid: Guid | undefined) {
        this.gridDefinition = gridDefinition;
        this.rowCache = new GridRowCache(undefined);
        this.liveUpdates = liveUpdates;
        this.itemTerm = itemTerm;
        this.entityTypeGuid = entityTypeGuid;

        if (gridDefinition?.customColumns && gridDefinition.customColumns.length > 0) {
            const tempColumns = [...columns];
            insertCustomColumns(tempColumns, gridDefinition.customColumns);
            this.internalColumns = tempColumns;
        }
        else {
            this.internalColumns = [...columns];
        }

        this.internalVisibleColumns = this.columns.filter(c => !c.hideOnScreen);
    }

    /**
     * Dispose of all resources this grid state has. This includes any watchers
     * and other things that might need to be manually destroyed to free up
     * memory. A common pattern would be to call this in the onUmounted() callback.
     */
    public dispose(): void {
        if (this.internalRowsWatcher) {
            this.internalRowsWatcher();
            this.internalRowsWatcher = undefined;
        }
    }

    // #endregion

    // #region IGridState Implementation

    public get rows(): ReadonlyArray<Record<string, unknown>> {
        return this.internalRows;
    }

    public get filteredRows(): ReadonlyArray<Record<string, unknown>> {
        return this.internalFilteredRows;
    }

    private set filteredRows(value: ReadonlyArray<Record<string, unknown>>) {
        this.internalFilteredRows = value;
        this.emitter.emit("filteredRowsChanged", this);
    }

    public get sortedRows(): ReadonlyArray<Record<string, unknown>> {
        return this.internalSortedRows;
    }

    private set sortedRows(value: ReadonlyArray<Record<string, unknown>>) {
        this.internalSortedRows = value;
        this.emitter.emit("sortedRowsChanged", this);
    }

    public get columns(): ColumnDefinition[] {
        return [...this.internalColumns];
    }

    public get visibleColumns(): ReadonlyArray<ColumnDefinition> {
        return this.internalVisibleColumns;
    }

    private set visibleColumns(value: ReadonlyArray<ColumnDefinition>) {
        this.internalVisibleColumns = value;
        this.emitter.emit("visibleColumnsChanged", this);
    }

    public get selectedKeys(): string[] {
        return this.internalSelectedKeys;
    }

    public set selectedKeys(value: string[]) {
        this.internalSelectedKeys = value;
        this.emitter.emit("selectedKeysChanged", this);
    }

    public get isFiltered(): boolean {
        return this.internalIsFiltered;
    }

    private set isFiltered(value: boolean) {
        this.internalIsFiltered = value;
        this.emitter.emit("isFilteredChanged", this);
    }

    public get isSorted(): boolean {
        return this.internalIsSorted;
    }

    private set isSorted(value: boolean) {
        this.internalIsSorted = value;
        this.emitter.emit("isSortedChanged", this);
    }

    public readonly cache: IGridCache = new GridCache();

    public readonly rowCache: GridRowCache;

    public readonly itemTerm: string;

    public readonly entityTypeGuid?: Guid | undefined;

    public getColumnCacheKey(column: ColumnDefinition, component: string, key: string): string {
        return `column-${column.name}-${component}-${key}`;
    }

    public getRowKey(row: Record<string, unknown>): string | undefined {
        return getRowKey(row, this.itemKey);
    }

    public getSortedRows(): Record<string, unknown>[] {
        return this.sortRows(this.internalRows);
    }

    on(event: keyof GridEvents, callback: (grid: IGridState) => void): void {
        this.emitter.on(event, callback);
    }

    off(event: keyof GridEvents, callback: (grid: IGridState) => void): void {
        this.emitter.off(event, callback);
    }

    // #endregion

    // #region Private Functions

    /**
     * Begins tracking all rows in the grid so that we can monitor for
     * changes and update the UI accordingly.
     */
    private initializeReactiveTracker(): void {
        const rows = unref(this.internalRows);

        this.rowReactiveTracker = {};

        for (let i = 0; i < rows.length; i++) {
            const key = getRowKey(rows[i], this.itemKey);

            if (key) {
                this.rowReactiveTracker[key] = JSON.stringify(rows[i]);
            }
        }
    }

    /**
     * Detects any changes to the row data from the last time we were called.
     * Must be called after {@link initializeReactiveTracker}.
     */
    private detectRowChanges(): void {
        const rows = unref(this.internalRows);
        const knownKeys = new Map<string, boolean>();
        let hasChanged = false;

        // Loop through all the rows we still have and check for any that
        // are new or have been modified.
        for (let i = 0; i < rows.length; i++) {
            const key = getRowKey(rows[i], this.itemKey);

            if (!key) {
                continue;
            }

            // Save the key for later.
            knownKeys.set(key, true);

            if (!this.rowReactiveTracker[key]) {
                hasChanged = true;
            }
            else if (this.rowReactiveTracker[key] !== JSON.stringify(rows[i])) {
                this.rowReactiveTracker[key] = JSON.stringify(rows[i]);
                this.rowCache.remove(rows[i]);
            }
        }

        // Loop through all the row key values that are being tracked and
        // see if any no longer exist in our data set.
        const oldKeys = Object.keys(this.rowReactiveTracker);
        for (let i = 0; i < oldKeys.length; i++) {
            if (!knownKeys.has(oldKeys[i])) {
                this.rowCache.removeByRowKey(oldKeys[i], undefined);
                delete this.rowReactiveTracker[oldKeys[i]];
                hasChanged = true;
            }
        }

        if (hasChanged) {
            this.emitter.emit("rowsChanged", this);
        }
    }

    /**
     * Performs filtering of the {@link rows} and determines which rows
     * match the filters.
     */
    private updateFilteredRows(): void {
        if (this.visibleColumns.length === 0) {
            this.filteredRows = [];
            this.selectedKeys = [];
            this.updateSortedRows();

            return;
        }

        const columns = this.visibleColumns;
        const quickFilterRawValue = this.quickFilter.toLowerCase();
        const oldFilteredKeys = this.filteredRows.map(r => this.getRowKey(r));

        const result = toRaw(this.rows).filter(row => {
            // Check if the row matches the quick filter.
            const quickFilterMatch = !quickFilterRawValue || columns.some((column): boolean => {
                const value = column.quickFilterValue(row, column, this);

                if (value === undefined) {
                    return false;
                }

                return value.toLowerCase().includes(quickFilterRawValue);
            });

            // Bail out early if the quick filter didn't match.
            if (!quickFilterMatch) {
                return false;
            }

            // Check if the row matches the column specific filters.
            return columns.every(column => {
                if (!column.filter) {
                    return true;
                }

                const columnFilterValue = this.columnFilters[column.name];

                if (columnFilterValue === undefined) {
                    return true;
                }

                const value: unknown = column.filterValue(row, column, this);

                return column.filter.matches(columnFilterValue, value, column, this);
            });
        });

        this.filteredRows = result;

        // If anything actually changed, clear the selection.
        const newFilteredKeys = this.filteredRows.map(r => this.getRowKey(r));
        if (!deepEqual(oldFilteredKeys, newFilteredKeys, true)) {
            this.selectedKeys = [];
        }

        this.updateSortedRows();
    }

    /**
     * Sorts the given set of rows.
     *
     * @param rows The rows that should be sorted according to the current sorting definition.
     *
     * @returns A new array of rows that is properly sorted.
     */
    private sortRows(rows: ReadonlyArray<Record<string, unknown>>): Record<string, unknown>[] {
        const columnSort = this.columnSort;

        // Bail early if we don't have any sorting to perform.
        if (!columnSort) {
            return [...rows];
        }

        const column = this.visibleColumns.find(c => c.name === columnSort.column);
        const order = columnSort.isDescending ? -1 : 1;

        if (!column) {
            console.warn("Ignoring invalid sort definition.", toRaw(this.columnSort));
            return [...rows];
        }

        const sortValue = column.sortValue;

        // Pre-process each row to calculate the sort value. Otherwise it will
        // be calculated exponentially during sort. This provides a serious
        // performance boost when sorting Lava columns. Even though we have
        // cache we do it this way because we may not have an itemKey which
        // would disable the cache.
        const rowsToSort = rows.map(r => {
            let value: string | number | undefined;

            if (sortValue) {
                value = sortValue(r, column, this);
            }
            else {
                value = undefined;
            }

            return {
                row: r,
                value
            };
        });

        rowsToSort.sort((a, b) => {
            if (a.value === undefined) {
                return -order;
            }
            else if (b.value === undefined) {
                return order;
            }
            else if (a.value < b.value) {
                return -order;
            }
            else if (a.value > b.value) {
                return order;
            }
            else {
                return 0;
            }
        });

        return rowsToSort.map(r => r.row);
    }

    /**
     * Takes the {@link filteredRows} and sorts them according to the information
     * tracked by the Grid and updates the {@link sortedRows} property.
     */
    private updateSortedRows(): void {
        this.sortedRows = this.sortRows(this.filteredRows);
    }

    /**
     * Gets the row key and row index of the specified key or index.
     *
     * @param keyOrIndex The row key or index.
     *
     * @returns An object that contains both the key and index or `undefined` if it was not found.
     */
    private getRowKeyAndIndex(keyOrIndex: string | number): { key: string, index: number } | undefined {
        let key: string | undefined = undefined;
        let index: number = -1;

        if (typeof keyOrIndex === "string") {
            key = keyOrIndex;
            index = this.rows.findIndex(r => getRowKey(r, this.itemKey) === key);
        }
        else if (keyOrIndex < this.rows.length) {
            index = keyOrIndex;
            key = getRowKey(this.rows[index], this.itemKey);
        }

        if (!key || index === -1) {
            return undefined;
        }

        return { key, index };
    }

    // #endregion

    // #region Public Functions

    /**
     * Sets the item key used to uniquely identify rows.
     *
     * @param value The field name that contains the item key.
     */
    public setItemKey(value: string | undefined): void {
        this.itemKey = value;
        (this.rowCache as GridRowCache).setRowItemKey(value);
    }

    /**
     * Sets the columns that can be used on the grid.
     *
     * @param columns The new columns that should be available on the grid.
     */
    public setColumns(columns: ColumnDefinition[]): void {
        // Stop the cache worker if it is running.
        if (this.populateRowCacheWorker) {
            this.populateRowCacheWorker.cancel();
            this.populateRowCacheWorker = null;
        }

        // Clear all the cache.
        this.cache.clear();
        this.rowCache.clear();

        // Update the columns.
        if (this.gridDefinition?.customColumns && this.gridDefinition.customColumns.length > 0) {
            const tempColumns = [...columns];
            insertCustomColumns(tempColumns, this.gridDefinition.customColumns);
            this.internalColumns = tempColumns;
        }
        else {
            this.internalColumns = [...columns];
        }

        this.visibleColumns = this.columns.filter(c => !c.hideOnScreen);

        // Start the cache worker.
        this.populateRowCacheWorker = new BackgroundGridRowCacheWorker(this);
        this.populateRowCacheWorker.run().catch(err => {
            if (!(err instanceof Error) || err.message !== "Cancellation requested.") {
                console.error(err);
            }
        });
    }

    /**
     * Sets the rows to be used by the Grid. This will replace all existing
     * row data.
     *
     * @param rows The array of row data to use for the Grid.
     */
    public setDataRows(rows: Record<string, unknown>[]): void {
        // Stop the cache worker if it is running.
        if (this.populateRowCacheWorker) {
            this.populateRowCacheWorker.cancel();
            this.populateRowCacheWorker = null;
        }

        // Stop watching the old rows if we are currently watching for changes.
        if (this.internalRowsWatcher) {
            this.internalRowsWatcher();
            this.internalRowsWatcher = undefined;
        }

        // Update our internal rows and clear all the cache.
        this.internalRows = this.liveUpdates ? rows : reactive(rows);
        this.cache.clear();
        this.rowCache.clear();

        // Start watching for changes if we are reactive.
        if (this.liveUpdates) {
            this.initializeReactiveTracker();

            this.internalRowsWatcher = watch(() => rows, () => {
                this.detectRowChanges();
                this.updateFilteredRows();
            }, { deep: true });
        }

        // Start the cache worker.
        this.populateRowCacheWorker = new BackgroundGridRowCacheWorker(this);
        this.populateRowCacheWorker.run().catch(err => {
            if (!(err instanceof Error) || err.message !== "Cancellation requested.") {
                console.error(err);
            }
        });

        this.emitter.emit("rowsChanged", this);
        this.updateFilteredRows();
    }

    /**
     * Sets the filters to be used to filter the rows down to a limited set.
     *
     * @param quickFilter The value to use for quick filtering.
     * @param columnFilters The column filters to apply to the data.
     */
    public setFilters(quickFilter: string | undefined, columnFilters: Record<string, unknown> | undefined): void {
        this.quickFilter = quickFilter ?? "";
        this.columnFilters = columnFilters ?? {};
        this.updateFilteredRows();

        const hasColumnFilter = Object.values(this.columnFilters)
            .some(v => v !== undefined);

        this.isFiltered = this.quickFilter !== "" || hasColumnFilter;
    }

    /**
     * Sets the column that will be used to sort the filtered rows.
     *
     * @param columnSort The column that will be used for sorting.
     */
    public setSort(columnSort: ColumnSort | undefined): void {
        this.columnSort = columnSort;
        this.updateSortedRows();

        this.isSorted = this.columnSort !== undefined;
    }

    /**
     * Deletes a row from the grid when not tracking live updates.
     *
     * @param keyOrIndex The row key or row index that should be deleted.
     */
    public deleteRow(keyOrIndex: string | number): void {
        const item = this.getRowKeyAndIndex(keyOrIndex);

        if (!item) {
            throw new Error(`Row '${keyOrIndex}' was not found in grid.`);
        }

        this.internalRows.splice(item.index, 1);
        this.rowCache.removeByRowKey(item.key, undefined);

        if (this.liveUpdates) {
            delete this.rowReactiveTracker[item.key];
        }

        this.updateFilteredRows();

        this.emitter.emit("rowsChanged", this);
    }

    /**
     * Informs the grid that the specified row has been modified.
     *
     * @param keyOrIndex The row key or index that was updated.
     */
    public rowUpdated(keyOrIndex: string | number): void {
        const item = this.getRowKeyAndIndex(keyOrIndex);

        if (!item) {
            throw new Error(`Row '${keyOrIndex}' was not found in grid.`);
        }

        this.rowCache.remove(this.rows[item.index]);

        if (this.liveUpdates) {
            this.rowReactiveTracker[item.key] = JSON.stringify(this.rows[item.index]);
        }

        this.updateFilteredRows();
    }

    /**
     * Informs the grid that the specified rows have been modified.
     *
     * @param keysOrIndexes The row keys or indexes that have been updated.
     */
    public rowsUpdated(keysOrIndexes: readonly string[] | readonly number[]): void {
        for (const keyOrIndex of keysOrIndexes) {
            const item = this.getRowKeyAndIndex(keyOrIndex);

            if (!item) {
                throw new Error(`Row '${keyOrIndex}' was not found in grid.`);
            }

            this.rowCache.remove(this.rows[item.index]);

            if (this.liveUpdates) {
                this.rowReactiveTracker[item.key] = JSON.stringify(this.rows[item.index]);
            }
        }

        this.updateFilteredRows();
    }

    // #endregion
}

// #endregion

// #region Internal Components

/**
 * This is a special cell we use when no default format cell has been defined.
 */
const defaultCell = defineComponent({
    props: standardCellProps,

    setup(props) {
        return () => props.column.field ? props.row[props.column.field] : "";
    }
});

/**
 * This is a special cell we use to display custom column cells.
 */
const htmlCell = defineComponent({
    props: standardCellProps,

    setup(props) {
        return () => {
            let html = props.column.field ? props.row[props.column.field] : "";

            if (typeof html !== "string") {
                html = "";
            }

            return createElementVNode("div", {
                innerHTML: html
            });
        };
    }
});

// #endregion
