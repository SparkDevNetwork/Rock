﻿// <copyright>
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

import { GatewayControlModel } from '../../../Controls/GatewayControl';
import { Guid } from '../../../Util/Guid';
import Attribute from '../../../ViewModels/CodeGenerated/AttributeViewModel';
import { RegistrationEntryBlockArgs, RegistrationEntryBlockSession } from './RegistrationEntryBlockArgs';

export enum RegistrationPersonFieldType
{
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

export enum RegistrationFieldSource
{
    PersonField = 0,
    PersonAttribute = 1,
    GroupMemberAttribute = 2,
    RegistrantAttribute = 4
}

export enum FilterExpressionType
{
    Filter = 0,
    GroupAll = 1,
    GroupAny = 2,
    GroupAllFalse = 3,
    GroupAnyFalse = 4
}

export enum ComparisonType
{
    EqualTo = 0x1,
    NotEqualTo = 0x2,
    StartsWith = 0x4,
    Contains = 0x8,
    DoesNotContain = 0x10,
    IsBlank = 0x20,
    IsNotBlank = 0x40,
    GreaterThan = 0x80,
    GreaterThanOrEqualTo = 0x100,
    LessThan = 0x200,
    LessThanOrEqualTo = 0x400,
    EndsWith = 0x800,
    Between = 0x1000,
    RegularExpression = 0x2000
}

export enum RegistrarOption
{
    PromptForRegistrar = 0,
    PrefillFirstRegistrant = 1,
    UseFirstRegistrant = 2,
    UseLoggedInPerson = 3
}

export enum RegistrantsSameFamily
{
    No = 0,
    Yes = 1,
    Ask = 2
}

export interface SessionRenewalResult
{
    spotsSecured: number;
    expirationDateTime: string;
}

export interface RegistrationEntryBlockViewModel
{
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
    registrationAttributesStart: Attribute[];
    registrationAttributesEnd: Attribute[];
    forceEmailUpdate: boolean;
    registrarOption: RegistrarOption;
    cost: number;
    gatewayControl: GatewayControlModel,
    isRedirectGateway: boolean,
    spotsRemaining: number | null,
    waitListEnabled: boolean,
    instanceName: string,
    amountDueToday: number | null,
    initialAmountToPay: number | null,
    session: RegistrationEntryBlockSession | null,
    isUnauthorized: boolean,
    hasDiscountsAvailable: boolean,
    redirectGatewayUrl: string,
    loginRequiredToRegister: boolean,
    successViewModel: RegistrationEntryBlockSuccessViewModel | null,
    allowRegistrationUpdates: boolean,
    startAtBeginning: boolean,
    gatewayGuid: Guid | null
}

export interface RegistrationEntryBlockFamilyMemberViewModel
{
    guid: Guid;
    familyGuid: Guid;
    fullName: string;
    fieldValues: Record<Guid, unknown>;
}

export interface RegistrationEntryBlockFeeViewModel
{
    name: string;
    guid: Guid;
    allowMultiple: boolean;
    isRequired: boolean;
    items: RegistrationEntryBlockFeeItemViewModel[];
    discountApplies: boolean;
}

export interface RegistrationEntryBlockFeeItemViewModel
{
    name: string;
    guid: Guid;
    cost: number;
    countRemaining: number | null;
}

export interface RegistrationEntryBlockFormViewModel
{
    fields: RegistrationEntryBlockFormFieldViewModel[];
}

export interface RegistrationEntryBlockFormFieldViewModel
{
    fieldSource: RegistrationFieldSource;
    personFieldType: RegistrationPersonFieldType;
    isRequired: boolean;
    isSharedValue: boolean;
    attribute: Attribute;
    visibilityRuleType: FilterExpressionType;
    visibilityRules: RegistrationEntryBlockFormFieldRuleViewModel[];
    preHtml: string;
    postHtml: string;
    showOnWaitList: boolean;
    guid: Guid;
}

export interface RegistrationEntryBlockFormFieldRuleViewModel
{
    comparedToRegistrationTemplateFormFieldGuid: Guid;
    comparisonType: ComparisonType;
    comparedToValue: string;
}

export type RegistrantInfo = {
    IsOnWaitList: boolean;

    /** The family guid that this person is to be a part of */
    FamilyGuid: Guid;

    /** If the person were an existing person, this is his/her guid */
    PersonGuid: Guid;
    FieldValues: Record<Guid, unknown>;
    FeeItemQuantities: Record<Guid, number>;

    /** If the person were in their own family, this would be that family's guid */
    OwnFamilyGuid: Guid;
    Guid: Guid;
};

export type RegistrarInfo = {
    NickName: string;
    LastName: string;
    Email: string;
    UpdateEmail: boolean;
    FamilyGuid: Guid | null;
    OwnFamilyGuid: Guid
};

export type RegistrationEntryBlockSuccessViewModel = {
    titleHtml: string;
    messageHtml: string;
    transactionCode: string;
    gatewayPersonIdentifier: string;
};