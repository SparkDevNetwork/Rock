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

import { InjectionKey, Ref } from "vue";
import { GroupBag } from "@Obsidian/ViewModels/Blocks/Group/Scheduling/GroupScheduleToolbox/groupBag";

/**
 * Keys for page parameters.
 */
export const enum PageParameterKey {
    ToolboxActionType = "ToolboxActionType",
    ToolboxGroupId = "ToolboxGroupId"
}

/**
 * An injection key to provide the selected person unique identifier (or empty string if none selected).
 */
export const SelectedPersonGuid: InjectionKey<Ref<string>> = Symbol("selected-person-guid");

/**
 * An injection key to provide the selected group bag.
 */
export const SelectedGroup: InjectionKey<Ref<GroupBag | null | undefined>> = Symbol("selected-group");

/**
 * An injection key to provide the selected sign-up key.
 */
export const SelectedSignUpKey: InjectionKey<Ref<string>> = Symbol("selected-sign-up-key");

/**
 * An injection key to provide whether a sign-up is currently being saved.
 */
export const IsSavingSignUp: InjectionKey<Ref<boolean>> = Symbol("is-saving-sign-up");

/**
 * An injection key to provide the save sign-up error message.
 */
export const SaveSignUpErrorMessage: InjectionKey<Ref<string>> = Symbol("save-sign-up-error-message");
