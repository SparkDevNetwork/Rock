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
import { RegistrantBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrantBag";
import { RegistrarBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrarBag";
import { RegistrationEntryInitializationBox as GeneratedRegistrationEntryInitializationBox } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryInitializationBox";
import { RegistrationEntryFeeBag as GeneratedRegistrationEntryFeeBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFeeBag";
import { RegistrationEntryFeeItemBag as GeneratedRegistrationEntryFeeItemBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFeeItemBag";
import { RegistrationEntrySuccessBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntrySuccessBag";
import { RegistrationEntryFormFieldBag as GeneratedRegistrationEntryFormFieldBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFormFieldBag";
import { TypeBuilder } from "@Obsidian/Utility/typeUtils";
import { RegistrationEntryFormBag as GeneratedRegistrationEntryFormBag } from "@Obsidian/ViewModels/Blocks/Event/RegistrationEntry/registrationEntryFormBag";

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

export type RegistrationCostSummaryInfo = {
    paidAmount: number;
    remainingAmount: number;
    minimumRemainingAmount: number;
};

const registrationEntryFeeItemBagBuilder = TypeBuilder.createFrom<GeneratedRegistrationEntryFeeItemBag>()
    // Fix the RegistrationEntryFeeItemBag.guid property to be required and not nullable.
    .makeProperties("guid").required().and.defined()
    // Fix the RegistrationEntryFeeItemBag.countRemaining property to not be undefinable.
    .makeProperties("countRemaining").required();
export type RegistrationEntryFeeItemBag = typeof registrationEntryFeeItemBagBuilder.build;

const registrationEntryFeeBagBuilder = TypeBuilder.createFrom<GeneratedRegistrationEntryFeeBag>()
    // Used the fixed items type.
    .makeProperties("items").required().and.typed<RegistrationEntryFeeItemBag[] | null>();
export type RegistrationEntryFeeBag = typeof registrationEntryFeeBagBuilder.build;

const registrationEntryInitializationBoxBuilder = TypeBuilder.createFrom<GeneratedRegistrationEntryInitializationBox>()
    // Make optional properties required since they cannot be set to undefined, and must be null or have a value.
    .makeProperties("spotsRemaining", "gatewayControl", "savedAccounts").required()
    .makeProperties("registrantForms").required().and.typed<RegistrationEntryFormBag[] | null>()
    .makeProperties("fees").required().and.typed<RegistrationEntryFeeBag[] | null>();
export type RegistrationEntryInitializationBox = typeof registrationEntryInitializationBoxBuilder.build;

const registrationEntryFormBagBuilder = TypeBuilder.createFrom<GeneratedRegistrationEntryFormBag>()
    .makeProperties("fields").required().and.typed<RegistrationEntryFormFieldBag[] | null>();
export type RegistrationEntryFormBag = typeof registrationEntryFormBagBuilder.build;

const registrationEntryFormFieldBagBuilder = TypeBuilder.createFrom<GeneratedRegistrationEntryFormFieldBag>()
    .makeProperties("guid").required().and.typed<Guid>();
export type RegistrationEntryFormFieldBag = typeof registrationEntryFormFieldBagBuilder.build;

export type RegistrationEntryState = {
    steps: Record<Step, Step>;
    viewModel: RegistrationEntryInitializationBox;
    currentStep: string;
    firstStep: string;
    navBack: boolean;
    currentRegistrantIndex: number;
    currentRegistrantFormIndex: number;
    registrants: RegistrantBag[];
    registrationFieldValues: Record<Guid, unknown>;
    registrar: RegistrarBag;
    gatewayToken: string;
    savedAccountGuid: Guid | null;
    discountCode: string;
    discountAmount: number;
    discountPercentage: number;
    discountMaxRegistrants: number;
    successViewModel: RegistrationEntrySuccessBag | null;
    amountToPayToday: number;
    sessionExpirationDateMs: number | null;
    registrationSessionGuid: Guid;
    ownFamilyGuid: Guid;
};