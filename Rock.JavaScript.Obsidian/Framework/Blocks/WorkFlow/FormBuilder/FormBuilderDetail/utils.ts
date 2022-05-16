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

import { inject, provide } from "vue";
import { getFieldType } from "../../../../Fields/utils";
import { FilterExpressionType } from "../../../../Reporting/filterExpressionType";
import { areEqual } from "../../../../Util/guid";
import { FieldFilterGroup } from "../../../../ViewModels/Reporting/fieldFilterGroup";
import { FieldFilterRule } from "../../../../ViewModels/Reporting/fieldFilterRule";
import { FieldFilterSource } from "../../../../ViewModels/Reporting/fieldFilterSource";
import { FormField } from "../Shared/types";
import { FormValueSources } from "./types";

// Unique key used to track the sources for the FormTemplateDetail block.
const sourcesKey = Symbol();

/**
 * Make the list of value sources available to child components.
 *
 * @param sources The value sources to make available.
 */
export function provideFormSources(options: FormValueSources): void {
    provide(sourcesKey, options);
}

/**
 * Uses the value sources previously made available by the parent component.
 *
 * @returns The value sources that were provided by the parent component.
 */
export function useFormSources(): FormValueSources {
    return inject<FormValueSources>(sourcesKey) ?? {};
}

/**
 * Get the friendly formatted title of a filter group. This returns an HTML
 * string.
 * 
 * @param group The group that contains the comparison type information.
 *
 * @returns An HTML formatted string with the comparison type text.
 */
export function getFilterGroupTitle(group: FieldFilterGroup): string {
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
export function getFilterRuleDescription(rule: FieldFilterRule, sources: FieldFilterSource[], fields: FormField[]): string {
    const ruleField = fields.filter(f => areEqual(f.guid, rule.attributeGuid));
    const ruleSource = sources.filter(s => areEqual(s.guid, rule.attributeGuid));

    if (ruleField.length === 1 && ruleSource.length === 1 && ruleSource[0].attribute) {
        const fieldType = getFieldType(ruleField[0].fieldTypeGuid);

        if (fieldType) {
            const descr = fieldType.getFilterValueDescription({
                comparisonType: rule.comparisonType,
                value: rule.value
            }, ruleSource[0].attribute.configurationValues ?? {});

            return `${ruleSource[0].attribute.name} ${descr}`;
        }
    }

    return "";
}

/**
 * Creates a promise that rejects when the timeout has elapsed.
 * 
 * @param ms The timeout in milliseconds.
 */
export function timeoutAsync(ms: number): Promise<void> {
    return new Promise<void>((_resolve, reject) => {
        setTimeout(reject, ms);
    });
}
