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

import { CurrencyInfoBag } from "../ViewModels/Rest/Utilities/currencyInfoBag";

// From https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl/NumberFormat/NumberFormat
// Number.toLocaleString takes the same options as Intl.NumberFormat
// Most of the options probably won't get used, so just add the ones you need to use to this when needed
type NumberFormatOptions = {
    useGrouping?: boolean // MDN gives other possible values, but TS is complaining that it should only be boolean
};

/**
 * Get a formatted string.
 * Ex: 10001.2 => 10,001.2
 * @param num
 */
export function asFormattedString(num: number | null, digits?: number, options: NumberFormatOptions = {}): string {
    if (num === null) {
        return "";
    }

    return num.toLocaleString(
        "en-US",
        {
            minimumFractionDigits: digits,
            maximumFractionDigits: digits ?? 9,
            ...options
        }
    );
}

/**
 * Get a number value from a formatted string. If the number cannot be parsed, then zero is returned by default.
 * Ex: $1,000.20 => 1000.2
 * @param str
 */
export function toNumber(str?: string | number | null): number {
    return toNumberOrNull(str) || 0;
}

/**
 * Get a number value from a formatted string. If the number cannot be parsed, then null is returned by default.
 * Ex: $1,000.20 => 1000.2
 * @param str
 */
export function toNumberOrNull(str?: string | number | null): number | null {
    if (str === null || str === undefined || str === "") {
        return null;
    }

    if (typeof str === "number") {
        return str;
    }

    const replaced = str.replace(/[$,]/g, "");
    const num = Number(replaced);

    return !isNaN(num) ? num : null;
}

/**
 * Get a currency value from a string or number. If the number cannot be parsed, then null is returned by default.
 * Ex: 1000.20 => $1,000.20
 * @param value The value to be converted to a currency.
 */
export function toCurrencyOrNull(value?: string | number | null, currencyInfo: CurrencyInfoBag | null = null): string | null {
    if (typeof value === "string") {
        value = toNumberOrNull(value);
    }

    if (value === null || value === undefined) {
        return null;
    }
    const currencySymbol = currencyInfo?.symbol ?? "$";
    const currencyDecimalPlaces = currencyInfo?.decimalPlaces ?? 2;
    return `${currencySymbol}${asFormattedString(value, currencyDecimalPlaces)}`;
}

/**
 * Adds an ordinal suffix.
 * Ex: 1 => 1st
 * @param num
 */
export function toOrdinalSuffix(num?: number | null): string {
    if (!num) {
        return "";
    }

    const j = num % 10;
    const k = num % 100;

    if (j == 1 && k != 11) {
        return num + "st";
    }
    if (j == 2 && k != 12) {
        return num + "nd";
    }
    if (j == 3 && k != 13) {
        return num + "rd";
    }
    return num + "th";
}

/**
 * Convert a number to an ordinal.
 * Ex: 1 => first
 * @param num
 */
export function toOrdinal(num?: number | null): string {
    if (!num) {
        return "";
    }

    switch (num) {
        case 1: return "first";
        case 2: return "second";
        case 3: return "third";
        case 4: return "fourth";
        case 5: return "fifth";
        case 6: return "sixth";
        case 7: return "seventh";
        case 8: return "eighth";
        case 9: return "ninth";
        case 10: return "tenth";
        default: return toOrdinalSuffix(num);
    }
}

/**
 * Convert a number to a word.
 * Ex: 1 => one
 * @param num
 */
export function toWord(num?: number | null): string {
    if (num === null || num === undefined) {
        return "";
    }

    switch (num) {
        case 1: return "one";
        case 2: return "two";
        case 3: return "three";
        case 4: return "four";
        case 5: return "five";
        case 6: return "six";
        case 7: return "seven";
        case 8: return "eight";
        case 9: return "nine";
        case 10: return "ten";
        default: return `${num}`;
    }
}

export function zeroPad(num: number, length: number): string {
    let str = num.toString();

    while (str.length < length) {
        str = "0" + str;
    }

    return str;
}

export function toDecimalPlaces(num: number, decimalPlaces: number): number {
    decimalPlaces = Math.floor(decimalPlaces); // ensure it's an integer

    return Math.round(num * 10 ** decimalPlaces) / 10 ** decimalPlaces;
}

export default {
    toOrdinal,
    toOrdinalSuffix,
    toNumberOrNull,
    asFormattedString
};
