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
import { PersonBag } from "@Obsidian/ViewModels/Entities/personBag";
import { newGuid } from "@Obsidian/Utility/guid";
import { RegistrantBasicInfo, RegistrantInfo, RegistrantsSameFamily, RegistrationEntryBlockFormFieldViewModel, RegistrationEntryBlockFormViewModel, RegistrationEntryBlockViewModel, RegistrationPersonFieldType, RegistrationFieldSource } from "./types";

/** If all registrants are to be in the same family, but there is no currently authenticated person,
 *  then this guid is used as a common family guid */
const unknownSingleFamilyGuid = newGuid();

/**
 * If there is a forced family guid because of RegistrantsSameFamily setting, then this returns that guid
 * @param currentPerson
 * @param viewModel
 */
export function getForcedFamilyGuid(currentPerson: PersonBag | null, viewModel: RegistrationEntryBlockViewModel): string | null {
    return (currentPerson && viewModel.registrantsSameFamily === RegistrantsSameFamily.Yes) ?
        (currentPerson.primaryFamilyGuid || unknownSingleFamilyGuid) :
        null;
}

/**
 * Get a default registrant object with the current family guid set.
 * @param currentPerson
 * @param viewModel
 * @param familyGuid
 */
export function getDefaultRegistrantInfo(currentPerson: PersonBag | null, viewModel: RegistrationEntryBlockViewModel, familyGuid: Guid | null): RegistrantInfo {
    const forcedFamilyGuid = getForcedFamilyGuid(currentPerson, viewModel);

    if (forcedFamilyGuid) {
        familyGuid = forcedFamilyGuid;
    }

    // If the family is not specified, then assume the person is in their own family
    if (!familyGuid) {
        familyGuid = newGuid();
    }

    return {
        isOnWaitList: false,
        familyGuid: familyGuid,
        fieldValues: {},
        feeItemQuantities: {},
        guid: newGuid(),
        personGuid: null
    } as RegistrantInfo;
}

export function getRegistrantBasicInfo(registrant: RegistrantInfo, registrantForms: RegistrationEntryBlockFormViewModel[]): RegistrantBasicInfo {
    const fields = registrantForms?.reduce((acc, f) => acc.concat(f.fields), [] as RegistrationEntryBlockFormFieldViewModel[]) || [];

    const firstNameGuid = fields.find(f => f.personFieldType === RegistrationPersonFieldType.FirstName && f.fieldSource === RegistrationFieldSource.PersonField)?.guid || "";
    const lastNameGuid = fields.find(f => f.personFieldType === RegistrationPersonFieldType.LastName && f.fieldSource === RegistrationFieldSource.PersonField)?.guid || "";
    const emailGuid = fields.find(f => f.personFieldType === RegistrationPersonFieldType.Email && f.fieldSource === RegistrationFieldSource.PersonField)?.guid || "";

    return {
        firstName: (registrant?.fieldValues[firstNameGuid] || "") as string,
        lastName: (registrant?.fieldValues[lastNameGuid] || "") as string,
        email: (registrant?.fieldValues[emailGuid] || "") as string,
        guid: registrant?.guid
    };
}
