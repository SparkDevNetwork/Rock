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
import { isNullOrWhiteSpace } from "../Services/string";
import { defineRule } from "vee-validate";
import { toNumberOrNull } from "../Services/number";

export type ValidationRuleFunction = (value: unknown) => boolean | string | Promise<boolean | string>;

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

export function ruleStringToArray (rulesString: string): string[] {
    return rulesString.split("|");
}

export function ruleArrayToString (rulesArray: string[]): string {
    return rulesArray.join("|");
}

defineRule("required", ((value: unknown, [ optionsJson ]: unknown[]) => {
    const options = typeof optionsJson === "string" ? JSON.parse(optionsJson) : {};

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
}) as ValidationRuleFunction);

defineRule("email", (value => {
    // Field is empty, should pass
    if (isNullOrWhiteSpace(value)) {
        return true;
    }

    // Check if email
    if (!isEmail(value)) {
        return "must be a valid email";
    }

    return true;
}) as ValidationRuleFunction);

defineRule("notequal", ((value: unknown, [ compare ]: unknown[]) => {
    if (isNumeric(value) && isNumeric(compare)) {
        if (convertToNumber(value) !== convertToNumber(compare)) {
            return true;
        }
    }
    else if (value !== compare) {
        return true;
    }

    return `must not equal ${compare}`;
}) as ValidationRuleFunction);

defineRule("equal", ((value: unknown, [ compare ]: unknown[]) => {
    if (isNumeric(value) && isNumeric(compare)) {
        if (convertToNumber(value) === convertToNumber(compare)) {
            return true;
        }
    }
    else if (value === compare) {
        return true;
    }

    return `must equal ${compare}`;
}) as ValidationRuleFunction);

defineRule("gt", ((value: unknown, [ compare ]: unknown[]) => {
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
}) as ValidationRuleFunction);

defineRule("gte", ((value: unknown, [ compare ]: unknown[]) => {
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
}) as ValidationRuleFunction);

defineRule("lt", ((value: unknown, [ compare ]: unknown[]) => {
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
}) as ValidationRuleFunction);

defineRule("lte", ((value: unknown, [ compare ]: unknown[]) => {
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
}) as ValidationRuleFunction);

defineRule("datekey", (value => {
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
}) as ValidationRuleFunction);

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

    if (/^[0-9]{3}-[0-9]{2}-[0-9]{4}$/.test(String(value))) {
        return true;
    }

    if (/^[0-9]{9}$/.test(String(value))) {
        return true;
    }

    return "must be a valid social security number";
});
