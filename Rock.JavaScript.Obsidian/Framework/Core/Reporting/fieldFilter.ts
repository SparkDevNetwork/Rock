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
import { FieldFilterSourceType } from "@Obsidian/Enums/Reporting/fieldFilterSourceType";
import { FilterExpressionType } from "@Obsidian/Enums/Reporting/filterExpressionType";
import { ComparisonValue } from "@Obsidian/Types/Reporting/comparisonValue";
import { FieldFilterAttributeData } from "@Obsidian/Types/Reporting/fieldFilterAttributeData";
import { getFieldType } from "@Obsidian/Utility/fieldTypes";
import { areEqual } from "@Obsidian/Utility/guid";
import { FieldFilterGroupBag } from "@Obsidian/ViewModels/Reporting/fieldFilterGroupBag";
import { FieldFilterRuleBag } from "@Obsidian/ViewModels/Reporting/fieldFilterRuleBag";
import { FieldFilterSourceBag } from "@Obsidian/ViewModels/Reporting/fieldFilterSourceBag";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";

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

// #region Filter

/**
 * Provides filtering logic for {@link FieldFilterGroupBag} objects. This can
 * either perform a one-time check against a single instance or build a
 * function that can be used repeatedly against different objects.
 */
export class FieldFilterFunctionBuilder<T = unknown> {
    /**
     * Checks if the object matches the rule(s) provided by the filter.
     *
     * @param instance The object to be checked to determine if it matches the filter.
     * @param filter The rule(s) that describe the filter conditions.
     *
     * @returns `true` if the object matched the rules; otherwise `false`.
     */
    public isMatch(instance: T, filter: FieldFilterGroupBag): boolean {
        if (!instance) {
            // If no object, then no match.
            return false;
        }

        return this.isGroupMatch(instance, filter);
    }

    /**
     * Gets a reusable function that can be called on various objects to determine
     * if they match the provided filter.
     *
     * @param filter The rule(s) that describe the filter conditions.
     *
     * @returns A function that can be called to determine if an object matches the filter.
     */
    public getIsMatchFunction(filter: FieldFilterGroupBag): (instance: T) => boolean {
        return (instance: T) => this.isMatch(instance, filter);
    }

    /**
     * Determines if the object matches the filter rule group.
     *
     * @param instance The object to be checked to determine if it matches the filter.
     * @param filter The rule(s) that describe the filter conditions.
     *
     * @returns `true` if the object matched the rules; otherwise `false`.
     */
    protected isGroupMatch(instance: T, filter: FieldFilterGroupBag): boolean {
        const conditionGroups = filter.groups
            ?.map(g => this.isGroupMatch(instance, g)) ?? [];
        const conditionRules = filter.rules
            ?.map(rule => this.isRuleMatch(instance, rule)) ?? [];
        const conditionResults = [...conditionGroups, ...conditionRules];

        if (conditionResults.length === 0) {
            // If there were not any rules, then return a match.
            return true;
        }

        let result: boolean;

        switch (filter.expressionType) {
            case FilterExpressionType.GroupAll:
            case FilterExpressionType.GroupAllFalse:
                result = conditionResults.every(cr => cr);
                break;

            case FilterExpressionType.GroupAny:
            case FilterExpressionType.GroupAnyFalse:
                result = conditionResults.some(cr => cr);
                break;

            default:
                throw new Error(`Invalid group expression type (${filter.expressionType}).`);
        }

        if (filter.expressionType === FilterExpressionType.GroupAllFalse || filter.expressionType === FilterExpressionType.GroupAnyFalse) {
            result = !result;
        }

        return result;
    }

    /**
     * Determines if the object matches a single filter rule.
     *
     * @param instance The object to be checked to determine if it matches the filter.
     * @param rule The single rule that describes the filter condition.
     *
     * @returns `true` if the object matched the rule; otherwise `false`.
     */
    protected isRuleMatch(instance: T, rule: FieldFilterRuleBag): boolean {
        if (rule.sourceType === FieldFilterSourceType.Property) {
            return this.isRulePropertyMatch(instance, rule);
        }
        else if (rule.sourceType === FieldFilterSourceType.Attribute) {
            return this.isRuleAttributeMatch(instance, rule);
        }
        else {
            // The rule was not fully configured, so don't use it to filter
            // results. This matches the DataView logic.
            // See: Rock.Reporting.DataFilter.PropertyFilter.GetExpression()
            return true;
        }
    }

    /**
     * Determines if the object matches a single property rule.
     *
     * @param instance The object to be checked to determine if it matches the filter.
     * @param rule The single rule that describes the filter condition.
     *
     * @returns `true` if the object matched the rule; otherwise `false`.
     */
    protected isRulePropertyMatch(instance: T, rule: FieldFilterRuleBag): boolean {
        // Make TypeScript happy about the parameters being unused.
        const _instance = instance;
        const _rule = rule;

        // This requires additional logic to deal with property types. For
        // example, how do we know that a string property is actually a date so
        // we can compare it with a propery date logic?
        throw new Error("Property filter rules are not supported yet.");
    }

    /**
     * Determines if the object matches a single attribute rule.
     *
     * @param instance The object to be checked to determine if it matches the filter.
     * @param rule The single rule that describes the filter condition.
     *
     * @returns `true` if the object matched the rule; otherwise `false`.
     */
    protected isRuleAttributeMatch(instance: T, rule: FieldFilterRuleBag): boolean {
        const value = this.getInstanceAttributeValue(instance, rule);
        const attribute = this.getInstanceAttributeData(instance, rule);

        if (attribute === undefined || value === undefined) {
            return false;
        }

        const fieldType = getFieldType(attribute.fieldTypeGuid);

        if (!fieldType) {
            return false;
        }

        const comparisonValue: ComparisonValue = {
            value: rule.value ?? "",
            comparisonType: rule.comparisonType
        };

        return fieldType.doesValueMatchFilter(value, comparisonValue, attribute.configurationValues ?? {});
    }

    /**
     * Gets the required data which describes the attribute. This is used to
     * load the field type and provide the configuration values when calling
     * the compare function on the field.
     *
     * @param instance The object to be checked to determine if it matches the filter.
     * @param rule The single rule that describes the filter condition.
     *
     * @returns An object that describes the attribute details or `undefined` if
     * the attribute could not be found.
     */
    protected getInstanceAttributeData(instance: T, rule: FieldFilterRuleBag): FieldFilterAttributeData | undefined {
        // If instance type does not support attributes then always return no
        // match. In this case, we have no object to work with.
        if (!instance || typeof instance !== "object") {
            return undefined;
        }

        // Look for the standard "attributes" property for entities.
        const attributes = instance["attributes"] as Record<string, PublicAttributeBag> | null | undefined;
        if (!attributes || !Array.isArray(attributes)) {
            return undefined;
        }

        // Try to find the attribute with a matching guid.
        const attribute = Object.values<PublicAttributeBag>(attributes)
            .find(a => areEqual(a.attributeGuid, rule.attributeGuid));

        return attribute ?? undefined;
    }

    /**
     * Gets the current value of the attribute on the object. The value must
     * be in public edit format in order for the filter to work correctly.
     *
     * @param instance The object to be checked to determine if it matches the filter.
     * @param rule The single rule that describes the filter condition.
     *
     * @returns A string that represents the current edit value.
     */
    protected getInstanceAttributeValue(instance: T, rule: FieldFilterRuleBag): string | undefined {
        // If instance type does not support attributes then always return no
        // match. In this case, we have no object to work with.
        if (!instance || typeof instance !== "object") {
            return undefined;
        }

        // Look for the standard "attributes" property for entities.
        const attributes = instance["attributes"] as Record<string, PublicAttributeBag> | null | undefined;
        if (!attributes || !Array.isArray(attributes)) {
            return undefined;
        }

        // Try to find the attribute with a matching guid.
        const attribute = Object.values<PublicAttributeBag>(attributes)
            .find(a => areEqual(a.attributeGuid, rule.attributeGuid));

        if (!attribute || !attribute.key) {
            return undefined;
        }

        // Look for the standard "attributeValues" property for entities.
        const attributeValues = instance["attributeValues"] as Record<string, string> | null | undefined;
        if (!attributeValues || typeof attributeValues !== "object") {
            return undefined;
        }

        return attributeValues[attribute.key] ?? "";
    }
}

// #endregion
