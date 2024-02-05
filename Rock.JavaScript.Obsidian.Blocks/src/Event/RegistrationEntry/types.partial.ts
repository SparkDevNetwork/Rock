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
import { RegistrationEntryInitializationBox } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryInitializationBox";
// import { GatewayControlBag } from "@Obsidian/ViewModels/Controls/gatewayControlBag";
// import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
// import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";
// import { SavedFinancialAccountListItemBag } from "@Obsidian/ViewModels/Finance/savedFinancialAccountListItemBag";
// import { FilterExpressionType } from "@Obsidian/Core/Reporting/filterExpressionType";
// import { ComparisonValue } from "@Obsidian/Types/Reporting/comparisonValue";
import { RegistrantBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrantBag";
import { RegistrationEntrySuccessBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntrySuccessBag";
import { RegistrarBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrarBag";

export const enum RegistrationPersonFieldType {
    FirstName = 0,
    LastName = 1,
    Campus = 2,
    Address = 3,
    Email = 4,
    Birthdate = 5,
    Gender = 6,
    MaritalStatus = 7,
    MobilePhone = 8,
    HomePhone = 9,
    WorkPhone = 10,
    Grade = 11,
    ConnectionStatus = 12,
    MiddleName = 13,
    AnniversaryDate = 14,
    Race = 15,
    Ethnicity = 16
}

export const enum RegistrationFieldSource {
    PersonField = 0,
    PersonAttribute = 1,
    GroupMemberAttribute = 2,
    RegistrantAttribute = 4
}

export const enum RegistrarOption {
    PromptForRegistrar = 0,
    PrefillFirstRegistrant = 1,
    UseFirstRegistrant = 2,
    UseLoggedInPerson = 3
}

export const enum RegistrantsSameFamily {
    No = 0,
    Yes = 1,
    Ask = 2
}

// export type SessionRenewalResult = {
//     spotsSecured: number;
//     expirationDateTime: string;
// };

// export type RegistrationEntryBlockViewModel = {
//     currentPersonFamilyGuid?: Guid | null;
//     timeoutMinutes: number | null;
//     registrantsSameFamily: RegistrantsSameFamily;
//     maxRegistrants: number;
//     registrationAttributeTitleStart: string;
//     registrationAttributeTitleEnd: string;
//     instructionsHtml: string;
//     registrantTerm: string;
//     registrationTerm: string;
//     pluralRegistrationTerm: string;
//     pluralRegistrantTerm: string;
//     pluralFeeTerm: string;
//     registrantForms: RegistrationEntryBlockFormViewModel[];
//     fees: RegistrationEntryBlockFeeViewModel[];
//     familyMembers: RegistrationEntryBlockFamilyMemberViewModel[];
//     registrationAttributesStart: PublicAttributeBag[];
//     registrationAttributesEnd: PublicAttributeBag[];
//     forceEmailUpdate: boolean;
//     registrarOption: RegistrarOption;
//     cost: number;
//     gatewayControl: GatewayControlBag;
//     isRedirectGateway: boolean;
//     spotsRemaining: number | null;
//     waitListEnabled: boolean;
//     instanceName: string;
//     amountDueToday: number | null;
//     initialAmountToPay: number | null;
//     session: RegistrationEntryBlockSession | null;
//     isUnauthorized: boolean;
//     hasDiscountsAvailable: boolean;
//     redirectGatewayUrl: string;
//     loginRequiredToRegister: boolean;
//     successViewModel: RegistrationEntryBlockSuccessViewModel | null;
//     allowRegistrationUpdates: boolean;
//     isExistingRegistration: boolean;
//     startAtBeginning: boolean;
//     gatewayGuid: Guid | null;
//     campuses: ListItemBag[];
//     maritalStatuses: ListItemBag[];
//     connectionStatuses: ListItemBag[];
//     grades: ListItemBag[];
//     enableSaveAccount: boolean;
//     savedAccounts: SavedFinancialAccountListItemBag[] | null;
//     registrationInstanceNotFoundMessage: string | null;
//     races: ListItemBag[];
//     ethnicities: ListItemBag[];
//     showSmsOptIn: boolean;

//     isInlineSignatureRequired: boolean;
//     isSignatureDrawn: boolean;
//     signatureDocumentTerm?: string | null;
//     signatureDocumentTemplateName?: string | null;

//     hideProgressBar: boolean;
// };

// export type RegistrationEntryBlockFamilyMemberViewModel = {
//     guid: Guid;
//     familyGuid: Guid;
//     fullName: string;
//     fieldValues: Record<Guid, unknown>;
// };

// export type RegistrationEntryBlockFeeViewModel = {
//     name: string;
//     guid: Guid;
//     allowMultiple: boolean;
//     isRequired: boolean;
//     items: RegistrationEntryBlockFeeItemViewModel[];
//     discountApplies: boolean;
//     hideWhenNoneRemaining?: boolean;
// };

// export type RegistrationEntryBlockFeeItemViewModel = {
//     name: string;
//     guid: Guid;
//     cost: number;
//     originalCountRemaining: number | null;
//     countRemaining: number | null;
// };

// export type RegistrationEntryBlockFormViewModel = {
//     fields: RegistrationEntryBlockFormFieldViewModel[];
// };

// export type RegistrationEntryBlockFormFieldViewModel = {
//     fieldSource: RegistrationFieldSource;
//     personFieldType: RegistrationPersonFieldType;
//     isRequired: boolean;
//     isSharedValue: boolean;
//     attribute: PublicAttributeBag | null;
//     visibilityRuleType: FilterExpressionType;
//     visibilityRules: RegistrationEntryBlockFormFieldRuleViewModel[];
//     preHtml: string;
//     postHtml: string;
//     showOnWaitList: boolean;
//     guid: Guid;
// };

// export type RegistrationEntryBlockFormFieldRuleViewModel = {
//     comparedToRegistrationTemplateFormFieldGuid: Guid;
//     comparisonValue: ComparisonValue;
// };

// export type RegistrantInfo = {
//     isOnWaitList: boolean;

//     /** The family guid that this person is to be a part of */
//     familyGuid: Guid | null;

//     /** If the person were an existing person, this is his/her guid */
//     personGuid: Guid | null;

//     fieldValues: Record<Guid, unknown>;
//     cost: number;
//     feeItemQuantities: Record<Guid, number>;
//     existingSignatureDocumentGuid?: Guid | null,
//     signatureData?: string | null;

//     guid: Guid;
// };

// export type RegistrarInfo = {
//     nickName: string;
//     lastName: string;
//     email: string;
//     updateEmail: boolean;
//     familyGuid: Guid | null;
// };

// export type RegistrationEntryBlockSuccessViewModel = {
//     titleHtml: string;
//     messageHtml: string;
//     transactionCode: string;
//     gatewayPersonIdentifier: string;
// };

// export type RegistrationEntryBlockArgs = {
//     registrationGuid: Guid | null;
//     registrationSessionGuid: Guid | null;
//     registrants: RegistrantInfo[];
//     fieldValues: Record<Guid, unknown> | null;
//     registrar: RegistrarInfo | null;
//     savedAccountGuid: Guid | null;
//     gatewayToken: string | null;
//     discountCode: string | null;
//     amountToPayNow: number;
// };

// export type RegistrationEntryBlockSession = RegistrationEntryBlockArgs & {
//     discountAmount: number;
//     discountPercentage: number;
//     previouslyPaid: number;
//     discountMaxRegistrants: number;
// };

export const enum Step {
    Intro = "intro",
    RegistrationStartForm = "registrationStartForm",
    PerRegistrantForms = "perRegistrantForms",
    RegistrationEndForm = "registrationEndForm",
    Review = "review",
    Payment = "payment",
    Success = "success"
}

// export type RegistrantBasicInfo = {
//     firstName: string;
//     lastName: string;
//     email: string;
//     guid: Guid;
// };

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
};
