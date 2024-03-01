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

import { Guid } from "@Obsidian/Types";
import { RegistrantBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrantBag";
import { RegistrarBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrarBag";
import { RegistrationEntryInitializationBox as GeneratedRegistrationEntryInitializationBox } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryInitializationBox";
import { RegistrationEntryFeeBag as GeneratedRegistrationEntryFeeBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFeeBag";
import { RegistrationEntryFeeItemBag as GeneratedRegistrationEntryFeeItemBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFeeItemBag";
import { RegistrationEntrySuccessBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntrySuccessBag";
import { RegistrationEntryFormFieldBag as GeneratedRegistrationEntryFormFieldBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFormFieldBag";
import { TypeBuilder } from "@Obsidian/Utility/typeUtils";
import { RegistrationEntryFormBag as GeneratedRegistrationEntryFormBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFormBag";

export const enum Step {
    Intro = "intro",
    RegistrationStartForm = "registrationStartForm",
    PerRegistrantForms = "perRegistrantForms",
    RegistrationEndForm = "registrationEndForm",
    Review = "review",
    Payment = "payment",
    Success = "success"
}

export type RegistrantBasicInfo = {
    firstName: string;
    lastName: string;
    email: string;
    guid: Guid;
};

export type RegistrationCostSummaryInfo = {
    paidAmount: number;
    remainingAmount: number;
    minimumRemainingAmount: number;
};

const registrationEntryFeeItemBagBuilder = TypeBuilder.createFrom<GeneratedRegistrationEntryFeeItemBag>()
    // Fix the RegistrationEntryFeeItemBag.guid property to be required and not nullable.
    .makeProperties("guid").required().and.defined()
    // Fix the RegistrationEntryFeeItemBag.countRemaining property to not be undefinable.
    .makeProperties("countRemaining").required();
export type RegistrationEntryFeeItemBag = typeof registrationEntryFeeItemBagBuilder.build;

const registrationEntryFeeBagBuilder = TypeBuilder.createFrom<GeneratedRegistrationEntryFeeBag>()
    // Used the fixed items type.
    .makeProperties("items").required().and.typed<RegistrationEntryFeeItemBag[] | null>();
export type RegistrationEntryFeeBag = typeof registrationEntryFeeBagBuilder.build;

const registrationEntryInitializationBoxBuilder = TypeBuilder.createFrom<GeneratedRegistrationEntryInitializationBox>()
    // Make optional properties required since they cannot be set to undefined, and must be null or have a value.
    .makeProperties("spotsRemaining", "gatewayControl", "savedAccounts").required()
    .makeProperties("registrantForms").required().and.typed<RegistrationEntryFormBag[] | null>()
    .makeProperties("fees").required().and.typed<RegistrationEntryFeeBag[] | null>();
export type RegistrationEntryInitializationBox = typeof registrationEntryInitializationBoxBuilder.build;

const registrationEntryFormBagBuilder = TypeBuilder.createFrom<GeneratedRegistrationEntryFormBag>()
    .makeProperties("fields").required().and.typed<RegistrationEntryFormFieldBag[] | null>();
export type RegistrationEntryFormBag = typeof registrationEntryFormBagBuilder.build;

const registrationEntryFormFieldBagBuilder = TypeBuilder.createFrom<GeneratedRegistrationEntryFormFieldBag>()
    .makeProperties("guid").required().and.typed<Guid>();
export type RegistrationEntryFormFieldBag = typeof registrationEntryFormFieldBagBuilder.build;

export type RegistrationEntryState = {
    steps: Record<Step, Step>;
    viewModel: RegistrationEntryInitializationBox;
    currentStep: string;
    firstStep: string;
    navBack: boolean;
    currentRegistrantIndex: number;
    currentRegistrantFormIndex: number;
    registrants: RegistrantBag[];
    registrationFieldValues: Record<Guid, unknown>;
    registrar: RegistrarBag;
    gatewayToken: string;
    savedAccountGuid: Guid | null;
    discountCode: string;
    discountAmount: number;
    discountPercentage: number;
    discountMaxRegistrants: number;
    successViewModel: RegistrationEntrySuccessBag | null;
    amountToPayToday: number;
    sessionExpirationDateMs: number | null;
    registrationSessionGuid: Guid;
    ownFamilyGuid: Guid;
    paymentPlanFrequencyGuid: string | null;
    paymentPlanAmountPerPayment: number | null;
    paymentPlanNumberOfPayments: number | null;
    paymentPlanFrequencyText: string | null;
};

export type CurrencyOptions = {
    precision: number,
    symbol: string,
    code: string,
    formatString: string
};

export type CurrencyParts = {
    isNegative: boolean;
    units: string;
    precision: number;
};

export function createReadonlyCurrencyOptions(options?: Partial<CurrencyOptions>): CurrencyOptions {
    return {
        ...{
            precision: options?.precision ?? defaultCurrencyOptions.precision,
            symbol: options?.symbol ?? defaultCurrencyOptions.symbol,
            code: options?.code ?? defaultCurrencyOptions.code,
            formatString: options?.formatString ?? defaultCurrencyOptions.formatString,
        }
    } as const;
}

export const CurrencyFormatString = {
    default: "-!#,###.%",
    defaultCodeSuffix: "-!#,###.% @",
    indic: "-!#,##,###.%",
    indicCodeSuffix: "-!#,##,###.% @",
} as const;

const defaultCurrencyOptions: CurrencyOptions = {
    precision: 2,
    symbol: "$",
    code: "USD",
    formatString: CurrencyFormatString.default
} as const;

export class Currency {
    // Omitting the TS `private` accessibility modifier so intellisense works as expected. Ideally, these should be marked `private`.
    // #region Fields
    // eslint-disable-next-line @typescript-eslint/naming-convention
    readonly _currencyOptions: CurrencyOptions;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    readonly _currencyParts: CurrencyParts;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    readonly _isZero: boolean;

    // TODO JMH Remove __createdFrom once the constructor issue is resolved.
    // eslint-disable-next-line @typescript-eslint/naming-convention
    readonly __createdFrom: number | CurrencyParts;

    // eslint-disable-next-line @typescript-eslint/naming-convention
    _toString: string | null = null;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    _majorUnits: string | null = null;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    _minorUnits: string | null = null;

    // #endregion Fields

    // #region Properties

    get units(): string {
        return this._currencyParts.units;
    }

    /** Returns `true` if this currency is negative; otherwise, `false` is returned. */
    get isNegative(): boolean {
        return this._currencyParts.isNegative;
    }

    /** Gets the precision (the number of minor unit digits after the decimal). */
    get precision(): number {
        return this._currencyOptions.precision;
    }

    /** Gets the symbol. */
    get symbol(): string {
        return this._currencyOptions.symbol;
    }

    /** Gets the code. */
    get code(): string {
        return this._currencyOptions.code;
    }

    /** Returns `true` if this currency is equal to 0; otherwise, `false`. */
    get isZero(): boolean {
        return this._isZero;
    }

    /** Gets the format string. */
    get formatString(): string {
        return this._currencyOptions.formatString;
    }

    /** Gets the minor units. */
    get minorUnits(): string {
        return (this._minorUnits ?? (this._minorUnits = this.units.slice(this.units.length - this.precision)));
    }

    /** Gets the major units. */
    get majorUnits(): string {
        return (this._majorUnits ?? (this._majorUnits = this.units.slice(0, this.units.length - this.precision)));
    }

    // #endregion Properties

    // #region Constructors

    /**
     * Creates a new instance of the Currency type.
     */
    constructor(
        currency: number | CurrencyParts,
        options?: Partial<CurrencyOptions>) {
        // Set the currency options as readonly and const so they cannot be modified once set.
        this._currencyOptions = createReadonlyCurrencyOptions(options);

        // TODO JMH Remove this debug tracking code.
        // Set the origin from which this currency is created.
        this.__createdFrom = currency;

        // Get the currency parts from the number.
        const { isNegative, units, precision } = Currency.getCurrencyParts(currency, this._currencyOptions.precision);

        // Set the currency parts as readonly and const so they cannot be modified once set.
        this._currencyParts = {
            isNegative,
            units,
            precision
        } as const;

        if (typeof currency === "number") {

            // Set whether or not this currency is equal to zero.
            this._isZero = currency === 0;
        }
        else {
            // Set whether or not this currency is equal to zero.
            const zeroRegex = /^0*$/;
            this._isZero = zeroRegex.test(currency.units);
        }
    }

    // #endregion Constructors

    // #region Public Methods

    /**
     * Adds an amount to this Currency.
     *
     * @param currency The amount to add to this currency. Fractional amounts that exceed this currency's precision will be truncated; e.g., if this currency has a precision of two ($31.45), and the amount being added has a precision of three ($2.289), the "9" will be truncated to $2.28.
     * @returns A new Currency instance containing the sum of the two currencies.
     * @example
     * new Currency(2.61).add(3.999);               // returns new Currency(6.60)
     * new Currency(2.61).add(new Currency(3.999)); // returns new Currency(6.60)
     */
    add(currency: Currency | number): Currency {
        const $return = ((result: Currency): Currency => {
            console.debug(`${this} + ${currency} = ${result}`);
            return result;
        }).bind(this);

        if (this.isZero) {
            // 0 + n = n (identity property).
            // This currency is 0, so return the other currency.
            return $return(typeof currency === "number" ? new Currency(currency, this._currencyOptions) : currency);
        }
        else if (Currency.isCurrencyZero(currency)) {
            // n + 0 = n (identity property).
            // The other currency is 0, so return this currency.
            return $return(this);
        }

        const { isNegative: otherIsNegative, units: otherUnits } = Currency.getCurrencyParts(currency, this.precision);

        if (this.isNegative === otherIsNegative) {
            // Add the currencies if they have the same sign (+/-).
            // n + m

            // Return the new Currency object.
            const units = Currency.addUnits(this.units, otherUnits);

            return $return(new Currency(
                {
                    isNegative: this.isNegative,
                    units,
                    precision: this.precision
                },
                this._currencyOptions
            ));
        }
        else {
            // Adding currencies with a different sign (+/-) is the
            // same as subtracting one from the other.

            if (this.units === otherUnits) {
                // n + -n = 0 (additive inverse)
                return $return(Currency.createZeroCurrency(this._currencyOptions));
            }

            if (!otherIsNegative) {
                // The other number is negative.
                // n + -m = n - m
                const { isNegative, units } = Currency.substractUnits(this.units, otherUnits);
                return new Currency(
                    {
                        isNegative,
                        units,
                        precision: this.precision
                    },
                    this._currencyOptions);
            }
            else {
                // This number is negative.
                // -n + m = -1 * (n - m)
                const { isNegative, units } = Currency.substractUnits(this.units, otherUnits);
                return new Currency({
                    isNegative: !isNegative, // Flip the sign of the result in this case.
                    units,
                    precision: this.precision
                }, this._currencyOptions);
            }
        }
    }

    /**
     * Gets the negation of this Currency.
     *
     * @returns A new Currency instance containing the negation of this Currency.
     * @example
     * new Currency(2.61).negate(); // returns new Currency(-2.61)
     */
    negate(): Currency {
        const $return = ((result: Currency): Currency => {
            console.debug(`-${this} = ${result}`);
            return result;
        }).bind(this);

        return $return(new Currency(
            {
                isNegative: !this.isNegative,
                units: this.units,
                precision: this.precision
            },
            this._currencyOptions
        ));
    }

    /**
     * Divides this currency by a number.
     *
     * @param divisor The number by which to divide this currency.
     * @returns The quotient and remainder of the division as separate Currency instances.
     * @example
     * new Currency(3.50).divide(3); // returns { quotient: new Currency(1.16), remainder: new Currency(0.02) }
     */
    divide(divisor: number): { quotient: Currency, remainder: Currency } {
        const $return = ((quotientResult: Currency, remainderResult: Currency): { quotient: Currency, remainder: Currency } => {
            console.debug(`${this} / ${divisor} = ${quotientResult} r${remainderResult}`);
            return {
                quotient: quotientResult,
                remainder: remainderResult
            };
        }).bind(this);

        if (this.isZero) {
            // 0 / n = 0 (zero property)
            return $return(this, this);
        }
        else if (divisor === 1) {
            // n / 1 = n (identity property)
            return $return(this, Currency.createZeroCurrency(this._currencyOptions));
        }

        const divisorParts = Currency.getCurrencyParts(divisor, this.precision);

        const dividend = this.units;

        let dividendIndex: number = -1;
        let dividendPart: string = "";
        const quotientParts: string[] = [];

        while (++dividendIndex < dividend.length) {
            dividendPart += dividend[dividendIndex];
            const currentDividend = +dividendPart;
            // Truncate the fractional bits. (~~ is the same as floor for positive numbers and ceiling for negative numbers)
            const currentQuotient = ~~(currentDividend / divisor);
            quotientParts.push(currentQuotient.toString());

            if (currentQuotient !== 0) {
                const currentRemainder = currentDividend - (currentQuotient * divisor);
                dividendPart = currentRemainder.toString();
            }

            if (dividendPart === "0") {
                dividendPart = "";
            }
        }

        // The result is negative if only the dividend or the divisor is negative, but not both.
        const isNegative = this.isNegative ? !divisorParts.isNegative : divisorParts.isNegative;
        const quotient: Currency = new Currency(
            {
                isNegative,
                units: quotientParts.join(""),
                precision: this.precision
            },
            this._currencyOptions
        );

        // The leftover dividend is the remainder.
        // Prepend zeroes to convert the remainder to a major and minor amount without the decimal point.
        if (dividendPart.length < (this.precision + 1)) {
            // TODO JMH These prepend/append methods should take into account that they are working with the major + minor units.
            dividendPart = Currency.prependPaddedZeroesOrTruncateStart(dividendPart, this.precision + 1);
        }
        const remainder: Currency = new Currency(
            {
                isNegative,
                units: dividendPart,
                precision: this.precision
            },
            this._currencyOptions
        );

        return $return(quotient, remainder);
    }

    /**
     * Subtracts an amount from this currency.
     *
     * @param currency The amount to subtract from this currency. Fractional amounts that exceed this currency's precision will be truncated; e.g., if this currency has a precision of two ($31.45), and the amount being subtracted has a precision of three ($2.289), the "9" will be truncated to $2.28.
     * @returns A new Currency instance containing the difference of the two currencies.
     * @example
     * new Currency(2.61).subtract(3.999);               // returns new Currency(-1.38)
     * new Currency(2.61).subtract(new Currency(3.999)); // returns new Currency(-1.38)
     */
    subtract(currency: number | Currency): Currency {
        const $return = ((result: Currency): Currency => {
            console.debug(`${this} - ${currency} = ${result}`);
            return result;
        }).bind(this);

        if (this.isZero) {
            // 0 - n = -n (identity property; 0 + -n = -n)
            return $return(Currency.negate(currency));
        }
        else if (Currency.isCurrencyZero(currency)) {
            // n - 0 = n (identity property).
            return $return(this);
        }

        const { isNegative: otherIsNegative, units: otherUnits } = Currency.getCurrencyParts(currency, this.precision);

        if (this.isNegative !== otherIsNegative) {
            // The currencies have different signs.
            if (otherIsNegative) {
                // n - (-m) = n + m
                const units = Currency.addUnits(this.units, otherUnits);
                return $return(new Currency(
                    {
                        isNegative: false,
                        units,
                        precision: this.precision
                    },
                    this._currencyOptions
                ));
            }
            else {
                // -n - m = -1 * (n + m)
                const units = Currency.addUnits(this.units, otherUnits);
                return $return(new Currency(
                    {
                        isNegative: true,
                        units,
                        precision: this.precision
                    },
                    this._currencyOptions
                ));
            }
        }
        else {
            // The currencies have the same sign.
            if (this.units === otherUnits) {
                // n - n = 0 (zero identity when both positive)
                // -n - (-n) = -n + n = 0 (zero identity when both negative)
                return $return(Currency.createZeroCurrency(this._currencyOptions));
            }
            else if (this.isNegative) {
                // -n - (-m) = -n + m = -1 * (n - m)
                const { isNegative, units } = Currency.substractUnits(this.units, otherUnits);
                return $return(new Currency({
                        isNegative: !isNegative, // Flip the sign in this case.
                        units,
                        precision: this.precision
                    },
                    this._currencyOptions
                ));
            }
            else {
                // n - m
                const { isNegative, units } = Currency.substractUnits(this.units, otherUnits);
                return $return(new Currency({
                        isNegative,
                        units,
                        precision: this.precision
                    },
                    this._currencyOptions
                ));
            }
        }
    }

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
     * new Currency(-3467.56).format(); // default format "-$3,467.56"
     * new Currency(-3467.56).format("-#.%"); // omit the symbol and ignore grouping "-3467.56"
     * new Currency(-12345678.90).format("-₹#,##,###.%") // The indic numbering system groups the first thousand, then every ten digits after "-₹1,23,45,678.90"
     * new Currency(-12345678.90).format("!#") // absolute value integer with currency symbol "$12345678"
     * new Currency(12.50).format("-!#,###.% @"); // default format with currency code "$12.50 USD"
     */
    format(formatString: string = CurrencyFormatString.default): string {
        const $return = ((formatString: string) => (result: string): string => {
            //console.debug(`"${this.isNegative ? "-" : ""}${this.units}".format("${formatString}") => "${result}"`);
            return result;
        })(formatString);

        type MajorUnitGroup = {
            count: number;
            separator: string;
        };

        // Create the group strategy.
        function formatMajorUnits(number: string, groups: MajorUnitGroup[]): string {
            if (groups.length === 0) {
                // No need to format the number if there are no groups.
                return number;
            }

            const groupsCopy: MajorUnitGroup[] = [...groups];
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            const repeatGroup: MajorUnitGroup = groupsCopy.pop()!;

            function getNextGroup(): MajorUnitGroup {
                return groupsCopy.pop() ?? repeatGroup;
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

        const minorUnitsPlaceholder = "MINOR_UNITS" as const;
        const majorUnitsPlaceholder = "MAJOR_UNITS" as const;

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

        return $return(formatString
            .replace("-", this.isNegative ? "-" : "")
            .replace("!", this.symbol)
            .replace(majorUnitsPlaceholder, formatMajorUnits(this.majorUnits, groups))
            .replace(minorUnitsPlaceholder, this.minorUnits)
            .replace("@", this.code));
    }

    /**
     * Determines if this currency is equal another currency.
     *
     * @param currency The currency to which to compare.
     * @returns `true` if the currencies are equal; otherwise, `false` is returned.
     */
    isEqualTo(currency: Currency | number): boolean {
        const $return = ((result: boolean): boolean => {
            console.debug(`${this} == ${currency} = ${result}`);
            return result;
        }).bind(this);

        const isZero = Currency.isCurrencyZero(currency);
        if (this.isZero || isZero) {
            // Both currencies must be 0.
            return $return(this.isZero && isZero);
        }

        const currencyParts = Currency.getCurrencyParts(currency, this.precision);

        return $return(currencyParts.isNegative === this.isNegative
            && currencyParts.units === this.units);
    }

    /**
     * Determines if this currency is not equal to another currency.
     *
     * @param currency The currency to which to compare.
     * @returns `true` if the currencies are not equal; otherwise, `false` is returned.
     */
    isNotEqualTo(currency: Currency | number): boolean {
        const $return = ((result: boolean): boolean => {
            console.debug(`${this} != ${currency} = ${result}`);
            return result;
        }).bind(this);

        const isZero = Currency.isCurrencyZero(currency);
        if (this.isZero || isZero) {
            // Both currencies must not be 0.
            return $return(!(this.isZero && isZero));
        }

        const currencyParts = Currency.getCurrencyParts(currency, this.precision);

        return $return(currencyParts.isNegative !== this.isNegative
            || currencyParts.units !== this.units);
    }

    /**
     * Determines if this currency is less than to another currency.
     *
     * @param currency The currency to which to compare.
     * @returns `true` if this currency is less than the provided currency; otherwise, `false` is returned.
     */
    isLessThan(currency: Currency | number): boolean {
        const $return = ((result: boolean): boolean => {
            console.debug(`${this} < ${currency} = ${result}`);
            return result;
        }).bind(this);

        if (this.isZero && Currency.isCurrencyZero(currency)) {
            // 0 < 0 (false)
            return $return(false);
        }

        const currencyParts = Currency.getCurrencyParts(currency, this.precision);

        if (this.isNegative && !currencyParts.isNegative) {
            // -n < m? Always. A negative number is always less than a positive number.
            return $return(true);
        }
        else if (!this.isNegative && currencyParts.isNegative) {
            // n < -m? Never. A positive number is never less than a negative number.
            return $return(false);
        }

        const unitsLengthDiff = this.units.length - currencyParts.units.length;
        if (unitsLengthDiff < 0) {
            // Return true if the length of this currency's units is smaller than the other units' length.
            return $return(true);
        }
        else if (unitsLengthDiff > 0) {
            // Return false if the length of this currency's units is larger than the other units' length.
            return $return(false);
        }
        else {
            // The units have the same string length.
            // Compare them by value.
            return this.units.localeCompare(currencyParts.units) < 0;
        }
    }

    /** Gets the absolute value of this currency. */
    abs(): Currency {
        const $return = (result: Currency): Currency => {
            console.debug(`|${this}| => ${result}`);
            return result;
        };

        if (!this.isNegative) {
            return $return(this);
        }
        else {
            return $return(new Currency(
                {
                    isNegative: false,
                    precision: this.precision,
                    units: this.units
                },
                this._currencyOptions
            ));
        }
    }

    /**
     * Returns the remainder after dividing this currency by a number.
     */
    mod(divisor: number): Currency {
        const $return = ((result: Currency): Currency => {
            console.debug(`${this} % ${divisor} = ${result}`);
            return result;
        }).bind(this);

        if (this.isZero) {
            // 0 % n = 0 (zero property)
            return $return(this);
        }

        if (Currency.isCurrencyZero(divisor)) {
            // n % 0 = n (identity property)
            return $return(this);
        }

        if (this.abs().isLessThan(divisor)) {
            if (!this.isLessThan(0)) {
                // n % m = n, where |n| < m and n > 0
                return $return(this);
            }
            else {
                // n % m = n + m, where |n| < m and n < 0
                return $return(this.add(divisor));
            }
        }

        // Calculate the remainder by division.
        const { remainder } = this.divide(divisor);
        return $return(remainder);
    }

    /**
     * Gets the formatted string value of this currency.
     */
    toString = (): string => {
        // Cache the formatted value of this immutable currency.
        if (this._toString === null) {
            this._toString = this.format(this._currencyOptions.formatString);
        }

        return this._toString;
    };

    // #endregion Public Methods

    // #region Private Static Methods

    private static isCurrencyZero(currency: Currency | number): boolean {
        return (typeof currency === "number" && currency === 0) || (typeof currency !== "number" && currency.isZero);
    }

    private static negate(currency: Currency | number, options?: Partial<CurrencyOptions>): Currency {
        if (typeof currency === "number") {
            return new Currency(
                currency * -1,
                options
            );
        }
        else {
            return currency.negate();
        }
    }

    private static createZeroCurrency(options?: Partial<CurrencyOptions>): Currency {
        return new Currency(0, options);
    }

    /** This is intended when the minor portion of the units is already the correct precision. */
    private static prependPaddedZeroesOrTruncateStart(units: string, length: number): string {
        if (units.length === length) {
            return units;
        }
        else if (units.length < length) {
            // Prepend 0s until the same length is met.
            return "0".repeat(length - units.length) + units;
        }
        else {
            // Truncate if the units are larger than the allowed length.
            return units.substring(units.length - length);
        }
    }

    /** This is intended for fixing the precision of the minor units. */
    private static appendPaddedZeroesOrTruncateEnd(units: string, length: number): string {
        if (units.length === length) {
            return units;
        }
        else if (units.length < length) {
            // Append 0s until the same length is met.
            return units + "0".repeat(length - units.length);
        }
        else {
            // Truncate if the units are larger than the allowed length.
            return units.substring(0, length);
        }
    }

    private static getIntegerUnits(majorUnits: string, minorUnits: string, targetPrecision: number): string {
        const $return = (value: string): string => {
            //console.debug(`getIntegerUnits("${majorUnits}", "${minorUnits}", ${precision}) => "${value}"`);
            return value;
        };
        // Trim leading zeroes...
        majorUnits = majorUnits.replace(/^0+/, "");
        // ...and ensure majorUnits is not an empty string.
        if (majorUnits === "") {
            majorUnits = "0";
        }
        // TODO JMH Should we round? I think we should round.
        return $return(`${majorUnits}${Currency.appendPaddedZeroesOrTruncateEnd(minorUnits, targetPrecision)}`);
    }

    private static getCurrencyParts(currency: number | Currency | CurrencyParts, targetPrecision: number): CurrencyParts {
        if (typeof currency !== "number") {
            // Ensure the precision is the same as the supplied value using truncation.
            const precisionDiff = targetPrecision - currency.precision;
            if (precisionDiff !== 0) {
                return {
                    isNegative: currency.isNegative,
                    units: Currency.appendPaddedZeroesOrTruncateEnd(currency.units, currency.units.length + precisionDiff),// "00", precision=2, currency.units="000", currency.precision = 2, precisionDiff=0
                    precision: targetPrecision,
                };
            }
            else {
                return {
                    isNegative: currency.isNegative,
                    units: currency.units,
                    precision: targetPrecision
                };
            }
        }

        // Per, https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number/toString#description...
        // If the number is not a whole number, the decimal point . is used to separate the decimal places.
        // Therefore, it is safe to split number.toString() by "." to get the whole and fractional parts of a number,
        // assuming the number is not NaN, Infinity, NegativeInfinity, or scientific notation.
        if (!Number.isFinite(currency)) {
            throw "The currency must be a finite number.";
        }

        let currencyAsString = currency.toString();

        if (currencyAsString.includes("e")) {
            // Convert scientific notation to decimal number.
            currencyAsString = Currency.eToNumber(currencyAsString);
        }

        // It is safe to split the decimal number by "." here regardless of the locale.
        const parts = currencyAsString.split(".");

        let [majorUnits] = parts;
        const [_, minorUnits] = parts;
        const isNegative: boolean = majorUnits.charAt(0) === "-";

        // Remove the sign.
        majorUnits = isNegative ? majorUnits.substring(1) : majorUnits;

        const units = Currency.getIntegerUnits(majorUnits ?? "0", minorUnits ?? "0", targetPrecision);
        return {
            isNegative,
            units,
            precision: targetPrecision
        };
    }

    /**
     * Subtracts units2 from units1.
     *
     * The string lengths must be the same; the smaller string should have zeroes prepended or appended depending on whether they are whole or fractional numbers.
     *
     * The resulting subtraction array will have the same length as the input arguments,
     * and each element will be a single number character including leading and trailing zeroes.
     */
    private static substractUnits(units1: string, units2: string): { units: string; isNegative: boolean; } {
        if (units1.length > units2.length) {
            units2 = Currency.prependPaddedZeroesOrTruncateStart(units2, units1.length);
        }
        else if (units1.length < units2.length) {
            units1 = Currency.prependPaddedZeroesOrTruncateStart(units1, units2.length);
        }

        if (units1 === units2) {
            // The units are the same value so return zeroes.
            return { units: "0".repeat(units1.length), isNegative: false };
        }

        let isNegative: boolean = false;

        // Make sure the bigger and smaller unit variables are accurately assigned.
        // When subtracting, working from left-to-right, the bigger number should always be >= the smaller number at the same index.
        let units1Array: number[] = Array<number>(units1.length);
        let units2Array: number[] = Array<number>(units1.length);

        let checkForNegative = true;
        for (let i = 0; i < units1.length; i++) {
            // Convert the strings into a number array.
            units1Array[i] = +units1[i];
            units2Array[i] = +units2[i];

            if (checkForNegative) {
                if (units1Array[i] < units2Array[i]) {
                    // The second number is bigger than the first number,
                    // so the subtraction will result in a negative number.
                    isNegative = true;
                    checkForNegative = false;
                }
                else if (units1Array[i] !== units2Array[i]) {
                    checkForNegative = false;
                }
            }
        }

        // Swap the array pointers so the smaller number is subtracted from the larger number.
        // The negation will be applied at the end.
        if (isNegative) {
            const temp = units1Array;
            units1Array = units2Array;
            units2Array = temp;
        }

        const units: string[] = [];

        for (let i = units1Array.length - 1; i >= 0; i--) {
            let unit1: number = units1Array[i];
            const unit2: number = units2Array[i];

            // Borrow if needed.
            if (unit1 < unit2) {
                // Keep borrowing from the left until no more borrowing is needed.
                let j = i - 1;
                while (j >= 0) {
                    if (units1Array[j] > 0) {
                        // The current number can be borrowed from,
                        // so stop borrowing from the left.
                        unit1 += 10;
                        units1Array[j] = units1Array[j] - 1;
                        break;
                    }
                    else {
                        if (j !== 0) {
                            // The number we're trying to borrow from is 0.
                            // Make it 9 and keep borrowing from the left.
                            units1Array[j] = 9;
                            j--;
                        }
                        else {
                            // There are no more numbers to the left to borrow from.
                            // This shouldn't be able to happen unless there is an error in this or a calling method,
                            // but throw an error just in case.
                            throw `An unexpected error occurred while substracting ${units1} from ${units2}. Failed at index ${i} subtracting ${units2Array[i]} from ${units1Array[i]}.`;
                        }
                    }
                }
            }

            // Perform the subtraction and store the result.
            const difference: number = unit1 - unit2;
            units.unshift(difference.toString());
        }

        return {
            units: units.join(""),
            isNegative
        };
    }

    // TODO JMH Do we ever use `isFirstElementCarriedOver`?
    private static addUnits(units1: string, units2: string): string {
        if (units1.length > units2.length) {
            units2 = Currency.prependPaddedZeroesOrTruncateStart(units2, units1.length);
        }
        else if (units1.length < units2.length) {
            units1 = Currency.prependPaddedZeroesOrTruncateStart(units1, units2.length);
        }

        const units: string[] = [];
        let isCarriedOver: boolean = false;

        for (let i = units1.length - 1; i >= 0; i--) {
            const unit1: number = +units1[i];
            const unit2: number = +units2[i];
            const sum: number = unit1 + unit2 + (isCarriedOver ? 1 : 0);

            if (sum <= 9) {
                isCarriedOver = false;
                units.unshift(sum.toString());
            }
            else {
                const sumParts = sum.toString().split("");
                if (i > 0) {
                    isCarriedOver = true;
                    units.unshift(sumParts[1]);
                }
                else {
                    // This is the last number,
                    // so no need to carry again.
                    // Just insert the sum into the front of the array.
                    units.unshift(...sumParts);
                }
            }
        }

        return units.join("");
    }

    /** Converts scientific e-notication numbers to a decimal string. */
    private static eToNumber(numberOrString: number | string): string {
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
            // TODO JMH Do I need this? Return "0" (without a sign) if the final number is zero.
            return "0";
        }

        return sign + decimalNumberString;
    }

    // #endregion Private Static Methods
}