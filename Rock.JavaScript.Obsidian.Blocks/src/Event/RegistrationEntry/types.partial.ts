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
import { UnwrapBuilderType, buildTypeFrom } from "@Obsidian/Utility/typeUtils";
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

const sessionRenewalResultValue =
    buildTypeFrom<SessionRenewalResultBag>()
    .withRequiredNonNullableProps("expirationDateTime")
    .build();
export type SessionRenewalResult = UnwrapBuilderType<typeof sessionRenewalResultValue>;

const registrationEntryBlockViewModelValue =
    buildTypeFrom<RegistrationEntryInitializationBox>()
    .withRequiredProps(
        "timeoutMinutes", "spotsRemaining", "amountDueToday", "initialAmountToPay", "session",
        "successViewModel", "gatewayGuid", "savedAccounts", "registrationInstanceNotFoundMessage")
    .withRequiredNonNullableProps(
        "registrationAttributeTitleStart", "registrationAttributeTitleEnd", "instructionsHtml",
        "registrantTerm", "registrationTerm", "pluralRegistrationTerm", "pluralRegistrantTerm",
        "pluralFeeTerm", "registrantForms", "fees", "familyMembers", "registrationAttributesStart",
        "registrationAttributesEnd", "gatewayControl", "instanceName", "redirectGatewayUrl",
        "campuses", "maritalStatuses", "connectionStatuses", "grades", "races", "ethnicities")
    .withOverriddenType<RegistrationEntryBlockFamilyMemberViewModel[]>().forProps("familyMembers")
    .withOverriddenType<RegistrationEntryBlockFeeViewModel[]>().forProps("fees")
    .withOverriddenType<RegistrationEntryBlockFormViewModel[]>().forProps("registrantForms")
    .withOverriddenType<RegistrarOption>().forProps("registrarOption")
    .withOverriddenType<RegistrantsSameFamily>().forProps("registrantsSameFamily")
    .withOverriddenType<RegistrationEntryBlockSession | null>().forProps("session")
    .withOverriddenType<RegistrationEntryBlockSuccessViewModel | null>().forProps("successViewModel")
    .build();
export type RegistrationEntryBlockViewModel = UnwrapBuilderType<typeof registrationEntryBlockViewModelValue>;

const registrationEntryBlockFamilyMemberViewModelValue =
    buildTypeFrom<RegistrationEntryFamilyMemberBag>()
    .withRequiredNonNullableProps("familyGuid", "fieldValues", "fullName", "guid")
    .build();
export type RegistrationEntryBlockFamilyMemberViewModel = UnwrapBuilderType<typeof registrationEntryBlockFamilyMemberViewModelValue>;

const registrationEntryBlockFeeViewModelValue =
    buildTypeFrom<RegistrationEntryFeeBag>()
    .withOptionalProps("hideWhenNoneRemaining")
    .withRequiredNonNullableProps("guid", "items", "name")
    .withOverriddenType<RegistrationEntryBlockFeeItemViewModel[]>().forProps("items")
    .build();
export type RegistrationEntryBlockFeeViewModel = UnwrapBuilderType<typeof registrationEntryBlockFeeViewModelValue>;

const registrationEntryBlockFeeItemViewModelValue =
    buildTypeFrom<RegistrationEntryFeeItemBag>()
    .withRequiredProps("countRemaining", "originalCountRemaining")
    .withRequiredNonNullableProps("guid", "name")
    .build();
export type RegistrationEntryBlockFeeItemViewModel = UnwrapBuilderType<typeof registrationEntryBlockFeeItemViewModelValue>;

const registrationEntryBlockFormViewModelValue =
    buildTypeFrom<RegistrationEntryFormBag>()
    .withRequiredProps("fields")
    .withOverriddenType<RegistrationEntryBlockFormFieldViewModel[]>().forProps("fields")
    .build();
export type RegistrationEntryBlockFormViewModel = UnwrapBuilderType<typeof registrationEntryBlockFormViewModelValue>;

const registrationEntryBlockFormFieldViewModelValue =
    buildTypeFrom<RegistrationEntryFormFieldBag>()
    .withRequiredProps("attribute", "visibilityRules")
    .withRequiredNonNullableProps("guid", "preHtml", "postHtml")
    .withOverriddenType<RegistrationFieldSource>().forProps("fieldSource")
    .withOverriddenType<RegistrationEntryBlockFormFieldRuleViewModel[]>().forProps("visibilityRules")
    .build();
export type RegistrationEntryBlockFormFieldViewModel = UnwrapBuilderType<typeof registrationEntryBlockFormFieldViewModelValue>;

export type RegistrationEntryBlockFormFieldRuleViewModel = {
    comparedToRegistrationTemplateFormFieldGuid: Guid;
    comparisonValue: ComparisonValue;
};

const registrantInfoValue =
    buildTypeFrom<RegistrantBag>()
    .withRequiredProps("familyGuid", "personGuid")
    .withRequiredNonNullableProps("feeItemQuantities", "fieldValues", "guid")
    .build();
export type RegistrantInfo = UnwrapBuilderType<typeof registrantInfoValue>;

const registrarInfoValue =
    buildTypeFrom<RegistrarBag>()
    .withRequiredProps("familyGuid")
    .withRequiredNonNullableProps("email", "lastName", "nickName")
    .build();
export type RegistrarInfo = UnwrapBuilderType<typeof registrarInfoValue>;

const registrationEntryBlockSuccessViewModelValue =
    buildTypeFrom<RegistrationEntrySuccessBag>()
    .withRequiredNonNullableProps("gatewayPersonIdentifier", "messageHtml", "titleHtml", "transactionCode")
    .build();
export type RegistrationEntryBlockSuccessViewModel = UnwrapBuilderType<typeof registrationEntryBlockSuccessViewModelValue>;

const registrationEntryBlockArgsValue =
    buildTypeFrom<RegistrationEntryArgsBag>()
    .withRequiredProps(
        "discountCode", "fieldValues", "gatewayToken", "registrants", "registrar",
        "registrationGuid", "registrationSessionGuid", "savedAccountGuid")
    .withOverriddenType<RegistrantInfo[]>().forProps("registrants")
    .withOverriddenType<RegistrarInfo | null>().forProps("registrar")
    .build();
export type RegistrationEntryBlockArgs = UnwrapBuilderType<typeof registrationEntryBlockArgsValue>;

const registrationEntryBlockSessionValue =
    buildTypeFrom<RegistrationEntrySessionBag>()
    .withRequiredProps(
        "discountCode", "fieldValues", "gatewayToken", "registrants", "registrar",
        "registrationGuid", "registrationSessionGuid")
    .withOverriddenType<RegistrantInfo[]>().forProps("registrants")
    .withOverriddenType<RegistrarInfo | null>().forProps("registrar")
    .build();
export type RegistrationEntryBlockSession = UnwrapBuilderType<typeof registrationEntryBlockSessionValue>;

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
