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

import { Guid } from '../../../Util/Guid';
import Attribute from '../../../ViewModels/CodeGenerated/AttributeViewModel';

export enum RegistrationPersonFieldType {
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

export enum RegistrationFieldSource {
    PersonField = 0,
    PersonAttribute = 1,
    GroupMemberAttribute = 2,
    RegistrantAttribute = 4
}

export enum FilterExpressionType {
    Filter = 0,
    GroupAll = 1,
    GroupAny = 2,
    GroupAllFalse = 3,
    GroupAnyFalse = 4
}

export enum ComparisonType {
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

export interface RegistrationEntryBlockViewModel {
    InstructionsHtml: string;
    RegistrantTerm: string;
    PluralRegistrantTerm: string;
    PluralFeeTerm: string;
    RegistrantForms: RegistrationEntryBlockFormViewModel[];
}

export interface RegistrationEntryBlockFormViewModel {
    Fields: RegistrationEntryBlockFormFieldViewModel[];
}

export interface RegistrationEntryBlockFormFieldViewModel {
    FieldSource: RegistrationFieldSource;
    PersonFieldType: RegistrationPersonFieldType;
    IsRequired: boolean;
    Attribute: Attribute;
    VisibilityRuleType: FilterExpressionType;
    VisibilityRules: RegistrationEntryBlockFormFieldRuleViewModel[];
    Guid: Guid;
}

export interface RegistrationEntryBlockFormFieldRuleViewModel {
    ComparedToRegistrationTemplateFormFieldGuid: Guid;
    ComparisonType: ComparisonType;
    ComparedToValue: string;
}