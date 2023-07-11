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

import type { RulesPropType, ValidationResult, ValidationRule, ValidationRuleFunction, ValidationRuleReference } from "@Obsidian/Types/validationRules";
import type { PropType } from "vue";

/** The currently defined rules by name. */
const definedRules: Record<string, ValidationRuleFunction | undefined> = {};

/** Defines the property type for a component's rules. */
export const rulesPropType: RulesPropType = {
    type: [Array, Object, String] as PropType<ValidationRule | ValidationRule[]>,
    default: ""
};

/**
 * Parse a string into a valid rule reference. Basically this does the heavy
 * lifting to take a string and spit out the rule name and parameters. This
 * assumes the rule has already been normalized and does not contain multiple
 * rules separated by a | character.
 *
 * @param rule The rule to be parsed.
 *
 * @returns The rule reference that contains the name and parameters.
 */
export function parseRule(rule: string): ValidationRuleReference {
    let name = "";
    let params: unknown[] = [];

    const colonIndex = rule.indexOf(":");
    if (colonIndex === -1) {
        name = rule;
    }
    else {
        name = rule.substring(0, colonIndex);
        params = rule.substring(colonIndex + 1).split(",");
    }

    return {
        name,
        params
    };
}

/**
 * Normalize a single rule or array of rules into a flat array of rules. This
 * handles strings that contain multiple rules and splits them out into individual
 * rule strings.
 *
 * @param rules The rules to be normalized.
 *
 * @returns A flattened array that contains all the individual rules.
 */
export function normalizeRules(rules: ValidationRule | ValidationRule[]): ValidationRule[] {
    if (typeof rules === "string") {
        if (rules.indexOf("|") !== -1) {
            return rules.split("|").filter(r => r !== "");
        }
        else if (rules !== "") {
            return [rules];
        }
    }
    else if (Array.isArray(rules)) {
        // Normalize the rule, since it may contain a string like "required|notzero"
        // which needs to be further normalized.
        const normalizedRules: ValidationRule[] = [];

        for (const r of rules) {
            normalizedRules.push(...normalizeRules(r));
        }

        return normalizedRules;
    }
    else if (typeof rules === "function") {
        return [rules];
    }
    else if (typeof rules === "object") {
        return [rules];
    }

    return [];
}

/**
 * Checks if any of the specified rules indicates a rule that requires the value
 * to be filled in.
 *
 * @param rules The rules to be checked.
 *
 * @returns True if any of the rules is considered a required rule; otherwise false.
 */
export function containsRequiredRule(rules: ValidationRule | ValidationRule[]): boolean {
    return normalizeRules(rules).some(r => r === "required");
}

/**
 * Normalizes rules to callable functions. This is used to translate string
 * and reference rules to their final function that will be called.
 *
 * @param rules The rules to be normalized to functions.
 *
 * @returns An array of rule functions that will perform validation checks.
 */
function normalizeRulesToFunctions(rules: ValidationRule[]): ValidationRuleFunction[] {
    const ruleFunctions: ValidationRuleFunction[] = [];

    for (const rule of rules) {
        if (typeof rule === "string") {
            const ruleRef = parseRule(rule);
            const fn = definedRules[ruleRef.name];

            if (fn) {
                ruleFunctions.push((value) => fn(value, ruleRef.params));
            }
            else {
                console.warn(`Attempt to validate with unknown rule ${rule}.`);
            }
        }
        else if (typeof rule === "function") {
            ruleFunctions.push(rule);
        }
        else if (typeof rule === "object") {
            const fn = definedRules[rule.name];

            if (fn) {
                ruleFunctions.push((value) => fn(value, rule.params));
            }
            else {
                console.warn(`Attempt to validate with unknown rule ${rule.name}.`);
            }
        }
    }

    return ruleFunctions;
}

/**
 * Normalize a validation result into a useful text message that can be
 * displayed to the user.
 *
 * @param result The validation error message or a blank string if validation passed.
 */
function normalizeRuleResult(result: ValidationResult): string {
    if (typeof result === "string") {
        return result;
    }
    else if (result === true) {
        return "";
    }
    else {
        return "failed validation";
    }
}

/**
 * Runs validation on the value for all the rules provided.
 *
 * @param value The value to be checked.
 * @param rule The array of rules that will be used during validation.
 *
 * @returns An array of error messages, or empty if value passed.
 */
export function validateValue(value: unknown, rule: ValidationRule | ValidationRule[]): string[] {
    const fns = normalizeRulesToFunctions(normalizeRules(rule));

    const results: string[] = [];

    for (const fn of fns) {
        const result = normalizeRuleResult(fn(value));

        if (result !== "") {
            results.push(result);
        }
    }

    return results;
}

/**
 * Define a new rule by name and provide the validation function.
 *
 * @param ruleName The name of the rule to be registered.
 * @param validator The validation function.
 */
export function defineRule(ruleName: string, validator: ValidationRuleFunction): void {
    if (definedRules[ruleName] !== undefined) {
        console.warn(`Attempt to redefine validation rule ${ruleName}.`);
    }
    else {
        definedRules[ruleName] = validator;
    }
}
