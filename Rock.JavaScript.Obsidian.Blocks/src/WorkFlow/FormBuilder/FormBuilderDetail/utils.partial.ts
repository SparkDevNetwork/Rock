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
import { getFieldType } from "@Obsidian/Utility/fieldTypes";
import { areEqual } from "@Obsidian/Utility/guid";
import { FieldFilterRuleBag } from "@Obsidian/ViewModels/Reporting/fieldFilterRuleBag";
import { FieldFilterSourceBag } from "@Obsidian/ViewModels/Reporting/fieldFilterSourceBag";
import { FormField } from "../Shared/types.partial";
import { FormValueSources } from "./types.partial";

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
 * Get the description of the rule, including the name of the field it depends on.
 *
 * @param rule The rule to be represented.
 * @param sources The field filter sources to use when looking up the source field.
 * @param fields The fields that contain the attribute information.
 *
 * @returns A plain text string that represents the rule in a human friendly format.
 */
export function getFilterRuleDescription(rule: FieldFilterRuleBag, sources: FieldFilterSourceBag[], fields: FormField[]): string {
    const ruleField = fields.filter(f => areEqual(f.guid, rule.attributeGuid));
    const ruleSource = sources.filter(s => areEqual(s.guid, rule.attributeGuid));

    if (ruleField.length === 1 && ruleSource.length === 1 && ruleSource[0].attribute) {
        const fieldType = getFieldType(ruleField[0].universalFieldTypeGuid ?? ruleField[0].fieldTypeGuid);

        if (fieldType) {
            const descr = fieldType.getFilterValueDescription({
                comparisonType: rule.comparisonType,
                value: rule.value ?? ""
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
