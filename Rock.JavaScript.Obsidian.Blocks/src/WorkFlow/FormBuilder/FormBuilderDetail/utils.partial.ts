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
import { FieldType } from "@Obsidian/SystemGuids/fieldType";
import { FieldFilterRuleBag } from "@Obsidian/ViewModels/Reporting/fieldFilterRuleBag";
import { FieldFilterSourceBag } from "@Obsidian/ViewModels/Reporting/fieldFilterSourceBag";
import { FormField } from "../Shared/types.partial";
import { FormValueSources } from "./types.partial";

/*
    Tabler icon class per field type, keyed by GUID. Renders in place
    of the server-supplied SVG when present. Shared between the
    sidebar tile grid and the field-edit aside title bar so both
    surface the same icon for a given field type.
*/
const fieldIconMap: Record<string, string> = {
    // Common
    [FieldType.Boolean]: "ti ti-square-check",
    [FieldType.Text]: "ti ti-typography",
    [FieldType.Integer]: "ti ti-hash",
    [FieldType.SingleSelect]: "ti ti-select",
    [FieldType.MultiSelect]: "ti ti-list-check",
    [FieldType.Memo]: "ti ti-align-left",
    [FieldType.Date]: "ti ti-calendar",
    [FieldType.DateTime]: "ti ti-calendar-clock",
    [FieldType.Time]: "ti ti-clock",
    // Additional
    [FieldType.Address]: "ti ti-map-pin",
    [FieldType.Email]: "ti ti-mail",
    [FieldType.PhoneNumber]: "ti ti-phone",
    [FieldType.Gender]: "ti ti-friends",
    [FieldType.Campus]: "ti ti-building",
    [FieldType.Campuses]: "ti ti-building-community",
    [FieldType.DefinedValueCategorized]: "ti ti-folder",
    [FieldType.DefinedValue]: "ti ti-book-2",
    [FieldType.KeyValueList]: "ti ti-list-details",
    [FieldType.DayOfWeek]: "ti ti-calendar-event",
    [FieldType.DaysOfWeek]: "ti ti-calendar-week",
    [FieldType.DateRange]: "ti ti-calendar-month",
    [FieldType.MonthDay]: "ti ti-calendar",
    [FieldType.Currency]: "ti ti-currency-dollar",
    [FieldType.Decimal]: "ti ti-decimal",
    [FieldType.DecimalRange]: "ti ti-decimal",
    [FieldType.IntegerRange]: "ti ti-hash",
    [FieldType.RangeSlider]: "ti ti-arrow-right-bar",
    [FieldType.File]: "ti ti-upload",
    [FieldType.Image]: "ti ti-photo",
    [FieldType.StructureContentEditor]: "ti ti-template",
    [FieldType.Rating]: "ti ti-star-half",
    [FieldType.Ssn]: "ti ti-id",
    [FieldType.UrlLink]: "ti ti-link"
};

/*
    Display-name overrides keyed by field type GUID. Lets surfaces
    present a name that differs from the server's field-type Name
    without changing the underlying type.
*/
const fieldLabelMap: Record<string, string> = {
    [FieldType.DateTime]: "Date/Time",
    [FieldType.DefinedValueCategorized]: "Cat. Def. Value",
    [FieldType.StructureContentEditor]: "Structured Content",
    [FieldType.Ssn]: "SSN",
    [FieldType.UrlLink]: "URL Link"
};

/*
    Range-style field types — paired with a secondary
    `arrows-horizontal` glyph in any icon-rendering surface so the
    visual communicates "this is a range" alongside the base type.
*/
const rangeIndicatorFieldTypes: string[] = [
    FieldType.DecimalRange,
    FieldType.IntegerRange
];

/**
 * Resolves the Tabler icon class for a field type, or null if there
 * is no mapping (in which case callers can fall back to the
 * server-supplied SVG).
 */
export function getFieldTablerIcon(fieldTypeGuid: string | null | undefined): string | null {
    if (!fieldTypeGuid) {
        return null;
    }
    for (const guid of Object.keys(fieldIconMap)) {
        if (areEqual(guid, fieldTypeGuid)) {
            return fieldIconMap[guid];
        }
    }
    return null;
}

/**
 * Resolves the display label for a field type, falling back to the
 * supplied text when no override is registered.
 */
export function getFieldDisplayLabel(fieldTypeGuid: string | null | undefined, fallback: string): string {
    if (!fieldTypeGuid) {
        return fallback;
    }
    for (const guid of Object.keys(fieldLabelMap)) {
        if (areEqual(guid, fieldTypeGuid)) {
            return fieldLabelMap[guid];
        }
    }
    return fallback;
}

/**
 * Returns true if the field type should render a secondary
 * `arrows-horizontal` range indicator alongside its main icon.
 */
export function fieldHasRangeIndicator(fieldTypeGuid: string | null | undefined): boolean {
    if (!fieldTypeGuid) {
        return false;
    }
    return rangeIndicatorFieldTypes.some(g => areEqual(g, fieldTypeGuid));
}

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
