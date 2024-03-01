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
    units: number;
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
    // eslint-disable-next-line @typescript-eslint/naming-convention
    _unitsString: string | null = null;

    // #endregion Fields

    // #region Properties

    get units(): number {
        return this._currencyParts.units;
    }

    get unitsString(): string {
        if (this._unitsString === null) {
            // 0 =>
            let parts = this.units.toString();
            if (parts.length < (this.precision + 1)) {
                parts = "0".repeat(this.precision + 1 - parts.length) + parts;
            }
            this._minorUnits = parts.substring(parts.length - this.precision);
            this._majorUnits = parts.substring(0, parts.length - this.precision);
            this._unitsString = `${this._majorUnits}${this._minorUnits}`;
        }
        return this._unitsString;
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
        return (this._minorUnits ?? (this._minorUnits = this.unitsString.slice(this.unitsString.length - this.precision)));
    }

    /** Gets the major units. */
    get majorUnits(): string {
        return (this._majorUnits ?? (this._majorUnits = this.unitsString.slice(0, this.unitsString.length - this.precision)));
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
            // Set whether or not this currency is equal to zero.
            this._isZero = currency === 0;
        }
        else {
            // Set whether or not this currency is equal to zero.
            this._isZero = currency.units === 0;
        }

        // Get the currency parts from the number.
        const { isNegative, units, precision } = Currency.getCurrencyParts(currency, this._currencyOptions.precision);

        // Set the currency parts as readonly and const so they cannot be modified once set.
        this._currencyParts = {
            isNegative: isNegative && !this.isZero,
            units: Math.abs(units),
            precision
        } as const;
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

        const thisUnits = this._currencyParts.units;
        const { isNegative: otherIsNegative, units: otherUnits } = Currency.getCurrencyParts(currency, this.precision);

        if (this.isNegative === otherIsNegative) {
            // Add the currencies if they have the same sign (+/-).
            // n + m

            // Return the new Currency object.
            const units = thisUnits + otherUnits;

            return $return(new Currency(
                {
                    isNegative: this.isNegative,
                    units: Math.abs(units),
                    precision: this.precision
                },
                this._currencyOptions
            ));
        }
        else {
            // Adding currencies with a different sign (+/-) is the
            // same as subtracting one from the other.

            if (thisUnits === otherUnits) {
                // n + -n = 0 (additive inverse)
                return $return(Currency.createZeroCurrency(this._currencyOptions));
            }

            if (!otherIsNegative) {
                // The other number is negative.
                // n + -m = n - m
                const { isNegative, units, precision } = Currency.getCurrencyParts(thisUnits - otherUnits, this.precision);
                return new Currency(
                    {
                        isNegative,
                        units: Math.abs(units),
                        precision
                    },
                    this._currencyOptions);
            }
            else {
                // This number is negative.
                // -n + m = -1 * (n - m)
                const { isNegative, units, precision } = Currency.getCurrencyParts(thisUnits - otherUnits, this.precision);
                return new Currency({
                    isNegative: !isNegative, // Flip the sign of the result in this case.
                    units: Math.abs(units),
                    precision
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
                units: Math.abs(this._currencyParts.units),
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

        // Let divide by zero cause an error for now.
        const quotientUnits = ~~(this._currencyParts.units / divisor);
        const remainderUnits = this._currencyParts.units - (quotientUnits * divisor);

        const isNegative = this.isNegative ? divisor >= 0 : divisor < 0;

        const quotient = new Currency(
            {
                isNegative,
                precision: this.precision,
                units: Math.abs(quotientUnits)
            },
            this._currencyOptions
        );

        const remainder = new Currency(
            {
                isNegative,
                precision: this.precision,
                units: Math.abs(remainderUnits)
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

        const thisUnits = this._currencyParts.units;
        const { isNegative: otherIsNegative, units: otherUnits } = Currency.getCurrencyParts(currency, this.precision);

        if (this.isNegative !== otherIsNegative) {
            // The currencies have different signs.
            if (otherIsNegative) {
                // n - (-m) = n + m
                const units = thisUnits + otherUnits;
                return $return(new Currency(
                    {
                        isNegative: false,
                        units: Math.abs(units),
                        precision: this.precision
                    },
                    this._currencyOptions
                ));
            }
            else {
                // -n - m = -1 * (n + m)
                const units = thisUnits + otherUnits;
                return $return(new Currency(
                    {
                        isNegative: true,
                        units: Math.abs(units),
                        precision: this.precision
                    },
                    this._currencyOptions
                ));
            }
        }
        else {
            // The currencies have the same sign.
            if (thisUnits === otherUnits) {
                // n - n = 0 (zero identity when both positive)
                // -n - (-n) = -n + n = 0 (zero identity when both negative)
                return $return(Currency.createZeroCurrency(this._currencyOptions));
            }
            else if (this.isNegative) {
                // -n - (-m) = -n + m = -1 * (n - m)
                const units = thisUnits - otherUnits;
                const isNegative = units < 0;
                return $return(new Currency({
                        isNegative: !isNegative, // Flip the sign in this case.
                        units: Math.abs(units),
                        precision: this.precision
                    },
                    this._currencyOptions
                ));
            }
            else {
                // n - m
                const units = thisUnits - otherUnits;
                const isNegative = units < 0;
                return $return(new Currency({
                        isNegative,
                        units: Math.abs(units),
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
            console.debug(`"${this.isNegative ? "-" : ""}${this.units}".format("${formatString}") => "${result}"`);
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
            && currencyParts.units === this._currencyParts.units);
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
            || currencyParts.units !== this._currencyParts.units);
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

        return this._currencyParts.units < currencyParts.units;
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
                    units: Math.abs(this.units)
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

    private static getCurrencyParts(currency: number | CurrencyParts, targetPrecision: number): CurrencyParts {
        const $return = ((currency, targetPrecision) => (result: CurrencyParts): CurrencyParts => {
            console.debug(`getCurrencyParts(${JSON.stringify(currency)}, ${targetPrecision}) => ${JSON.stringify(result)}`);
            return result;
        })(currency, targetPrecision);

        if (typeof currency !== "number") {
            // Ensure the precision is the same as the supplied value using truncation.
            const precisionDiff = targetPrecision - currency.precision;
            if (precisionDiff !== 0) {
                return $return({
                    isNegative: currency.isNegative,
                    units: Math.abs(~~(currency.units * (Math.pow(10, precisionDiff)))),
                    precision: targetPrecision,
                });
            }
            else {
                return $return({
                    isNegative: currency.isNegative,
                    units: Math.abs(currency.units),
                    precision: targetPrecision
                });
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

        const isNegative: boolean = parts[0]?.startsWith("-") ?? false;
        const majorUnits: string = parts[0]?.substring(isNegative ? 1 : 0) ?? "";
        let minorUnits: string = parts[1]?.substring(0, targetPrecision) ?? "";
        if (minorUnits.length < targetPrecision) {
            minorUnits = minorUnits + "0".repeat(targetPrecision - minorUnits.length);
        }
        const units: number = +(`${majorUnits}${minorUnits}`);
        return $return({
            isNegative,
            units: Math.abs(units),
            precision: targetPrecision
        });
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