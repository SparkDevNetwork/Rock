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

import { areEqual, toGuidOrNull } from "./guid";
import { Pluralize } from "@Obsidian/Libs/pluralize";

/**
 * Is the value an empty string?
 * @param val
 */
export function isEmpty(val: unknown): boolean {
    if (typeof val === "string") {
        return val.length === 0;
    }

    return false;
}

/**
 * Is the value an empty string?
 * @param val
 */
export function isWhiteSpace(val: unknown): boolean {
    if (typeof val === "string") {
        return val.trim().length === 0;
    }

    return false;
}

/**
 * Is the value null, undefined or whitespace?
 * @param val
 */
export function isNullOrWhiteSpace(val: unknown): boolean {
    return isWhiteSpace(val) || val === undefined || val === null;
}

/**
 * Turns camelCase or PascalCase strings into separate strings - "MyCamelCaseString" turns into "My Camel Case String"
 * @param val
 */
export function splitCase(val: string): string {
    // First, insert a space before sequences of capital letters followed by a lowercase letter (e.g., "RESTKey" -> "REST Key")
    val = val.replace(/([A-Z]+)([A-Z][a-z])/g, "$1 $2");
    // Then, insert a space before sequences of a lowercase letter or number followed by a capital letter (e.g., "myKey" -> "my Key")
    return val.replace(/([a-z0-9])([A-Z])/g, "$1 $2");
}

/**
 * Returns a string that has each item comma separated except for the last
 * which will use the word "and".
 *
 * @example
 * ['a', 'b', 'c'] => 'a, b and c'
 *
 * @param strs The strings to be joined.
 * @param andStr The custom string to use instead of the word "and".
 *
 * @returns A string that represents all the strings.
 */
export function asCommaAnd(strs: string[], andStr?: string): string {
    if (strs.length === 0) {
        return "";
    }

    if (strs.length === 1) {
        return strs[0];
    }

    if (!andStr) {
        andStr = "and";
    }

    if (strs.length === 2) {
        return `${strs[0]} ${andStr} ${strs[1]}`;
    }

    const last = strs.pop();
    return `${strs.join(", ")} ${andStr} ${last}`;
}

/**
 * Convert the string to the title case.
 * hellO worlD => Hello World
 * @param str
 */
export function toTitleCase(str: string | null): string {
    if (!str) {
        return "";
    }

    return str.replace(/\w\S*/g, (word) => {
        return word.charAt(0).toUpperCase() + word.substring(1).toLowerCase();
    });
}

/**
 * Capitalize the first character
 */
export function upperCaseFirstCharacter(str: string | null): string {
    if (!str) {
        return "";
    }

    return str.charAt(0).toUpperCase() + str.substring(1);
}

/**
 * Pluralizes the given word. If count is specified and is equal to 1 then
 * the singular form of the word is returned. This will also de-pluralize a
 * word if required.
 *
 * @param word The word to be pluralized or singularized.
 * @param count An optional count to indicate when the word should be singularized.
 *
 * @returns The word in plural or singular form depending on the options.
 */
export function pluralize(word: string, count?: number): string {
    return Pluralize(word, count);
}

/**
 * Returns a singular or plural phrase depending on if the number is 1.
 * (0, Cat, Cats) => Cats
 * (1, Cat, Cats) => Cat
 * (2, Cat, Cats) => Cats
 * @param num
 * @param singular
 * @param plural
 */
export function pluralConditional(num: number, singular: string, plural: string): string {
    return num === 1 ? singular : plural;
}

/**
 * Pad the left side of a string so it is at least length characters long.
 *
 * @param str The string to be padded.
 * @param length The minimum length to make the string.
 * @param padCharacter The character to use to pad the string.
 */
export function padLeft(str: string | undefined | null, length: number, padCharacter: string = " "): string {
    if (padCharacter == "") {
        padCharacter = " ";
    }
    else if (padCharacter.length > 1) {
        padCharacter = padCharacter.substring(0, 1);
    }

    if (!str) {
        return Array(length + 1).join(padCharacter);
    }

    if (str.length >= length) {
        return str;
    }

    return Array(length - str.length + 1).join(padCharacter) + str;
}

/**
 * Pad the right side of a string so it is at least length characters long.
 *
 * @param str The string to be padded.
 * @param length The minimum length to make the string.
 * @param padCharacter The character to use to pad the string.
 */
export function padRight(str: string | undefined | null, length: number, padCharacter: string = " "): string {
    if (padCharacter == "") {
        padCharacter = " ";
    }
    else if (padCharacter.length > 1) {
        padCharacter = padCharacter.substring(0, 1);
    }

    if (!str) {
        return Array(length).join(padCharacter);
    }

    if (str.length >= length) {
        return str;
    }

    return str + Array(length - str.length + 1).join(padCharacter);
}

export type TruncateOptions = {
    ellipsis?: boolean;
};

/**
 * Ensure a string does not go over the character limit. Truncation happens
 * on word boundaries.
 *
 * @param str The string to be truncated.
 * @param limit The maximum length of the resulting string.
 * @param options Additional options that control how truncation will happen.
 *
 * @returns The truncated string.
 */
export function truncate(str: string, limit: number, options?: TruncateOptions): string {
    // Early out if the string is already under the limit.
    if (str.length <= limit) {
        return str;
    }

    // All the whitespace characters that we can split on.
    const trimmable = "\u0009\u000A\u000B\u000C\u000D\u0020\u00A0\u1680\u180E\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u202F\u205F\u2028\u2029\u3000\uFEFF";
    const reg = new RegExp(`(?=[${trimmable}])`);
    const words = str.split(reg);
    let count = 0;

    // If we are appending ellipsis, then shorten the limit size.
    if (options && options.ellipsis === true) {
        limit -= 3;
    }

    // Get a list of words that will fit within our length requirements.
    const visibleWords = words.filter(function (word) {
        count += word.length;
        return count <= limit;
    });

    return `${visibleWords.join("")}...`;
}

/** The regular expression that contains the characters to be escaped. */
const escapeHtmlRegExp = /["'&<>]/g;

/** The character map of the characters to be replaced and the strings to replace them with. */
const escapeHtmlMap: Record<string, string> = {
    '"': "&quot;",
    "&": "&amp;",
    "'": "&#39;",
    "<": "&lt;",
    ">": "&gt;"
};

/**
 * Escapes a string so it can be used in HTML. This turns things like the <
 * character into the &lt; sequence so it will still render as "<".
 *
 * @param str The string to be escaped.
 * @returns A string that has all HTML entities escaped.
 */
export function escapeHtml(str: string): string {
    return str.replace(escapeHtmlRegExp, (ch) => {
        return escapeHtmlMap[ch];
    });
}

/**
 * The default compare value function for UI controls. This checks if both values
 * are GUIDs and if so does a case-insensitive compare, otherwise it does a
 * case-sensitive compare of the two values.
 *
 * @param value The value selected in the UI.
 * @param itemValue The item value to be compared against.
 *
 * @returns true if the two values are considered equal; otherwise false.
 */
export function defaultControlCompareValue(value: string, itemValue: string): boolean {
    const guidValue = toGuidOrNull(value);
    const guidItemValue = toGuidOrNull(itemValue);

    if (guidValue !== null && guidItemValue !== null) {
        return areEqual(guidValue, guidItemValue);
    }

    return value === itemValue;
}

/**
 * Determins whether or not a given string contains any HTML tags in.
 *
 * @param value The string potentially containing HTML
 *
 * @returns true if it contains HTML, otherwise false
 */
export function containsHtmlTag(value: string): boolean {
    return /<[/0-9a-zA-Z]/.test(value);
}

export default {
    asCommaAnd,
    containsHtmlTag,
    escapeHtml,
    splitCase,
    isNullOrWhiteSpace,
    isWhiteSpace,
    isEmpty,
    toTitleCase,
    pluralConditional,
    padLeft,
    padRight,
    truncate
};
