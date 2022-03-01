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

import { GatewayControlModel } from "../../../Controls/gatewayControl";
import { Guid } from "../../../Util/guid";
import { PublicEditableAttributeValue, ListItem, SavedFinancialAccountListItem } from "../../../ViewModels";
import { ComparisonType } from "../../../Reporting/comparisonType";
import { FilterExpressionType } from "../../../Reporting/filterExpressionType";
import { RegistrationEntryBlockSession } from "./registrationEntryBlockArgs";

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
    AnniversaryDate = 14
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

export type SessionRenewalResult = {
    spotsSecured: number;
    expirationDateTime: string;
};

export type RegistrationEntryBlockViewModel = {
    timeoutMinutes: number | null;
    registrantsSameFamily: RegistrantsSameFamily;
    maxRegistrants: number;
    registrationAttributeTitleStart: string;
    registrationAttributeTitleEnd: string;
    instructionsHtml: string;
    registrantTerm: string;
    registrationTerm: string;
    pluralRegistrationTerm: string;
    pluralRegistrantTerm: string;
    pluralFeeTerm: string;
    registrantForms: RegistrationEntryBlockFormViewModel[];
    fees: RegistrationEntryBlockFeeViewModel[];
    familyMembers: RegistrationEntryBlockFamilyMemberViewModel[];
    registrationAttributesStart: PublicEditableAttributeValue[];
    registrationAttributesEnd: PublicEditableAttributeValue[];
    forceEmailUpdate: boolean;
    registrarOption: RegistrarOption;
    cost: number;
    gatewayControl: GatewayControlModel;
    isRedirectGateway: boolean;
    spotsRemaining: number | null;
    waitListEnabled: boolean;
    instanceName: string;
    amountDueToday: number | null;
    initialAmountToPay: number | null;
    session: RegistrationEntryBlockSession | null;
    isUnauthorized: boolean;
    hasDiscountsAvailable: boolean;
    redirectGatewayUrl: string;
    loginRequiredToRegister: boolean;
    successViewModel: RegistrationEntryBlockSuccessViewModel | null;
    allowRegistrationUpdates: boolean;
    startAtBeginning: boolean;
    gatewayGuid: Guid | null;
    campuses: ListItem[];
    maritalStatuses: ListItem[];
    connectionStatuses: ListItem[];
    grades: ListItem[];
    enableSaveAccount: boolean;
    savedAccounts: SavedFinancialAccountListItem[] | null;
};

export type RegistrationEntryBlockFamilyMemberViewModel = {
    guid: Guid;
    familyGuid: Guid;
    fullName: string;
    fieldValues: Record<Guid, unknown>;
};

export type RegistrationEntryBlockFeeViewModel = {
    name: string;
    guid: Guid;
    allowMultiple: boolean;
    isRequired: boolean;
    items: RegistrationEntryBlockFeeItemViewModel[];
    discountApplies: boolean;
};

export type RegistrationEntryBlockFeeItemViewModel = {
    name: string;
    guid: Guid;
    cost: number;
    countRemaining: number | null;
};

export type RegistrationEntryBlockFormViewModel = {
    fields: RegistrationEntryBlockFormFieldViewModel[];
};

export type RegistrationEntryBlockFormFieldViewModel = {
    fieldSource: RegistrationFieldSource;
    personFieldType: RegistrationPersonFieldType;
    isRequired: boolean;
    isSharedValue: boolean;
    attribute: PublicEditableAttributeValue | null;
    visibilityRuleType: FilterExpressionType;
    visibilityRules: RegistrationEntryBlockFormFieldRuleViewModel[];
    preHtml: string;
    postHtml: string;
    showOnWaitList: boolean;
    guid: Guid;
};

export type RegistrationEntryBlockFormFieldRuleViewModel = {
    comparedToRegistrationTemplateFormFieldGuid: Guid;
    comparisonType: ComparisonType;
    comparedToValue: string;
};

export type RegistrantInfo = {
    isOnWaitList: boolean;

    /** The family guid that this person is to be a part of */
    familyGuid: Guid;

    /** If the person were an existing person, this is his/her guid */
    personGuid: Guid | null;
    fieldValues: Record<Guid, unknown>;
    feeItemQuantities: Record<Guid, number>;

    guid: Guid;
};

export type RegistrarInfo = {
    nickName: string;
    lastName: string;
    email: string;
    updateEmail: boolean;
    familyGuid: Guid | null;
};

export type RegistrationEntryBlockSuccessViewModel = {
    titleHtml: string;
    messageHtml: string;
    transactionCode: string;
    gatewayPersonIdentifier: string;
};
