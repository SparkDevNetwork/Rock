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

export type MoneyOptions = {
    decimals: number,
    symbol: string,
    thousandsSeparator: string,
    fractionalSeparator: string
};

export type MoneyNumber = {
    isNegative: boolean;
    wholeUnits: string;
    fractionalUnits: string;
};

export function createMoneyOptions(options?: Partial<MoneyOptions>): MoneyOptions {
    return {
        ...{
            decimals: options?.decimals ?? defaultMoneyOptions.decimals,
            fractionalSeparator: options?.fractionalSeparator ?? defaultMoneyOptions.fractionalSeparator,
            symbol: options?.symbol ?? defaultMoneyOptions.fractionalSeparator,
            thousandsSeparator: options?.symbol ?? defaultMoneyOptions.thousandsSeparator
        }
    } as const;
}

const defaultMoneyOptions: MoneyOptions = {
    decimals: 2,
    fractionalSeparator: ".",
    symbol: "$",
    thousandsSeparator: ","
} as const;

export class Money {
    readonly privateOptions: MoneyOptions;
    readonly privateNumber: MoneyNumber;
    readonly privateIsZero: boolean;

    get isNegative(): boolean {
        return this.privateNumber.isNegative;
    }

    get wholeUnits(): string {
        return this.privateNumber.wholeUnits;
    }

    get fractionalUnits(): string {
        return this.privateNumber.fractionalUnits;
    }

    get decimals(): number {
        return this.privateOptions.decimals;
    }

    get thousandsSeparator(): string {
        return this.privateOptions.thousandsSeparator;
    }

    get fractionalSeparator(): string {
        return this.privateOptions.fractionalSeparator;
    }

    get symbol(): string {
        return this.privateOptions.symbol;
    }

    get isZero(): boolean {
        return this.privateIsZero;
    }

    /**
     * Creates a new instance of the Money type.
     */
    constructor(
        number: number | MoneyNumber,
        options?: Partial<MoneyOptions>) {
        // Create a readonly, const copy of the options so they cannot be modified.
        this.privateOptions = createMoneyOptions(options);

        if (typeof number === "number") {
            const { isNegative, wholeUnits, fractionalUnits } = Money.getNumberParts(number);
            this.privateNumber = {
                isNegative,
                wholeUnits,
                fractionalUnits: Money.appendPaddedZeroesOrTruncate(fractionalUnits, this.decimals)
            } as const;
            this.privateIsZero = number === 0;
        }
        else {
            this.privateNumber = {
                isNegative: number.isNegative,
                wholeUnits: number.wholeUnits,
                fractionalUnits: Money.appendPaddedZeroesOrTruncate(number.fractionalUnits, this.decimals)
            } as const;
            const zeroRegex = /^0*$/;
            this.privateIsZero = zeroRegex.test(number.wholeUnits) && zeroRegex.test(number.fractionalUnits);
        }
    }

    add(number: Money | number): Money {
        const numberParts = Money.getNumberParts(number);
        const { isNegative, wholeUnits: numberWholeUnits } = numberParts;
        let { fractionalUnits: numberFractionalUnits } = numberParts;

        // Ensure the fractional part of the number matches this Money's decimal precision.
        numberFractionalUnits = Money.appendPaddedZeroesOrTruncate(numberFractionalUnits, this.decimals);

        if (this.isNegative === isNegative) {
            // If both are positive or both are negative,
            // add the numbers.
            let newWholeUnits = Money.addWholeUnits(this.wholeUnits, numberWholeUnits);
            const { wholeUnits: extraWholeUnits, fractionalUnits: newFractionalUnits } = Money.addFractionalUnits(this.fractionalUnits, numberFractionalUnits);
            if (extraWholeUnits) {
                newWholeUnits = Money.addWholeUnits(newWholeUnits, extraWholeUnits);
            }

            // Return the new Money object.
            return new Money(
                {
                    isNegative: this.privateNumber.isNegative,
                    fractionalUnits: newFractionalUnits,
                    wholeUnits: newWholeUnits
                },
                this.privateOptions
            );
        }
        else {
            // Subtract the negative number from the positive number.
            let wholeUnitsResult: { isNegative: boolean; wholeUnits: string; };
            let fractionalUnitsResult: { isNegative: boolean; fractionalUnits: string; };

            if (!this.isNegative) {
                // Subtract the numbers.
                wholeUnitsResult = Money.subtractWholeUnits(this.wholeUnits, numberWholeUnits);
                fractionalUnitsResult = Money.subtractFractionalUnits(this.fractionalUnits, numberFractionalUnits);
            }
            else {
                wholeUnitsResult = Money.subtractWholeUnits(numberWholeUnits, this.wholeUnits);
                fractionalUnitsResult = Money.subtractFractionalUnits(numberFractionalUnits, this.fractionalUnits);
            }

            const { isNegative: newIsNegativeWhole } = wholeUnitsResult;
            let { wholeUnits: newWholeUnits } = wholeUnitsResult;
            const { isNegative: newIsNegativeFractional } = fractionalUnitsResult;
            let { fractionalUnits: newFractionalUnits } = fractionalUnitsResult;

            if (newWholeUnits !== "0".repeat(newWholeUnits.length) && newIsNegativeWhole !== newIsNegativeFractional) {
                // Whole needs to be reduced by 1.
                const { wholeUnits: adjustedWholeUnits } = Money.subtractWholeUnits(newWholeUnits, Money.prependPaddedZeroesOrTruncate("1", newWholeUnits.length));
                newWholeUnits = adjustedWholeUnits;
                // The fractional units need to be inversed. Ex, if fractional units were "999", then we need to do "1000" - "0999".
                const { fractionalUnits: adjustedFractionalUnits } = Money.subtractFractionalUnits(Money.appendPaddedZeroesOrTruncate("1", newFractionalUnits.length + 1), Money.prependPaddedZeroesOrTruncate(newFractionalUnits, newFractionalUnits.length + 1));
                newFractionalUnits = adjustedFractionalUnits;
            }

            return new Money({
                    isNegative: newIsNegativeWhole || newIsNegativeFractional,
                    wholeUnits: newWholeUnits,
                    fractionalUnits: newFractionalUnits,
                },
                this.privateOptions
            );
        }
    }

    subtract(number: number | Money): Money {
        const numberParts = Money.getNumberParts(number);
        const { isNegative: numberIsNegative, wholeUnits: numberWholeUnits } = numberParts;
        let { fractionalUnits: numberFractionalUnits } = numberParts;

        // Ensure the fractional part of the number matches this Money's decimal precision.
        numberFractionalUnits = Money.appendPaddedZeroesOrTruncate(numberFractionalUnits, this.decimals);

        if (this.isNegative !== numberIsNegative) {
            // Subtracting a number from a number with a different sign,
            // is the same as adding them together and keeping the sign from the first number.
            let newWholeUnits = Money.addWholeUnits(this.wholeUnits, numberWholeUnits);
            const { wholeUnits: extraWholeUnits, fractionalUnits: newFractionalUnits } = Money.addFractionalUnits(this.fractionalUnits, numberFractionalUnits);
            if (extraWholeUnits) {
                newWholeUnits = Money.addWholeUnits(newWholeUnits, extraWholeUnits);
            }

            // Return the new Money object.
            return new Money({
                    isNegative: this.isNegative, // The result always has the same negation as the first operand.
                    wholeUnits: newWholeUnits,
                    fractionalUnits: newFractionalUnits,
                },
                this.privateOptions
            );
        }
        else {
            // The numbers have the same sign (+/-).

            // Subtract the numbers.
            const wholeUnitsResult = Money.subtractWholeUnits(this.wholeUnits, numberWholeUnits);
            const { isNegative: newIsNegativeWhole } = wholeUnitsResult;
            let { wholeUnits: newWholeUnits } = wholeUnitsResult;
            const fractionalUnitsResult = Money.subtractFractionalUnits(this.fractionalUnits, numberFractionalUnits);
            const { isNegative: newIsNegativeFractional } = fractionalUnitsResult;
            let { fractionalUnits: newFractionalUnits } = fractionalUnitsResult;

            // 1.000
            // 1.001
            // wholeRes = { wholeUnits: "0", isNegative: false }
            // fracRes = { fracUnits: "001", isNegative: true }
            // -0.001 (negation is wholeNeg || fracNeg)

            // 1.000
            // 2.001
            // wholeRes = { wholeUnits: "1", isNegative: true }
            // fracRes = { fracUnits: "001", isNegative: true }
            // -1.001 (negation is wholeNeg || fracNeg)

            // 5.001
            // 7.000
            // wholeRes = { wholeUnits: "2", isNegative: true }
            // fracRes = { fracUnits: "001", isNegative: false }
            // Because whole is neg and frac is positive,
            // whole needs to be reduced by 1 (regardless of sign),
            // and frac needs to be assigned "1000" - "001".
            // -1.999 (negation is wholeNeg || fracNeg)
            // TODO JMH This can be made more efficient by doing the whole number calc first,
            // then only doing one adjusted fractional calc.
            if (newWholeUnits !== "0".repeat(newWholeUnits.length) && newIsNegativeWhole !== newIsNegativeFractional) {
                // Whole needs to be reduced by 1.
                const { wholeUnits: adjustedWholeUnits } = Money.subtractWholeUnits(newWholeUnits, Money.prependPaddedZeroesOrTruncate("1", newWholeUnits.length));
                newWholeUnits = adjustedWholeUnits;
                // The fractional units need to be inversed. Ex, if fractional units were "999", then we need to do "1000" - "0999".
                const { fractionalUnits: adjustedFractionalUnits } = Money.subtractFractionalUnits(Money.appendPaddedZeroesOrTruncate("1", newFractionalUnits.length + 1), Money.prependPaddedZeroesOrTruncate(newFractionalUnits, newFractionalUnits.length + 1));
                newFractionalUnits = adjustedFractionalUnits;
            }

            return new Money({
                    isNegative: newIsNegativeWhole || newIsNegativeFractional,
                    wholeUnits: newWholeUnits,
                    fractionalUnits: newFractionalUnits,
                },
                this.privateOptions
            );
        }
    }

    isEqualTo(number: Money | number): boolean {
        if (this.isZero) {
            // Skip character matching logic for the zero case.
            return (typeof number === "number" && number === 0) || (typeof number !== "number" && number.isZero);
        }

        const numberParts = Money.getNumberParts(number);

        return numberParts.isNegative === this.isNegative
            && numberParts.wholeUnits === this.wholeUnits
            && numberParts.fractionalUnits === this.fractionalUnits;
    }

    isNotEqualTo(number: Money | number): boolean {
        return !this.isEqualTo(number);
    }

    isLessThan(number: Money | number): boolean {
        if (this.isZero) {
            // Skip character matching logic for the zero case.
            const isOtherNumberZero = (typeof number === "number" && number === 0) || (typeof number !== "number" && number.isZero);
            if (isOtherNumberZero) {
                return false;
            }
        }

        const numberParts = Money.getNumberParts(number);

        if (this.isNegative && !numberParts.isNegative) {
            // Return true if this number is negative and the other number is positive.
            return true;
        }

        /**
         * Returns -1 if units1 < units2; 0 if units1 === units2; 1 if units1 > units2.
         *
         * Assumes units1 and units2 are the same length.
         */
        function compareUnits(units1: string, units2: string): -1 | 0 | 1 {
            if (units1.length !== units2.length) {
                throw "Units must be the same length to compare them.";
            }

            for (let i = 0; i < units1.length; i++) {
                const thisDigit = units1[i];
                const otherDigit = units2[i];
                if (thisDigit < otherDigit) {
                    // Return -1 once we find a digit that is less than a digit in the other number at the same index.
                    return -1;
                }
                else if (thisDigit > otherDigit) {
                    // Return 1 once we find a digit in the other number that is less than a digit in this number at the same index.
                    return 1;
                }
                else {
                    // Keep processing.
                }
            }

            // If we get to this point then the digits are the same.
            return 0;
        }

        const wholeUnitsLengthDiff = this.wholeUnits.length - numberParts.wholeUnits.length;
        if (wholeUnitsLengthDiff < 0) {
            // Return true if this whole number is smaller than the other whole number in length.
            return true;
        }
        else if (wholeUnitsLengthDiff > 0) {
            // Return false if this whole number is larger than the other whole number in length.
            return false;
        }
        else {
            const comparison = compareUnits(this.wholeUnits, numberParts.wholeUnits);
            if (comparison === -1) {
                // Return true if this whole number is larger than the other whole number in value.
                return true;
            }
        }

        // If we get to this point, we need to check the fractional parts of the this and the other number.
        // It is impossible to determine which fractional number is larger or smaller based on the string length;
        // e.g., .1 and .1001 (the first is smaller and shorter length), .3 and .1001 (the first is LARGER and shorter length).

        // Zero-pad so they are the same length.
        const fractionUnitsLengthDiff = this.fractionalUnits.length - numberParts.fractionalUnits.length;
        if (fractionUnitsLengthDiff < 0) {
            // Zero-pad this number.
            return -1 === compareUnits(Money.appendPaddedZeroesOrTruncate(this.fractionalUnits, numberParts.fractionalUnits.length), numberParts.fractionalUnits);
        }
        else if (fractionUnitsLengthDiff > 0) {
            // Zero-pad the other number.
            return -1 === compareUnits(this.fractionalUnits, Money.appendPaddedZeroesOrTruncate(numberParts.fractionalUnits, this.fractionalUnits.length));
        }
        else {
            return -1 === compareUnits(this.fractionalUnits, numberParts.fractionalUnits);
        }
    }

    /**
     * Returns the remainder after dividing this money by another number.
     */
    mod(divisor: number): Money {
        // excessCents = temporarilyRemoveDec(remaining - desiredOneTimePayment) mod numberPayments
        // This is the smallest fractional unit that needs to be subtracted from the original number to be able to divide evenly by numberPayments.
        // (numberPayments - excessCents) is the smallest fractional unit that needs to be added to the original number to be able to divide evenly by numberPayments.

        if (this.isZero) {
            // Short-circuit the calculation if this Money value is 0; i.e., 0 % anything is 0.
            // Since money objects are immutable, it's safe to return this zero-amount money object as the remainder.
            return this;
        }

        if (this.isLessThan(divisor)) {
            // If this number is less than the divisor, then this amount IS the remainder.
            return this;
        }

        // Calculate the remainder by division.
        const { remainder } = this.divide(divisor);
        return remainder;
    }

    divide(divisor: number): { quotient: Money, remainder: Money } {
        if (this.isZero) {
            // Since money objects are immutable, it's safe to return this zero-amount money object as the quotient and remainder.
            return { quotient: this, remainder: this };
        }

        // Temporarily join the whole and fractional bits together. The decimal will be added at the end.
        const numberParts = Money.getNumberParts(divisor);

        const longDividend = `${this.wholeUnits}${this.fractionalUnits}${"0".repeat(numberParts.fractionalUnits.length)}`;
        const longDivisorString = `${numberParts.wholeUnits}${numberParts.fractionalUnits}`;
        const longDivisor = +longDivisorString;
        const longDividendLength = longDividend.length;
        const longDivisorLength = longDivisorString.length;
        let dividend = longDividend;

        // Do long division and capture the remainder.
        const dividendChunkSize: number = longDivisorLength;
        const quotientParts: string[] = [];
        while(dividendChunkSize <= dividend.length) {
            let currentChunkSize = dividendChunkSize;
            let dividendChunk: number = +dividend.substring(0, currentChunkSize);
            if (dividendChunk < longDivisor) {
                if (dividendChunkSize === dividend.length) {
                    // We have no more numbers to divide.
                    break;
                }
                else {
                    quotientParts.push("0");
                    currentChunkSize++;
                    dividendChunk = +dividend.substring(0, currentChunkSize);
                }
            }

            const quotientPart: number = Math.floor(dividendChunk / longDivisor);
            quotientParts.push(quotientPart.toString());

            const remainder: number = dividendChunk - (quotientPart * longDivisor);
            dividend = `${remainder}${dividend.substring(currentChunkSize)}`;
        }

        // The left over dividend is the remainder.

        // The result is negative if only the dividend or the divisor is negative, but not both.
        const isNegative = this.isNegative ? !numberParts.isNegative : numberParts.isNegative;
        const quotient: Money = new Money(
            {
                isNegative,
                wholeUnits: quotientParts.slice(0, quotientParts.length - this.decimals).join(),
                fractionalUnits: quotientParts.slice(quotientParts.length - this.decimals).join()
            },
            this.privateOptions
        );
        const remainder: Money = new Money(
            {
                isNegative,
                wholeUnits: dividend.slice(0, dividend.length - this.decimals) || "0",
                fractionalUnits: dividend.slice(dividend.length - this.decimals) || "0"
            },
            this.privateOptions
        );
        return { quotient, remainder };
    }

    private static subtractWholeUnits(wholeUnits1: string, wholeUnits2: string): { isNegative: boolean; wholeUnits: string; } {
        // Zero-pad the units so they are the same size.
        if (wholeUnits1.length > wholeUnits2.length) {
            wholeUnits2 = Money.prependPaddedZeroesOrTruncate(wholeUnits2, wholeUnits1.length);
        }
        else if (wholeUnits1.length < wholeUnits2.length) {
            wholeUnits1 = Money.prependPaddedZeroesOrTruncate(wholeUnits1, wholeUnits2.length);
        }

        const { isNegative, digits } = Money.substractUnits(wholeUnits1, wholeUnits2);
        return { isNegative, wholeUnits: digits.join() };
    }

    private static subtractFractionalUnits(fractionalUnits1: string, fractionalUnits2: string): { isNegative: boolean; fractionalUnits: string; } {
        // Zero-pad the units so they are the same size.
        if (fractionalUnits1.length > fractionalUnits2.length) {
            fractionalUnits2 = Money.appendPaddedZeroesOrTruncate(fractionalUnits2, fractionalUnits1.length);
        }
        else if (fractionalUnits1.length < fractionalUnits2.length) {
            fractionalUnits1 = Money.appendPaddedZeroesOrTruncate(fractionalUnits1, fractionalUnits2.length);
        }

        const { digits, isNegative } = Money.substractUnits(fractionalUnits1, fractionalUnits2);
        return { isNegative, fractionalUnits: digits.join() };
    }

    private static prependPaddedZeroesOrTruncate(integerUnits: string, precision: number): string {
        if (integerUnits.length === precision) {
            return integerUnits;
        }
        else if (integerUnits.length < precision) {
            // Pad the units with 0s until the same precision is met.
            return "0".repeat(precision - integerUnits.length) + integerUnits;
        }
        else {
            // Truncate if the units are more precise than the allowed precision.
            return integerUnits.substring(integerUnits.length - precision);
        }
    }

    private static appendPaddedZeroesOrTruncate(fractionalUnits: string, precision: number): string {
        if (fractionalUnits.length === precision) {
            return fractionalUnits;
        }
        else if (fractionalUnits.length < precision) {
            // Pad the fractional units with 0s until the same precision is met.
            return fractionalUnits + "0".repeat(precision - fractionalUnits.length);
        }
        else {
            // Truncate if the fractional units are more precise than the allowed precision.
            return fractionalUnits.substring(0, precision);
        }
    }

    private static getNumberParts(number: number | Money): { isNegative: boolean, wholeUnits: string, fractionalUnits: string } {
        if (typeof number !== "number") {
            return {
                isNegative: number.isNegative,
                fractionalUnits: number.fractionalUnits,
                wholeUnits: number.wholeUnits
            };
        }

        // Per, https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number/toString#description...
        // If the number is not a whole number, the decimal point . is used to separate the decimal places.
        // Therefore, it is safe to split number.toString() by "." to get the whole and fractional parts of a number,
        // assuming the number is not NaN, Infinity, NegativeInfinity, or scientific notation.
        if (!Number.isFinite(number)) {
            throw "The number to add must be a finite number.";
        }

        let numberAsString = number.toString();

        if (numberAsString.includes("e")) {
            // Convert scientific notation to number.
            numberAsString = Money.eToNumber(numberAsString);
        }

        const numberParts = numberAsString.split(".");
        let [numberWholeUnits] = numberParts;
        const [_, numberFractionalUnits] = numberParts;
        const isNegative: boolean = numberWholeUnits.charAt(0) === "-";

        // Remove the sign.
        numberWholeUnits = isNegative ? numberWholeUnits.substring(1) : numberWholeUnits;

        return {
            isNegative,
            wholeUnits: numberWholeUnits ?? "",
            fractionalUnits: numberFractionalUnits ?? ""
        };
    }

    private static addWholeUnits(wholeUnits1: string, wholeUnits2: string): string {
        // Zero-pad the units so they are the same size.
        if (wholeUnits1.length > wholeUnits2.length) {
            wholeUnits2 = Money.prependPaddedZeroesOrTruncate(wholeUnits2, wholeUnits1.length);
        }
        else if (wholeUnits2.length > wholeUnits1.length) {
            wholeUnits1 = Money.prependPaddedZeroesOrTruncate(wholeUnits1, wholeUnits2.length);
        }

        const { digits } = Money.addUnits(wholeUnits1, wholeUnits2);
        return digits.join();
    }

    /**
     * Subtracts units2 from units1.
     *
     * The string lengths must be the same; the smaller string should have zeroes prepended or appended depending on whether they are whole or fractional numbers.
     *
     * The resulting subtraction array will have the same length as the input arguments,
     * and each element will be a single number character including leading and trailing zeroes.
     */
    private static substractUnits(units1: string, units2: string): { digits: string[]; isNegative: boolean; } {
        if (units1.length !== units2.length) {
            throw "The number strings must be the same size (the smaller string should have zeroes prepended or appended depending on whether they whole numbers or fractional numbers)";
        }

        if (units1 === units2) {
            // The numbers are the same so return zeroes the same length as the numbers.
            return { digits: [..."0".repeat(units1.length)], isNegative: false };
        }

        let isNegative: boolean = false;

        // Make sure the bigger and smaller numbers variables are accurately assigned.
        // Working from left-to-right, the bigger number should always be >= the smaller number at the same index.
        let units1Array: number[] = Array<number>(units1.length);
        let units2Array: number[] = Array<number>(units1.length);

        for (let i = 0; i < units1.length; i++) {
            // Convert the strings into a number array.
            units1Array[i] = +units1[i];
            units2Array[i] = +units2[i];

            if (units1Array[i] < units2Array[i]) {
                // The second number is bigger than the first number,
                // so the subtraction will result in a negative number.
                isNegative = true;
            }
        }

        // Swap the array pointers so the smaller number is subtracted from the larger number.
        // The negation will be applied at the end.
        if (isNegative) {
            const temp = units1Array;
            units1Array = units2Array;
            units2Array = temp;
        }

        const digits: string[] = [];

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
            digits.unshift(difference.toString());
        }

        return {
            digits,
            isNegative
        };
    }

    private static addUnits(units1: string, units2: string): { digits: string[]; isFirstElementCarriedOver: boolean; } {
        if (units1.length !== units2.length) {
            throw "The numbers must be the same size";
        }

        const digits: string[] = [];
        let isCarriedOver: boolean = false;

        for (let i = units1.length - 1; i >= 0; i--) {
            const int1: number = +units1[i];
            const int2: number = +units2[i];
            const sum: number = int1 + int2 + (isCarriedOver ? 1 : 0);

            if (sum <= 9) {
                isCarriedOver = false;
                digits.unshift(sum.toString());
            }
            else {
                const sumParts = sum.toString().split("");
                if (i > 0) {
                    isCarriedOver = true;
                    digits.unshift(sumParts[1]);
                }
                else {
                    // This is the last number,
                    // so no need to carry again.
                    digits.unshift(...sumParts);
                }
            }
        }

        return {
            digits: digits,
            isFirstElementCarriedOver: isCarriedOver
        };
    }

    private static addFractionalUnits(fractionalUnits1: string, fractionalUnits2: string): { wholeUnits: string; fractionalUnits: string; } {
        // Zero-pad the units so they are the same size.
        if (fractionalUnits1.length > fractionalUnits2.length) {
            fractionalUnits2 = Money.appendPaddedZeroesOrTruncate(fractionalUnits2, fractionalUnits1.length);
        }
        else if (fractionalUnits2.length > fractionalUnits1.length) {
            fractionalUnits1 = Money.appendPaddedZeroesOrTruncate(fractionalUnits1, fractionalUnits2.length);
        }

        const { digits: addition, isFirstElementCarriedOver } = Money.addUnits(fractionalUnits1, fractionalUnits2);

        // If summing two fractional units end in a carry,
        // then the first element represents a whole unit.
        // 0.999 + 0.990 = 1.989 => { addition: [1,9,8,9], isFirstElementCarriedOver: true }
        if (isFirstElementCarriedOver) {
            // The first element represents whole unit,
            // while the remaining elements are the fractional units.
            const wholeUnits = addition[0];

            // Remove the whole units from the fractional units.
            addition.splice(0, 1);

            return { wholeUnits, fractionalUnits: addition.join() };
        }
        else {
            return { wholeUnits: "", fractionalUnits: addition.join() };
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

    privateToStringCached: string | null = null;
    public toString = (): string => {
        function addThousandsSeparator(number: string, separator: string): string {
            if (!separator) {
                // There is no separator, so just return the number.
                return number;
            }
            const chars: string[] = [];
            let thousandsCounter = 0;
            for (let i = number.length - 1; i >= 0; i--) {
                thousandsCounter++;
                if (thousandsCounter === 3) {
                    chars.unshift(separator, number[i]);
                    thousandsCounter = 0;
                }
                else {
                    chars.unshift(number[i]);
                }
            }
            return chars.join();
        }

        if (this.privateToStringCached === null) {
            this.privateToStringCached = `${this.isNegative ? "-" : ""}${this.symbol}${addThousandsSeparator(this.wholeUnits, this.thousandsSeparator)}${this.fractionalSeparator}${this.fractionalUnits}`;
        }

        return this.privateToStringCached;
    };
}