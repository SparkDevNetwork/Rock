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
import { FieldFilterGroupBag } from "@Obsidian/ViewModels/Reporting/fieldFilterGroupBag";
import { FilterExpressionType } from "@Obsidian/Enums/Reporting/filterExpressionType";
import { FieldFilterRuleBag } from "@Obsidian/ViewModels/Reporting/fieldFilterRuleBag";
import { FieldFilterSourceBag } from "@Obsidian/ViewModels/Reporting/fieldFilterSourceBag";
import { getFieldType } from "@Obsidian/Utility/fieldTypes";
import { areEqual } from "@Obsidian/Utility/guid";

/**
 * Get the friendly formatted title of a filter group. This returns an HTML
 * string.
 *
 * @param group The group that contains the comparison type information.
 *
 * @returns An HTML formatted string with the comparison type text.
 */
export function getFilterGroupTitleHtml(group: FieldFilterGroupBag): string {
    switch (group.expressionType) {
        case FilterExpressionType.GroupAll:
            return "<strong>Show</strong> when <strong>all</strong> of the following match:";

        case FilterExpressionType.GroupAny:
            return "<strong>Show</strong> when <strong>any</strong> of the following match:";

        case FilterExpressionType.GroupAllFalse:
            return "<strong>Hide</strong> when <strong>all</strong> of the following match:";

        case FilterExpressionType.GroupAnyFalse:
            return "<strong>Hide</strong> when <strong>any</strong> of the following match:";

        default:
            return "";
    }
}

/**
 * Get the description of the rule, including the name of the field it depends on.
 *
 * @param rule The rule to be represented.
 * @param sources The field filter sources to use when looking up the source field.
 * @param fields The fields that contain the attribute information.
 *
 * @returns A plain text string that represents the rule in a human friendly format.
 */
export function getFilterRuleDescription(rule: FieldFilterRuleBag, sources: FieldFilterSourceBag[]): string {
    const ruleSource = sources.find(s => (s.attribute && areEqual(s.attribute?.attributeGuid, rule.attributeGuid))
        || (s.property && s.property?.name === rule.propertyName));

    if (!ruleSource) {
        return "";
    }

    if (ruleSource.attribute) {
        const fieldType = getFieldType(ruleSource.attribute.fieldTypeGuid);

        if (!fieldType) {
            return "";
        }

        const descr = fieldType.getFilterValueDescription({
            comparisonType: rule.comparisonType,
            value: rule.value ?? ""
        }, ruleSource.attribute.configurationValues ?? {});

        return `${ruleSource.attribute.name} ${descr}`;
    }
    else if (ruleSource.property) {
        const fieldType = getFieldType(ruleSource.property.fieldTypeGuid);

        if (!fieldType) {
            return "";
        }

        const descr = fieldType.getFilterValueDescription({
            comparisonType: rule.comparisonType,
            value: rule.value ?? ""
        }, ruleSource.property.configurationValues ?? {});

        return `${ruleSource.property.title} ${descr}`;
    }

    return "";
}
