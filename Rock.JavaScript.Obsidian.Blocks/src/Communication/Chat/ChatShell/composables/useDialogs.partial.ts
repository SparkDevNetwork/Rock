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
// The dialogs and the card drawn over the frame: a room's details and its choices, the person's
// Chat settings, a person's card and what it opens, and a member row's Message and face. Each
// closes with what it belongs to, and focus goes somewhere the person can see as it does.

import { nextTick, onBeforeUnmount, ref, Ref, shallowRef, watch } from "vue";
import type { ChatShell } from "../shell.partial";
import { ChatPerson, PersonProfile } from "../types.partial";
import { shownMembers } from "./useMembers.partial";
import { Frame } from "./useFrame.partial";
import { MessageActionsView } from "./useMessageActionsView.partial";
import { RoomChoice } from "./usePrefs.partial";

/** What the dialogs read from the block: the shell and the open room, and the frame's panes and views they open and close. */
export type DialogsDependencies = Pick<Frame, "panelKind" | "mainView" | "panelElement" | "isPhone" | "closePanel" | "openFromMainView" | "openMainView" | "onListPanel"> & {
    /** The block's shell, read on every use since a birthdate replaces it. */
    shell: Readonly<Ref<ChatShell>>;

    activeChannelId: Readonly<Ref<string | null>>;

    /** The block's root, which focus goes back into as a dialog closes. */
    rootElement: Readonly<Ref<HTMLElement | undefined>>;

    closeReactionPicker: () => void;

    /** Opens the block confirmation for a person, with what focus goes back to. */
    askBlock: MessageActionsView["askBlock"];
};

/**
 * Creates the dialogs' and the card's state and the handlers their parts call.
 *
 * @param dependencies What the dialogs read from the block.
 *
 * @returns The dialogs' values and handlers.
 */
export function useDialogs(dependencies: DialogsDependencies) {
    const { shell, activeChannelId, panelKind, mainView, rootElement, panelElement, isPhone, closePanel, openFromMainView, openMainView, onListPanel, closeReactionPicker, askBlock } = dependencies;

    /** Whether the open conversation's details dialog is open. */
    const isDetailsOpen = ref(false);

    const isSettingsOpen = ref(false);

    /** What asked for the profile card, which it is placed beside, and a count that draws each card afresh. */
    const cardAnchor = shallowRef<HTMLElement | null>(null);
    const cardOpenCount = ref(0);

    /** Moved on by every change a member row's Message may not outlive; see the sync watcher below. */
    let memberMessageGeneration = 0;

    // The blocked people are read as Chat settings opens, so the list is never older than the dialog.
    watch(isSettingsOpen, isOpen => {
        if (isOpen) {
            void shell.value.blocks.load();
        }
    });

    /** Opens a person's card for a room beside what asked, drawn afresh each time. */
    function openCard(anchor: HTMLElement, personAliasGuid: string, channelId: string): void {
        cardAnchor.value = anchor;
        cardOpenCount.value++;
        shell.value.rooms.openCard(personAliasGuid, channelId);
    }

    /**
     * The card's Message: the conversation the two already have, else New message with the person
     * chosen. The card goes first, so it never stands over what opens.
     */
    function onCardMessage(person: PersonProfile): void {
        shell.value.rooms.closeCard();
        closePanel();
        if (person.dm_channel_id) {
            void openFromMainView(person.dm_channel_id);
            return;
        }

        void openMainView("newdm", [{ person_alias_guid: person.person_alias_guid, nick_name: person.nick_name, last_name: person.last_name, avatar_url: person.avatar_url }]);
    }

    function onLeave(): void {
        if (activeChannelId.value) {
            void shell.value.rooms.leave(activeChannelId.value);
        }
    }

    function onRename(name: string): void {
        if (activeChannelId.value) {
            void shell.value.rooms.rename(activeChannelId.value, name);
        }
    }

    /** Focus goes to the title button, since the phone's overflow item that may have opened the details has left with its menu. */
    function closeDetails(): void {
        isDetailsOpen.value = false;
        void nextTick(() => rootElement.value?.querySelector<HTMLElement>("[data-testid='frame-main'] [data-testid='header-details']")?.focus());
    }

    /** The open room's notify mode, star or hide, from its details or its header. */
    function onRoomChoice(choice: RoomChoice): void {
        void shell.value.prefs.choose(activeChannelId.value, choice);
    }

    /** The person's own card's switch for their details: Rock is asked again for the card once the save lands. */
    function onCardToggleDetails(value: boolean): void {
        shell.value.rooms.refreshCardDetails(shell.value.prefs.setSwitch("profile_details", value));
    }

    /** The card goes first, so the confirmation never stands under it, and focus comes back to what opened the card. */
    function onCardBlock(person: PersonProfile): void {
        const anchor = cardAnchor.value;
        shell.value.rooms.closeCard();
        askBlock(person, anchor, null);
    }

    /** Needs no confirmation, since everything comes back as it was. */
    function onCardUnblock(person: PersonProfile): void {
        shell.value.rooms.closeCard();
        void shell.value.blocks.unblock(person, false);
    }

    function onSettingsUnblock(person: ChatPerson): void {
        void shell.value.blocks.unblock(person, true);
    }

    /** The card goes first, so it never stands over the dialog. */
    function onCardSettings(): void {
        shell.value.rooms.closeCard();
        isSettingsOpen.value = true;
    }

    /**
     * The dialog gives focus back to what opened it; a card that opened it has gone, so focus then
     * goes to the sidebar's foot, or on a phone the title.
     */
    function closeSettings(): void {
        isSettingsOpen.value = false;
        void nextTick(() => {
            const active = document.activeElement;
            if (active && active !== document.body && rootElement.value?.contains(active)) {
                return;
            }

            const target = isPhone.value ? "[data-testid='frame-main'] [data-header-focus]" : "[data-testid='sidebar-me']";
            rootElement.value?.querySelector<HTMLElement>(target)?.focus();
        });
    }

    /** A member row's Message: the conversation with that person, or a draft for them, as New message would open it. */
    async function onMemberMessage(personAliasGuid: string): Promise<void> {
        const askedShell = shell.value;
        const channelId = askedShell.members.state.channelId;
        const member = shownMembers(askedShell.members.state).find(m => m.person_alias_guid === personAliasGuid);
        if (!member || !channelId) {
            return;
        }

        const generation = ++memberMessageGeneration;
        const person = await askedShell.rooms.readPerson(personAliasGuid, channelId);
        // A slow answer must not replace another conversation or a newer member's Message.
        if (generation !== memberMessageGeneration || shell.value !== askedShell || activeChannelId.value !== channelId || panelKind.value !== "members") {
            return;
        }

        if (person?.can_message) {
            onCardMessage(person);
        }
    }

    /** A member row's face or name: the card is anchored on the row, beside the panel, and Escape gives focus back to it. */
    function onMemberPerson(personAliasGuid: string): void {
        const channelId = shell.value.members.state.channelId;
        const row = panelElement.value?.querySelector<HTMLElement>(`[data-testid='member-who'][data-person='${personAliasGuid}']`) ?? null;
        if (!channelId || !row) {
            return;
        }

        // Safari leaves a pressed button unfocused, and the card hands focus back to what held it.
        row.focus({ preventScroll: true });
        openCard(row, personAliasGuid, channelId);
    }

    /** The details dialog's member count gives way to the members panel, which takes focus. */
    function onDetailsMembers(): void {
        isDetailsOpen.value = false;
        if (panelKind.value !== "members") {
            void onListPanel("members");
        }
    }

    // A Message belongs to this opening of Members, even if the same room's panel is closed and
    // reopened before Vue's next render, so it is invalidated at each change, not after batching.
    watch([shell, activeChannelId, panelKind], () => {
        memberMessageGeneration++;
    }, { flush: "sync" });

    // A picker or a card belongs to a message on screen, so another conversation or a full view
    // over it closes them; so are the details, which belong to the conversation.
    watch([activeChannelId, mainView], () => {
        closeReactionPicker();
        if (shell.value.rooms.state.card) {
            shell.value.rooms.closeCard();
        }
        isDetailsOpen.value = false;
    });

    // Rock is asked each time the details open whether the person may rename the room, as its
    // rename door would answer now.
    watch(isDetailsOpen, isOpen => {
        if (isOpen && activeChannelId.value) {
            void shell.value.rooms.detailsOpened(activeChannelId.value);
        }
    });

    // Once the person has left, the room's details have nothing left to show.
    watch(() => activeChannelId.value ? shell.value.rooms.leaveOf(activeChannelId.value).kind : null, kind => {
        if (kind === "left" && isDetailsOpen.value) {
            closeDetails();
        }
    });

    onBeforeUnmount(() => {
        memberMessageGeneration++;
    });

    return {
        isDetailsOpen, isSettingsOpen, cardAnchor, cardOpenCount, openCard, onCardMessage, onLeave, onRename, closeDetails, onRoomChoice,
        onCardToggleDetails, onCardBlock, onCardUnblock, onSettingsUnblock, onCardSettings, closeSettings, onMemberMessage, onMemberPerson, onDetailsMembers
    };
}
