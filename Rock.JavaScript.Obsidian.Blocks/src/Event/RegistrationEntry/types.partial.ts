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
import { TypeBuilder } from "@Obsidian/Utility/typeUtils";
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
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

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

const sessionRenewalResultType =
    TypeBuilder
    .createTypeFrom<SessionRenewalResultBag>()
    .makeProperties("expirationDateTime").required().and.defined()
    .build;
export type SessionRenewalResult = typeof sessionRenewalResultType;

const registrationEntryBlockViewModel =
    TypeBuilder
    .createTypeFrom<RegistrationEntryInitializationBox>()
    .makeAllProperties().required()
    .makeProperties("currentPersonFamilyGuid").typed<Guid | null>()
    .makeProperties("gatewayGuid").typed<Guid | null>()
    .makeProperties("session").typed<RegistrationEntryBlockSession | null>()
    .makeProperties("registrantForms").typed<RegistrationEntryBlockFormViewModel[] | null>()
    .makeProperties("fees").typed<RegistrationEntryBlockFeeViewModel[] | null>()
    .makeProperties("familyMembers").typed<RegistrationEntryBlockFamilyMemberViewModel[] | null>()
    .makeProperties("successViewModel").typed<RegistrationEntryBlockSuccessViewModel | null>()
    .makeProperties("registrarOption").typed<RegistrarOption>()
    .build;
export type RegistrationEntryBlockViewModel = typeof registrationEntryBlockViewModel;

const registrationEntryBlockFamilyMemberViewModel =
    TypeBuilder
    .createTypeFrom<RegistrationEntryFamilyMemberBag>()
    .makeAllProperties().required()
    .makeProperties("familyGuid", "guid").typed<Guid>()
    .build;
export type RegistrationEntryBlockFamilyMemberViewModel = typeof registrationEntryBlockFamilyMemberViewModel;

const registrationEntryBlockFeeViewModel =
    TypeBuilder
    .createTypeFrom<RegistrationEntryFeeBag>()
    .makeAllProperties().required()
    .makeProperties("guid").typed<Guid>()
    .makeProperties("items").typed<RegistrationEntryBlockFeeItemViewModel[] | null>()
    .build;
export type RegistrationEntryBlockFeeViewModel = typeof registrationEntryBlockFeeViewModel;

const registrationEntryBlockFeeItemViewModel =
    TypeBuilder
    .createTypeFrom<RegistrationEntryFeeItemBag>()
    .makeAllProperties().required()
    .makeProperties("guid").typed<Guid>()
    .build;
export type RegistrationEntryBlockFeeItemViewModel = typeof registrationEntryBlockFeeItemViewModel;

const registrationEntryBlockFormViewModel =
    TypeBuilder
    .createTypeFrom<RegistrationEntryFormBag>()
    .makeAllProperties().required()
    .makeProperties("fields").typed<RegistrationEntryBlockFormFieldViewModel[] | null>()
    .build;
export type RegistrationEntryBlockFormViewModel = typeof registrationEntryBlockFormViewModel;

const registrationEntryBlockFormFieldViewModel =
    TypeBuilder
    .createTypeFrom<RegistrationEntryFormFieldBag>()
    .makeAllProperties().required()
    .makeProperties("guid").typed<Guid>()
    .makeProperties("fieldSource").typed<RegistrationFieldSource>()
    .makeProperties("visibilityRuleType").typed<FilterExpressionType>()
    .makeProperties("personFieldType").typed<RegistrationPersonFieldType>()
    .makeProperties("visibilityRules").typed<RegistrationEntryBlockFormFieldRuleViewModel[] | null>()
    .build;
export type RegistrationEntryBlockFormFieldViewModel = typeof registrationEntryBlockFormFieldViewModel;

const registrationEntryBlockFormFieldRuleViewModel =
    TypeBuilder
    .createTypeFrom<RegistrationEntryVisibilityBag>()
    .makeAllProperties().required()
    .makeProperties("comparedToRegistrationTemplateFormFieldGuid").typed<Guid>()
    .build;
export type RegistrationEntryBlockFormFieldRuleViewModel = typeof registrationEntryBlockFormFieldRuleViewModel;

const registrantInfo =
    TypeBuilder
    .createTypeFrom<RegistrantBag>()
    .makeAllProperties().required()
    .makeProperties("guid").typed<Guid>()
    .makeProperties("familyGuid", "personGuid", "existingSignatureDocumentGuid").typed<Guid | null>()
    .build;
export type RegistrantInfo = typeof registrantInfo;

const registrarInfo =
    TypeBuilder
    .createTypeFrom<RegistrarBag>()
    .makeAllProperties().required()
    .makeProperties("familyGuid").typed<Guid | null>()
    .build;
export type RegistrarInfo = typeof registrarInfo;

const registrationEntryBlockSuccessViewModel =
    TypeBuilder
    .createTypeFrom<RegistrationEntrySuccessBag>()
    .makeAllProperties().required()
    .build;
export type RegistrationEntryBlockSuccessViewModel = typeof registrationEntryBlockSuccessViewModel;

const registrationEntryBlockArgs =
    TypeBuilder
    .createTypeFrom<RegistrationEntryArgsBag>()
    .makeAllProperties().required()
    .makeProperties("registrationSessionGuid").typed<Guid>()
    .makeProperties("registrationGuid", "savedAccountGuid").typed<Guid | null>()
    .makeProperties("registrants").typed<RegistrantInfo[] | null>()
    .makeProperties("registrar").typed<RegistrarInfo | null>()
    .build;
export type RegistrationEntryBlockArgs = typeof registrationEntryBlockArgs;

const registrationEntryBlockSessionValue =
    TypeBuilder
    .createTypeFrom<RegistrationEntrySessionBag>()
    .makeAllProperties().required()
    .makeProperties("registrationSessionGuid").typed<Guid>()
    .makeProperties("registrationGuid").typed<Guid | null>()
    .makeProperties("registrants").typed<RegistrantInfo[] | null>()
    .makeProperties("registrar").typed<RegistrarInfo | null>()
    .build;
export type RegistrationEntryBlockSession = typeof registrationEntryBlockSessionValue;

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