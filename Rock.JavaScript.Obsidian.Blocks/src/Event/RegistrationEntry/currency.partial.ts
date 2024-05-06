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

import { newGuid } from "@Obsidian/Utility/guid";

// #region Types

export type CurrencyOptions = {
    precision: number;
    symbol: string;
    code: string;
    formatString: string;
    isLoggingEnabled: boolean;
};

export type CurrencyParts = {
    units: number;
    precision: number;
};

export const CurrencyFormatString = {
    default: "-!#,###.%",
    defaultCodeSuffix: "-!#,###.% @",
    /**
     * Formats the currency as a decimal number without currency symbols.
     * 
     * @example
     * createCurrency(-1.6789, defaultCurrencyOptions).format(CurrencyFormatString.decimal); // "-1.68"
     * createCurrency(1.6789, defaultCurrencyOptions).format(CurrencyFormatString.decimal); // "1.68"
     */
    decimal: "-#.%",
    indic: "-!#,##,###.%",
    indicCodeSuffix: "-!#,##,###.% @",
} as const;

const defaultCurrencyOptions: CurrencyOptions = {
    precision: 2,
    symbol: "$",
    code: "USD",
    formatString: CurrencyFormatString.default,
    isLoggingEnabled: false,
} as const;

export type Currency = {
    /**
     * Gets the decimal value of this currency.
     *
     * Use this sparingly as it can produce rounding errors, of which this Currency type was created to avoid.
     *
     * @example
     * createCurrency(0.02, { precision: 2 }).number;      // 0.02 (no rounding issues)
     * createCurrency(0.02, { precision: 2 }).units;       // 2 (no rounding issues)
     * createCurrency(0.1 * 0.2, { precision: 2 }).number; // 0.020000000000000004 (rounding issues)
     * createCurrency(0.1 * 0.2, { precision: 2 }).units;  // 2 (no rounding issues)
     */
    get number(): number;

    /**
     * Gets the value of this currency as integer units.
     *
     * For example, the units for $2.40 would be 240.
     *
     * All operations are performed against this value to avoid rounding issues.
     *
     * @example
     * createCurrency(0.02, { precision: 2 }).number;      // 0.02 (no rounding issues)
     * createCurrency(0.02, { precision: 2 }).units;       // 2 (no rounding issues)
     * createCurrency(0.1 * 0.2, { precision: 2 }).number; // 0.020000000000000004 (rounding issues)
     * createCurrency(0.1 * 0.2, { precision: 2 }).units;  // 2 (no rounding issues)
     */
    get units(): number;

    /** Gets the unsigned units string. */
    get unsignedUnitsString(): string;

    /** Returns `true` if this currency is negative; otherwise, `false` is returned. */
    get isNegative(): boolean;

    /** Gets the precision (the number of minor unit digits after the decimal). */
    get precision(): number;

    /** Gets the symbol. */
    get symbol(): string;

    /** Gets the code. */
    get code(): string;

    /** Returns `true` if this currency is equal to 0; otherwise, `false`. */
    get isZero(): boolean;

    /** Gets the format string. */
    get formatString(): string;

    /** Gets the unsigned major units string. */
    get unsignedMajorUnitsString(): string;

    /** Gets the unsigned minor units string. */
    get unsignedMinorUnitsString(): string;

    /** Gets the currency options. */
    get currencyOptions(): CurrencyOptions;

    /** Gets whether this currency is invalid. */
    get isInvalid(): boolean;

    /**
     * Adds an amount to this Currency.
     *
     * @param currency The amount to add to this currency. Fractional amounts that exceed this currency's precision will be truncated; e.g., if this currency has a precision of two ($31.45), and the amount being added has a precision of three ($2.289), the "9" will be truncated to $2.28.
     * @returns A createCurrency instance containing the sum of the two currencies.
     * @example
     * createCurrency(2.61).add(3.999);               // returns createCurrency(6.60)
     * createCurrency(2.61).add(createCurrency(3.999)); // returns createCurrency(6.60)
     */
    add(currency: Currency | number): Currency;

    /**
     * Gets the negation of this Currency.
     *
     * @returns A createCurrency instance containing the negation of this Currency.
     * @example
     * createCurrency(2.61).negate(); // returns createCurrency(-2.61)
     */
    negate(): Currency;

    /**
     * Divides this currency by a number.
     *
     * @param divisor The number by which to divide this currency.
     * @returns The quotient and remainder of the division as separate Currency instances.
     * @example
     * createCurrency(3.50).divide(3); // returns { quotient: createCurrency(1.16), remainder: createCurrency(0.02) }
     */
    divide(divisor: number): { quotient: Currency, remainder: Currency };

    /**
     * Subtracts an amount from this currency.
     *
     * @param currency The amount to subtract from this currency. Fractional amounts that exceed this currency's precision will be truncated; e.g., if this currency has a precision of two ($31.45), and the amount being subtracted has a precision of three ($2.289), the "9" will be truncated to $2.28.
     * @returns A createCurrency instance containing the difference of the two currencies.
     * @example
     * createCurrency(2.61).subtract(3.999);               // returns createCurrency(-1.38)
     * createCurrency(2.61).subtract(createCurrency(3.999)); // returns createCurrency(-1.38)
     */
    subtract(currency: number | Currency): Currency;

    /**
     * Formats the currency as a string using the following placeholders:
     *
     * - `"-"` for the negative symbol
     * - `"!"` for the currency symbol
     * - `"#"` for the major units (the number to the left of the decimal)
     *   - These must appear consecutively if multiple "#" are used in the format string.
     *   - Grouping characters can be specified like the comma in `"#,###"`. Some currencies use `"."` instead of `","`.
     *   - The left-most grouping will be used repeatedly to format large numbers;
     *     - `"#,###"` will add a `","` for every thousandth like in `"1,234,567"`.
     * - `"%"` for the minor units (the number to the right of the decimal).
     * - `"@"` for the currency code ("USD", "CAD", etc.).
     *
     * @param formatString The format string. (defaults to `"-!#,###.%"`, see `CurrencyFormatString` for common formats)
     * @example
     * createCurrency(-3467.56).format(); // default format "-$3,467.56"
     * createCurrency(-3467.56).format("-#.%"); // omit the symbol and ignore grouping "-3467.56"
     * createCurrency(-12345678.90).format("-₹#,##,###.%") // The indic numbering system groups the first thousand, then every ten digits after "-₹1,23,45,678.90"
     * createCurrency(-12345678.90).format("!#") // absolute value integer with currency symbol "$12345678"
     * createCurrency(12.50).format("-!#,###.% @"); // default format with currency code "$12.50 USD"
     */
    format(formatString?: string): string;

    /**
     * Determines if this currency is equal another currency.
     *
     * @param currency The currency to which to compare.
     * @returns `true` if the currencies are equal; otherwise, `false` is returned.
     */
    isEqualTo(currency: Currency | number): boolean;

    /**
     * Determines if this currency is not equal to another currency.
     *
     * @param currency The currency to which to compare.
     * @returns `true` if the currencies are not equal; otherwise, `false` is returned.
     */
    isNotEqualTo(currency: Currency | number): boolean;

    /**
     * Determines if this currency is less than to another currency.
     *
     * @param currency The currency to which to compare.
     * @returns `true` if this currency is less than the provided currency; otherwise, `false` is returned.
     */
    isLessThan(currency: Currency | number): boolean;

    /**
     * Determines if this currency is greater than to another currency.
     *
     * @param currency The currency to which to compare.
     * @returns `true` if this currency is greater than the provided currency; otherwise, `false` is returned.
     */
    isGreaterThan(currency: Currency | number): boolean;

    /** Gets the absolute value of this currency. */
    abs(): Currency;

    /**
     * Returns the remainder after dividing this currency by a number.
     */
    mod(divisor: number): Currency;

    /**
     * Gets the formatted string value of this currency.
     */
    toString: () => string;
};

// #endregion Types

// #region Currency Functions (similar to class instance methods)

/**
 * Returns the sum of two currencies.
 *
 * @example
 * add(createCurrency(1), createCurrency(2)); // createCurrency(3)
 */
function add(currency1: Currency, currency2: Currency | number): Currency {
    if (isCurrencyZero(currency2)) {
        // n + 0 = n (identity property).
        // The second currency is 0, so return the first currency.
        return currency1;
    }
    else if (currency1.isZero) {
        // 0 + m = m (identity property).
        // The first currency is 0, so return the second currency.
        return typeof currency2 === "number" ? createCurrency(currency2, currency1.currencyOptions) : currency2;
    }

    const { units: otherUnits } = getCurrencyParts(currency2, currency1.precision);

    return createCurrency(
        {
            precision: currency1.precision,
            units: currency1.units + otherUnits
        },
        currency1.currencyOptions
    );
}

/**
 * Returns the negation of a Currency.
 *
 * @example
 * negate(createCurrency(2.61)); // createCurrency(-2.61)
 */
function negate(currency: Currency): Currency {
    if (currency.isZero) {
        // -1 * 0 = 0
        return currency;
    }

    // -1 * n = -n
    return createCurrency(
        {
            units: -1 * currency.units,
            precision: currency.precision
        },
        currency.currencyOptions
    );
}

/**
 * Returns the quotient and remainder from dividing a currency by a number.
 *
 * @example
 * divide(createCurrency(3.50), 3); // returns { quotient: createCurrency(1.16), remainder: createCurrency(0.02) }
 */
function divide(currency: Currency, divisor: number): { quotient: Currency, remainder: Currency } {
    if (currency.isZero) {
        // 0 / m = 0 (zero property)
        return { quotient: currency, remainder: currency };
    }
    else if (divisor === 1) {
        // n / 1 = n (identity property)
        return {
            quotient: currency,
            remainder: createZeroCurrency(currency.currencyOptions)
        };
    }

    const isNegative = currency.isNegative ? divisor >= 0 : divisor < 0;
    const truncateUnits = isNegative ? Math.ceil : Math.floor;

    // Let divide by zero cause an error for now.
    const quotientUnits = truncateUnits(currency.units / divisor);
    const remainderUnits = currency.units - (quotientUnits * divisor);

    const quotient = createCurrency(
        {
            precision: currency.precision,
            units: quotientUnits
        },
        currency.currencyOptions
    );

    const remainder = createCurrency(
        {
            precision: currency.precision,
            units: remainderUnits
        },
        currency.currencyOptions
    );

    return {
        quotient,
        remainder
    };
}

/**
 * Returns the difference between two currencies.
 *
 * @example
 * subtract(createCurrency(2.61), 3.999);                 // returns createCurrency(-1.38)
 * subtract(createCurrency(2.61), createCurrency(3.999)); // returns createCurrency(-1.38)
 */
function subtract(currency1: Currency, currency2: number | Currency): Currency {
    if (isCurrencyZero(currency2)) {
        // n - 0 = n (identity property).
        return currency1;
    }
    else if (currency1.isZero) {
        // 0 - m = -m (identity property; 0 + -m = -m)
        if (typeof currency2 === "number") {
            return negate(createCurrency(currency2, currency1.currencyOptions));
        }
        else {
            return negate(currency2);
        }
    }

    const { units: otherUnits } = getCurrencyParts(currency2, currency1.precision);

    return createCurrency(
        {
            units: currency1.units - otherUnits,
            precision: currency1.precision
        },
        currency1.currencyOptions
    );
}

/**
 * Formats a currency as a string using the following placeholders:
 *
 * - `"-"` for the negative symbol
 * - `"!"` for the currency symbol
 * - `"#"` for the major units (the number to the left of the decimal)
 *   - These must appear consecutively if multiple "#" are used in the format string.
 *   - Grouping characters can be specified like the comma in `"#,###"`. Some currencies use `"."` instead of `","`.
 *   - The left-most grouping will be used repeatedly to format large numbers;
 *     - `"#,###"` will add a `","` for every thousandth like in `"1,234,567"`.
 * - `"%"` for the minor units (the number to the right of the decimal).
 * - `"@"` for the currency code ("USD", "CAD", etc.).
 * - The string between the last `"#"` and the first `"%"` will be considered the decimal separator (including whitespace). It will be omitted if the currency has a precision of 0.
 *
 * @param formatString The format string. (defaults to `"-!#,###.%"`, see `CurrencyFormatString` for common formats)
 * @example
 * format(createCurrency(-3467.56));                    // default format "-$3,467.56"
 * format(createCurrency(-3467.56), "-#.%");            // omit the symbol and ignore grouping "-3467.56"
 * format(createCurrency(-12345678.90), "-₹#,##,###.%") // the indic numbering system groups the first thousand, then every ten digits after "-₹1,23,45,678.90"
 * format(createCurrency(-12345678.90), "!#")           // absolute value integer with currency symbol "$12345678"
 * format(createCurrency(12.50), "-!#,###.% @");        // default format with currency code "$12.50 USD"
 */
function format(currency: Currency, formatString: string = CurrencyFormatString.default): string {
    if (!Number.isFinite(currency.units)) {
        // If the units are not a finite number, then just convert the units using toString.
        return currency.units.toString();
    }

    type MajorUnitGroup = {
        count: number;
        separator: string;
    };

    // Create the group strategy.
    function formatMajorUnits(number: string, groups: MajorUnitGroup[]): string {
        // Remove the leading negative sign if present.
        if (number.startsWith("-")) {
            number = number.substring(1);
        }

        if (groups.length === 0) {
            // No need to format the number if there are no groups.
            return number;
        }

        const groupsCopy: MajorUnitGroup[] = [...groups];
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        const repeatGroup: MajorUnitGroup = groupsCopy.shift()!;

        function getNextGroup(): MajorUnitGroup {
            return groupsCopy.shift() ?? repeatGroup;
        }

        let currentGroup: MajorUnitGroup = getNextGroup();
        let currentGroupCount: number = 0;

        let groupedNumber: string = "";

        for (let i = number.length - 1; i >= 0; i--) {
            currentGroupCount++;

            if (currentGroupCount === currentGroup.count && i !== 0) {
                groupedNumber = currentGroup.separator + number[i] + groupedNumber;
                currentGroup = getNextGroup();
                currentGroupCount = 0;
            }
            else {
                groupedNumber = number[i] + groupedNumber;
            }
        }

        return groupedNumber;
    }

    const guid = newGuid().slice(0, 6); // Make the placeholders unique to avoid clashing with format strings that may contain placeholders.
    const minorUnitsPlaceholder = `MINOR_UNITS${guid}` as const;
    const majorUnitsPlaceholder = `MAJOR_UNITS${guid}` as const;
    const decimalPlaceholder = `DECIMAL${guid}` as const;

    // Get the minor units format section.
    const minorUnitsStart = formatString.indexOf("%");
    const minorUnitsEnd = formatString.lastIndexOf("%");
    if (minorUnitsStart !== -1 && minorUnitsEnd !== -1) {
        formatString = formatString.slice(0, minorUnitsStart) + minorUnitsPlaceholder + formatString.slice(minorUnitsEnd + 1);
    }

    // Get the major units format section.
    const majorUnitsStart = formatString.indexOf("#");
    const majorUnitsEnd = formatString.lastIndexOf("#");
    const groups: { count: number, separator: string }[] = [];
    if (majorUnitsStart !== -1 && majorUnitsEnd !== -1) {
        // Strip out the major units format section.
        const majorUnitsFormatSection = formatString.substring(majorUnitsStart, majorUnitsEnd + 1).replace(/^#*/, "");
        formatString = formatString.slice(0, majorUnitsStart) + majorUnitsPlaceholder + formatString.slice(majorUnitsEnd + 1);

        // Find the groups working from right to left.
        // #
        // ###
        // #,###
        // #,#
        // #,##,###
        // # , ## , ###
        // #,###, ###
        // The leftmost group (at index 0) is the one that will repeat.
        // The leftmost "#" characters will be ignored.
        let currentGroup: MajorUnitGroup = {
            count: 0,
            separator: ""
        };
        for (let i = majorUnitsFormatSection.length - 1; i >= 0; i--) {
            if (majorUnitsFormatSection[i] === "#") {
                if (currentGroup.separator === "") {
                    // Still haven't found the separator character for this group,
                    // so increase the group count and move on.
                    currentGroup.count++;
                }
                else {
                    // "#" was encountered after recording the separator character,
                    // which means we're on the next group,
                    // so save the last one and start fresh.
                    groups.unshift(currentGroup);
                    currentGroup = {
                        count: 1, // This should start at 1 since we already found one "#" for the next group.
                        separator: ""
                    };
                }
            }
            else {
                currentGroup.separator = majorUnitsFormatSection[i] + currentGroup.separator;
            }
        }

        if (currentGroup.separator) {
            // If the last group has a separator defined then add it.
            // This could happen if the format string section was ",###".
            groups.unshift(currentGroup);
        }
    }

    // Get the decimal separator section.
    const majorUnitsPlaceholderStart = formatString.indexOf(majorUnitsPlaceholder);
    const majorUnitsPlaceholderEnd = majorUnitsPlaceholderStart === -1 ? -1 : majorUnitsPlaceholderStart + majorUnitsPlaceholder.length - 1;
    const minorUnitsPlaceholderStart = formatString.indexOf(minorUnitsPlaceholder);
    const decimalSeparatorStart = (majorUnitsPlaceholderEnd !== -1 && minorUnitsPlaceholderStart !== -1 && majorUnitsPlaceholderEnd + 1 < minorUnitsPlaceholderStart)
        ? majorUnitsPlaceholderEnd + 1
        : -1;
    const decimalSeparatorEnd = (decimalSeparatorStart !== -1)
        ? minorUnitsPlaceholderStart - 1
        : -1;
    const hasDecimalSeparator = decimalSeparatorStart !== -1 && decimalSeparatorEnd !== -1;
    const decimalSeparator = hasDecimalSeparator ? formatString.slice(decimalSeparatorStart, decimalSeparatorEnd + 1) : "";
    if (hasDecimalSeparator) {
        formatString = formatString.slice(0, decimalSeparatorStart) + decimalPlaceholder + formatString.slice(decimalSeparatorEnd + 1);
    }

    return formatString
        .replace("-", currency.isNegative ? "-" : "")
        .replace("!", currency.symbol)
        .replace(majorUnitsPlaceholder, formatMajorUnits(currency.unsignedMajorUnitsString, groups))
        .replace(decimalPlaceholder, currency.precision > 0 ? decimalSeparator : "")
        .replace(minorUnitsPlaceholder, currency.unsignedMinorUnitsString)
        .replace("@", currency.code);
}

/**
 * Returns `true` if two currencies are equal.
 *
 * @example
 * isEqualTo(createCurrency(1), createCurrency(1.00)); // true
 * isEqualTo(createCurrency(1), createCurrency(1.01)); // false
 */
function isEqualTo(currency1: Currency, currency2: Currency | number): boolean {
    const isZero = isCurrencyZero(currency2);
    if (currency1.isZero || isZero) {
        // Both currencies must be 0 if at least one of them is 0.
        return currency1.isZero && isZero;
    }

    const { units: otherUnits } = getCurrencyParts(currency2, currency1.precision);

    return currency1.units === otherUnits;
}

/**
 * Returns `true` if two currencies are not equal.
 *
 * @example
 * isNotEqualTo(createCurrency(1), createCurrency(1.00)); // false
 * isNotEqualTo(createCurrency(1), createCurrency(1.01)); // true
 */
function isNotEqualTo(currency1: Currency, currency2: Currency | number): boolean {
    const isZero = isCurrencyZero(currency2);
    if (currency1.isZero || isZero) {
        // Both currencies must not be 0 if at least one of them is 0.
        return !(currency1.isZero && isZero);
    }

    const { units: otherUnits } = getCurrencyParts(currency2, currency1.precision);

    return currency1.units !== otherUnits;
}

/**
 * Returns `true` if `currency1` is less than `currency2`.
 *
 * @example
 * isLessThan(createCurrency(1), 2);                 // true
 * isLessThan(createCurrency(2), 2);                 // false
 * isLessThan(createCurrency(3), createCurrency(2)); // false
 */
function isLessThan(currency1: Currency, currency2: Currency | number): boolean {
    if (currency1.isZero && isCurrencyZero(currency2)) {
        // 0 < 0 (false)
        return false;
    }

    const { units: otherUnits } = getCurrencyParts(currency2, currency1.precision);

    return currency1.units < otherUnits;
}

/**
 * Returns `true` if `currency1` is greater than `currency2`.
 *
 * @example
 * isGreaterThan(createCurrency(1), 2);                 // false
 * isGreaterThan(createCurrency(2), 2);                 // false
 * isGreaterThan(createCurrency(3), createCurrency(2)); // true
 */
function isGreaterThan(currency1: Currency, currency2: Currency | number): boolean {
    if (currency1.isZero && isCurrencyZero(currency2)) {
        // 0 > 0 (false)
        return false;
    }

    const { units: otherUnits } = getCurrencyParts(currency2, currency1.precision);

    return currency1.units > otherUnits;
}

/**
 * Gets the absolute value of a currency.
 *
 * @example
 * abs(createCurrency(1));  // 1
 * abs(createCurrency(-1)); // 1
 */
function abs(currency: Currency): Currency {
    if (!currency.isNegative) {
        // |n| = n
        return currency;
    }
    else {
        // |-n| = n
        return createCurrency(
            {
                precision: currency.precision,
                units: Math.abs(currency.units)
            },
            currency.currencyOptions
        );
    }
}

/**
 * Returns the remainder after dividing a currency by a number.
 *
 * @example
 * mod(createCurrency(1.00), 2); // 0.00
 * mod(createCurrency(2.21), 2); // 0.01 (2.21 is evenly divided into 1.10 twice with 0.01 remaining)
 * mod(createCurrency(2.21), 9); // 0.05 (2.21 is evenly divided into 0.24 nine times with 0.05 remaining)
 */
function mod(currency: Currency, divisor: number): Currency {
    if (currency.isZero) {
        // 0 % m = 0 (zero property)
        return currency;
    }
    else if (divisor === 1) {
        // n % 1 = 0 (zero property)
        return createZeroCurrency(currency.currencyOptions);
    }
    else if (isCurrencyZero(divisor)) {
        // n % 0 = n (identity property)
        return currency;
    }

    // Some of the "shortcuts" below require that the divisor
    // be treated as though it were units of a currency with the same precision.
    const divisorAsCurrency = createCurrency(
        {
            units: divisor,
            precision: currency.precision
        },
        currency.currencyOptions
    );

    // Do not use the Currency methods here since they can add noise to the logs.
    if (isLessThan(abs(currency), divisorAsCurrency)) {
        if (!currency.isNegative) {
            // n % m = n, where |n| < m and n is positive
            return currency;
        }
        else {
            // n % m = n + m, where |n| < m and n is negative
            return add(currency, divisorAsCurrency);
        }
    }

    // Calculate the remainder by division.
    const { remainder } = divide(currency, divisor);
    return remainder;
}

// #endregion Currency Functions

// #region Helper Functions (similar to private static methods in a class)

/**
 * Returns `true` if the currency is equal to zero.
 *
 * @example
 * isCurrencyZero(createCurrency(0));             // true
 * isCurrencyZero(createCurrency(1));             // false
 * isCurrencyZero(createCurrency(1).subtract(1)); // true
 */
function isCurrencyZero(currency: Currency | number): boolean {
    if (currency === undefined || currency === null) {
        return true;
    }
    else if (typeof currency === "number") {
        return currency === 0;
    }
    else if (typeof currency !== "number") {
        return currency.isZero;
    }
    else {
        return false;
    }
}

/**
 * Returns the parts of a currency number or Currency object with a specific precision.
 *
 * @example
 * getCurrencyParts(1.234, 2);  // { units: 123, precision: 2 }
 * getCurrencyParts(1.234, 3);  // { units: 1234, precision: 3 }
 * getCurrencyParts(21.234, 2); // { units: 2123, precision: 2 }
 * getCurrencyParts(21.234, 1); // { units: 212, precision: 1 }
 * getCurrencyParts(21.234, 0); // { units: 21, precision: 0 }
 */
function getCurrencyParts(currency: number | CurrencyParts, targetPrecision: number): CurrencyParts {
    if (currency === undefined || currency === null) {
        // TypeScript prevents currency from being null/undefined;
        // however, it is still possible to pass in either of those values,
        // in which case this will return 0.
        return {
            precision: targetPrecision,
            units: 0
        };
    }
    else if (typeof currency !== "number") {
        // Ensure the precision is the same as the supplied value using truncation.
        const precisionDiff = targetPrecision - currency.precision;

        if (precisionDiff !== 0) {
            const adjustedUnits = currency.units * (Math.pow(10, precisionDiff));
            return {
                units: currency.units < 0 ? Math.ceil(adjustedUnits) : Math.floor(adjustedUnits),
                precision: targetPrecision,
            };
        }
        else {
            return {
                units: currency.units,
                precision: targetPrecision
            };
        }
    }

    // Per, https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number/toString#description...
    // If the number is not a whole number, the decimal point "." is used to separate the decimal places.
    // Therefore, it is safe to split number.toString() by "." to get the whole and fractional parts of a number,
    // assuming the number is not NaN, Infinity, NegativeInfinity, or scientific notation.

    if (!Number.isFinite(currency)) {
        // Handle invalid numbers gracefully (no exceptions) by returning the non-finite units as is.
        // External code can utilize Currency.isInvalid to determine if the resulting Currency is invalid.
        return {
            units: currency,
            precision: targetPrecision
        };
    }

    let currencyAsString = currency.toString();

    if (currencyAsString.includes("e")) {
        // Convert scientific notation to decimal number.
        currencyAsString = eToNumber(currencyAsString);
    }

    // It is safe to split the decimal number by "." here regardless of the locale.
    const parts = currencyAsString.split(".");

    const isNegative: boolean = parts[0]?.startsWith("-") ?? false;
    let majorUnits: string = parts[0]?.substring(isNegative ? 1 : 0) ?? "";
    let minorUnits: string = parts[1]?.substring(0, targetPrecision + 1) ?? ""; // Temporarily increase the precision by 1 to handle rounding.

    if (minorUnits.length === targetPrecision + 1) {
        if (minorUnits[targetPrecision] >= "5") {
            if (targetPrecision === 0) {
                // Round the major units up.
                majorUnits = (+majorUnits + 1).toString();
                minorUnits = "";
            }
            else {
                // Round the minor units up.
                // "069" => (Number("06") + 1).toString() => "7"; target precision is 2, so the number needs to be zero-padded in front => "07" (done below).
                // "999" => (Number("99") + 1).toString() => "100"; result is larger than target precision 2, so major units need to be incremented, and the tens and ones digits become the minor units => "00" (done below).
                minorUnits = (+minorUnits.slice(0, targetPrecision) + 1).toString();
            }
        }
        else {
            // Truncate the extra precision that was added for rounding.
            minorUnits = minorUnits.slice(0, targetPrecision);
        }
    }

    if (minorUnits.length < targetPrecision) {
        minorUnits = "0".repeat(targetPrecision - minorUnits.length) + minorUnits;
    }
    else if (minorUnits.length === targetPrecision + 1) {
        // Rounding the minor units resulted in an additional major unit
        // (e.g., "1.995" rounds up to "2.00") so increment the major unit,
        // and truncate the leading minor unit number.
        majorUnits = (+majorUnits + 1).toString();
        minorUnits = minorUnits.substring(1);
    }

    const units: number = +(`${isNegative ? "-" : ""}${majorUnits}${minorUnits}`);
    return {
        units: units,
        precision: targetPrecision
    };
}

/** Converts scientific e-notication numbers to a decimal string. */
function eToNumber(numberOrString: number | string): string {
    let numberAsString = numberOrString += "";
    const sign = numberAsString.charAt(0) === "-" ? "-" : "";

    // Strip the sign. It will be added back at the end.
    if (sign) {
        numberAsString = numberAsString.substring(1);
    }

    const scientificNotationParts = numberAsString.split(/[e]/ig);

    if (scientificNotationParts.length < 2) {
        return sign + numberAsString;
    }

    const dot = (.1).toLocaleString().substring(1, 2); // Get the "dot" for the current locale.
    const signlessCoefficient = scientificNotationParts[0].replace(/^0+/, ""); // "4.323" is the signless coefficient in "-4.323E10". (Remove leading zeroes)

    if (+signlessCoefficient === 0) {
        // Return "0" (without a sign) if the signless coefficient is 0.
        return "0";
    }

    const exponent = +scientificNotationParts[1]; // "10" is the number of powers of ten in "-4.323E10"
    const signlessCoefficientWithoutDot = signlessCoefficient.replace(dot, ""); // "4323" is this value in "-4.323E10". (Remove the dot from the coefficient instead of doing the math)
    const moveDotCount = // "11" in "-4.323E10" (coefficient integer part length + exponent) (positive will move the dot to the right, negative will move the dot to the left.)
        signlessCoefficient.includes(dot) // Is there a mantissa (fractional bit) in the coefficient?
            ? signlessCoefficient.indexOf(dot) + exponent
            : signlessCoefficient.length + exponent;
    const adjustedMoveDotCount = moveDotCount - signlessCoefficientWithoutDot.length; // "7" in "-4.323E10" ("11" - "4")
    const signlessCoefficientWithoutDotIntString = "" + BigInt(signlessCoefficientWithoutDot);
    const decimalNumberString = exponent >= 0 // Is the dot being moved to the right to make the number larger, or to the left to make the number smaller?
        ? (adjustedMoveDotCount >= 0
            ? signlessCoefficientWithoutDotIntString + "0".repeat(adjustedMoveDotCount)
            : signlessCoefficientWithoutDot.replace(new RegExp(`^(.{${moveDotCount}})(.)`), `$1.$2`))
        : (moveDotCount <= 0
            ? "0." + "0".repeat(Math.abs(moveDotCount)) + signlessCoefficientWithoutDotIntString
            : signlessCoefficientWithoutDot.replace(new RegExp(`^(.{${moveDotCount}})(.)`), `$1.$2`));

    if (+decimalNumberString === 0 && signlessCoefficientWithoutDotIntString === "0") {
        return "0";
    }

    return sign + decimalNumberString;
}

// #endregion Helper Functions

// #region Creation Functions

export function createReadonlyCurrencyOptions(options?: Partial<CurrencyOptions>): CurrencyOptions {
    return {
        ...{
            precision: options?.precision ?? defaultCurrencyOptions.precision,
            symbol: options?.symbol ?? defaultCurrencyOptions.symbol,
            code: options?.code ?? defaultCurrencyOptions.code,
            formatString: options?.formatString ?? defaultCurrencyOptions.formatString,
            isLoggingEnabled: options?.isLoggingEnabled ?? defaultCurrencyOptions.isLoggingEnabled,
        }
    } as const;
}

export function createZeroCurrency(options: CurrencyOptions): Currency {
    return createCurrency(0, options);
}
export function createCurrency(currency: number | CurrencyParts, options: CurrencyOptions): Currency {
    // #region Fields

    // eslint-disable-next-line @typescript-eslint/naming-convention
    let _isZero: boolean | null = null;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    let _toString: string | null = null;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    let _majorUnits: string | null = null;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    let _minorUnits: string | null = null;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    let _unitsString: string | null = null;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    let _isNegative: boolean | null = null;

    // Set the currency options as readonly and const so they cannot be modified once set.
    // eslint-disable-next-line @typescript-eslint/naming-convention
    const _currencyOptions = createReadonlyCurrencyOptions(options);
    const {
        precision,
        // eslint-disable-next-line @typescript-eslint/naming-convention
        symbol: _symbol,
        // eslint-disable-next-line @typescript-eslint/naming-convention
        code: _code,
        // eslint-disable-next-line @typescript-eslint/naming-convention
        formatString: _formatString,
        // eslint-disable-next-line @typescript-eslint/naming-convention
        isLoggingEnabled: _isLoggingEnabled,
    } = _currencyOptions;

    // Get the currency parts from the number.
    const {
        // eslint-disable-next-line @typescript-eslint/naming-convention
        units: _units,
        // eslint-disable-next-line @typescript-eslint/naming-convention
        precision: _precision
    } = getCurrencyParts(currency ?? 0, precision);

    // #endregion Fields

    // eslint-disable-next-line @typescript-eslint/naming-convention
    return {
        get number(): number {
            return this.units * Math.pow(10, -1 * this.precision);
        },

        get units(): number {
            return _units;
        },

        get unsignedUnitsString(): string {
            if (_unitsString === null) {
                let parts = this.units.toString();
                if (parts.startsWith("-")) {
                    parts = parts.substring(1);
                }
                if (parts.length < (this.precision + 1)) {
                    parts = "0".repeat(this.precision + 1 - parts.length) + parts;
                }
                _minorUnits = parts.substring(parts.length - this.precision);
                _majorUnits = parts.substring(0, parts.length - this.precision);
                _unitsString = `${_majorUnits}${_minorUnits}`;
            }
            return _unitsString;
        },

        get isNegative(): boolean {
            return (_isNegative ?? (_isNegative = this.units < 0));
        },

        get precision(): number {
            return _precision;
        },

        get symbol(): string {
            return _symbol;
        },

        get code(): string {
            return _code;
        },

        get isZero(): boolean {
            return (_isZero ?? (_isZero = this.units === 0));
        },

        get formatString(): string {
            return _formatString;
        },

        get unsignedMinorUnitsString(): string {
            return (_minorUnits ?? (_minorUnits = this.unsignedUnitsString.slice(this.unsignedUnitsString.length - this.precision)));
        },

        get unsignedMajorUnitsString(): string {
            return (_majorUnits ?? (_majorUnits = this.unsignedUnitsString.slice(0, this.unsignedUnitsString.length - this.precision)));
        },

        get currencyOptions(): CurrencyOptions {
            return _currencyOptions;
        },

        get isInvalid(): boolean {
            return !Number.isFinite(this.units);
        },

        add(currency: Currency | number): Currency {
            const logAndReturn = (result: Currency): Currency => {
                _isLoggingEnabled && console.debug(`${this} + ${currency} = ${result}`);
                return result;
            };

            return logAndReturn(add(this, currency));
        },

        negate(): Currency {
            const logAndReturn = (result: Currency): Currency => {
                _isLoggingEnabled && console.debug(`-${this} = ${result}`);
                return result;
            };

            return logAndReturn(negate(this));
        },

        divide(divisor: number): { quotient: Currency, remainder: Currency } {
            const logAndReturn = (result: { quotient: Currency; remainder: Currency }): { quotient: Currency; remainder: Currency } => {
                _isLoggingEnabled && console.debug(`${this} / ${divisor} = ${result.quotient} r${result.remainder}`);
                return result;
            };

            return logAndReturn(divide(this, divisor));
        },

        subtract(currency: number | Currency): Currency {
            const logAndReturn = (result: Currency): Currency => {
                _isLoggingEnabled && console.debug(`${this} - ${currency} = ${result}`);
                return result;
            };

            return logAndReturn(subtract(this, currency));
        },

        format(formatString: string = CurrencyFormatString.default): string {
            const logAndReturn = (result: string): string => {
                _isLoggingEnabled && console.debug(`format with ${formatString} = ${result}`);
                return result;
            };

            return logAndReturn(format(this, formatString));
        },

        isEqualTo(currency: Currency | number): boolean {
            const logAndReturn = (result: boolean): boolean => {
                _isLoggingEnabled && console.debug(`${this} == ${currency} = ${result}`);
                return result;
            };

            return logAndReturn(isEqualTo(this, currency));
        },

        isNotEqualTo(currency: Currency | number): boolean {
            const logAndReturn = (result: boolean): boolean => {
                _isLoggingEnabled && console.debug(`${this} != ${currency} = ${result}`);
                return result;
            };

            return logAndReturn(isNotEqualTo(this, currency));
        },

        isLessThan(currency: Currency | number): boolean {
            const logAndReturn = (result: boolean): boolean => {
                _isLoggingEnabled && console.debug(`${this} < ${currency} = ${result}`);
                return result;
            };

            return logAndReturn(isLessThan(this, currency));
        },

        isGreaterThan(currency: Currency | number): boolean {
            const logAndReturn = (result: boolean): boolean => {
                _isLoggingEnabled && console.debug(`${this} > ${currency} = ${result}`);
                return result;
            };

            return logAndReturn(isGreaterThan(this, currency));
        },

        abs(): Currency {
            const logAndReturn = (result: Currency): Currency => {
                _isLoggingEnabled && console.debug(`|${this}| = ${result}`);
                return result;
            };

            return logAndReturn(abs(this));
        },

        mod(divisor: number): Currency {
            const logAndReturn = (result: Currency): Currency => {
                _isLoggingEnabled && console.debug(`${this} % ${divisor} = ${result}`);
                return result;
            };

            return logAndReturn(mod(this, divisor));
        },

        toString(): string {
            // Cache the formatted value of this immutable currency.
            if (_toString === null) {
                _toString = format(this, this.formatString);
            }

            return _toString;
        }
    };
}

// #endregion Creation Functions