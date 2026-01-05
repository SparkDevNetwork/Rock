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

import { ValidationResult, ValidationRuleFunction } from "@Obsidian/ValidationRules";

/**
 * Validates that a value is entered.
 *
 * This was copied from the "required" validation rule.
 */
export function required(value: unknown, params?: unknown[]): ValidationResult {
    // This needs to be changed. JSON is not safe in rules because of the
    // comma and pipe characters.
    const options = params && params.length >= 1 && typeof params[0] === "string" ? JSON.parse(params[0]) : {};

    if (typeof value === "string") {
        const allowEmptyString = !!(options.allowEmptyString);

        if (!allowEmptyString && !(value?.trim())) {
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
}

/**
 * Returns a wrapper around a validation function which replaces a validation error message.
 */
export function createRuleWithReplacement(rule: ValidationRuleFunction, replacement: string): ValidationRuleFunction {
    return (value: unknown, params?: unknown[]): ValidationResult => {
        const result = rule(value, params);

        if (typeof result === "string" && result && replacement) {
            return replacement;
        }

        return result;
    };
}