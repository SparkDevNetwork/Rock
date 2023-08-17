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

import { RockDateTime } from "@Obsidian/Utility/rockDateTime";
import { LibraryViewerItemBag } from "@Obsidian/ViewModels/Blocks/Cms/LibraryViewer/libraryViewerItemBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

const thousandsAbbreviationMap = {
    2: "K",   // Thousands
    3: "M",   // Millions
    4: "B",   // Billions
    5: "t",   // trillions
    6: "q",   // quadrillions
    // 7: "Q",   // Quintillion
    // 8: "s",   // sextillion
    // 9: "S",   // Septillion
    // 10: "o",  // octillion
    // 11: "n",  // nonillion
    // 12: "d",  // decillion
} as const;

/**
 * Abbreviates the supplied number in the format "1K" or "1K+".
 * @param value The value to abbreviate.
 * @example
 * abbreviateNumber(78); // "78"
 * abbreviateNumber(1000); // "1K"
 * abbreviateNumber(1234); // "1K+"
 * abbreviateNumber(-1234567); // "-1M+"
 * abbreviateNumber(1234567890); // "1B+"
 * abbreviateNumber(1000000000000); // "1t"
 * abbreviateNumber(-1000000000000001); // "-1q+"
 * abbreviateNumber(1000000000000000000); // "1*", anything larger than quadrillion uses a "*" character.
 * abbreviateNumber(12.34E9); // "12B+"
 * abbreviateNumber(Number.POSITIVE_INFINITY); // "Infinity"
 * abbreviateNumber(Number.NEGATIVE_INFINITY); // "-Infinity"
 * abbreviateNumber(NaN); // "NaN"
 */
export function abbreviateNumber(value: number): string {
    if (typeof value === "undefined" || value === null) {
        return "" + value;
    }

    if (Number.isNaN(value) || !Number.isFinite(value)) {
        return value.toString();
    }

    // Throw away the fractional bits.
    // toFixed will also convert scientific notation to standard form.
    let integer = value.toFixed(0);
    let negativeSymbol = "";
    if (integer.startsWith("-")) {
        negativeSymbol = "-";
        integer = integer.substring(1);
    }

    const thousandsChunks = splitIntoThousands(integer);

    const length = thousandsChunks.length;

    // Less than one thousand, so return the original value.
    if (length === 1) {
        return integer;
    }

    const largestChunkNumber = thousandsChunks[0];
    // For numbers larger than quadrillions, fallback to "*".
    const abbreviationCharacter = thousandsAbbreviationMap[length] ?? "*";
    let suffix = "";
    for (let i = 1; i < length; i++) {
        if (thousandsChunks[i] !== "000") {
            suffix = "+";
            break;
        }
    }
    return `${negativeSymbol}${largestChunkNumber}${abbreviationCharacter}${suffix}`;
}

/**
 * Adds a thousands separator to a number.
 * This method assumes that "," is the thousands separator.
 * This method will attempt to convert numbers in scientific notation (e.g., 1.23E3) to standard form (e.g., 1,230).
 * @param value The value.
 * @example
 * abbreviateNumber(78); // "78"
 * abbreviateNumber(1000); // "1,000"
 * abbreviateNumber(1234); // "1,234"
 * abbreviateNumber(-1234567); // "-1,234,567"
 * abbreviateNumber(1234567890); // "1,234,567,890"
 * abbreviateNumber(1000000000000); // "1,000,000,000,000"
 * abbreviateNumber(-1000000000000001); // "-1,000,000,000,000,001"
 * abbreviateNumber(12.34E9); // "12,340,000,000"
 * abbreviateNumber(Number.POSITIVE_INFINITY); // "Infinity"
 * abbreviateNumber(Number.NEGATIVE_INFINITY); // "-Infinity"
 * abbreviateNumber(NaN); // "NaN"
 */
export function addThousandsSeparator(value: number): string {
    if (typeof value === "undefined" || value === null) {
        return "" + value;
    }

    if (Number.isNaN(value) || !Number.isFinite(value)) {
        return value.toString();
    }

    // Throw away the fractional bits.
    // toFixed will also convert scientific notation to standard form.
    let integer = value.toFixed(0);
    let negativeSymbol = "";
    if (integer.startsWith("-")) {
        negativeSymbol = "-";
        integer = integer.substring(1);
    }

    const thousandsChunks = splitIntoThousands(integer);

    return `${negativeSymbol}${thousandsChunks.join(",")}`;
}

/**
 * Splits an integer into thousands chunks.
 *
 * @param integer The value to split.
 * @example
 * const integer = "1234567890";
 * splitIntoThousands(integer); // returns ["1", "234", "567", "890"];
 */
function splitIntoThousands(integer: string): string[] {
    const cleanValue = integer.replace(/[^\d]/, "");
    let pointer = cleanValue.length - 1;
    const chunks: string[] = [];
    while (pointer >= 0) {
        chunks.unshift(`${cleanValue[pointer - 2] ?? ""}${cleanValue[pointer - 1] ?? ""}${cleanValue[pointer] ?? ""}`);
        pointer = pointer - 3;
    }
    return chunks;
}

export function getItemCategories(item: LibraryViewerItemBag, topics: ListItemBag[]): string[] {
    const categories: string[] = [];

    const topic = topics.find(topic => topic.value === item.topic?.value)?.text;

    if (item.experienceLevel?.text) {
        categories.push(item.experienceLevel.text);
    }

    if (topic) {
        categories.push(topic);
    }

    return categories;
}

/**
 * Compares two string values and returns -1 if value1 < value2, 0 if value1 == value2, or 1 if value1 > value2.
 */
export function compareDateStrings(value1: string | null | undefined, value2: string | null | undefined): number {
    if (!value1 && !value2) {
        return 0;
    }

    if (!value1) {
        return -1;
    }

    if (!value2) {
        return 1;
    }

    const date1 = RockDateTime.parseISO(value1);
    const date2 = RockDateTime.parseISO(value2);

    return compareDates(date1, date2);
}

/**
 * Compares two RockDateTime values and returns -1 if value1 < value2, 0 if value1 == value2, or 1 if value1 > value2.
 */
export function compareDates(value1: RockDateTime | null | undefined, value2: RockDateTime | null | undefined): number {
    if (!value1 && !value2) {
        return 0;
    }

    if (!value1) {
        return -1;
    }

    if (!value2) {
        return 1;
    }

    const ms1 = value1.toMilliseconds();
    const ms2 = value2.toMilliseconds();

    if (ms1 < ms2) {
        return -1;
    }

    if (ms1 > ms2) {
        return 1;
    }

    return 0;
}

/**
 * Compares two string values and returns -1 if value1 < value2, 0 if value1 == value2, or 1 if value1 > value2.
 */
export function compareStrings(value1: string | null | undefined, value2: string | null | undefined): number {
    if (!value1 && !value2) {
        return 0;
    }

    if (!value1) {
        return -1;
    }

    if (!value2) {
        return 1;
    }

    return value1.localeCompare(value2);
}

/**
 * Compares two number values and returns -1 if value1 < value2, 0 if value1 == value2, or 1 if value1 > value2.
 */
export function compareNumbers(value1: number | null | undefined, value2: number | null | undefined): number {
    if (!value1 && !value2) {
        return 0;
    }

    if (!value1) {
        return -1;
    }

    if (!value2) {
        return 1;
    }

    if (value1 < value2) {
        return -1;
    }

    if (value1 > value2) {
        return 1;
    }

    // Values are the same.
    return 0;
}

/** Formats an ISO date as a short date string (M/d/yyyy). */
export function toDateString(date: string | null | undefined): string | null | undefined {
    if (!date) {
        return date;
    }

    const d = RockDateTime.parseISO(date);

    if (!d) {
        return null;
    }

    return d.toASPString("M/d/yyyy");
}

/** Updates item properties. Used for updating an item when details are retrieved or when an item is downloaded. */
export function updateItemProperties(item: LibraryViewerItemBag, newProperties: Partial<LibraryViewerItemBag>): void {
    Object.assign(item, newProperties);
}