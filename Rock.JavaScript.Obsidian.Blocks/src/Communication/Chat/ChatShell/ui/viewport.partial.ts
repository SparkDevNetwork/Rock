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

/**
 * Whether the chat is drawn at a phone's width, provided once by the shell for a part that sits
 * several layers below it, such as a message's marker, to open its menu as a sheet the way the
 * composer's menus do without every layer between carrying the flag. A part drawn alone reads it
 * as false.
 */
export const isPhoneKey: InjectionKey<Ref<boolean>> = Symbol("chat-is-phone");
