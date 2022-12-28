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

import { FilterMode } from "./filterMode";
import { ComparisonType } from "@Obsidian/Enums/Reporting/comparisonType";

/** The comparison types typically used for simple binary comparison. */
export const binaryComparisonTypes: ComparisonType = ComparisonType.EqualTo
    | ComparisonType.NotEqualTo;

/** The comparison types typically used for string-type values. */
export const stringComparisonTypes: ComparisonType = ComparisonType.Contains
    | ComparisonType.DoesNotContain
    | ComparisonType.EqualTo
    | ComparisonType.NotEqualTo
    | ComparisonType.IsBlank
    | ComparisonType.IsNotBlank
    | ComparisonType.StartsWith
    | ComparisonType.EndsWith;

/** The comparison types typically used for multiple choice values. */
export const containsComparisonTypes: ComparisonType = ComparisonType.Contains
    | ComparisonType.DoesNotContain
    | ComparisonType.IsBlank;

/** The comparison types typically used for numeric values. */
export const numericComparisonTypes: ComparisonType = ComparisonType.EqualTo
    | ComparisonType.IsBlank
    | ComparisonType.IsNotBlank
    | ComparisonType.NotEqualTo
    | ComparisonType.GreaterThan
    | ComparisonType.GreaterThanOrEqualTo
    | ComparisonType.LessThan
    | ComparisonType.LessThanOrEqualTo;

/** The comparison types typically used for date values. */
export const dateComparisonTypes: ComparisonType = ComparisonType.EqualTo
    | ComparisonType.IsBlank
    | ComparisonType.IsNotBlank
    | ComparisonType.GreaterThan
    | ComparisonType.GreaterThanOrEqualTo
    | ComparisonType.LessThan
    | ComparisonType.LessThanOrEqualTo
    | ComparisonType.Between;

/**
 * Gets the user friendly name for the comparison type.
 *
 * @param type The type of comparison.
 *
 * @returns A string containing the name of the comparison.
 */
export function getComparisonName(type: ComparisonType): string {
    switch (type) {
        case ComparisonType.EqualTo:
            return "Equal To";

        case ComparisonType.NotEqualTo:
            return "Not Equal To";

        case ComparisonType.StartsWith:
            return "Starts With";

        case ComparisonType.Contains:
            return "Contains";

        case ComparisonType.DoesNotContain:
            return "Does Not Contain";

        case ComparisonType.IsBlank:
            return "Is Blank";

        case ComparisonType.IsNotBlank:
            return "Is Not Blank";

        case ComparisonType.GreaterThan:
            return "Greater Than";

        case ComparisonType.GreaterThanOrEqualTo:
            return "Greater Than Or Equal To";

        case ComparisonType.LessThan:
            return "Less Than";

        case ComparisonType.LessThanOrEqualTo:
            return "Less Than Or Equal To";

        case ComparisonType.EndsWith:
            return "Ends With";

        case ComparisonType.Between:
            return "Between";

        case ComparisonType.RegularExpression:
            return "Regular Expression";

        default:
            return "";
    }
}

/**
 * Checks if the standard comparison type component should be visible or not for
 * the given comparison type options and filter mode.
 *
 * @param comparisonType The comparison types available to be selected.
 * @param filterMode The type of filtering UI to show.
 *
 * @returns true if the comparison type component should be visible; otherwise false.
 */
export function isCompareVisibleForComparisonFilter(comparisonType: ComparisonType, filterMode: FilterMode): boolean {
    if (filterMode !== FilterMode.Simple) {
        return true;
    }

    const isHideable = comparisonType === binaryComparisonTypes
        || comparisonType === stringComparisonTypes
        || comparisonType === containsComparisonTypes;

    return !isHideable;
}

/**
 * Determines if this comparision type specifies one and only one type. This
 * uses bitwise logic to ensure that only a single bit is set.
 *
 * @param comparisionType The comparison type to check.
 *
 * @returns true if the comparison type specifies one and only one type.
 */
export function isSingleComparisonType(comparisionType: ComparisonType): boolean {
    return comparisionType === ComparisonType.EqualTo
        || comparisionType === ComparisonType.NotEqualTo
        || comparisionType === ComparisonType.StartsWith
        || comparisionType === ComparisonType.Contains
        || comparisionType === ComparisonType.DoesNotContain
        || comparisionType === ComparisonType.IsBlank
        || comparisionType === ComparisonType.IsNotBlank
        || comparisionType === ComparisonType.GreaterThan
        || comparisionType === ComparisonType.GreaterThanOrEqualTo
        || comparisionType === ComparisonType.LessThan
        || comparisionType === ComparisonType.LessThanOrEqualTo
        || comparisionType === ComparisonType.EndsWith
        || comparisionType === ComparisonType.Between
        || comparisionType === ComparisonType.RegularExpression;
}
