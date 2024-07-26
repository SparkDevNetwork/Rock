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
import { RegistrationEntryInitializationBox } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryInitializationBox";
import { RegistrationEntrySuccessBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntrySuccessBag";
import { RegistrationEntryCreatePaymentPlanRequestBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryCreatePaymentPlanRequestBag";
import { RockCurrency } from "@Obsidian/Utility/rockCurrency";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

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

export type PaymentPlanFrequency = {
    transactionFrequency: TransactionFrequency;
    startPaymentDate: RockDateTime;
    paymentDeadlineDate: RockDateTime;
    maxNumberOfPayments: number;
    listItemBag: ListItemBag;
    /** Returns the desired date if it is valid; otherwise, the next valid date is returned or null if there are no valid dates. */
    getValidTransactionDate(desiredDate: RockDateTime): RockDateTime | null;
    /** Returns the next valid date following the previous date or null if there are no valid dates. */
    getNextTransactionDate(previousDate: RockDateTime): RockDateTime | null;
};

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
    paymentPlan: RegistrationEntryCreatePaymentPlanRequestBag | null;
};

export type PaymentPlanConfiguration = {
    balanceDue: RockCurrency;
    paymentPlanFrequencies: PaymentPlanFrequency[];
    paymentPlanFrequency: PaymentPlanFrequency;
    startDate: RockDateTime;
    endDate: RockDateTime;
    amountToPayToday: RockCurrency;
    amountToPayTodayAdjustment: RockCurrency;
    amountToPayTodayPlusAdjustment: RockCurrency;
    numberOfPayments: number;
    amountPerPayment: RockCurrency;
    minAmountToPayToday: RockCurrency;
};

export type PaymentPlanConfigurationOptions = {
    balanceDue: RockCurrency;
    desiredAllowedPaymentPlanFrequencies: PaymentPlanFrequency[];
    desiredPaymentPlanFrequency: PaymentPlanFrequency;
    desiredStartDate: RockDateTime;
    endDate: RockDateTime;
    amountToPayToday: RockCurrency;
    desiredNumberOfPayments: number;
    minAmountToPayToday: RockCurrency;
};

export type PersonGuid = Guid; // Not the registrant guid.
export type FormFieldGuid = Guid;
export type FormFieldValue = unknown;

export type TransactionFrequency = {
    readonly definedValueGuid: Guid;

    /** Determines if this transaction frequency matches the definedValueGuid. */
    hasDefinedValueGuid(guid: Guid): boolean;

    /**
     * Gets the number of transactions between the first and second dates, inclusively.
     *
     * Assuming each transaction can pay as little as 1 cent (for USD), the amountToPay will also limit the max number of transactions; e.g., an amountToPay of $0.25 can only have a maximum of 25 transactions.
     */
    getMaxNumberOfTransactionsBetweenDates(firstDateTime: RockDateTime, secondDateTime: RockDateTime, amountToPay: RockCurrency): number;

    /** Returns the desired date if it is valid; otherwise, the next valid date is returned or null if there are no valid dates. */
    getValidTransactionDate(firstDateTime: RockDateTime, secondDateTime: RockDateTime, desiredDate: RockDateTime): RockDateTime | null;

    /** Returns the next valid date following the previous date or null if there are no valid dates. */
    getNextTransactionDate(firstDateTime: RockDateTime, secondDateTime: RockDateTime, previousDate: RockDateTime): RockDateTime | null;

    maxNumberOfPaymentsForOneYear: number;
};

export type GetNextDayOption = "end-of-month";

export type RegistrationEntryTerminology = {
    discountCodeSingularLowerCase: string;
    discountCodeSingularTitleCase: string;
    feePluralLowerCase: string;
    feePluralTitleCase: string;
    feeSingularLowerCase: string;
    registrantPluralLowerCase: string;
    registrantSingularLowerCase: string;
    registrantSingularTitleCase: string;
    registrationPluralLowerCase: string;
    registrationSingularLowerCase: string;
    registrationSingularTitleCase: string;
    signatureDocumentSingularTitleCase: string;
};