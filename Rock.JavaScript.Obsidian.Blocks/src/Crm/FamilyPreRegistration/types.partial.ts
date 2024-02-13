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
    .from<FamilyPreRegistrationPersonBag>()
    .makeProperties("firstName", "lastName", "email", "mobilePhone", "mobilePhoneCountryCode", "attributeValues").required().and.notNullable()
    .build;
/**
 * Represents a person pre-registration request.
 */
export type PersonRequestBag = typeof personRequestBag;

const childRequestBag =
    TypeBuilder
    .from<PersonRequestBag>()
    .makeProperties("familyRoleGuid").required().and.typed<Guid>()
    .build;
/**
 * Represents a child pre-registration request.
 */
export type ChildRequestBag = typeof childRequestBag;

const createAccountRequest =
    TypeBuilder
    .from<FamilyPreRegistrationCreateAccountRequestBag>()
    .makeProperties("username", "password").required().and.notNullable()
    .build;
/**
 * Represents a create account request.
 */
export type CreateAccountRequest = typeof createAccountRequest;
