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
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";

/**
 * An injection key to provide the member attributes.
 */
export const MemberAttributes: InjectionKey<Ref<Record<string, PublicAttributeBag>>> = Symbol("member-attributes");

/**
 * An injection key to provide the member opportunity attributes.
 */
export const MemberOpportunityAttributes: InjectionKey<Ref<Record<string, PublicAttributeBag>>> = Symbol("member-opportunity-attributes");