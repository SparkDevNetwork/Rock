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

import { CommunicationChannelType } from "@Obsidian/Enums/Blocks/Communication/EmailPreferenceEntry/communicationChannelType";
import { CommunicationType } from "@Obsidian/Enums/Communication/communicationType";

export const enum NavigationUrlKey {
    ManageMyAccountPage = "ManageMyAccountPage",
}

/**
 * The event payload when a channel subscription is managed.
 */
export type ManageChannelSubscriptionEvent = {
    idKey: string;
    channelType: CommunicationChannelType;
};

/**
 * The event payload when a channel's communication preference is updated.
 */
export type UpdateCommunicationPreferenceEvent = {
    idKey: string;
    channelType: CommunicationChannelType;
    preference: CommunicationType;
};
