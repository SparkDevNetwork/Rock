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
// Where a message's bar and menu send their actions: to the open conversation, or with a root to
// the thread the drawer draws, and nowhere when that place is gone. A press on an author opens the
// person's card through the block, for the room it was pressed in; the delete confirm names a
// channel by its title and anything else as this conversation. The shell's message actions are
// faked in their real shape, with only the calls these cells reach.
/* eslint-disable @typescript-eslint/naming-convention */
import { reactive, ref, shallowRef } from "vue";
import { MessageActionsView, useMessageActionsView } from "../../../../../src/Communication/Chat/ChatShell/composables/useMessageActionsView.partial";
import { MessageActionRequest } from "../../../../../src/Communication/Chat/ChatShell/composables/useMessageActions.partial";
import { OpenThreadState } from "../../../../../src/Communication/Chat/ChatShell/composables/useThreads.partial";
import { ChatShell } from "../../../../../src/Communication/Chat/ChatShell/shell.partial";
import { TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const team = "c0000001-0000-4000-8000-000000000000";
const pair = "c0000002-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";

const message = { id: 41, body: "Hello" } as TimelineMessage;

function setUp(activeChannelId: string | null, thread: { channelId: string, rootId: number } | null = null): { view: MessageActionsView, actions: { startQuote: jest.Mock, askDelete: jest.Mock }, openCard: jest.Mock, openReactionPicker: jest.Mock } {
    const state = reactive({ quotes: {} as Record<string, TimelineMessage>, editing: null, confirmingDelete: null as { channelId: string, message: TimelineMessage } | null });
    const actions = {
        state,
        startQuote: jest.fn(),
        askDelete: jest.fn((channelId: string, m: TimelineMessage) => {
            state.confirmingDelete = { channelId, message: m };
        })
    };
    const shell = shallowRef({ messageActions: actions } as unknown as ChatShell);
    const openCard = jest.fn();
    const openReactionPicker = jest.fn();
    const view = useMessageActionsView({
        shell,
        activeChannelId: ref(activeChannelId),
        drawnThread: shallowRef(thread as OpenThreadState | null),
        personAliasGuid: ref(anna),
        roomTitles: ref({ [team]: "young-adults", [pair]: "Anna Reyes" }),
        roomKinds: ref({ [team]: "channel", [pair]: "person" }),
        rootElement: ref(undefined),
        panelElement: ref(undefined),
        openReactionPicker,
        openCard
    });
    return { view, actions, openCard, openReactionPicker };
}

test("an author pressed opens their card through the block, for the room it was pressed in", () => {
    const { view, openCard } = setUp(team, { channelId: pair, rootId: 7 });
    const anchor = document.createElement("button");

    view.onMessageAction({ kind: "person", personAliasGuid: anna, anchor } as MessageActionRequest, null);
    view.onMessageAction({ kind: "person", personAliasGuid: anna, anchor } as MessageActionRequest, 7);

    expect(openCard.mock.calls).toEqual([[anchor, anna, team], [anchor, anna, pair]]);
});

test("the picker opens for the room it was asked in, with what opened it and where it starts", () => {
    const { view, openReactionPicker } = setUp(team);
    const anchor = document.createElement("div");
    const opener = document.createElement("button");

    view.onMessageAction({ kind: "openPicker", message, anchor, opener } as MessageActionRequest, null);
    view.onMessageAction({ kind: "openPicker", message, anchor, startsOn: "full" } as MessageActionRequest, null);

    expect(openReactionPicker.mock.calls).toEqual([[team, message, anchor, opener, undefined], [team, message, anchor, undefined, "full"]]);
});

test("a quote waits under its place's key, and nothing is done once that place is gone", () => {
    const { view, actions } = setUp(null, { channelId: pair, rootId: 7 });

    view.onMessageAction({ kind: "quote", message } as MessageActionRequest, null);
    expect(actions.startQuote).not.toHaveBeenCalled();

    view.onMessageAction({ kind: "quote", message } as MessageActionRequest, 7);
    expect(actions.startQuote).toHaveBeenCalledWith(view.threadQuoteKey.value, message);
});

test("the delete confirm names a channel by its title, and anything else as this conversation", () => {
    const channel = setUp(team);
    channel.view.onMessageAction({ kind: "delete", message } as MessageActionRequest, null);
    expect(channel.view.deleteBody.value).toBe("It will be removed for everyone in young-adults. You can't undo this.");

    const direct = setUp(pair);
    direct.view.onMessageAction({ kind: "delete", message } as MessageActionRequest, null);
    expect(direct.view.deleteBody.value).toBe("It will be removed for everyone in this conversation. You can't undo this.");
});
