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

import { CurrencyInfoBag } from "@Obsidian/ViewModels/Rest/Utilities/currencyInfoBag";

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
 * Ex: 1 => first, 10 => tenth
 *
 * Anything larger than 10 will be converted to the number with an ordinal suffix.
 * Ex: 123 => 123rd, 1000 => 1000th
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
 * Ex: 1 => "one", 10 => "ten"
 *
 * Anything larger than 10 will be returned as a number string instead of a word.
 * Ex: 123 => "123", 1000 => "1000"
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

/**
 * Returns the string representation of an integer.
 * Ex: 1 => "1", 123456 => "one hundred twenty-three thousand four hundred fifty-six"
 *
 * Not reliable for numbers in the quadrillions and greater.
 *
 * @example
 * numberToWord(1)      // one
 * numberToWord(2)      // two
 * numberToWord(123456) // one hundred twenty-three thousand four hundred fifty-six
 * @param numb The number for which to get the string representation.
 * @returns "one", "two", ..., "one thousand", ..., (up to the max number allowed for JS).
 */
export function toWordFull(numb: number): string {
    const numberWords = {
        0: "zero",
        1: "one",
        2: "two",
        3: "three",
        4: "four",
        5: "five",
        6: "six",
        7: "seven",
        8: "eight",
        9: "nine",
        10: "ten",
        11: "eleven",
        12: "twelve",
        13: "thirteen",
        14: "fourteen",
        15: "fifteen",
        16: "sixteen",
        17: "seventeen",
        18: "eighteen",
        19: "nineteen",
        20: "twenty",
        30: "thirty",
        40: "forty",
        50: "fifty",
        60: "sixty",
        70: "seventy",
        80: "eighty",
        90: "ninety",
        100: "one hundred",
        1000: "one thousand",
        1000000: "one million",
        1000000000: "one billion",
        1000000000000: "one trillion",
        1000000000000000: "one quadrillion"
    };

    // Store constants for these since it is hard to distinguish between them at larger numbers.
    const oneHundred = 100;
    const oneThousand = 1000;
    const oneMillion = 1000000;
    const oneBillion = 1000000000;
    const oneTrillion = 1000000000000;
    const oneQuadrillion = 1000000000000000;

    if (numberWords[numb]) {
        return numberWords[numb];
    }

    function quadrillionsToWord(numb: number): string {
        const trillions = trillionsToWord(numb);
        if (numb >= oneQuadrillion) {
            const quadrillions = hundredsToWord(Number(numb.toString().slice(-18, -15)));
            if (trillions) {
                return `${quadrillions} quadrillion ${trillions}`;
            }
            else {
                return `${quadrillions} quadrillion`;
            }
        }
        else {
            return trillions;
        }
    }

    function trillionsToWord(numb: number): string {
        numb = Number(numb.toString().slice(-15));
        const billions = billionsToWord(numb);
        if (numb >= oneTrillion) {
            const trillions = hundredsToWord(Number(numb.toString().slice(-15, -12)));
            if (billions) {
                return `${trillions} trillion ${billions}`;
            }
            else {
                return `${trillions} trillion`;
            }
        }
        else {
            return billions;
        }
    }

    function billionsToWord(numb: number): string {
        numb = Number(numb.toString().slice(-12));
        const millions = millionsToWord(numb);
        if (numb >= oneBillion) {
            const billions = hundredsToWord(Number(numb.toString().slice(-12, -9)));
            if (millions) {
                return `${billions} billion ${millions}`;
            }
            else {
                return `${billions} billion`;
            }
        }
        else {
            return millions;
        }
    }

    function millionsToWord(numb: number): string {
        numb = Number(numb.toString().slice(-9));
        const thousands = thousandsToWord(numb);
        if (numb >= oneMillion) {
            const millions = hundredsToWord(Number(numb.toString().slice(-9, -6)));
            if (thousands) {
                return `${millions} million ${thousands}`;
            }
            else {
                return `${millions} million`;
            }
        }
        else {
            return thousands;
        }
    }

    function thousandsToWord(numb: number): string {
        numb = Number(numb.toString().slice(-6));
        const hundreds = hundredsToWord(numb);
        if (numb >= oneThousand) {
            const thousands = hundredsToWord(Number(numb.toString().slice(-6, -3)));
            if (hundreds) {
                return `${thousands} thousand ${hundreds}`;
            }
            else {
                return `${thousands} thousandths`;
            }
        }
        else {
            return hundreds;
        }
    }

    function hundredsToWord(numb: number): string {
        numb = Number(numb.toString().slice(-3));

        if (numberWords[numb]) {
            return numberWords[numb];
        }

        const tens = tensToWord(numb);

        if (numb >= oneHundred) {
            const hundreds = Number(numb.toString().slice(-3, -2));
            if (tens) {
                return `${numberWords[hundreds]} hundred ${tens}`;
            }
            else {
                return `${numberWords[hundreds]} hundred`;
            }
        }
        else {
            return tens;
        }
    }

    function tensToWord(numb: number): string {
        numb = Number(numb.toString().slice(-2));

        if (numberWords[numb]) {
            return numberWords[numb];
        }

        const ones = onesToWord(numb);

        if (numb >= 20) {
            const tens = Number(numb.toString().slice(-2, -1));

            if (ones) {
                return `${numberWords[tens * 10]}-${ones}`;
            }
            else {
                return numberWords[tens * 10];
            }
        }
        else {
            return ones;
        }
    }

    function onesToWord(numb: number): string {
        numb = Number(numb.toString().slice(-1));
        return numberWords[numb];
    }

    return quadrillionsToWord(numb);
}

export default {
    toOrdinal,
    toOrdinalSuffix,
    toNumberOrNull,
    asFormattedString
};
