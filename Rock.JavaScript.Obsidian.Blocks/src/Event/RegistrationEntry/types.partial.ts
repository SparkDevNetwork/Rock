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

import { FilterExpressionType } from "@Obsidian/Core/Reporting/filterExpressionType";
import { Guid } from "@Obsidian/Types";
import { UnwrapTypeBuilder, TypeBuilder } from "@Obsidian/Utility/typeUtils";
import { RegistrantBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrantBag";
import { RegistrarBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrarBag";
import { RegistrationEntryArgsBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryArgsBag";
import { RegistrationEntryFamilyMemberBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFamilyMemberBag";
import { RegistrationEntryFeeBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFeeBag";
import { RegistrationEntryFeeItemBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFeeItemBag";
import { RegistrationEntryFormBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFormBag";
import { RegistrationEntryFormFieldBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFormFieldBag";
import { RegistrationEntryInitializationBox } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryInitializationBox";
import { RegistrationEntrySessionBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntrySessionBag";
import { RegistrationEntrySuccessBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntrySuccessBag";
import { RegistrationEntryVisibilityBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryVisibilityBag";
import { SessionRenewalResultBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/sessionRenewalResultBag";

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
    TypeBuilder.fromType<SessionRenewalResultBag>()
    .makePropertiesWithName("expirationDateTime").required().and.defined()
    .build();
export type SessionRenewalResult = UnwrapTypeBuilder<typeof sessionRenewalResultValue>;

const registrationEntryBlockViewModelValue =
    TypeBuilder.fromType<RegistrationEntryInitializationBox>()
    .makeAllProperties().required()
    .makePropertiesWithName("currentPersonFamilyGuid").type<Guid | null>()
    .makePropertiesWithName("gatewayGuid").type<Guid | null>()
    .makePropertiesWithName("session").type<RegistrationEntryBlockSession | null>()
    .makePropertiesWithName("registrantForms").type<RegistrationEntryBlockFormViewModel[] | null>()
    .makePropertiesWithName("fees").type<RegistrationEntryBlockFeeViewModel[] | null>()
    .makePropertiesWithName("familyMembers").type<RegistrationEntryBlockFamilyMemberViewModel[] | null>()
    .makePropertiesWithName("successViewModel").type<RegistrationEntryBlockSuccessViewModel | null>()
    .makePropertiesWithName("registrarOption").type<RegistrarOption>()
    .build();
export type RegistrationEntryBlockViewModel = UnwrapTypeBuilder<typeof registrationEntryBlockViewModelValue>;

const registrationEntryBlockFamilyMemberViewModelValue =
    TypeBuilder.fromType<RegistrationEntryFamilyMemberBag>()
    .makeAllProperties().required()
    .makePropertiesWithName("familyGuid", "guid").type<Guid>()
    .build();
export type RegistrationEntryBlockFamilyMemberViewModel = UnwrapTypeBuilder<typeof registrationEntryBlockFamilyMemberViewModelValue>;

const registrationEntryBlockFeeViewModelValue =
    TypeBuilder.fromType<RegistrationEntryFeeBag>()
    .makeAllProperties().required()
    .makePropertiesWithName("guid").type<Guid>()
    .makePropertiesWithName("items").type<RegistrationEntryBlockFeeItemViewModel[] | null>()
    .build();
export type RegistrationEntryBlockFeeViewModel = UnwrapTypeBuilder<typeof registrationEntryBlockFeeViewModelValue>;

const registrationEntryBlockFeeItemViewModelValue =
    TypeBuilder.fromType<RegistrationEntryFeeItemBag>()
    .makeAllProperties().required()
    .makePropertiesWithName("guid").type<Guid>()
    .build();
export type RegistrationEntryBlockFeeItemViewModel = UnwrapTypeBuilder<typeof registrationEntryBlockFeeItemViewModelValue>;

const registrationEntryBlockFormViewModelValue =
    TypeBuilder.fromType<RegistrationEntryFormBag>()
    .makeAllProperties().required()
    .makePropertiesWithName("fields").type<RegistrationEntryBlockFormFieldViewModel[] | null>()
    .build();
export type RegistrationEntryBlockFormViewModel = UnwrapTypeBuilder<typeof registrationEntryBlockFormViewModelValue>;

const registrationEntryBlockFormFieldViewModelValue =
    TypeBuilder.fromType<RegistrationEntryFormFieldBag>()
    .makeAllProperties().required()
    .makePropertiesWithName("guid").type<Guid>()
    .makePropertiesWithName("fieldSource").type<RegistrationFieldSource>()
    .makePropertiesWithName("visibilityRuleType").type<FilterExpressionType>()
    .makePropertiesWithName("personFieldType").type<RegistrationPersonFieldType>()
    .makePropertiesWithName("visibilityRules").type<RegistrationEntryBlockFormFieldRuleViewModel[] | null>()
    .build();
export type RegistrationEntryBlockFormFieldViewModel = UnwrapTypeBuilder<typeof registrationEntryBlockFormFieldViewModelValue>;

const registrationEntryBlockFormFieldRuleViewModelValue =
    TypeBuilder.fromType<RegistrationEntryVisibilityBag>()
    .makeAllProperties().required()
    .makePropertiesWithName("comparedToRegistrationTemplateFormFieldGuid").type<Guid>()
    .build();
export type RegistrationEntryBlockFormFieldRuleViewModel = UnwrapTypeBuilder<typeof registrationEntryBlockFormFieldRuleViewModelValue>;

const registrantInfoValue =
    TypeBuilder.fromType<RegistrantBag>()
    .makeAllProperties().required()
    .makePropertiesWithName("guid").type<Guid>()
    .makePropertiesWithName("familyGuid", "personGuid", "existingSignatureDocumentGuid").type<Guid | null>()
    .build();
export type RegistrantInfo = UnwrapTypeBuilder<typeof registrantInfoValue>;

const registrarInfoValue =
    TypeBuilder.fromType<RegistrarBag>()
    .makeAllProperties().required()
    .makePropertiesWithName("familyGuid").type<Guid | null>()
    .build();
export type RegistrarInfo = UnwrapTypeBuilder<typeof registrarInfoValue>;

const registrationEntryBlockSuccessViewModelValue =
    TypeBuilder.fromType<RegistrationEntrySuccessBag>()
    .makeAllProperties().required()
    .build();
export type RegistrationEntryBlockSuccessViewModel = UnwrapTypeBuilder<typeof registrationEntryBlockSuccessViewModelValue>;

const registrationEntryBlockArgsValue =
    TypeBuilder.fromType<RegistrationEntryArgsBag>()
    .makeAllProperties().required()
    .makePropertiesWithName("registrationSessionGuid").type<Guid>()
    .makePropertiesWithName("registrationGuid", "savedAccountGuid").type<Guid | null>()
    .makePropertiesWithName("registrants").type<RegistrantInfo[] | null>()
    .makePropertiesWithName("registrar").type<RegistrarInfo | null>()
    .build();
export type RegistrationEntryBlockArgs = UnwrapTypeBuilder<typeof registrationEntryBlockArgsValue>;

const registrationEntryBlockSessionValue =
    TypeBuilder.fromType<RegistrationEntrySessionBag>()
    .makeAllProperties().required()
    .makePropertiesWithName("registrationSessionGuid").type<Guid>()
    .makePropertiesWithName("registrationGuid").type<Guid | null>()
    .makePropertiesWithName("registrants").type<RegistrantInfo[] | null>()
    .makePropertiesWithName("registrar").type<RegistrarInfo | null>()
    .build();
export type RegistrationEntryBlockSession = UnwrapTypeBuilder<typeof registrationEntryBlockSessionValue>;

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
