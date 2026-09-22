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
import { ChatConfigurationBag } from "@Obsidian/ViewModels/Blocks/Communication/Chat/ChatConfiguration/chatConfigurationBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

/**
 * The settings the form edits. The half the platform issued when chat was enabled is
 * deliberately absent: the screen shows those values from the box and never through
 * the form, so an edit cannot reach them and a save cannot carry them back.
 */
export type ChatConfigurationFormModel = {
    areChatProfilesVisible: boolean;
    isOpenDirectMessagingAllowed: boolean;
    minimumAge: number | null;
    directMessageAccessDataView: ListItemBag | null;
    chatBadgeDataViews: ListItemBag[];
};

/** Reads the settings the form may edit out of what the block sent. */
export function toFormModel(bag: Partial<ChatConfigurationBag> | null | undefined): ChatConfigurationFormModel {
    return {
        areChatProfilesVisible: bag?.areChatProfilesVisible ?? false,
        isOpenDirectMessagingAllowed: bag?.isOpenDirectMessagingAllowed ?? false,
        minimumAge: bag?.minimumAge ?? null,
        directMessageAccessDataView: bag?.directMessageAccessDataView ?? null,
        chatBadgeDataViews: bag?.chatBadgeDataViews ?? []
    };
}

/**
 * Whether the form holds edits that have not been saved. Sync Now sends what is stored, so while
 * this is true a press would send settings other than the ones on the screen.
 *
 * @param form The form as it stands.
 * @param saved The form as it was when last saved or loaded.
 *
 * @returns True when the two differ in any setting the form edits.
 */
export function hasUnsavedChanges(form: ChatConfigurationFormModel, saved: ChatConfigurationFormModel): boolean {
    throw new Error("not implemented");
}

/** Builds what the save action is sent: the settings the church owns, and nothing else. */
export function toBag(form: ChatConfigurationFormModel): Partial<ChatConfigurationBag> {
    return {
        areChatProfilesVisible: form.areChatProfilesVisible,
        isOpenDirectMessagingAllowed: form.isOpenDirectMessagingAllowed,
        minimumAge: form.minimumAge,
        directMessageAccessDataView: form.directMessageAccessDataView,
        chatBadgeDataViews: form.chatBadgeDataViews
    };
}
