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
import { ChatConfigurationBag } from "@Obsidian/ViewModels/Blocks/Administration/SparkConnectedServices/chatConfigurationBag";

/**
 * Said on the card when the organization was enabled and this Rock server cannot read the chat
 * credentials it was given. The Chat Configuration screen says the same sentence.
 */
export const credentialUnreadableSentence = "Chat was enabled for this organization, but this Rock server cannot read its chat credentials; contact Spark to restore chat.";

/** What the chat card renders, in whichever state the organization is in. */
export type ChatCardState = {
    isEnabled: boolean;
    isEnableActionShown: boolean;
    tenantId: string | null;
    projectUrl: string | null;
    credentialUnreadableMessage: string | null;
};

/**
 * Reads the card's state out of what the block sent. Every field is named here,
 * so anything else the bag happens to carry stops at this function.
 */
export function toChatCardState(bag: Partial<ChatConfigurationBag> | null | undefined): ChatCardState {
    const isEnabled = bag?.isEnabled === true;

    return {
        isEnabled,

        // Enabling again would mint a second signing key for the organization and
        // orphan the first, and nothing here can rotate one, so there is no second
        // press to offer.
        isEnableActionShown: !isEnabled,
        tenantId: isEnabled ? bag?.tenantId ?? null : null,
        projectUrl: isEnabled ? bag?.projectUrl ?? null : null,
        credentialUnreadableMessage: isEnabled && bag?.isCredentialUnreadable === true ? credentialUnreadableSentence : null
    };
}
