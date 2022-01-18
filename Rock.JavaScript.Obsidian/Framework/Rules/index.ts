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
import DateKey from "../Services/dateKey";
import { isEmail } from "../Services/email";
import { isUrl } from "../Services/url";
import { isNullOrWhiteSpace } from "../Services/string";
import { toNumberOrNull } from "../Services/number";
import { PropType } from "vue";


/** The custom validation function signature. */
export type ValidationRuleFunction = (value: unknown, params?: unknown[]) => ValidationResult;

/** The return type allowed in a validation function. */
export type ValidationResult = string | boolean;

/**
 * A reference to a defined validation rule with parameters. This is primarily
 * an internal use type but is perfectly valid for public use as well.
 */
export type ValidationRuleReference = {
    /** The name of the rule. */
    name: string;

    /** Any parameters that were found. */
    params: unknown[];
};

/** The rule types that are valid to be processed during validation checks. */
export type ValidationRule = string | ValidationRuleReference | ValidationRuleFunction;

/** The currently defined rules by name. */
const definedRules: Record<string, ValidationRuleFunction | undefined> = {};

/** Defines the property type for a component's rules. */
export const rulesPropType = {
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
 * Normalizes rules to callable functions. This is used to translate string
 * and reference rules to their final function that will be called.
 * 
 * @param rules The ruels to be normalized to functions.
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

/**
 * Convert the string to a number
 * @param val
 */
function convertToNumber(value: unknown): number {
    if (typeof value === "number") {
        return value;
    }

    if (typeof value === "string") {
        return toNumberOrNull(value) || 0;
    }

    return 0;
}

/**
 * Is the value numeric?
 * '0.9' => true
 * 0.9 => true
 * '9a' => false
 * @param value
 */
function isNumeric(value: unknown): boolean {
    if (typeof value === "number") {
        return true;
    }

    if (typeof value === "string") {
        return toNumberOrNull(value) !== null;
    }

    return false;
}

defineRule("required", (value: unknown, params?: unknown[]): ValidationResult => {
    // This needs to be changed. JSON is not safe in rules because of the
    // comma and pipe characters.
    const options = params && params.length >= 1 && typeof params[0] === "string" ? JSON.parse(params[0]) : {};

    if (typeof value === "string") {
        const allowEmptyString = !!(options.allowEmptyString);

        if (!allowEmptyString && isNullOrWhiteSpace(value)) {
            return "is required";
        }

        return true;
    }

    if (typeof value === "number" && value === 0) {
        return "is required";
    }

    // Special case for booleans, required rule is ignored. Otherwise things
    // like checkbox and toggle would always require a True value.
    if (typeof value === "boolean") {
        return true;
    }

    if (!value) {
        return "is required";
    }

    return true;
});

defineRule("email", value => {
    // Field is empty, should pass
    if (isNullOrWhiteSpace(value)) {
        return true;
    }

    // Check if email
    if (!isEmail(value)) {
        return "must be a valid email";
    }

    return true;
});

defineRule("notequal", (value: unknown, params?: unknown[]) => {
    const compare = params && params.length >= 1 ? params[0] : undefined;

    if (isNumeric(value) && isNumeric(compare)) {
        if (convertToNumber(value) !== convertToNumber(compare)) {
            return true;
        }
    }
    else if (value !== compare) {
        return true;
    }

    return `must not equal ${compare}`;
});

defineRule("equal", (value: unknown, params?: unknown[]) => {
    const compare = params && params.length >= 1 ? params[0] : undefined;

    if (isNumeric(value) && isNumeric(compare)) {
        if (convertToNumber(value) === convertToNumber(compare)) {
            return true;
        }
    }
    else if (value === compare) {
        return true;
    }

    return `must equal ${compare}`;
});

defineRule("gt", (value: unknown, params?: unknown[]) => {
    const compare = params && params.length >= 1 ? params[0] : undefined;

    // Field is empty, should pass
    if (isNullOrWhiteSpace(value)) {
        return true;
    }

    if (isNumeric(value) && isNumeric(compare)) {
        if (convertToNumber(value) > convertToNumber(compare)) {
            return true;
        }
    }

    return `must be greater than ${compare}`;
});

defineRule("gte", (value: unknown, params?: unknown[]) => {
    const compare = params && params.length >= 1 ? params[0] : undefined;

    // Field is empty, should pass
    if (isNullOrWhiteSpace(value)) {
        return true;
    }

    if (isNumeric(value) && isNumeric(compare)) {
        if (convertToNumber(value) >= convertToNumber(compare)) {
            return true;
        }
    }

    return `must not be less than ${compare}`;
});

defineRule("lt", (value: unknown, params?: unknown[]) => {
    const compare = params && params.length >= 1 ? params[0] : undefined;

    // Field is empty, should pass
    if (isNullOrWhiteSpace(value)) {
        return true;
    }

    if (isNumeric(value) && isNumeric(compare)) {
        if (convertToNumber(value) < convertToNumber(compare)) {
            return true;
        }
    }

    return `must be less than ${compare}`;
});

defineRule("lte", (value: unknown, params?: unknown[]) => {
    const compare = params && params.length >= 1 ? params[0] : undefined;

    // Field is empty, should pass
    if (isNullOrWhiteSpace(value)) {
        return true;
    }

    if (isNumeric(value) && isNumeric(compare)) {
        if (convertToNumber(value) <= convertToNumber(compare)) {
            return true;
        }
    }

    return `must not be more than ${compare}`;
});

defineRule("datekey", value => {
    const asString = value as string;

    if (!DateKey.getYear(asString)) {
        return "must have a year";
    }

    if (!DateKey.getMonth(asString)) {
        return "must have a month";
    }

    if (!DateKey.getDay(asString)) {
        return "must have a day";
    }

    return true;
});

defineRule("integer", (value: unknown) => {
    // Field is empty, should pass
    if (isNullOrWhiteSpace(value)) {
        return true;
    }

    if (/^-?[0-9]+$/.test(String(value))) {
        return true;
    }

    return "must be an integer value.";
});

defineRule("decimal", (value: unknown) => {
    // Field is empty, should pass
    if (isNullOrWhiteSpace(value)) {
        return true;
    }

    if (/^-?[0-9]+(\.[0-9]+)?$/.test(String(value))) {
        return true;
    }

    return "must be a decimal value.";
});

defineRule("ssn", (value: unknown) => {
    // Field is empty, should pass
    if (isNullOrWhiteSpace(value)) {
        return true;
    }

    // Test for a format like 111-22-3333.
    if (/^[0-9]{3}-[0-9]{2}-[0-9]{4}$/.test(String(value))) {
        return true;
    }

    // Test for a format like 111223333.
    if (/^[0-9]{9}$/.test(String(value))) {
        return true;
    }

    return "must be a valid social security number";
});

defineRule("url", (value: unknown) => {
    // Field is empty, should pass
    if (isNullOrWhiteSpace(value)) {
        return true;
    }

    if (isUrl(String(value))) {
        return true;
    }

    return "must be a valid URL";
});

defineRule("endswith", (value: unknown, params?: unknown[]) => {
    // Field is empty, should pass
    if (isNullOrWhiteSpace(value)) {
        return true;
    }

    // No parameters, should pass
    if (!params || params.length === 0) {
        return true;
    }

    if (String(value).endsWith(String(params[0]))) {
        return true;
    }

    return `must end with "${params[0]}"`;
});

defineRule("startswith", (value: unknown, params?: unknown[]) => {
    // Field is empty, should pass
    if (isNullOrWhiteSpace(value)) {
        return true;
    }

    // No parameters, should pass
    if (!params || params.length === 0) {
        return true;
    }

    if (String(value).startsWith(String(params[0]))) {
        return true;
    }

    return `must start with "${params[0]}"`;
});
