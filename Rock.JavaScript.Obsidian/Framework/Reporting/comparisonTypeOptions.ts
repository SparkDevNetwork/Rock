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

import { toNumber } from "../Services/number";
import { ListItem } from "../ViewModels";
import { ComparisonType, getComparisonName } from "./comparisonType";

/**
 * The full list of comparison type options that can be used in pickers.
 */
export const comparisonTypeOptions: ListItem[] = [
    {
        value: ComparisonType.EqualTo.toString(),
        text: getComparisonName(ComparisonType.EqualTo)
    },
    {
        value: ComparisonType.NotEqualTo.toString(),
        text: getComparisonName(ComparisonType.NotEqualTo)
    },
    {
        value: ComparisonType.Contains.toString(),
        text: getComparisonName(ComparisonType.Contains)
    },
    {
        value: ComparisonType.DoesNotContain.toString(),
        text: getComparisonName(ComparisonType.DoesNotContain)
    },
    {
        value: ComparisonType.IsBlank.toString(),
        text: getComparisonName(ComparisonType.IsBlank)
    },
    {
        value: ComparisonType.IsNotBlank.toString(),
        text: getComparisonName(ComparisonType.IsNotBlank)
    },
    {
        value: ComparisonType.GreaterThan.toString(),
        text: getComparisonName(ComparisonType.GreaterThan)
    },
    {
        value: ComparisonType.GreaterThanOrEqualTo.toString(),
        text: getComparisonName(ComparisonType.GreaterThanOrEqualTo)
    },
    {
        value: ComparisonType.LessThan.toString(),
        text: getComparisonName(ComparisonType.LessThan)
    },
    {
        value: ComparisonType.LessThanOrEqualTo.toString(),
        text: getComparisonName(ComparisonType.LessThanOrEqualTo)
    },
    {
        value: ComparisonType.StartsWith.toString(),
        text: getComparisonName(ComparisonType.StartsWith)
    },
    {
        value: ComparisonType.EndsWith.toString(),
        text: getComparisonName(ComparisonType.EndsWith)
    },
    {
        value: ComparisonType.Between.toString(),
        text: getComparisonName(ComparisonType.Between)
    },
    {
        value: ComparisonType.RegularExpression.toString(),
        text: getComparisonName(ComparisonType.RegularExpression)
    }
];

/**
 * Gets the comparison type options that match the provides comparison type values.
 * 
 * @param comparisonTypes The comparison type values to include in the list of options.
 *
 * @returns A filtered collection of ListItem objects that contain only the comparison types specified.
 */
export function getFilteredComparisonTypeOptions(...comparisonTypes: ComparisonType[]): ListItem[] {
    let realComparisonTypes: ComparisonType = 0;

    for (const comparisonType of comparisonTypes) {
        realComparisonTypes |= comparisonType;
    }

    return comparisonTypeOptions.filter(c => {
        return (realComparisonTypes & toNumber(c.value)) !== 0;
    });
}
