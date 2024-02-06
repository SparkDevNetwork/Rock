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
import { PartialProps, RequiredNonNullableProps, RequiredProps, TypeProps } from "@Obsidian/Utility/typeUtils";
import { RegistrationEntryInitializationBox } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryInitializationBox";
import { RegistrantBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrantBag";
import { RegistrationEntrySuccessBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntrySuccessBag";
import { RegistrarBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrarBag";
import { RegistrationEntryArgsBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryArgsBag";
import { SessionRenewalResultBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/sessionRenewalResultBag";
import { RegistrationEntryFamilyMemberBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFamilyMemberBag";
import { RegistrationEntryFeeItemBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFeeItemBag";
import { RegistrationEntryFeeBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFeeBag";
import { RegistrationEntryFormBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFormBag";
import { RegistrationEntryFormFieldBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFormFieldBag";
import { ComparisonValue } from "@Obsidian/Types/Reporting/comparisonValue";
import { RegistrationEntrySessionBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntrySessionBag";

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

export type SessionRenewalResult =
    RequiredNonNullableProps<

    // Base data structure.
    SessionRenewalResultBag,

    // Define properties that must have a value.
    "expirationDateTime">;

export type RegistrationEntryBlockViewModel =
    TypeProps<
    TypeProps<
    TypeProps<
    TypeProps<
    TypeProps<
    TypeProps<
    TypeProps<
    RequiredNonNullableProps<
    RequiredProps<

    // Base data structure.
    RegistrationEntryInitializationBox,

    // Define required properties (can still be null).
    "timeoutMinutes" | "spotsRemaining" | "amountDueToday" | "initialAmountToPay" | "session" | "successViewModel" | "gatewayGuid" | "savedAccounts" | "registrationInstanceNotFoundMessage">,

    // Define properties that must have a value.
    "registrationAttributeTitleStart" | "registrationAttributeTitleEnd" | "instructionsHtml" | "registrantTerm" | "registrationTerm" | "pluralRegistrationTerm" | "pluralRegistrantTerm" | "pluralFeeTerm"
    | "registrantForms" | "fees" | "familyMembers" | "registrationAttributesStart" | "registrationAttributesEnd" | "gatewayControl" | "instanceName" | "redirectGatewayUrl" | "campuses" | "maritalStatuses"
    | "connectionStatuses" | "grades" | "races" | "ethnicities">,

    // Override property types.
    "familyMembers", RegistrationEntryBlockFamilyMemberViewModel[]>,
    "fees", RegistrationEntryBlockFeeViewModel[]>,
    "registrantForms", RegistrationEntryBlockFormViewModel[]>,
    "registrarOption", RegistrarOption>,
    "registrantsSameFamily", RegistrantsSameFamily>,
    "session", RegistrationEntryBlockSession | null>,
    "successViewModel", RegistrationEntryBlockSuccessViewModel | null>;

export type RegistrationEntryBlockFamilyMemberViewModel =
    RequiredNonNullableProps<

    // Base data structure.
    RegistrationEntryFamilyMemberBag,

    // Define properties that must have a value.
    "familyGuid" | "fieldValues" | "fullName" | "guid">;

export type RegistrationEntryBlockFeeViewModel =
    TypeProps<
    RequiredNonNullableProps<
    PartialProps<

    // Base data structure.
    RegistrationEntryFeeBag,

    // Define optional properties.
    "hideWhenNoneRemaining">,

    // Define properties that must have a value.
    "guid" | "items" | "name">,

    // Override property type.
    "items", RegistrationEntryBlockFeeItemViewModel[]>;

export type RegistrationEntryBlockFeeItemViewModel =
    RequiredNonNullableProps<
    RequiredProps<

    // Base data structure.
    RegistrationEntryFeeItemBag,

    // Define required properties (can still be null).
    "countRemaining" | "originalCountRemaining">,

    // Define properties that must have a value.
    "guid" | "name">;

export type RegistrationEntryBlockFormViewModel =
    TypeProps<
    RequiredNonNullableProps<

    // Base data structure.
    RegistrationEntryFormBag,

    // Define properties that must have a value.
    "fields">,

    // Override property type.
    "fields", RegistrationEntryBlockFormFieldViewModel[]>;

export type RegistrationEntryBlockFormFieldViewModel =
    TypeProps<
    TypeProps<
    RequiredNonNullableProps<
    RequiredProps<

    // Base data structure.
    RegistrationEntryFormFieldBag,

    // Define required properties (can still be null).
    "attribute" | "visibilityRules">,

    // Define properties that must have a value.
    "guid" | "preHtml" | "postHtml">,

    // Override property types.
    "fieldSource", RegistrationFieldSource>,
    "visibilityRules", RegistrationEntryBlockFormFieldRuleViewModel[]>;

export type RegistrationEntryBlockFormFieldRuleViewModel = {
    comparedToRegistrationTemplateFormFieldGuid: Guid;
    comparisonValue: ComparisonValue;
};

export type RegistrantInfo =
    RequiredNonNullableProps<
    RequiredProps<

    // Base data structure.
    RegistrantBag,

    // Define required properties (can still be null).
    "familyGuid" | "personGuid">,

    // Define properties that must have a value.
    "feeItemQuantities" | "fieldValues" | "guid">;

export type RegistrarInfo =
    RequiredNonNullableProps<
    RequiredProps<

    // Base data structure.
    RegistrarBag,

    // Define required properties (can still be null).
    "familyGuid">,

    // Define properties that must have a value.
    "email" | "lastName" | "nickName">;


export type RegistrationEntryBlockSuccessViewModel =
    RequiredNonNullableProps<

    // Base data structure.
    RegistrationEntrySuccessBag,

    // Define required properties (can still be null).
    "gatewayPersonIdentifier" | "messageHtml" | "titleHtml" | "transactionCode">;

export type RegistrationEntryBlockArgs =
    TypeProps<
    TypeProps<
    RequiredProps<

    // Base data structure.
    RegistrationEntryArgsBag,

    // Define required properties (can still be null).
    "discountCode" | "fieldValues" | "gatewayToken" | "registrants" | "registrar" | "registrationGuid" | "registrationSessionGuid" | "savedAccountGuid">,

    // Override property types.
    "registrants", RegistrantInfo[]>,
    "registrar", RegistrarInfo | null>;

export type RegistrationEntryBlockSession =
    TypeProps<
    TypeProps<
    RequiredProps<

    // Base data structure.
    RegistrationEntrySessionBag,

    // Define required properties (can still be null).
    "discountCode" | "fieldValues" | "gatewayToken" | "registrants" | "registrar" | "registrationGuid" | "registrationSessionGuid">,

    // Override property types.
    "registrants", RegistrantInfo[]>,
    "registrar", RegistrarInfo | null>;

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

export type RegistrationEntryState = {
    steps: Record<Step, Step>;
    viewModel: RegistrationEntryBlockViewModel;
    currentStep: string;
    firstStep: string;
    navBack: boolean;
    currentRegistrantIndex: number;
    currentRegistrantFormIndex: number;
    registrants: RegistrantInfo[];
    registrationFieldValues: Record<Guid, unknown>;
    registrar: RegistrarInfo;
    gatewayToken: string;
    savedAccountGuid: Guid | null;
    discountCode: string;
    discountAmount: number;
    discountPercentage: number;
    discountMaxRegistrants: number;
    successViewModel: RegistrationEntryBlockSuccessViewModel | null;
    amountToPayToday: number;
    sessionExpirationDateMs: number | null;
    registrationSessionGuid: Guid;
    ownFamilyGuid: Guid;
};
