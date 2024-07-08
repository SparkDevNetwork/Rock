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
import { newGuid } from "@Obsidian/Utility/guid";
import {
    RegistrationEntryState,
    RegistrationCostSummaryInfo,
    RegistrantBasicInfo,
    PaymentPlanConfiguration,
    PersonGuid,
    FormFieldGuid,
    FormFieldValue
} from "./types.partial";
import { InjectionKey, Ref, inject, nextTick } from "vue";
import { smoothScrollToTop } from "@Obsidian/Utility/page";
import { PublicComparisonValueBag } from "@Obsidian/ViewModels/Utility/publicComparisonValueBag";
import { ComparisonValue } from "@Obsidian/Types/Reporting/comparisonValue";
import { RegistrationEntryArgsBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryArgsBag";
import { RegistrantsSameFamily } from "@Obsidian/Enums/Event/registrantsSameFamily";
import { RegistrationPersonFieldType } from "@Obsidian/Enums/Event/registrationPersonFieldType";
import { RegistrationFieldSource } from "@Obsidian/Enums/Event/registrationFieldSource";
import { RegistrantBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrantBag";
import { RegistrationEntryFormBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFormBag";
import { RegistrationEntryFormFieldBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFormFieldBag";
import { RegistrationEntryInitializationBox } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryInitializationBox";

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

/**
 * An injection key to provide the data to configure a new payment plan.
 */
export const ConfigurePaymentPlan: InjectionKey<{
    wipPaymentPlanConfiguration: Ref<PaymentPlanConfiguration | null | undefined>;
    finalPaymentPlanConfiguration: Ref<PaymentPlanConfiguration | null | undefined>;
}> = Symbol("registration-configure-payment-plan");

/** An injection key to provide the original field values for each registrant. */
export const OriginalFormFieldValues: InjectionKey<Ref<Record<PersonGuid, Record<FormFieldGuid, FormFieldValue>>>> = Symbol("original-field-values");