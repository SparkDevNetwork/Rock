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
import type { RulesPropType, ValidationResult, ValidationRule, ValidationRuleFunction, ValidationRuleReference } from "@Obsidian/Types/validationRules";
import { asBooleanOrNull } from "@Obsidian/Utility/booleanUtils";
import DateKey from "@Obsidian/Utility/dateKey";
import { isEmail } from "@Obsidian/Utility/email";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { isNullOrWhiteSpace, containsHtmlTag } from "@Obsidian/Utility/stringUtils";
import { isUrl } from "@Obsidian/Utility/url";
import { containsRequiredRule, defineRule, normalizeRules, parseRule, rulesPropType, validateValue } from "@Obsidian/Utility/validationRules";
import { getSpecialCharacterPattern, getEmojiPattern, getSpecialFontPattern } from "@Obsidian/Utility/regexPatterns";

// For backwards compatibility:
export {
    RulesPropType,
    ValidationResult,
    ValidationRule,
    ValidationRuleFunction,
    ValidationRuleReference,
    containsRequiredRule,
    defineRule,
    normalizeRules,
    parseRule,
    rulesPropType,
    validateValue,
};

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

    if (Array.isArray(value) && value.length === 0) {
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

defineRule("nospecialcharacters", (value: unknown): ValidationResult => {
    // Gets or sets a value indicating whether the an input will allow special characters. This property is meant to be used when dealing with Person names.
    if (typeof value === "string") {
        // Checks if a string contains special characters
        if (getSpecialCharacterPattern().test(value)) {
            return "cannot contain special characters such as quotes, parentheses, etc.";
        }
    }

    return true;
});

defineRule("noemojisorspecialfonts", (value: unknown): ValidationResult => {
    // Gets or sets a value indicating whether the an input will allow emojis and special fonts. This property is meant to be used when dealing with Person names.
    if (typeof value === "string") {
        // Checks if a string contains emojis or special fonts.
        if (getEmojiPattern().test(value) || getSpecialFontPattern().test(value)) {
            return "cannot contain emojis or special fonts.";
        }
    }

    return true;
});

// This is like "required" but slightly less strict (doesn't fail on 0 or empty array)
defineRule("notblank", (value: unknown) => {
    if (value === undefined || value === null || value === "") {
        return "cannot be blank";
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
    else if (typeof value === "boolean") {
        if (value !== asBooleanOrNull(compare)) {
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
    else if (typeof value === "boolean") {
        if (value === asBooleanOrNull(compare)) {
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

defineRule("number", (value: unknown) => {
    // Field is empty, should pass
    if (isNullOrWhiteSpace(value)) {
        return true;
    }

    if (isNumeric(value)) {
        return true;
    }

    return "must be a valid number.";
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
    const compare = params && params.length >= 1 ? params[0] : undefined;

    // Field is empty, should pass
    if (isNullOrWhiteSpace(value)) {
        return true;
    }

    // No parameters, should pass
    if (!String(compare)) {
        return true;
    }

    if (String(value).endsWith(String(compare))) {
        return true;
    }

    return `must end with "${compare}"`;
});

defineRule("startswith", (value: unknown, params?: unknown[]) => {
    const compare = params && params.length >= 1 ? params[0] : undefined;

    // Field is empty, should pass
    if (isNullOrWhiteSpace(value)) {
        return true;
    }

    // No parameters, should pass
    if (!String(compare)) {
        return true;
    }

    if (String(value).startsWith(String(compare))) {
        return true;
    }

    return `must start with "${compare}"`;
});

defineRule("equalsfield", (value: unknown, params?: unknown[]) => {
    // Validator params are comma "," delimited.
    // The first param is the name of the field to display in the error message.
    // The remaining params need to be joined together into a single string for comparison.
    const error = params && params.length >= 1 ? params[0] : undefined;
    const compare = params ? params.slice(1).join(",") : "";

    if (isNumeric(value) && isNumeric(compare)) {
        if (convertToNumber(value) === convertToNumber(compare)) {
            return true;
        }
    }
    else if (typeof value === "boolean") {
        if (value === asBooleanOrNull(compare)) {
            return true;
        }
    }
    else if (value === compare) {
        return true;
    }

    // Do not expose the value in case we are matching sensitive confirmation fields.
    return typeof error === "string" ? error : "must match value";
});

defineRule("nohtml", (value: unknown) => {
    // Field is empty, should pass
    if (isNullOrWhiteSpace(value)) {
        return true;
    }

    return !containsHtmlTag(String(value)) || "contains invalid characters. Please make sure that your entries do not contain any angle brackets like < or >.";
});