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
// What the block draws of the roster: the members panel, and the two dialogs that change who is in
// the room, the remove confirm and Add people, which stand over the frame rather than in the panel.
// All three read the same roster and the same room, by its title, its kind and its head count, so
// they are worked out in one place.

import { computed, Ref } from "vue";
import { ChatShell } from "../shell.partial";
import { searchStatus, shownMembers, successorChoices } from "./useMembers.partial";

/** What the members panel and its dialogs read from the block. */
export type MembersPanelDependencies = {
    /** The block's shell, read on every draw since a birthdate replaces it. */
    shell: Readonly<Ref<ChatShell>>;

    /** Each room's title by channel id, as the header writes it. */
    roomTitles: Readonly<Ref<Record<string, string>>>;

    /** Each room's kind by channel id: a channel, a person, or a group. */
    roomKinds: Readonly<Ref<Record<string, string>>>;

    /** The church's campuses, from the init box. */
    campuses: Array<{ guid: string, name?: string | null }>;
};

/**
 * Works out what the members panel, the remove confirm and Add people draw.
 *
 * @param dependencies What they read from the block.
 *
 * @returns Each part's props.
 */
export function useMembersPanel(dependencies: MembersPanelDependencies) {
    const { shell, roomTitles, roomKinds } = dependencies;

    /** The church's campus names by guid in lower case, for a person's campus. */
    const campusNames: Record<string, string> = Object.fromEntries(dependencies.campuses.map(campus => [campus.guid.toLowerCase(), campus.name ?? ""]));

    /** The room the roster is of: its name, whether it is a channel, and its head count once its facts are read. */
    const room = computed(() => {
        const channelId = shell.value.members.state.channelId ?? "";
        return {
            channelName: roomTitles.value[channelId] ?? "",
            isChannel: roomKinds.value[channelId] === "channel",
            memberCount: shell.value.rooms.factsOf(channelId)?.member_count ?? null
        };
    });

    const membersView = computed(() => {
        const members = shell.value.members.state;
        const isQuery = members.query.trim() !== "";
        return {
            ...room.value,
            campusNames,
            members: shownMembers(members),
            hasMore: isQuery ? members.search.hasMore : members.hasMore,
            isLoading: members.isLoading,
            isLoadingMore: members.isLoadingMore,
            error: members.error,
            moreError: members.moreError,
            refusal: members.refusal,
            query: members.query,
            searchStatus: searchStatus(members),
            canManageMembers: members.canManageMembers,
            isChatAdministrator: members.isChatAdministrator,
            removals: members.removals
        };
    });

    const removeView = computed(() => {
        const members = shell.value.members.state;
        const confirming = members.confirming;
        return {
            channelName: room.value.channelName,
            isChannel: room.value.isChannel,
            campusNames,
            member: confirming,
            isLastLeader: !!confirming && members.isSuccessorAsked,
            choices: successorChoices(members),
            query: members.successorQuery,
            refusal: members.successorRefusal,
            isRemoving: !!confirming && members.removals[confirming.person_alias_guid]?.kind === "removing"
        };
    });

    const addView = computed(() => {
        const adding = shell.value.members.state.adding;
        return {
            ...room.value,
            campusNames,
            isOpen: adding.isOpen,
            query: adding.query,
            people: adding.people,
            isSearching: adding.isSearching,
            error: adding.error,
            isAdding: adding.isAdding,
            refusal: adding.refusal
        };
    });

    return { membersView, removeView, addView };
}
