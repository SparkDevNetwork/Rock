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
import { RegistrantsSameFamily, RegistrationPersonFieldType, RegistrationFieldSource, RegistrationEntryState, RegistrantBasicInfo, RegistrantInfo, RegistrationEntryBlockFormViewModel, RegistrationEntryBlockFormFieldViewModel, RegistrationEntryBlockViewModel, RegistrationEntryBlockArgs } from "./types.partial";
import { InjectionKey, inject, nextTick } from "vue";
import { smoothScrollToTop } from "@Obsidian/Utility/page";
import { PublicComparisonValueBag } from "@Obsidian/ViewModels/Utility/publicComparisonValueBag";
import { ComparisonValue } from "@Obsidian/Types/Reporting/comparisonValue";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";
import { DefinedType } from "@Obsidian/SystemGuids/definedType";
import { DefinedValue } from "@Obsidian/SystemGuids/definedValue";

/** If all registrants are to be in the same family, but there is no currently authenticated person,
 *  then this guid is used as a common family guid */
const unknownSingleFamilyGuid = newGuid();

/**
 * If there is a forced family guid because of RegistrantsSameFamily setting, then this returns that guid
 * @param currentPerson
 * @param viewModel
 */
export function getForcedFamilyGuid(currentPerson: CurrentPersonBag | null, viewModel: RegistrationEntryBlockViewModel): string | null {
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
export function getDefaultRegistrantInfo(currentPerson: CurrentPersonBag | null, viewModel: RegistrationEntryBlockViewModel, familyGuid: Guid | null): RegistrantInfo {
    const forcedFamilyGuid = getForcedFamilyGuid(currentPerson, viewModel);

    if (forcedFamilyGuid) {
        familyGuid = forcedFamilyGuid;
    }

    // If the family is not specified, then assume the person is in their own family.
    if (!familyGuid && viewModel.registrantsSameFamily === RegistrantsSameFamily.No) {
        familyGuid = newGuid();
    }

    return {
        isOnWaitList: false,
        familyGuid: familyGuid,
        fieldValues: {},
        feeItemQuantities: {},
        guid: newGuid(),
        personGuid: null
    } as RegistrantInfo;
}

export function getRegistrantBasicInfo(registrant: RegistrantInfo, registrantForms: RegistrationEntryBlockFormViewModel[]): RegistrantBasicInfo {
    const fields = registrantForms?.reduce((acc, f) => acc.concat(f.fields ?? []), [] as RegistrationEntryBlockFormFieldViewModel[]) || [];

    const firstNameGuid = fields.find(f => f.personFieldType === RegistrationPersonFieldType.FirstName && f.fieldSource === RegistrationFieldSource.PersonField)?.guid || "";
    const lastNameGuid = fields.find(f => f.personFieldType === RegistrationPersonFieldType.LastName && f.fieldSource === RegistrationFieldSource.PersonField)?.guid || "";
    const emailGuid = fields.find(f => f.personFieldType === RegistrationPersonFieldType.Email && f.fieldSource === RegistrationFieldSource.PersonField)?.guid || "";

    return {
        firstName: (registrant?.fieldValues?.[firstNameGuid] || "") as string,
        lastName: (registrant?.fieldValues?.[lastNameGuid] || "") as string,
        email: (registrant?.fieldValues?.[emailGuid] || "") as string,
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
export const GetPersistSessionArgs: InjectionKey<() => RegistrationEntryBlockArgs> = Symbol("get-persist-session-args");

/** An injection key to provide the function that persists the session. */
export const PersistSession: InjectionKey<(force?: boolean) => Promise<void>> = Symbol("persist-session");

export type TransactionFrequency = {
    /** Determines if this transaction frequency matches the definedValueGuid. */
    hasDefinedValueGuid(definedValueGuid: Guid): boolean;

    /** Gets the number of transactions between the firstDate and secondDate. */
    getMaxNumberOfTransactionsBetweenDates(firstDate: RockDateTime, secondDate: RockDateTime): number;

    getNextTransactionDate(): RockDateTime;
};

const transactionFrequencyOneTime: TransactionFrequency = {
    hasDefinedValueGuid(definedValueGuid: string): boolean {
        return areEqual(definedValueGuid, DefinedValue.TransactionFrequencyOneTime);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDate: RockDateTime, secondDate: RockDateTime): number {
        if (firstDate.isEarlierThan(secondDate)) {
            return 1;
        }
        else {
            return 0;
        }
    },

    getNextTransactionDate(): RockDateTime {
        return RockDateTime.now().date;
    }
};

const transactionFrequencyWeekly: TransactionFrequency = {
    hasDefinedValueGuid(definedValueGuid: string): boolean {
        return areEqual(definedValueGuid, DefinedValue.TransactionFrequencyWeekly);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDate: RockDateTime, secondDate: RockDateTime): number {
        let date = firstDate;
        let numberOfDays = 0;

        while (date.isEarlierThan(secondDate)) {
            numberOfDays++;

            // Add 7 days.
            date = date.addDays(7);

        }

        return numberOfDays;
    },

    getNextTransactionDate(): RockDateTime {
        return RockDateTime.now().date;
    }
};

const transactionFrequencyBiWeekly: TransactionFrequency = {
    hasDefinedValueGuid(definedValueGuid: string): boolean {
        return areEqual(definedValueGuid, DefinedValue.TransactionFrequencyBiweekly);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDate: RockDateTime, secondDate: RockDateTime): number {
        let date = firstDate;
        let numberOfDays = 0;

        while (date.isEarlierThan(secondDate)) {
            numberOfDays++;

            // Add 14 days.
            date = date.addDays(14);
        }

        return numberOfDays;
    },

    getNextTransactionDate(): RockDateTime {
        return RockDateTime.now().date;
    }
};

// These must be strings.
type GetNextDayOption = "end-of-month";

/**
 * Starting from the current date, finds the next day in the month matching one the specified days.
 * @param days Represents the days of the month; e.g., 1 is the 1st, 2 is the 2nd, etc.
 * @example
 * // Assume current date is 2-8-2024.
 * getNextDay(13, 15, 31); // 2-13-2024
 * getNextDay(1, 15); // 2-15-2024
 * getNextDay(1); // 3-1-2024
 * getNextDay(8); // 2-8-2024
 */
function getNextDay(...days: (number | GetNextDayOption)[]): RockDateTime | null {
    let date = RockDateTime.now();

    // // Since there are a maximum of 31 days in a month,
    // // stop processing after we've tried matching all days
    // // between now and 31 days from now.
    // let attempts = 0;
    // while (attempts++ < 31) {
    //     for (const day of days) {
    //         if (date.day === day) {
    //             return date;
    //         }
    //     }

    //     // Try the next day.
    //     date = date.addDays(1);
    // }

    // No matches were found.
    //return null;

    days = days
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

    // Try once per month for an entire year.
    let attempt = 0;
    while (attempt++ <= 12) {
        // Find the first day that either matches or is later than the current date.
        for (let i = 0; i < days.length; i++) {
            const day = days[i];
            if (typeof day === "number") {
                if (day === date.day) {
                    // The current date was one of the provided days
                    // so return it.
                    return date;
                }
                else if (date.day < day) {
                    // This is the first day that is later than the current date
                    // so return it.
                    // If date is 2-8-2024, and sortedDays[i] === 15
                    // then add 15 minus 8 days to the date
                    // to get a date of 2-15-2024.
                    return date.addDays(day - date.day);
                }
            }
            // Check special options.
            else if (day === "end-of-month") {
                // All other days in the list were before the current date
                // and the next allowed date is the end of the current month,
                // so return the end of the current month.
                // TODO JMH Does the date get modified by calling endOfMonth?
                debugger;
                return date.endOfMonth();
            }
        }

        // If we get here, then all the provided dates are before the current date.
        // Move to the start of the next month and try again.
            date = date.addDays(1 - date.day).addMonths(1);
    }

    // This shouldn't happen but return null if we get here.
    return null;
}

const transactionFrequencyTwiceAMonth: TransactionFrequency = {
    hasDefinedValueGuid(definedValueGuid: string): boolean {
        return areEqual(definedValueGuid, DefinedValue.TransactionFrequencyFirstAndFifteenth) || areEqual(definedValueGuid, DefinedValue.TransactionFrequencyTwicemonthly);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDate: RockDateTime, secondDate: RockDateTime): number {
        let date = firstDate;

        if (date.day > 15) {
            // Set the date to the 1st of the next month.
            date = date.addDays(1 - date.day).addMonths(1);
        }
        else if (date.day < 15 && date.day > 1) {
            // Set the date to the 15th of the current month.
            date = date.addDays(15 - date.day);
        }

        let numberOfDays = 0;

        while (date.isEarlierThan(secondDate)) {
            if (date.day === 1 || date.day === 15) {
                numberOfDays++;
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

        return numberOfDays;
    },

    getNextTransactionDate(): RockDateTime {
        return getNextDay(1, 15) ?? RockDateTime.now().date;
    }
};

const transactionFrequencyMonthly: TransactionFrequency = {
    hasDefinedValueGuid(definedValueGuid: string): boolean {
        return areEqual(definedValueGuid, DefinedValue.TransactionFrequencyMonthly);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDate: RockDateTime, secondDate: RockDateTime): number {
        let date = firstDate;
        let numberOfDays = 0;

        while (date.isEarlierThan(secondDate)) {
            numberOfDays++;

            // Ideally, we would keep adding 1 month, but this might not work if we start on the 31st of a month and the following month only has 30 days.
            // TODO JMH test if RockDateTime can handle this.
            date = date.addMonths(1);
        }

        return numberOfDays;
    },

    getNextTransactionDate(): RockDateTime {
        return RockDateTime.now().date;
    }
};

const transactionFrequencyQuarterly: TransactionFrequency = {
    hasDefinedValueGuid(definedValueGuid: string): boolean {
        return areEqual(definedValueGuid, DefinedValue.TransactionFrequencyQuarterly);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDate: RockDateTime, secondDate: RockDateTime): number {
        let date = firstDate;
        let numberOfTransactions = 0;

        while (date.isEarlierThan(secondDate)) {
            numberOfTransactions++;

            // Ideally, we would keep adding 3 months, but this might not work if we start on the 31st of a month and the third following month only has 30 days.
            // TODO JMH test if RockDateTime can handle this.
            date = date.addMonths(3);
        }

        return numberOfTransactions;
    },

    getNextTransactionDate(): RockDateTime {
        return RockDateTime.now().date;
    }
};

const transactionFrequencyTwiceAYear: TransactionFrequency = {
    hasDefinedValueGuid(definedValueGuid: string): boolean {
        return areEqual(definedValueGuid, DefinedValue.TransactionFrequencyTwiceyearly);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDate: RockDateTime, secondDate: RockDateTime): number {
        let date = firstDate;
        let numberOfTransactions = 0;

        while (date.isEarlierThan(secondDate)) {
            numberOfTransactions++;

            // Ideally, we would keep adding 6 months, but this might not work if we start on the 31st of a month and the sixth following month only has 30 days.
            // TODO JMH test if RockDateTime can handle this.
            date = date.addMonths(6);
        }

        return numberOfTransactions;
    },

    getNextTransactionDate(): RockDateTime {
        return RockDateTime.now().date;
    }
};

const transactionFrequencyYearly: TransactionFrequency = {
    hasDefinedValueGuid(definedValueGuid: string): boolean {
        return areEqual(definedValueGuid, DefinedValue.TransactionFrequencyYearly);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDate: RockDateTime, secondDate: RockDateTime): number {
        let date = firstDate;
        let numberOfTransactions = 0;

        while (date.isEarlierThan(secondDate)) {
            numberOfTransactions++;

            // Ideally, we would keep adding 1 year, but this might not work if we start on the 29th of February in a leap year and the following year only has 28 days.
            // TODO JMH test if RockDateTime can handle this.
            date = date.addYears(1);
        }

        return numberOfTransactions;
    },

    getNextTransactionDate(): RockDateTime {
        return RockDateTime.now().date;
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
    hasDefinedValueGuid(definedValueGuid: string): boolean {
        return false;
    },

    getMaxNumberOfTransactionsBetweenDates(firstDate: RockDateTime, secondDate: RockDateTime): number {
        return 0;
    },

    getNextTransactionDate(): RockDateTime {
        return RockDateTime.now().date;
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