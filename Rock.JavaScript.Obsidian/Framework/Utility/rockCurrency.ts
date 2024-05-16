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

import Big from "@Obsidian/Libs/big";
import { CurrencyInfoBag } from "@Obsidian/ViewModels/Rest/Utilities/currencyInfoBag";
import { toCurrencyOrNull } from "./numberUtils";

type RockCurrencyValue = number | string | RockCurrency;

export class RockCurrency {
    private readonly big: Big;

    get isZero(): boolean {
        return this.big.eq(0);
    }

    get number(): number {
        return this.big.toNumber();
    }

    get isNegative(): boolean {
        return this.big.lt(0);
    }

    get units(): number {
        return this.big.times(new Big(10).pow(this.currencyInfo.decimalPlaces)).toNumber();
    }

    /**
     * Creates a new RockCurrency instance.
     *
     * Keep private so the constructor can hide reference to the underlying Big library usage.
     */
    private constructor(value: RockCurrencyValue | Big.Big, readonly currencyInfo: CurrencyInfoBag) {
        if (value instanceof RockCurrency) {
            // Always truncate the currency value decimal places (with Big.roundDown).
            this.big = new Big(value.big).round(this.currencyInfo.decimalPlaces, Big.roundDown);
        }
        else {
            // Always truncate the currency value decimal places (with Big.roundDown).
            this.big = new Big(value).round(this.currencyInfo.decimalPlaces, Big.roundDown);
        }
    }

    /**
     * Creates a new instance of the RockCurrency class.
     *
     * @param value The currency value.
     * @param currencyInfo The currency info.
     * @returns A new RockCurrency instance.
     */
    static create(value: RockCurrencyValue, currencyInfo: CurrencyInfoBag): RockCurrency {
        return new RockCurrency(value, currencyInfo);
    }

    asRockCurrency(value: RockCurrencyValue): RockCurrency {
        return value instanceof RockCurrency ? value : new RockCurrency(value, this.currencyInfo);
    }

    /**
     * Adds an amount to this RockCurrency.
     *
     * @param value The amount to add to this currency. Fractional amounts that exceed this currency's precision will be truncated; e.g., if this currency has a precision of two ($31.45), and the amount being added has a precision of three ($2.289), the "9" will be truncated to $2.28.
     * @returns A createCurrency instance containing the sum of the two currencies.
     * @example
     * createCurrency(2.61).add(3.999);               // returns createCurrency(6.60)
     * createCurrency(2.61).add(createCurrency(3.999)); // returns createCurrency(6.60)
     */
    add(value: RockCurrencyValue): RockCurrency {
        const currency = this.asRockCurrency(value);

        return new RockCurrency(this.big.plus(currency.big), this.currencyInfo);
    }

    /**
     * Gets the negation of this RockCurrency.
     *
     * @returns A createCurrency instance containing the negation of this RockCurrency.
     * @example
     * createCurrency(2.61).negate(); // returns createCurrency(-2.61)
     */
    negate(): RockCurrency {
        return new RockCurrency(this.big.neg(), this.currencyInfo);
    }

    /**
     * Divides this currency by a number.
     *
     * @param divisor The number by which to divide this currency. Must be a number as a currency cannot be divided by another currency.
     * @returns The quotient and remainder of the division as separate RockCurrency instances.
     * @example
     * createCurrency(3.50).divide(3); // returns { quotient: createCurrency(1.16), remainder: createCurrency(0.02) }
     */
    divide(divisor: number): { quotient: RockCurrency, remainder: RockCurrency } {
        // Always truncate the currency value decimal places (with Big.roundDown).
        const quotient = this.big.div(divisor).round(this.currencyInfo.decimalPlaces, Big.roundDown);
        const remainder = this.big.minus(quotient.times(divisor));

        return {
            quotient: new RockCurrency(quotient, this.currencyInfo),
            remainder: new RockCurrency(remainder, this.currencyInfo),
        };
    }

    /**
     * Subtracts an amount from this currency.
     *
     * @param currency The amount to subtract from this currency. Fractional amounts that exceed this currency's precision will be truncated; e.g., if this currency has a precision of two ($31.45), and the amount being subtracted has a precision of three ($2.289), the "9" will be truncated to $2.28.
     * @returns A createCurrency instance containing the difference of the two currencies.
     * @example
     * createCurrency(2.61).subtract(3.999);               // returns createCurrency(-1.38)
     * createCurrency(2.61).subtract(createCurrency(3.999)); // returns createCurrency(-1.38)
     */
    subtract(value: RockCurrencyValue): RockCurrency {
        const currency = this.asRockCurrency(value);

        return new RockCurrency(this.big.minus(currency.big), this.currencyInfo);
    }

    /**
     * Determines if this currency is equal another currency.
     *
     * @param value The currency to which to compare.
     * @returns `true` if the currencies are equal; otherwise, `false` is returned.
     */
    isEqualTo(value: RockCurrencyValue): boolean {
        const currency = this.asRockCurrency(value);
        return this.big.eq(currency.big);
    }

    /**
     * Determines if this currency is not equal to another currency.
     *
     * @param value The currency to which to compare.
     * @returns `true` if the currencies are not equal; otherwise, `false` is returned.
     */
    isNotEqualTo(value: RockCurrencyValue): boolean {
        const currency = this.asRockCurrency(value);
        return !this.big.eq(currency.big);
    }

    /**
     * Determines if this currency is less than to another currency.
     *
     * @param value The currency to which to compare.
     * @returns `true` if this currency is less than the provided currency; otherwise, `false` is returned.
     */
    isLessThan(value: RockCurrencyValue): boolean {
        const currency = this.asRockCurrency(value);
        return this.big.lt(currency.big);
    }

    /**
     * Returns this currency limited by the provided value.
     *
     * @param value The currency to which to compare.
     * @returns `this` currency if it is equal to or greater than the limit; otherwise, `value` is returned.
     */
    noLessThan(value: RockCurrencyValue): RockCurrency {
        const currency = this.asRockCurrency(value);
        if (this.big.lt(currency.big)) {
            return new RockCurrency(currency.big, this.currencyInfo);
        }
        else {
            return this;
        }
    }

    /**
     * Determines if this currency is greater than to another currency.
     *
     * @param value The currency to which to compare.
     * @returns `true` if this currency is greater than the provided currency; otherwise, `false` is returned.
     */
    isGreaterThan(value: RockCurrencyValue): boolean {
        const limit = this.asRockCurrency(value);
        return this.big.gt(limit.big);
    }

    /**
     * Returns this currency limited by the provided value.
     *
     * @param value The currency to which to compare.
     * @returns `this` currency if it is equal to or less than the limit; otherwise, `limit` is returned.
     */
    noGreaterThan(value: RockCurrencyValue): RockCurrency {
        const currency = this.asRockCurrency(value);
        if (this.big.gt(currency.big)) {
            return currency;
        }
        else {
            return this;
        }
    }

    /** Gets the absolute value of this currency. */
    abs(): RockCurrency {
        return new RockCurrency(this.big.abs(), this.currencyInfo);
    }

    /**
     * Returns the remainder after dividing this currency by a number.
     */
    mod(divisor: number): RockCurrency {
        const { remainder } = this.divide(divisor);
        return remainder;
    }

    /**
     * Gets the formatted string value of this currency.
     */
    toString(): string {
        // Always truncate the currency value decimal places (with Big.roundDown).
        const valueString = this.big.toFixed(this.currencyInfo.decimalPlaces, Big.roundDown);

        return toCurrencyOrNull(valueString, this.currencyInfo) ?? `${this.currencyInfo.symbol}${valueString}`;
    }
}