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
    RegistrantBasicInfo
} from "./types.partial";
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
    readonly definedValueGuid: Guid;

    /** Determines if this transaction frequency matches the definedValueGuid. */
    hasDefinedValueGuid(guid: Guid): boolean;

    /** Gets the number of transactions between the first and second dates, inclusively. */
    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number;

    /** Returns the desired date if it is valid; otherwise, the next valid date is returned or null if there are no valid dates. */
    getValidTransactionDate(firstDateTime: RockDateTime, secondDateTime: RockDateTime, desiredDate: RockDateTime): RockDateTime | null;

    /** Returns the next valid date following the previous date or null if there are no valid dates. */
    getNextTransactionDate(firstDateTime: RockDateTime, secondDateTime: RockDateTime, previousDate: RockDateTime): RockDateTime | null;

    maxNumberOfPaymentsForOneYear: number;
};

const transactionFrequencyOneTime: TransactionFrequency = {
    get definedValueGuid(): Guid {
        return DefinedValue.TransactionFrequencyOneTime;
    },

    hasDefinedValueGuid(guid: Guid): boolean {
        return areEqual(guid, this.definedValueGuid);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number {
        const date = this.getValidTransactionDate(firstDateTime, secondDateTime, firstDateTime);

        if (date) {
            return 1;
        }
        else {
            return 0;
        }
    },

    getValidTransactionDate(firstDate: RockDateTime, secondDate: RockDateTime, desiredDate: RockDateTime): RockDateTime | null {
        firstDate = firstDate.date;
        secondDate = secondDate.date;
        desiredDate = desiredDate.date;
        const today = RockDateTime.now().date;
        const tomorrow = today.addDays(1);

        const earliestPossibleDate =
            firstDate.isLaterThan(today)
                ? firstDate // First date is in the future (including tomorrow).
                : tomorrow; // Tomorrow is the earliest date.

        if (desiredDate.isLaterThan(secondDate)) {
            // The desired date is after the second date,
            // so there are no more valid dates.
            return null;
        }
        else if (desiredDate.isEarlierThan(earliestPossibleDate)) {
            // The desired date is before the earliest possible date (first date or tomorrow).
            // The next valid date to use is the earliest possible date.
            return earliestPossibleDate;
        }
        else {
            // The desired date is valid (falls between the first and second dates).
            return desiredDate;
        }
    },

    getNextTransactionDate(_firstDateTime: RockDateTime, _secondDateTime: RockDateTime, _previousDate: RockDateTime): RockDateTime | null {
        // One-time frequency doesn't have a next date so return null.
        return null;
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 1;
    }
};

const transactionFrequencyWeekly: TransactionFrequency = {
    get definedValueGuid(): Guid {
        return DefinedValue.TransactionFrequencyWeekly;
    },

    hasDefinedValueGuid(guid: Guid): boolean {
        return areEqual(guid, this.definedValueGuid);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number {
        let date = this.getValidTransactionDate(firstDateTime, secondDateTime, firstDateTime);
        let numberOfTransactions = 0;

        while (date) {
            numberOfTransactions++;
            date = this.getNextTransactionDate(firstDateTime, secondDateTime, date);
        }

        return numberOfTransactions;
    },

    getValidTransactionDate(firstDate: RockDateTime, secondDate: RockDateTime, desiredDate: RockDateTime): RockDateTime | null {
        firstDate = firstDate.date;
        secondDate = secondDate.date;
        desiredDate = desiredDate.date;
        const today = RockDateTime.now().date;
        const tomorrow = today.addDays(1);

        const earliestPossibleDate =
            firstDate.isLaterThan(today)
                ? firstDate // First date is in the future (including tomorrow).
                : tomorrow; // Tomorrow is the earliest date.

        if (desiredDate.isLaterThan(secondDate)) {
            // The desired date is after the second date,
            // so there are no more valid dates.
            return null;
        }
        else if (desiredDate.isEarlierThan(earliestPossibleDate)) {
            // The desired date is before the earliest possible date (first date or tomorrow).
            // The next valid date to use is the earliest possible date.
            return earliestPossibleDate;
        }
        else {
            // The desired date is valid (falls between the first and second dates).
            return desiredDate;
        }
    },

    getNextTransactionDate(firstDateTime: RockDateTime, secondDateTime: RockDateTime, previousDate: RockDateTime): RockDateTime | null {
        previousDate = previousDate.date;
        const desiredDate = previousDate.addDays(7);
        return this.getValidTransactionDate(firstDateTime, secondDateTime, desiredDate);
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 52;
    },
};

const transactionFrequencyBiWeekly: TransactionFrequency = {
    get definedValueGuid(): Guid {
        return DefinedValue.TransactionFrequencyBiweekly;
    },

    hasDefinedValueGuid(guid: Guid): boolean {
        return areEqual(guid, this.definedValueGuid);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number {
        let date = this.getValidTransactionDate(firstDateTime, secondDateTime, firstDateTime);
        let numberOfTransactions = 0;

        while (date) {
            numberOfTransactions++;
            date = this.getNextTransactionDate(firstDateTime, secondDateTime, date);
        }

        return numberOfTransactions;
    },

    getValidTransactionDate(firstDate: RockDateTime, secondDate: RockDateTime, desiredDate: RockDateTime): RockDateTime | null {
        firstDate = firstDate.date;
        secondDate = secondDate.date;
        desiredDate = desiredDate.date;
        const today = RockDateTime.now().date;
        const tomorrow = today.addDays(1);

        const earliestPossibleDate =
            firstDate.isLaterThan(today)
                ? firstDate // First date is in the future (including tomorrow).
                : tomorrow; // Tomorrow is the earliest date.

        if (desiredDate.isLaterThan(secondDate)) {
            // The desired date is after the second date,
            // so there are no more valid dates.
            return null;
        }
        else if (desiredDate.isEarlierThan(earliestPossibleDate)) {
            // The desired date is before the earliest possible date (first date or tomorrow).
            // The next valid date to use is the earliest possible date.
            return earliestPossibleDate;
        }
        else {
            // The desired date is valid (falls between the first and second dates).
            return desiredDate;
        }
    },

    getNextTransactionDate(firstDateTime: RockDateTime, secondDateTime: RockDateTime, previousDate: RockDateTime): RockDateTime | null {
        previousDate = previousDate.date;
        const desiredDate = previousDate.addDays(14);
        return this.getValidTransactionDate(firstDateTime, secondDateTime, desiredDate);
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 26;
    }
};

// These must be strings.
type GetNextDayOption = "end-of-month";

/**
 * From the starting date, finds the next day matching one the specified days.
 *
 * If the `startingDate` matches a `potentialDay`, then `startingDate` is returned.
 * @param potentialDays Represents the days of the month; e.g., 1 is the 1st, 2 is the 2nd, etc.
 * @example
 * // Assume date is a Date object with the value 2-8-2024.
 * getNextDay(date, 13, 15, 31); // 2-13-2024
 * getNextDay(date, 1, 15); // 2-15-2024
 * getNextDay(date, 1); // 3-1-2024
 * getNextDay(date, 8); // 2-8-2024
 * getNextDay(date, "end-of-month"); // 2-29-2024
 */
function getNextDay(startingDate: RockDateTime, ...potentialDays: (number | GetNextDayOption)[]): RockDateTime {
    let date = startingDate;

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
    throw "An unexpected error occurred while processing payment dates.";
}

const transactionFrequencyFirstAndFifteenth: TransactionFrequency = {
    get definedValueGuid(): Guid {
        return DefinedValue.TransactionFrequencyFirstAndFifteenth;
    },

    hasDefinedValueGuid(guid: Guid): boolean {
        return areEqual(guid, this.definedValueGuid);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number {
        let date = this.getValidTransactionDate(firstDateTime, secondDateTime, firstDateTime);
        let numberOfTransactions = 0;

        while (date) {
            numberOfTransactions++;
            date = this.getNextTransactionDate(firstDateTime, secondDateTime, date);
        }

        return numberOfTransactions;
    },

    getValidTransactionDate(firstDate: RockDateTime, secondDate: RockDateTime, desiredDate: RockDateTime): RockDateTime | null {
        const earliestPossibleDate = getNextDay(firstDate.date, 1, 15);
        secondDate = secondDate.date;
        const earliestDesiredDate = getNextDay(desiredDate.date, 1, 15);
        

        if (earliestDesiredDate.isLaterThan(secondDate)) {
            // The desired date is after the second date,
            // so there are no more valid dates.
            return null;
        }
        else if (earliestDesiredDate.isEarlierThan(earliestPossibleDate)) {
            // The desired date is before the earliest possible date (first date or tomorrow).
            // The next valid date to use is the earliest possible date.
            return earliestPossibleDate;
        }
        else {
            // The desired date is valid (falls between the first and second dates).
            return earliestDesiredDate;
        }
    },

    getNextTransactionDate(firstDateTime: RockDateTime, secondDateTime: RockDateTime, previousDate: RockDateTime): RockDateTime | null {
        previousDate = previousDate.date;
        const desiredDate = getNextDay(previousDate.addDays(1), ...[1, 15]);
        return this.getValidTransactionDate(firstDateTime, secondDateTime, desiredDate);
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 24;
    }
};

const transactionFrequencyTwiceMonthly: TransactionFrequency = {
    get definedValueGuid(): Guid {
        return DefinedValue.TransactionFrequencyTwicemonthly;
    },

    hasDefinedValueGuid(guid: Guid): boolean {
        return areEqual(guid, this.definedValueGuid);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number {
        let date = this.getValidTransactionDate(firstDateTime, secondDateTime, firstDateTime);
        let numberOfTransactions = 0;

        while (date) {
            numberOfTransactions++;
            date = this.getNextTransactionDate(firstDateTime, secondDateTime, date);
        }

        return numberOfTransactions;
    },

    getValidTransactionDate(firstDate: RockDateTime, secondDate: RockDateTime, desiredDate: RockDateTime): RockDateTime | null {
        firstDate = firstDate.date;
        secondDate = secondDate.date;
        desiredDate = desiredDate.date;
        const today = RockDateTime.now().date;
        const tomorrow = today.addDays(1);

        const earliestPossibleDate =
            firstDate.isLaterThan(today)
                ? firstDate // First date is in the future (including tomorrow).
                : tomorrow; // Tomorrow is the earliest date.

        if (desiredDate.isLaterThan(secondDate)) {
            // The desired date is after the second date,
            // so there are no more valid dates.
            return null;
        }
        else if (desiredDate.isEarlierThan(earliestPossibleDate)) {
            // The desired date is before the earliest possible date (first date or tomorrow).
            // The next valid date to use is the earliest possible date.
            return earliestPossibleDate;
        }
        else {
            // The desired date is valid (falls between the first and second dates).
            return desiredDate;
        }
    },

    getNextTransactionDate(firstDateTime: RockDateTime, secondDateTime: RockDateTime, previousDate: RockDateTime): RockDateTime | null {
        previousDate = previousDate.date;
        const desiredDate = getNextDay(previousDate.addDays(1), ...[1, 15]);
        return this.getValidTransactionDate(firstDateTime, secondDateTime, desiredDate);
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 24;
    }
};

const transactionFrequencyMonthly: TransactionFrequency = {
    get definedValueGuid(): Guid {
        return DefinedValue.TransactionFrequencyMonthly;
    },

    hasDefinedValueGuid(guid: Guid): boolean {
        return areEqual(guid, this.definedValueGuid);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number {
        let date = this.getValidTransactionDate(firstDateTime, secondDateTime, firstDateTime);
        let numberOfTransactions = 0;

        while (date) {
            numberOfTransactions++;
            date = this.getNextTransactionDate(firstDateTime, secondDateTime, date);
        }

        return numberOfTransactions;
    },

    getValidTransactionDate(firstDate: RockDateTime, secondDate: RockDateTime, desiredDate: RockDateTime): RockDateTime | null {
        firstDate = firstDate.date;
        secondDate = secondDate.date;
        desiredDate = desiredDate.date;
        const today = RockDateTime.now().date;
        const tomorrow = today.addDays(1);

        const earliestPossibleDate =
            firstDate.isLaterThan(today)
                ? firstDate // First date is in the future (including tomorrow).
                : tomorrow; // Tomorrow is the earliest date.

        if (desiredDate.isLaterThan(secondDate)) {
            // The desired date is after the second date,
            // so there are no more valid dates.
            return null;
        }
        else if (desiredDate.isEarlierThan(earliestPossibleDate)) {
            // The desired date is before the earliest possible date (first date or tomorrow).
            // The next valid date to use is the earliest possible date.
            return earliestPossibleDate;
        }
        else {
            // The desired date is valid (falls between the first and second dates).
            return desiredDate;
        }
    },

    getNextTransactionDate(firstDateTime: RockDateTime, secondDateTime: RockDateTime, previousDate: RockDateTime): RockDateTime | null {
        firstDateTime = firstDateTime.date;
        previousDate = previousDate.date;
        let desiredDate: RockDateTime;

        // If the first date is the last day of the month
        // then this function will increment by 1 months and
        // automatically choose the last day of the month.
        if (firstDateTime.isEqualTo(firstDateTime.endOfMonth().date)) {
            desiredDate = previousDate.addMonths(1).endOfMonth().date;
        }
        else {
            desiredDate = previousDate.addMonths(1);
        }

        return this.getValidTransactionDate(firstDateTime, secondDateTime, desiredDate);
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 12;
    }
};

const transactionFrequencyQuarterly: TransactionFrequency = {
    get definedValueGuid(): Guid {
        return DefinedValue.TransactionFrequencyQuarterly;
    },

    hasDefinedValueGuid(guid: Guid): boolean {
        return areEqual(guid, this.definedValueGuid);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number {
        let date = this.getValidTransactionDate(firstDateTime, secondDateTime, firstDateTime);
        let numberOfTransactions = 0;

        while (date) {
            numberOfTransactions++;
            date = this.getNextTransactionDate(firstDateTime, secondDateTime, date);
        }

        return numberOfTransactions;
    },

    getValidTransactionDate(firstDate: RockDateTime, secondDate: RockDateTime, desiredDate: RockDateTime): RockDateTime | null {
        firstDate = firstDate.date;
        secondDate = secondDate.date;
        desiredDate = desiredDate.date;
        const today = RockDateTime.now().date;
        const tomorrow = today.addDays(1);

        const earliestPossibleDate =
            firstDate.isLaterThan(today)
                ? firstDate // First date is in the future (including tomorrow).
                : tomorrow; // Tomorrow is the earliest date.

        if (desiredDate.isLaterThan(secondDate)) {
            // The desired date is after the second date,
            // so there are no more valid dates.
            return null;
        }
        else if (desiredDate.isEarlierThan(earliestPossibleDate)) {
            // The desired date is before the earliest possible date (first date or tomorrow).
            // The next valid date to use is the earliest possible date.
            return earliestPossibleDate;
        }
        else {
            // The desired date is valid (falls between the first and second dates).
            return desiredDate;
        }
    },

    getNextTransactionDate(firstDateTime: RockDateTime, secondDateTime: RockDateTime, previousDate: RockDateTime): RockDateTime | null {
        firstDateTime = firstDateTime.date;
        previousDate = previousDate.date;
        let desiredDate: RockDateTime;

        // If the first date is the last day of the month
        // then this function will increment by 3 months and
        // automatically choose the last day of the month.
        if (firstDateTime.isEqualTo(firstDateTime.endOfMonth().date)) {
            desiredDate = previousDate.addMonths(3).endOfMonth().date;
        }
        else {
            desiredDate = previousDate.addMonths(3);
        }

        return this.getValidTransactionDate(firstDateTime, secondDateTime, desiredDate);
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 4;
    }
};

const transactionFrequencyTwiceAYear: TransactionFrequency = {
    get definedValueGuid(): Guid {
        return DefinedValue.TransactionFrequencyTwiceyearly;
    },

    hasDefinedValueGuid(guid: Guid): boolean {
        return areEqual(guid, this.definedValueGuid);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number {
        let date = this.getValidTransactionDate(firstDateTime, secondDateTime, firstDateTime);
        let numberOfTransactions = 0;

        while (date) {
            numberOfTransactions++;
            date = this.getNextTransactionDate(firstDateTime, secondDateTime, date);
        }

        return numberOfTransactions;
    },

    getValidTransactionDate(firstDate: RockDateTime, secondDate: RockDateTime, desiredDate: RockDateTime): RockDateTime | null {
        firstDate = firstDate.date;
        secondDate = secondDate.date;
        desiredDate = desiredDate.date;
        const today = RockDateTime.now().date;
        const tomorrow = today.addDays(1);

        const earliestPossibleDate =
            firstDate.isLaterThan(today)
                ? firstDate // First date is in the future (including tomorrow).
                : tomorrow; // Tomorrow is the earliest date.

        if (desiredDate.isLaterThan(secondDate)) {
            // The desired date is after the second date,
            // so there are no more valid dates.
            return null;
        }
        else if (desiredDate.isEarlierThan(earliestPossibleDate)) {
            // The desired date is before the earliest possible date (first date or tomorrow).
            // The next valid date to use is the earliest possible date.
            return earliestPossibleDate;
        }
        else {
            // The desired date is valid (falls between the first and second dates).
            return desiredDate;
        }
    },

    getNextTransactionDate(firstDateTime: RockDateTime, secondDateTime: RockDateTime, previousDate: RockDateTime): RockDateTime | null {
        firstDateTime = firstDateTime.date;
        previousDate = previousDate.date;
        let desiredDate: RockDateTime;

        // If the first date is the last day of the month
        // then this function will increment by 6 months and
        // automatically choose the last day of the month.
        if (firstDateTime.isEqualTo(firstDateTime.endOfMonth().date)) {
            desiredDate = previousDate.addMonths(6).endOfMonth().date;
        }
        else {
            desiredDate = previousDate.addMonths(6);
        }

        return this.getValidTransactionDate(firstDateTime, secondDateTime, desiredDate);
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 2;
    }
};

const transactionFrequencyYearly: TransactionFrequency = {
    get definedValueGuid(): Guid {
        return DefinedValue.TransactionFrequencyYearly;
    },

    hasDefinedValueGuid(guid: Guid): boolean {
        return areEqual(guid, this.definedValueGuid);
    },

    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime): number {
        let date = this.getValidTransactionDate(firstDateTime, secondDateTime, firstDateTime);
        let numberOfTransactions = 0;

        while (date) {
            numberOfTransactions++;
            date = this.getNextTransactionDate(firstDateTime, secondDateTime, date);
        }

        return numberOfTransactions;
    },

    getValidTransactionDate(firstDate: RockDateTime, secondDate: RockDateTime, desiredDate: RockDateTime): RockDateTime | null {
        firstDate = firstDate.date;
        secondDate = secondDate.date;
        desiredDate = desiredDate.date;
        const today = RockDateTime.now().date;
        const tomorrow = today.addDays(1);

        const earliestPossibleDate =
            firstDate.isLaterThan(today)
                ? firstDate // First date is in the future (including tomorrow).
                : tomorrow; // Tomorrow is the earliest date.

        if (desiredDate.isLaterThan(secondDate)) {
            // The desired date is after the second date,
            // so there are no more valid dates.
            return null;
        }
        else if (desiredDate.isEarlierThan(earliestPossibleDate)) {
            // The desired date is before the earliest possible date (first date or tomorrow).
            // The next valid date to use is the earliest possible date.
            return earliestPossibleDate;
        }
        else {
            // The desired date is valid (falls between the first and second dates).
            return desiredDate;
        }
    },

    getNextTransactionDate(firstDateTime: RockDateTime, secondDateTime: RockDateTime, previousDate: RockDateTime): RockDateTime | null {
        firstDateTime = firstDateTime.date;
        previousDate = previousDate.date;
        let desiredDate: RockDateTime;

        // If the first date is the last day of the month
        // then this function will increment by 1 year and
        // automatically choose the last day of the month.
        if (firstDateTime.isEqualTo(firstDateTime.endOfMonth().date)) {
            desiredDate = previousDate.addYears(1).endOfMonth().date;
        }
        else {
            desiredDate = previousDate.addMonths(1);
        }

        return this.getValidTransactionDate(firstDateTime, secondDateTime, desiredDate);
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 1;
    }
};

const transactionFrequencies: TransactionFrequency[] = [
    transactionFrequencyOneTime,
    transactionFrequencyWeekly,
    transactionFrequencyBiWeekly,
    transactionFrequencyFirstAndFifteenth,
    transactionFrequencyTwiceMonthly,
    transactionFrequencyMonthly,
    transactionFrequencyQuarterly,
    transactionFrequencyTwiceAYear,
    transactionFrequencyYearly
];

export const noopTransactionFrequency: TransactionFrequency = {
    get definedValueGuid(): string {
        return "";
    },

    hasDefinedValueGuid(_guid: Guid): boolean {
        return false;
    },

    get maxNumberOfPaymentsForOneYear(): number {
        return 0;
    },

    getMaxNumberOfTransactionsBetweenDates(_firstDateTime: RockDateTime, _secondDateTime: RockDateTime): number {
        return 0;
    },

    getValidTransactionDate(_firstDateTime: RockDateTime, _secondDateTime: RockDateTime, _desiredDate: RockDateTime): RockDateTime | null {
        return null;
    },

    getNextTransactionDate: function (_firstDateTime: RockDateTime, _secondDateTime: RockDateTime, _previousDate: RockDateTime): RockDateTime | null {
        return null;
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
    startPaymentDate: RockDateTime | null;
    paymentDeadlineDate: RockDateTime;
    maxNumberOfPayments: number;
    listItemBag: ListItemBag;
    /** Returns the desired date if it is valid; otherwise, the next valid date is returned or null if there are no valid dates. */
    getValidTransactionDate(desiredDate: RockDateTime): RockDateTime | null;
    /** Returns the next valid date following the previous date or null if there are no valid dates. */
    getNextTransactionDate(previousDate: RockDateTime): RockDateTime | null;
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
    const transactionFrequency = getTransactionFrequency(listItemBag?.value ?? "") ?? noopTransactionFrequency;

    // Although tomorrow is the earliest possible date in the current implementation,
    // keep that logic in the `getValidTransactionDate` function.
    const today = RockDateTime.now().date;
    const startPaymentDate = transactionFrequency.getValidTransactionDate(today, paymentDeadlineDate, today);

    if (startPaymentDate) {
        return {
            listItemBag: listItemBag,
            transactionFrequency,
            startPaymentDate,
            paymentDeadlineDate,
            maxNumberOfPayments: transactionFrequency.getMaxNumberOfTransactionsBetweenDates(startPaymentDate, paymentDeadlineDate),
            getNextTransactionDate(previousDate: RockDateTime) {
                return transactionFrequency.getNextTransactionDate(startPaymentDate, paymentDeadlineDate, previousDate);
            },
            getValidTransactionDate(desiredDate: RockDateTime) {
                return transactionFrequency.getValidTransactionDate(startPaymentDate, paymentDeadlineDate, desiredDate);
            }
        };
    }
    else {
        return noopPaymentPlanFrequency;
    }
}

export const noopPaymentPlanFrequency: PaymentPlanFrequency = {
    listItemBag: {},
    transactionFrequency: noopTransactionFrequency,
    maxNumberOfPayments: 0,
    // Start and end dates don't matter.
    startPaymentDate: null,
    paymentDeadlineDate: RockDateTime.now().date.addDays(-1),
    getNextTransactionDate(_previousDate: RockDateTime) {
        return null;
    },
    getValidTransactionDate(_desiredDate: RockDateTime) {
        return null;
    }
};

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