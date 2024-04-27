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
import { CurrentPersonBag } from "@Obsidian/ViewModels/Crm/currentPersonBag";
import { areEqual, newGuid } from "@Obsidian/Utility/guid";
import {
    RegistrationEntryState,
    RegistrationCostSummaryInfo,
    RegistrantBasicInfo } from "./types.partial";
import { InjectionKey, Ref, inject, nextTick } from "vue";
import { smoothScrollToTop } from "@Obsidian/Utility/page";
import { PublicComparisonValueBag } from "@Obsidian/ViewModels/Utility/publicComparisonValueBag";
import { ComparisonValue } from "@Obsidian/Types/Reporting/comparisonValue";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";
import { DefinedValue } from "@Obsidian/SystemGuids/definedValue";
import { RegistrationEntryArgsBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryArgsBag";
import { RegistrantsSameFamily } from "@Obsidian/Enums/Event/registrantsSameFamily";
import { RegistrationPersonFieldType } from "@Obsidian/Enums/Event/registrationPersonFieldType";
import { RegistrationFieldSource } from "@Obsidian/Enums/Event/registrationFieldSource";
import { CurrencyInfoBag } from "@Obsidian/ViewModels/Utility/currencyInfoBag";
import { asFormattedString, toCurrencyOrNull } from "@Obsidian/Utility/numberUtils";
import { RegistrantBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrantBag";
import { RegistrationEntryFormBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFormBag";
import { RegistrationEntryFormFieldBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFormFieldBag";
import { RegistrationEntryInitializationBox } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryInitializationBox";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

/** If all registrants are to be in the same family, but there is no currently authenticated person,
 *  then this guid is used as a common family guid */
const unknownSingleFamilyGuid = newGuid();

/**
 * If there is a forced family guid because of RegistrantsSameFamily setting, then this returns that guid
 * @param currentPerson
 * @param viewModel
 */
export function getForcedFamilyGuid(currentPerson: CurrentPersonBag | null, viewModel: RegistrationEntryInitializationBox): string | null {
    return (currentPerson && viewModel.registrantsSameFamily === RegistrantsSameFamily.Yes) ?
        (viewModel.currentPersonFamilyGuid || unknownSingleFamilyGuid) :
        null;
}

/**
 * Get a default registrant object with the current family guid set.
 * @param currentPerson
 * @param viewModel
 * @param familyGuid
 */
export function getDefaultRegistrantInfo(currentPerson: CurrentPersonBag | null, viewModel: RegistrationEntryInitializationBox, familyGuid: Guid | null): RegistrantBag {
    const forcedFamilyGuid = getForcedFamilyGuid(currentPerson, viewModel);

    if (forcedFamilyGuid) {
        familyGuid = forcedFamilyGuid;
    }

    // If the family is not specified, then assume the person is in their own family.
    if (!familyGuid && viewModel.registrantsSameFamily === RegistrantsSameFamily.No) {
        familyGuid = newGuid();
    }

    const registrantBag: RegistrantBag = {
        cost: 0,
        isOnWaitList: false,
        familyGuid: familyGuid,
        fieldValues: {},
        feeItemQuantities: {},
        guid: newGuid(),
        personGuid: null
    };

    return registrantBag;
}

export function getRegistrantBasicInfo(registrant: RegistrantBag, registrantForms: RegistrationEntryFormBag[]): RegistrantBasicInfo {
    // TODO Should Guids here be enforced?
    const fields = registrantForms?.reduce((acc, f) => acc.concat(f.fields ?? []), [] as RegistrationEntryFormFieldBag[]) || [];

    const firstNameGuidOrEmptyString = fields.find(f => f.personFieldType === RegistrationPersonFieldType.FirstName && f.fieldSource === RegistrationFieldSource.PersonField)?.guid || "";
    const lastNameGuidOrEmptyString = fields.find(f => f.personFieldType === RegistrationPersonFieldType.LastName && f.fieldSource === RegistrationFieldSource.PersonField)?.guid || "";
    const emailGuidOrEmptyString = fields.find(f => f.personFieldType === RegistrationPersonFieldType.Email && f.fieldSource === RegistrationFieldSource.PersonField)?.guid || "";

    return {
        firstName: (registrant?.fieldValues?.[firstNameGuidOrEmptyString] || "") as string,
        lastName: (registrant?.fieldValues?.[lastNameGuidOrEmptyString] || "") as string,
        email: (registrant?.fieldValues?.[emailGuidOrEmptyString] || "") as string,
        guid: registrant?.guid || ""
    };
}

/** Scrolls to the top of the window after the next render. */
export function scrollToTopAfterNextRender(): void {
    nextTick(() => smoothScrollToTop());
}

/**
 * Injects a provided value.
 * Throws an exception if the value is undefined or not yet provided.
 */
export function use<T>(key: string | InjectionKey<T>): T {
    const result = inject<T>(key);

    if (result === undefined) {
        throw `Attempted to access ${key} before a value was provided.`;
    }

    return result;
}

export function convertComparisonValue(value: PublicComparisonValueBag): ComparisonValue {
    return {
        value: value.value ?? "",
        comparisonType: value.comparisonType
    };
}

/** An injection key to provide the registration entry state. */
export const CurrentRegistrationEntryState: InjectionKey<RegistrationEntryState> = Symbol("registration-entry-state");

/** An injection key to provide the function that gets the args to persist the session. */
export const GetPersistSessionArgs: InjectionKey<() => RegistrationEntryArgsBag> = Symbol("get-persist-session-args");

/** An injection key to provide the function that persists the session. */
export const PersistSession: InjectionKey<(force?: boolean) => Promise<void>> = Symbol("persist-session");

/** An injection key to provide the cost summary for the entire registration. */
export const RegistrationCostSummary: InjectionKey<{
    readonlyRegistrationCostSummary: Ref<RegistrationCostSummaryInfo>;
    updateRegistrationCostSummary: (newValue: Partial<RegistrationCostSummaryInfo>) => void;
}> = Symbol("registration-cost-summary");

export type TransactionFrequency = {
    /** Determines if this transaction frequency matches the definedValueGuid. */
    hasDefinedValueGuid(definedValueGuid: Guid): boolean;

    /** Gets the number of transactions between the first and second dates, inclusively. */
    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number;

    getNextTransactionDate(): RockDateTime;

    maxNumberOfPaymentsForOneYear: number;
};

const transactionFrequencyOneTime: TransactionFrequency = {
    hasDefinedValueGuid(definedValueGuid: Guid): boolean {
        return areEqual(definedValueGuid, DefinedValue.TransactionFrequencyOneTime);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number {
        // Add a day to the second date so this function only has to check
        // isEarlierThan(dayAfterSecondDate) instead of isEarlierThan(secondDate) || isEqualTo(secondDate).
        const dayAfterSecondDate = secondDateTime.addDays(1).date;
        const firstDate = firstDateTime.date;

        if (firstDate.isEarlierThan(dayAfterSecondDate)) {
            return 1;
        }
        else {
            return 0;
        }
    },

    getNextTransactionDate(): RockDateTime {
        // Always use today's date for one-time transactions.
        return RockDateTime.now().date;
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 1;
    }
};

const transactionFrequencyWeekly: TransactionFrequency = {
    hasDefinedValueGuid(definedValueGuid: string): boolean {
        return areEqual(definedValueGuid, DefinedValue.TransactionFrequencyWeekly);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number {
        // Add a day to the second date so this function only has to check
        // isEarlierThan(dayAfterSecondDate) instead of isEarlierThan(secondDate) || isEqualTo(secondDate).
        const dayAfterSecondDate = secondDateTime.addDays(1).date;
        let date = firstDateTime.date;

        let numberOfTransactions = 0;

        while (date.isEarlierThan(dayAfterSecondDate)) {
            numberOfTransactions++;

            // Add 7 days.
            date = date.addDays(7);
        }

        return numberOfTransactions;
    },

    getNextTransactionDate(): RockDateTime {
        // Always use tomorrow's date for recurring transactions.
        return RockDateTime.now().addDays(1).date;
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 52;
    }
};

const transactionFrequencyBiWeekly: TransactionFrequency = {
    hasDefinedValueGuid(definedValueGuid: string): boolean {
        return areEqual(definedValueGuid, DefinedValue.TransactionFrequencyBiweekly);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number {
        // Add a day to the second date so this function only has to check
        // isEarlierThan(dayAfterSecondDate) instead of isEarlierThan(secondDate) || isEqualTo(secondDate).
        const dayAfterSecondDate = secondDateTime.addDays(1).date;
        let date = firstDateTime.date;

        let numberOfTransactions = 0;

        while (date.isEarlierThan(dayAfterSecondDate)) {
            numberOfTransactions++;

            // Add 14 days.
            date = date.addDays(14);
        }

        return numberOfTransactions;
    },

    getNextTransactionDate(): RockDateTime {
        // Always use tomorrow's date for recurring transactions.
        return RockDateTime.now().addDays(1).date;
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 26;
    }
};

// These must be strings.
type GetNextDayOption = "end-of-month";

/**
 * Starting from the current date, finds the next day in the month matching one the specified days.
 * @param potentialDays Represents the days of the month; e.g., 1 is the 1st, 2 is the 2nd, etc.
 * @example
 * // Assume current date is 2-8-2024.
 * getNextDay(13, 15, 31); // 2-13-2024
 * getNextDay(1, 15); // 2-15-2024
 * getNextDay(1); // 3-1-2024
 * getNextDay(8); // 2-8-2024
 * getNextDay("end-of-month"); // 2-29-2024
 */
function getNextDay(...potentialDays: (number | GetNextDayOption)[]): RockDateTime | null {
    let date = RockDateTime.now().date;

    potentialDays = potentialDays
        // Remove invalid days.
        .filter(day => {
            if (typeof day === "number") {
                if (day >= 1 && day <= 31) {
                    return true;
                }
                else {
                    return false;
                }
            }
            else {
                // Don't filter special options.
            }
        })
        .sort((day1, day2) => {
            const isDay1Number = typeof day1 === "number";
            const isDay2Number = typeof day2 === "number";
            if (isDay1Number && isDay2Number) {
                return day1 > day2 ? 1 : day1 < day2 ? -1 : 0;
            }
            else if (isDay1Number) {
                // Always sort numbers before special options so matching logic checks numbers first.
                return -1;
            }
            else if (isDay2Number) {
                // Always sort numbers before special options so matching logic checks numbers first.
                return 1;
            }
            else {
                // Sort special options as they were provided.
                return 0;
            }
        });

    // Perform the search up to 12 times: once per month for an entire year.
    let attempt = 0;
    while (attempt++ <= 12) {
        for (let i = 0; i < potentialDays.length; i++) {
            const potentialDay = potentialDays[i];
            if (typeof potentialDay === "number") {
                if (potentialDay === date.day) {
                    // The date matches the current potential day so return it.
                    return date;
                }
                else if (date.day < potentialDay) {
                    // The date is before the current potential day.
                    // Since the potential days are in ascending order,
                    // the potential day IS the next day that should be returned.
                    // Bump the date forward to the next day and return it.
                    return date.addDays(potentialDay - date.day);
                }
            }
            // Check special options.
            else if (potentialDay === "end-of-month") {
                // The date is after all the potential days
                // and the next potential day that can be returned is the
                // end of the current month.
                // Bump the date forward to the end of the current month and return it.
                return date.endOfMonth().date;
            }
        }

        // If this code is reached, then all the potential days are before the current date.
        // Move to the start of the next month and try searching again.
        date = date.addDays(1 - date.day).addMonths(1);
    }

    // If this code is reached, then it's likely that the arguments are invalid.
    return null;
}

const transactionFrequencyTwiceAMonth: TransactionFrequency = {
    hasDefinedValueGuid(definedValueGuid: string): boolean {
        return areEqual(definedValueGuid, DefinedValue.TransactionFrequencyFirstAndFifteenth) || areEqual(definedValueGuid, DefinedValue.TransactionFrequencyTwicemonthly);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number {
        // For twice a month frequency, this will check how many 1st and 15th days are between the two dates.

        // Add a day to the second date so this function only has to check
        // isEarlierThan(dayAfterSecondDate) instead of isEarlierThan(secondDate) || isEqualTo(secondDate).
        const dayAfterSecondDate = secondDateTime.addDays(1).date;
        let date = firstDateTime.date;

        if (date.day > 15) {
            // Set the date to the 1st of the next month.
            date = date.addDays(1 - date.day).addMonths(1);
        }
        else if (date.day < 15 && date.day > 1) {
            // Set the date to the 15th of the current month.
            date = date.addDays(15 - date.day);
        }

        let numberOfTransactions = 0;

        while (date.isEarlierThan(dayAfterSecondDate)) {
            if (date.day === 1 || date.day === 15) {
                numberOfTransactions++;
            }

            if (date.day < 15) {
                // Set the date to the 15th of the current month.
                date = date.addDays(15 - date.day);
            }
            else {
                // Set the date to the 1st of the next month.
                date = date.addDays(-14).addMonths(1);
            }
        }

        return numberOfTransactions;
    },

    getNextTransactionDate(): RockDateTime {
        // Always use tomorrow's date for recurring transactions.
        return RockDateTime.now().addDays(1).date;
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 24;
    }
};

const transactionFrequencyMonthly: TransactionFrequency = {
    hasDefinedValueGuid(definedValueGuid: string): boolean {
        return areEqual(definedValueGuid, DefinedValue.TransactionFrequencyMonthly);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number {
        // Add a day to the second date so this function only has to check
        // isEarlierThan(dayAfterSecondDate) instead of isEarlierThan(secondDate) || isEqualTo(secondDate).
        const dayAfterSecondDate = secondDateTime.addDays(1).date;
        let date = firstDateTime.date;

        // If the first date is the last day of the month
        // then this function will increment by 1 months and
        // automatically choose the last day of the month.
        const getNextDate: (d: RockDateTime) => RockDateTime =
            date.isEqualTo(firstDateTime.endOfMonth().date)
                ? (d: RockDateTime) => d.addMonths(1).endOfMonth().date
                : (d: RockDateTime) => d.addMonths(1);

        let numberOfTransactions = 0;

        while (date.isEarlierThan(dayAfterSecondDate)) {
            numberOfTransactions++;
            date = getNextDate(date);
        }

        return numberOfTransactions;
    },

    getNextTransactionDate(): RockDateTime {
        // Always use tomorrow's date for recurring transactions.
        return RockDateTime.now().addDays(1).date;
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 12;
    }
};

const transactionFrequencyQuarterly: TransactionFrequency = {
    hasDefinedValueGuid(definedValueGuid: string): boolean {
        return areEqual(definedValueGuid, DefinedValue.TransactionFrequencyQuarterly);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number {
        // Add a day to the second date so this function only has to check
        // isEarlierThan(dayAfterSecondDate) instead of isEarlierThan(secondDate) || isEqualTo(secondDate).
        const dayAfterSecondDate = secondDateTime.addDays(1).date;
        let date = firstDateTime.date;

        // If the first date is the last day of the month
        // then this function will increment by 3 months and
        // automatically choose the last day of the month.
        const getNextDate: (d: RockDateTime) => RockDateTime =
            date.isEqualTo(firstDateTime.endOfMonth().date)
                ? (d: RockDateTime) => d.addMonths(3).endOfMonth().date
                : (d: RockDateTime) => d.addMonths(3);

        let numberOfTransactions = 0;

        while (date.isEarlierThan(dayAfterSecondDate)) {
            numberOfTransactions++;
            date = getNextDate(date);
        }

        return numberOfTransactions;
    },

    getNextTransactionDate(): RockDateTime {
        // Always use tomorrow's date for recurring transactions.
        return RockDateTime.now().addDays(1).date;
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 4;
    }
};

const transactionFrequencyTwiceAYear: TransactionFrequency = {
    hasDefinedValueGuid(definedValueGuid: string): boolean {
        return areEqual(definedValueGuid, DefinedValue.TransactionFrequencyTwiceyearly);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number {
        // Add a day to the second date so this function only has to check
        // isEarlierThan(dayAfterSecondDate) instead of isEarlierThan(secondDate) || isEqualTo(secondDate).
        const dayAfterSecondDate = secondDateTime.addDays(1).date;
        let date = firstDateTime.date;

        // If the first date is the last day of the month
        // then this function will increment by 6 months and
        // automatically choose the last day of the month.
        const getNextDate: (d: RockDateTime) => RockDateTime =
            date.isEqualTo(firstDateTime.endOfMonth().date)
                ? (d: RockDateTime) => d.addMonths(6).endOfMonth().date
                : (d: RockDateTime) => d.addMonths(6);

        let numberOfTransactions = 0;

        while (date.isEarlierThan(dayAfterSecondDate)) {
            numberOfTransactions++;
            date = getNextDate(date);
        }

        return numberOfTransactions;
    },

    getNextTransactionDate(): RockDateTime {
        // Always use tomorrow's date for recurring transactions.
        return RockDateTime.now().addDays(1).date;
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 2;
    }
};

const transactionFrequencyYearly: TransactionFrequency = {
    hasDefinedValueGuid(definedValueGuid: string): boolean {
        return areEqual(definedValueGuid, DefinedValue.TransactionFrequencyYearly);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number {
        let date = firstDateTime.date;
        const dayAfterSecondDate = secondDateTime.addDays(1).date;

        // If the first date is the last day of the month
        // then this function will increment by 1 year and
        // automatically choose the last day of the month.
        const getNextDate: (d: RockDateTime) => RockDateTime =
            date.isEqualTo(firstDateTime.endOfMonth().date)
                ? (d: RockDateTime) => d.addYears(1).endOfMonth().date
                : (d: RockDateTime) => d.addYears(1);

        let numberOfTransactions = 0;

        while (date.isEarlierThan(dayAfterSecondDate)) {
            numberOfTransactions++;
            date = getNextDate(date);
        }

        return numberOfTransactions;
    },

    getNextTransactionDate(): RockDateTime {
        // Always use tomorrow's date for recurring transactions.
        return RockDateTime.now().addDays(1).date;
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 1;
    }
};

const transactionFrequencies: TransactionFrequency[] = [
    transactionFrequencyOneTime,
    transactionFrequencyWeekly,
    transactionFrequencyBiWeekly,
    transactionFrequencyTwiceAMonth,
    transactionFrequencyMonthly,
    transactionFrequencyQuarterly,
    transactionFrequencyTwiceAYear,
    transactionFrequencyYearly
];

export const nullTransactionFrequency: TransactionFrequency = {
    hasDefinedValueGuid(_definedValueGuid: string): boolean {
        return false;
    },

    getMaxNumberOfTransactionsBetweenDates(_firstDateTime: RockDateTime, _secondDateTime: RockDateTime): number {
        return 0;
    },

    getNextTransactionDate(): RockDateTime {
        // Always use an invalid date for null transactions to prevent them from being scheduled.
        return RockDateTime.now().addDays(-7).date;
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 0;
    }
};

export function getTransactionFrequency(definedValueGuid: Guid): TransactionFrequency | null {
    const transactionFrequency = transactionFrequencies.find(a => a.hasDefinedValueGuid(definedValueGuid));
    if (transactionFrequency) {
        return transactionFrequency;
    }
    else {
        return null;
    }
}

export type PaymentPlanFrequency = {
    transactionFrequency: TransactionFrequency;
    startPaymentDate: RockDateTime;
    paymentDeadlineDate: RockDateTime;
    maxNumberOfPayments: number;
    listItemBag: ListItemBag;
};

// function isListItemBagArray(value: unknown): value is ListItemBag[] {
//     return Array.isArray(value) && !!value.length && (value[0] as ListItemBag).value !== undefined;
// }

// export function toPaymentPlanFrequencies(listItemBags: ListItemBag[], paymentDeadlineDate: RockDateTime): PaymentPlanFrequency[];
// export function toPaymentPlanFrequencies(transactionFrequencies: TransactionFrequency[], paymentDeadlineDate: RockDateTime): PaymentPlanFrequency[];
// export function toPaymentPlanFrequencies(frequencies: ListItemBag[] | TransactionFrequency[], paymentDeadlineDate: RockDateTime): PaymentPlanFrequency[] {
//     let transactionFrequencies: TransactionFrequency[];
//     if (isListItemBagArray(frequencies)) {
//         transactionFrequencies = frequencies.filter(frequency => !!frequency.value).map(frequency => getTransactionFrequency(frequency.value ?? "")) as TransactionFrequency[];
//     }
//     else {
//         transactionFrequencies = frequencies;
//     }

//     const paymentPlanFrequencies: PaymentPlanFrequency[] = transactionFrequencies.map(transactionFrequency => {
//         const startPaymentDate = transactionFrequency.getNextTransactionDate();
//         const maxNumberOfPayments = transactionFrequency.getMaxNumberOfTransactionsBetweenDates(startPaymentDate, paymentDeadlineDate);
//         const listItemBag = transactionFrequency.toListItemBag();
//         return {
//             transactionFrequency,
//             startPaymentDate,
//             paymentDeadlineDate,
//             maxNumberOfPayments,
//             listItemBag,
//         };
//     });

//     return paymentPlanFrequencies;
// }

export function getPaymentPlanFrequency(listItemBag: ListItemBag, paymentDeadlineDate: RockDateTime): PaymentPlanFrequency {
    const transactionFrequency = getTransactionFrequency(listItemBag.value ?? "") ?? nullTransactionFrequency;
    const startPaymentDate = transactionFrequency.getNextTransactionDate();
    return {
        listItemBag: listItemBag,
        transactionFrequency,
        startPaymentDate,
        paymentDeadlineDate,
        maxNumberOfPayments: transactionFrequency.getMaxNumberOfTransactionsBetweenDates(startPaymentDate, paymentDeadlineDate)
    };
}

export function formatCurrency(value: number, overrides?: Partial<CurrencyInfoBag> | null | undefined): string {
    const currencyBag: CurrencyInfoBag = {
        decimalPlaces: 2,
        symbol: "$",
        ...overrides
    };

    const formattedValue = toCurrencyOrNull(value, currencyBag);

    if (formattedValue) {
        return formattedValue;
    }
    else {
        return `${currencyBag.symbol}${asFormattedString(value, currencyBag.decimalPlaces)}`;
    }
}