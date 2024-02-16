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

import { TypeBuilder } from "@Obsidian/Utility/typeUtils";
import { FamilyPreRegistrationPersonBag } from "@Obsidian/ViewModels/Blocks/Crm/FamilyPreRegistration/familyPreRegistrationPersonBag";
import { FamilyPreRegistrationCreateAccountRequestBag } from "@Obsidian/ViewModels/Blocks/Crm/FamilyPreRegistration/familyPreRegistrationCreateAccountRequestBag";
import { Guid } from "@Obsidian/Types";

const personRequestBag =
    TypeBuilder
    .createFrom<FamilyPreRegistrationPersonBag>()
    .makeProperties("firstName", "lastName", "email", "mobilePhone", "mobilePhoneCountryCode", "attributeValues").required().and.notNullable()
    .build;
/**
 * Represents a person pre-registration request.
 */
export type PersonRequestBag = typeof personRequestBag;

const childRequestBag =
    TypeBuilder
    .createFrom<PersonRequestBag>()
    .makeProperties("familyRoleGuid").required().and.typed<Guid>()
    .build;
/**
 * Represents a child pre-registration request.
 */
export type ChildRequestBag = typeof childRequestBag;

const createAccountRequest =
    TypeBuilder
    .createFrom<FamilyPreRegistrationCreateAccountRequestBag>()
    .makeProperties("username", "password").required().and.notNullable()
    .build;
/**
 * Represents a create account request.
 */
export type CreateAccountRequest = typeof createAccountRequest;

type Thing = {
    optional?: number;
};

const test: Thing = {
    optional: undefined
};

test.optional = undefined;

// Generated problem bag.
type GeneratedBag1 = {
    guid?: string | null | undefined;                 // The C# type is a non-nullable Guid, so this should be `guid: Guid`.
    legacyEnum: number;                               // There is a TS legacy enum available, but the generated bag doesn't use it.
    someRequiredProperty?: string | null | undefined; // This is required by the front-end and back-end code, but generated bag only converts the C# string property to a TS optional, nullable string.
};

// After fix #1: Generate types with Guid properties.
type GeneratedBag2 = {
    guid: Guid; // Custom Obsidian type equivalent to `string`.
    legacyEnum: number;
    someRequiredProperty?: string | null | undefined;
};

// After fix #2: Generate types with legacy enum properties.
enum GeneratedLegacyEnum {
    Value0 = 0,
    Value1 = 1
}
type GeneratedBag3 = {
    guid: Guid;
    legacyEnum: GeneratedLegacyEnum; // Type is fixed to reference legacy enum.
    someRequiredProperty?: string | null | undefined;
};

// After fix #3: Generate types with required, non-nullable properties if C# property has [Required] attribute.
type GeneratedBag4 = {
    guid: Guid;
    legacyEnum: GeneratedLegacyEnum;
    someRequiredProperty: string; // This property is now a required, non-nullable string;
};

// In the meantime, a type builder can be used to address all code generator issues
// as a temporary and reusable solution. For example, it can be used to address all 3 issues:
const generatedBag5 = TypeBuilder.createFrom<GeneratedBag1>()
    .makeProperties("guid").required().and.typed<Guid>()
    .makeProperties("legacyEnum").typed<GeneratedLegacyEnum>()
    .makeProperties("someRequiredProperty").required().and.notNullable().and.prefixed("blah")
    .build;
type GeneratedBag5 = typeof generatedBag5;
// {
//     guid: Guid;
//     legacyEnum: GeneratedLegacyEnum;
//     someRequiredProperty: string;
// };


type OldType = {
    value1?: string | null;
    value2?: string | null;
    value3: number;
    value4: boolean | undefined;
};

const newTypeBuilder = TypeBuilder.createFrom<OldType>()
    .makeAllProperties().required()
    .makePropertiesOfType<number>().prefixed("nmb")
    .makeProperties("value4").suffixed("Boolean").and.defined()
    .makeAllProperties().prefixed("a");

type NewType = typeof newTypeBuilder.build;
// {
//     aValue1: string | null;
//     aValue2: string | null;
//     aNmbValue3: number;
//     aValue4Boolean: boolean;
// }
