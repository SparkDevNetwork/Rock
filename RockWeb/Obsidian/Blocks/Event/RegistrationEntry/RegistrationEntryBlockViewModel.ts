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

import Attribute from "../../../ViewModels/CodeGenerated/AttributeViewModel";

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

export interface RegistrationEntryBlockViewModel {
    InstructionsHtml: string;
    RegistrantTerm: string;
    PluralRegistrantTerm: string;
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
}