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
    majorUnits: string;
    minorUnits: string;
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

    // #endregion Fields

    // #region Properties

    /** Returns `true` if this currency is negative; otherwise, `false` is returned. */
    get isNegative(): boolean {
        return this._currencyParts.isNegative;
    }

    /** Gets the major units (the number of units before the decimal). */
    get majorUnits(): string {
        return this._currencyParts.majorUnits;
    }

    /** Gets the minor units (the number of units after the decimal). */
    get minorUnits(): string {
        return this._currencyParts.minorUnits;
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

        if (typeof currency === "number") {
            // Get the currency parts from the number.
            const { isNegative, majorUnits, minorUnits } = Currency.getCurrencyParts(currency);

            // Set the currency parts as readonly and const so they cannot be modified once set.
            this._currencyParts = {
                isNegative,
                majorUnits,
                // Enforce the precision here.
                minorUnits: Currency.appendPaddedZeroesOrTruncateEnd(minorUnits, this.precision)
            } as const;

            // Set whether or not this currency is equal to zero.
            this._isZero = currency === 0;
        }
        else {
            // Set the currency parts as readonly and const so they cannot be modified once set.
            this._currencyParts = {
                isNegative: currency.isNegative,
                majorUnits: currency.majorUnits,
                minorUnits: Currency.appendPaddedZeroesOrTruncateEnd(currency.minorUnits, this.precision)
            } as const;

            // Set whether or not this currency is equal to zero.
            const zeroRegex = /^0*$/;
            this._isZero = zeroRegex.test(currency.majorUnits) && zeroRegex.test(currency.minorUnits);
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
        const $return = ((currency: Currency): Currency => {
            console.debug(`${this} + ${currency} = ${currency}`);
            return currency;
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

        const currencyParts = Currency.getCurrencyParts(currency);
        const { isNegative, majorUnits } = currencyParts;
        let { minorUnits } = currencyParts;

        // Ensure the minor units match this Currency's decimal precision.
        minorUnits = Currency.appendPaddedZeroesOrTruncateEnd(minorUnits, this.precision);

        if (this.isNegative === isNegative) {
            // TODO JMH Concat the major and minor units, then do the addition. (no need to add major and minor separately)
            // Add the currencies if they have the same sign (+/-).
            let newMajorUnits = Currency.addMajorUnits(this.majorUnits, majorUnits);
            const { majorUnits: extraMajorUnits, minorUnits: newMinorUnits } = Currency.addMinorUnits(this.minorUnits, minorUnits);
            if (extraMajorUnits) {
                newMajorUnits = Currency.addMajorUnits(newMajorUnits, extraMajorUnits);
            }

            // Return the new Currency object.
            return new Currency(
                {
                    isNegative: this._currencyParts.isNegative,
                    majorUnits: newMajorUnits,
                    minorUnits: newMinorUnits,
                },
                this._currencyOptions
            );
        }
        else {
            // Adding currencies with a different sign (+/-) is the
            // same as subtracting one from the other.

            if (this.majorUnits === majorUnits && this.minorUnits === minorUnits) {
                // Adding n and -n is zero (additive inverse).
                return $return(Currency.createZeroCurrency(this._currencyOptions));
            }

            let majorUnitsResult: { isNegative: boolean; majorUnits: string; };
            let minorUnitsResult: { isNegative: boolean; minorUnits: string; };

            // TODO JMH Subtraction should apply to concatenated units. (no need to do two subtractions)
            if (!this.isNegative) {
                // Subtract the numbers.
                majorUnitsResult = Currency.subtractMajorUnits(this.majorUnits, majorUnits);
                minorUnitsResult = Currency.subtractMinorUnits(this.minorUnits, minorUnits);
            }
            else {
                majorUnitsResult = Currency.subtractMajorUnits(majorUnits, this.majorUnits);
                minorUnitsResult = Currency.subtractMinorUnits(minorUnits, this.minorUnits);
            }

            const { isNegative: isNegativeMajorUnits } = majorUnitsResult;
            let { majorUnits: newMajorUnits } = majorUnitsResult;
            const { isNegative: isNegativeMinorUnits } = minorUnitsResult;
            let { minorUnits: newMinorUnits } = minorUnitsResult;

            if (newMajorUnits !== "0".repeat(newMajorUnits.length) && isNegativeMajorUnits !== isNegativeMinorUnits) {
                // Major units need to be reduced by 1.
                const { majorUnits: adjustedWholeDigits } = Currency.subtractMajorUnits(newMajorUnits, Currency.prependPaddedZeroesOrTruncateStart("1", newMajorUnits.length));
                newMajorUnits = adjustedWholeDigits;
                // Minor units need to be inversed; e.g., if minor units were "999", then it needs to be "1000" - "0999" = "001".
                const { minorUnits: adjustedFractionalDigits } = Currency.subtractMinorUnits(Currency.appendPaddedZeroesOrTruncateEnd("1", newMinorUnits.length + 1), Currency.prependPaddedZeroesOrTruncateStart(newMinorUnits, newMinorUnits.length + 1));
                newMinorUnits = adjustedFractionalDigits;
            }

            return $return(new Currency({
                    isNegative: isNegativeMajorUnits || isNegativeMinorUnits,
                    majorUnits: newMajorUnits,
                    minorUnits: newMinorUnits,
                },
                this._currencyOptions
            ));
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
        const $return = ((currency: Currency): Currency => {
            console.debug(`-${this} = ${currency}`);
            return currency;
        }).bind(this);

        return $return(new Currency(
            {
                isNegative: !this.isNegative,
                majorUnits: this.majorUnits,
                minorUnits: this.minorUnits
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
        const $return = ((quotient: Currency, remainder: Currency): { quotient: Currency, remainder: Currency } => {
            console.debug(`${this} / ${divisor} = ${quotient} r${remainder}`);
            return {
                quotient,
                remainder
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

        // TODO JMH This should be a private field of this class and arithmetic operations should be performed on it.
        // Temporarily join the major and minor bits together. The decimal will be added at the end.
        const currencyParts = Currency.getCurrencyParts(divisor);

        const dividend = `${this.majorUnits}${this.minorUnits}${"0".repeat(currencyParts.minorUnits.length)}`;

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
        const isNegative = this.isNegative ? !currencyParts.isNegative : currencyParts.isNegative;
        const quotient: Currency = new Currency(
            {
                isNegative,
                majorUnits: quotientParts.slice(0, quotientParts.length - this.precision).join(""),
                minorUnits: quotientParts.slice(quotientParts.length - this.precision).join("")
            },
            this._currencyOptions
        );

        // The leftover dividend is the remainder.
        // Prepend zeroes to convert the remainder to a major and minor amount without the decimal point.
        if (dividendPart.length < (this.precision + 1)) {
            dividendPart = Currency.prependPaddedZeroesOrTruncateStart(dividendPart, this.precision + 1);
        }
        const remainder: Currency = new Currency(
            {
                isNegative,
                majorUnits: dividendPart.slice(0, dividendPart.length - this.precision) || "0",
                minorUnits: dividendPart.slice(dividendPart.length - this.precision) || "0"
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
        const $return = ((currency: Currency): Currency => {
            console.debug(`${this} - ${currency} = ${currency}`);
            return currency;
        }).bind(this);

        if (this.isZero) {
            // 0 - n = -n (identity property; 0 + -n = -n)
            return $return(Currency.negate(currency));
        }
        else if (Currency.isCurrencyZero(currency)) {
            // n - 0 = n (identity property).
            return $return(this);
        }

        const currencyParts = Currency.getCurrencyParts(currency);
        const { isNegative, majorUnits } = currencyParts;
        let { minorUnits } = currencyParts;

        // Ensure the minor units of the number matches this Currency's decimal precision.
        minorUnits = Currency.appendPaddedZeroesOrTruncateEnd(minorUnits, this.precision);

        if (this.isNegative !== isNegative) {
            // Subtracting one currency from another with a different sign (+/-),
            // is the same as adding them together and keeping the sign from the first currency.
            // n - (-m) = n + m
            let newMajorUnits = Currency.addMajorUnits(this.majorUnits, majorUnits);
            const { majorUnits: extraMajorUnits, minorUnits: newMinorUnits } = Currency.addMinorUnits(this.minorUnits, minorUnits);
            if (extraMajorUnits) {
                newMajorUnits = Currency.addMajorUnits(newMajorUnits, extraMajorUnits);
            }

            return $return(new Currency({
                    isNegative: this.isNegative, // The result always has the same sign this currency.
                    majorUnits: newMajorUnits,
                    minorUnits: newMinorUnits,
                },
                this._currencyOptions
            ));
        }
        else {
            // The currrencies have the same sign (+/-).

            if (this.majorUnits === majorUnits && this.minorUnits === minorUnits) {
                // n - n = 0 (zero identity)
                return $return(Currency.createZeroCurrency(this._currencyOptions));
            }

            // Subtract the currencies.
            // TODO JMH This should be a private readonly field.
            const combinedUnits1 = Currency.joinUnits(this.majorUnits, this.minorUnits);
            const combinedUnits2 = Currency.prependPaddedZeroesOrTruncateStart(`${majorUnits}${minorUnits}`, combinedUnits1.length);
            const { units, isNegative } = Currency.substractUnits(combinedUnits1, combinedUnits2);
            const { majorUnits: newMajorUnits, minorUnits: newMinorUnits } = Currency.splitUnits(units, this.precision);

            return $return(new Currency({
                majorUnits: newMajorUnits.join(""),
                minorUnits: newMinorUnits.join(""),
                isNegative: this.isNegative ? !isNegative : isNegative
            }, this._currencyOptions));
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
        type MajorUnitGroup = {
            count: number;
            separator: string;
        };

        function getGroupSeparator(units: string, startFromIndex: number): { group: MajorUnitGroup | null, nextIndex: number } {
            let groupSeparatorStart: number = -1;
            let groupSeparatorEnd: number = -1;

            for (let i = startFromIndex; i < units.length; i++) {
                if (units[i] !== "#") {
                    if (groupSeparatorStart === -1) {
                        groupSeparatorStart = i;
                        groupSeparatorEnd = i + 1;
                    }
                }
                else {
                    if (groupSeparatorStart !== -1) {
                        if (groupSeparatorEnd === -1) {
                            groupSeparatorEnd = i;
                        }
                        else {
                            break;
                        }
                    }
                }
            }

            const count = groupSeparatorEnd - groupSeparatorStart;

            return {
                group: count === 0 ? null : {
                    count: count,
                    separator: units.substring(groupSeparatorStart, groupSeparatorEnd)
                },
                nextIndex: groupSeparatorEnd,
            };
        }

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

                if (currentGroupCount === currentGroup.count) {
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

        // Get the minor units format section.
        const minorUnitsStart = formatString.indexOf("%");
        const minorUnitsEnd = formatString.lastIndexOf("%");
        if (minorUnitsStart !== -1 && minorUnitsEnd !== -1) {
            formatString = formatString.slice(0, minorUnitsStart) + "MINOR_UNITS" + formatString.slice(minorUnitsEnd + 1);
        }

        // Get the major units format section.
        const majorUnitsStart = formatString.indexOf("#");
        const majorUnitsEnd = formatString.lastIndexOf("#");
        const groups: { count: number, separator: string }[] = [];
        if (majorUnitsStart !== -1 && majorUnitsEnd !== -1) {
            const majorUnitsFormatSection = formatString.substring(majorUnitsStart, majorUnitsEnd + 1);
            formatString = formatString.slice(0, majorUnitsStart) + "MAJOR_UNITS" + formatString.slice(majorUnitsEnd + 1);

            // Figure out the group strategy.
            // Starting from the group separator, get the number of characters per group.
            // The first group is the one that will repeat.
            let { group, nextIndex } = getGroupSeparator(majorUnitsFormatSection, 0);
            while (group !== null) {
                groups.unshift(group);
                { group, nextIndex } = getGroupSeparator(majorUnitsFormatSection, nextIndex);
            }
            // TODO JMH REmove this.
            // if (groupSeparatorEnd !== -1) {
            //     let currentGroupCount: number = 0;
            //     let nextGroupSeparator: string = groupSeparator;

            //     for (let i = groupSeparatorStart + 1; i < majorUnitsFormatSection.length; i++) {
            //         if (majorUnitsFormatSection[i] === "#") {
            //             currentGroupCount++;
            //         }
            //         else {
            //             groups.unshift({ count: currentGroupCount, separator: nextGroupSeparator });

            //             // Reset the group count and separator.
            //             nextGroupSeparator = majorUnitsFormatSection[i];
            //             currentGroupCount = 0;
            //         }
            //     }

            //     // Add the group.
            //     if (currentGroupCount > 0) {
            //         groups.unshift({ count: currentGroupCount, separator: nextGroupSeparator });
            //     }
            // }
        }

        return formatString
            .replace("-", this.isNegative ? "-" : "")
            .replace("!", this.symbol)
            .replace("MAJOR_UNITS", formatMajorUnits(this.majorUnits, groups))
            .replace("MINOR_UNITS", this.minorUnits)
            .replace("@", this.code);
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

        const currencyParts = Currency.getCurrencyParts(currency);

        return $return(currencyParts.isNegative === this.isNegative
            && currencyParts.majorUnits === this.majorUnits
            && currencyParts.minorUnits === this.minorUnits);
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

        const currencyParts = Currency.getCurrencyParts(currency);

        return $return(currencyParts.isNegative !== this.isNegative
            || currencyParts.majorUnits !== this.majorUnits
            || currencyParts.minorUnits !== this.minorUnits);
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

        const currencyParts = Currency.getCurrencyParts(currency);

        if (this.isNegative && !currencyParts.isNegative) {
            // -n < m? Always. A negative number is always less than a positive number.
            return $return(true);
        }
        else if (!this.isNegative && currencyParts.isNegative) {
            // n < -m? Never. A positive number is never less than a negative number.
            return $return(false);
        }

        const majorUnitsLengthDiff = this.majorUnits.length - currencyParts.majorUnits.length;
        if (majorUnitsLengthDiff < 0) {
            // Return true if the length of this currency's major units is smaller than the other major units' length.
            return $return(true);
        }
        else if (majorUnitsLengthDiff > 0) {
            // Return false if the length of this currency's major units is larger than the other major units' length.
            return $return(false);
        }
        else {
            // The major units have the same string length.
            // Compare them by value.

            const comparison = this.majorUnits.localeCompare(currencyParts.majorUnits);
            if (comparison < 0) {
                // Return true if the major units are smaller than the other major units.
                return $return(true);
            }
            else if (comparison > 0) {
                // Return false if the major units are larger than the other major units.
                return $return(false);
            }
            else {
                // TODO JMH This comparison should be done on the combined units.
                // The major units are the same.
                // Compare the minor units.
                // The lengths cannot be used to short-circuit the comparison of minor units
                // so zero-pad accordingly.
                // e.g.,
                // .1 < .1001? yes and .1 has shorter length
                // .3 < .1001? no and .3 has shorter length

                const minorUnitsLengthDiff = this.minorUnits.length - currencyParts.minorUnits.length;
                if (minorUnitsLengthDiff < 0) {
                    // Zero-pad this currency's minor units.
                    return Currency.appendPaddedZeroesOrTruncateEnd(this.minorUnits, currencyParts.minorUnits.length).localeCompare(currencyParts.minorUnits) < 0;
                }
                else if (minorUnitsLengthDiff > 0) {
                    // Zero-pad the other currency's minor units.
                    return $return(this.minorUnits.localeCompare(Currency.appendPaddedZeroesOrTruncateEnd(currencyParts.minorUnits, this.minorUnits.length)) < 0);
                }
                else {
                    // The minor units have the same string length.
                    // Compare them without padding.
                    return $return(this.minorUnits.localeCompare(currencyParts.minorUnits) < 0);
                }
            }
        }
    }

    /**
     * Returns the remainder after dividing this currency by a number.
     */
    mod(divisor: number): Currency {
        const $return = ((currency: Currency): Currency => {
            console.debug(`${this} % ${divisor} = ${currency}`);
            return currency;
        }).bind(this);

        if (this.isZero) {
            // 0 % n = 0 (zero property)
            return $return(this);
        }

        if (Currency.isCurrencyZero(divisor)) {
            // n % 0 = n (identity property)
            return $return(this);
        }

        if (this.isLessThan(divisor)) {
            if (!this.isLessThan(0)) {
                // n % m = n, where 0 <= n < m
                return $return(this);
            }
            else {
                // n % m = n + m, where n < 0 < m
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

    private static splitUnits(units: string[] | string, precision: number): { majorUnits: string[], minorUnits: string[] } {
        if (typeof units === "string") {
            units = units.split("");
        }

        return {
            majorUnits: units.slice(0, units.length - precision),
            minorUnits: units.slice(precision)
        };
    }

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

    private static subtractMajorUnits(majorUnits1: string, majorUnits2: string): { isNegative: boolean; majorUnits: string; } {
        // Zero-pad the units so they are the same size.
        if (majorUnits1.length > majorUnits2.length) {
            majorUnits2 = Currency.prependPaddedZeroesOrTruncateStart(majorUnits2, majorUnits1.length);
        }
        else if (majorUnits1.length < majorUnits2.length) {
            majorUnits1 = Currency.prependPaddedZeroesOrTruncateStart(majorUnits1, majorUnits2.length);
        }

        const { isNegative, units } = Currency.substractUnits(majorUnits1, majorUnits2);
        return { isNegative, majorUnits: units.join("") };
    }

    private static subtractMinorUnits(minorUnits1: string, minorUnits2: string): { isNegative: boolean; minorUnits: string; } {
        // Zero-pad the units so they are the same size.
        if (minorUnits1.length > minorUnits2.length) {
            minorUnits2 = Currency.appendPaddedZeroesOrTruncateEnd(minorUnits2, minorUnits1.length);
        }
        else if (minorUnits1.length < minorUnits2.length) {
            minorUnits1 = Currency.appendPaddedZeroesOrTruncateEnd(minorUnits1, minorUnits2.length);
        }

        const { units, isNegative } = Currency.substractUnits(minorUnits1, minorUnits2);
        return { isNegative, minorUnits: units.join("") };
    }

    // TODO JMH This should go away once we have a concatenated readonly field per currency.
    private static joinUnits(majorUnits: string, minorUnits: string): string {
        return `${majorUnits}${minorUnits}`;
    }

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

    private static getCurrencyParts(currency: number | Currency): { isNegative: boolean, majorUnits: string, minorUnits: string } {
        if (typeof currency !== "number") {
            return {
                isNegative: currency.isNegative,
                minorUnits: currency.minorUnits,
                majorUnits: currency.majorUnits
            };
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

        return {
            isNegative,
            majorUnits: majorUnits ?? "0",
            minorUnits: minorUnits ?? "0"
        };
    }

    private static addMajorUnits(majorUnits1: string, majorUnits2: string): string {
        // Zero-pad the units so they are the same size.
        if (majorUnits1.length > majorUnits2.length) {
            majorUnits2 = Currency.prependPaddedZeroesOrTruncateStart(majorUnits2, majorUnits1.length);
        }
        else if (majorUnits2.length > majorUnits1.length) {
            majorUnits1 = Currency.prependPaddedZeroesOrTruncateStart(majorUnits1, majorUnits2.length);
        }

        // TODO JMH Do we need addUnits to return an array or can we return the string?
        const { units } = Currency.addUnits(majorUnits1, majorUnits2);
        return units.join("");
    }

    /**
     * Subtracts units2 from units1.
     *
     * The string lengths must be the same; the smaller string should have zeroes prepended or appended depending on whether they are whole or fractional numbers.
     *
     * The resulting subtraction array will have the same length as the input arguments,
     * and each element will be a single number character including leading and trailing zeroes.
     */
    private static substractUnits(units1: string, units2: string): { units: string[]; isNegative: boolean; } {
        if (units1.length !== units2.length) {
            throw "The units must be the same size (the smaller string should have zeroes prepended or appended depending on whether they are major or minor units)";
        }

        if (units1 === units2) {
            // The units are the same value so return zeroes.
            return { units: [..."0".repeat(units1.length)], isNegative: false };
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
            units,
            isNegative
        };
    }

    // TODO JMH Do we ever use `isFirstElementCarriedOver`?
    private static addUnits(units1: string, units2: string): { units: string[]; isFirstElementCarriedOver: boolean; } {
        if (units1.length !== units2.length) {
            throw "The numbers must be the same size";
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
                    units.unshift(...sumParts);
                }
            }
        }

        return {
            units,
            isFirstElementCarriedOver: isCarriedOver
        };
    }

    private static addMinorUnits(minorUnits1: string, minorUnits2: string): { majorUnits: string; minorUnits: string; } {
        // Zero-pad the units so they are the same size.
        if (minorUnits1.length > minorUnits2.length) {
            minorUnits2 = Currency.appendPaddedZeroesOrTruncateEnd(minorUnits2, minorUnits1.length);
        }
        else if (minorUnits2.length > minorUnits1.length) {
            minorUnits1 = Currency.appendPaddedZeroesOrTruncateEnd(minorUnits1, minorUnits2.length);
        }

        const { units: summedUnits, isFirstElementCarriedOver } = Currency.addUnits(minorUnits1, minorUnits2);

        // If summing two minor units end in a carry,
        // then the first element represents a single major unit.
        // 0.999 + 0.990 = 1.989 => { units: [1,9,8,9], isFirstElementCarriedOver: true }
        if (isFirstElementCarriedOver) {
            // The first element represents major unit,
            // while the remaining elements are the minor units.
            const majorUnits = summedUnits[0];

            // Remove the major units to be left with the minor units.
            summedUnits.splice(0, 1);

            return { majorUnits, minorUnits: summedUnits.join("") };
        }
        else {
            return { majorUnits: "", minorUnits: summedUnits.join("") };
        }
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