// Copyright by the Spark Development Network; Licensed under the Rock Community License

import { FilterExpressionType } from "@Obsidian/Enums/Reporting/filterExpressionType";
import { FilterGroupJoinType } from "@Obsidian/Enums/Reporting/filterGroupJoinType";
import { FilterGroupTruthType } from "@Obsidian/Enums/Reporting/filterGroupTruthType";
import { newGuid } from "@Obsidian/Utility/guid";
import { DataViewFilterBag } from "@Obsidian/ViewModels/Reporting/dataViewFilterBag";

export function cloneFilterNode(node: DataViewFilterBag | null | undefined): DataViewFilterBag {
    return JSON.parse(JSON.stringify(node ?? createDefaultDataViewFilter()));
}

export function createFilterNode(): DataViewFilterBag {
    return {
        guid: newGuid(),
        expressionType: FilterExpressionType.Filter,
        filterTypeGuid: null,
        selection: null,
        componentData: {},
        childFilters: []
    };
}

export function createGroupNode(expressionType: FilterExpressionType = FilterExpressionType.GroupAll): DataViewFilterBag {
    return {
        guid: newGuid(),
        expressionType,
        childFilters: [createFilterNode()]
    };
}

export function createDefaultDataViewFilter(): DataViewFilterBag {
    return createGroupNode(FilterExpressionType.GroupAll);
}

export function isFilterNode(node: DataViewFilterBag): boolean {
    return node.expressionType === FilterExpressionType.Filter;
}

export function getGroupJoinType(expressionType: FilterExpressionType): FilterGroupJoinType {
    return expressionType === FilterExpressionType.GroupAny || expressionType === FilterExpressionType.GroupAnyFalse
        ? FilterGroupJoinType.Any
        : FilterGroupJoinType.All;
}

export function getGroupTruthType(expressionType: FilterExpressionType): FilterGroupTruthType {
    return expressionType === FilterExpressionType.GroupAllFalse || expressionType === FilterExpressionType.GroupAnyFalse
        ? FilterGroupTruthType.False
        : FilterGroupTruthType.True;
}

export function setGroupJoinType(node: DataViewFilterBag, joinType: FilterGroupJoinType): void {
    const isFalse = node.expressionType === FilterExpressionType.GroupAllFalse || node.expressionType === FilterExpressionType.GroupAnyFalse;

    if (joinType === FilterGroupJoinType.All) {
        node.expressionType = isFalse ? FilterExpressionType.GroupAllFalse : FilterExpressionType.GroupAll;
    }
    else {
        node.expressionType = isFalse ? FilterExpressionType.GroupAnyFalse : FilterExpressionType.GroupAny;
    }
}

export function setGroupTruthType(node: DataViewFilterBag, truthType: FilterGroupTruthType): void {
    const isAll = node.expressionType === FilterExpressionType.GroupAll || node.expressionType === FilterExpressionType.GroupAllFalse;

    if (truthType === FilterGroupTruthType.True) {
        node.expressionType = isAll ? FilterExpressionType.GroupAll : FilterExpressionType.GroupAny;
    }
    else {
        node.expressionType = isAll ? FilterExpressionType.GroupAllFalse : FilterExpressionType.GroupAnyFalse;
    }
}
