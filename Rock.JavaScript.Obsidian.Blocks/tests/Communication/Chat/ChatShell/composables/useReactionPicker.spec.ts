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
// The block's one reaction picker: where it opens, the quick tray unless the control that asked
// has offered the quick reactions already, and what the message rows read of it while it is up.
// Every row asks about its own message, so opening and closing the picker reaches the rows of
// that message and no other. The shell is faked with only what the picker reads.
import { effect, reactive, shallowRef } from "vue";
import { useReactionPicker } from "../../../../../src/Communication/Chat/ChatShell/composables/useReactionPicker.partial";
import { ChatShell } from "../../../../../src/Communication/Chat/ChatShell/shell.partial";
import { TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const team = "c0000001-0000-4000-8000-000000000000";

function setUp(): ReturnType<typeof useReactionPicker> {
    const shell = shallowRef({
        reactions: {
            favorites: [],
            state: reactive({ catalog: null, recents: [] }),
            loadCatalog: jest.fn(async () => null),
            reactorNames: jest.fn(),
            recordRecent: jest.fn(),
            toggleCode: jest.fn()
        },
        timelines: { state: () => ({ messages: [] }) },
        threads: { state: { thread: null } }
    } as unknown as ChatShell);

    return useReactionPicker(shell, "Cedar Hill");
}

function message(id: number): TimelineMessage {
    return { id, body: "Hello" } as TimelineMessage;
}

test("opens on the quick tray, or on the full picker when the control that asked says so", () => {
    const picker = setUp();
    const anchor = document.createElement("button");
    const sheetPlus = document.createElement("button");

    picker.openReactionPicker(team, message(7), anchor);
    expect(picker.reactionPicker.value?.startsOn).toBe("tray");

    picker.openReactionPicker(team, message(7), sheetPlus, sheetPlus, { startsOn: "full" });
    expect(picker.reactionPicker.value?.startsOn).toBe("full");
});

test("keeps the quick reactions the control that asked already shows, for its tray to leave out", () => {
    const picker = setUp();
    const add = document.createElement("button");

    picker.openReactionPicker(team, message(7), add, add, { offered: ["thumbsup", "heart", "joy"] });
    expect(picker.reactionPicker.value).toMatchObject({ startsOn: "tray", offered: ["thumbsup", "heart", "joy"] });

    picker.closeReactionPicker();
    picker.openReactionPicker(team, message(7), add);
    expect(picker.reactionPicker.value?.offered).toEqual([]);
});

test("opening and closing reaches only the rows of the message it is open on", () => {
    const picker = setUp();
    const anchor = document.createElement("button");
    const runs = { 7: 0, 8: 0 };
    const seen: Record<number, HTMLElement | null> = {};
    for (const id of [7, 8] as const) {
        effect(() => {
            seen[id] = picker.reactionHost.pickerAnchorOf(id);
            runs[id]++;
        });
    }

    picker.openReactionPicker(team, message(7), anchor);
    expect(seen[7]).toBe(anchor);
    expect(seen[8]).toBeNull();

    picker.closeReactionPicker();
    expect(seen[7]).toBeNull();

    expect(runs).toEqual({ 7: 3, 8: 1 });
});
