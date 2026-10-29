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
// The phone's action sheet, mounted: a dialog from the bottom of the chat over a scrim, holding
// the same choices as a message's bar and menu as large items, and asking for each the way the
// bar does. Above the items, a row of the church's first six favorites and a plus for the full
// picker. It opens with focus on its first reaction, and Escape or a tap on the scrim closes it.
//
// The component these tests are written to, components/actionSheet.partial.obs:
//
//   props: {
//       isOpen: boolean,
//       message: TimelineMessage | null,   the message acted on, with the actions the server listed
//       isOwn: boolean,                    the person's own message, which they may edit and delete
//       isInThread: boolean,               inside the thread drawer, where Reply in thread has no place
//       favorites: [{ code, label, emoji, image_url }]   the bootstrap's favorites; the first six are drawn
//   }
//   emits: replyInThread(message), quote(message), edit(message), copy(message), delete(message),
//          serverAction(action), react(message, code), openPicker(message, anchor), close()
//
// The scrim is data-testid="action-sheet-scrim", the sheet data-testid="action-sheet", its row of
// reactions data-testid="action-sheet-reactions": each reaction read "React with thumbs up" or
// "Remove your thumbs up", pressed when the reader's, and the plus read "More reactions".
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import ActionSheet from "../../../../../src/Communication/Chat/ChatShell/components/actionSheet.partial.obs";
import { TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const me = "a0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";

function message(extra: Partial<TimelineMessage> = {}): TimelineMessage {
    return {
        id: 42,
        parent_id: null,
        person_alias_guid: anna,
        sender_nick_name: "Anna",
        sender_last_name: "Reyes",
        sender_avatar_url: null,
        sender_listed: true,
        message_type: "text",
        body: "Morning all",
        pieces: null,
        blocks: null,
        created_at: new Date(2026, 9, 12, 9, 2).toISOString(),
        edited_at: null,
        deleted_at: null,
        visibility: "visible",
        visibility_notice: null,
        actions: [],
        ...extra
    } as TimelineMessage;
}

const own = (extra: Partial<TimelineMessage> = {}): TimelineMessage => message({ person_alias_guid: me, ...extra });

const approve = { id: "approve", label: "Approve request", call: "chat_action_approve" };

/** The church's favorites as the bootstrap carries them, in order; the sheet draws the first six. */
const favorites = [
    { code: "thumbsup", label: "thumbs up", emoji: "👍", image_url: null },
    { code: "heart", label: "red heart", emoji: "❤️", image_url: null },
    { code: "joy", label: "face with tears of joy", emoji: "😂", image_url: null },
    { code: "open_mouth", label: "face with open mouth", emoji: "😮", image_url: null },
    { code: "cry", label: "crying face", emoji: "😢", image_url: null },
    { code: "pray", label: "folded hands", emoji: "🙏", image_url: null },
    { code: "tada", label: "party popper", emoji: "🎉", image_url: null },
    { code: "raised_hands", label: "raising hands", emoji: "🙌", image_url: null }
];

let wrapper: VueWrapper | null = null;

async function open(props: Record<string, unknown>): Promise<VueWrapper> {
    wrapper = mount(ActionSheet, { props: { isOpen: true, isOwn: false, isInThread: false, favorites, ...props }, attachTo: document.body });
    await nextTick();
    await nextTick();
    return wrapper;
}

function sheet(w: VueWrapper): ReturnType<VueWrapper["get"]> {
    return w.get("[data-testid='action-sheet']");
}

/** The sheet's items, below its row of reactions. */
function items(w: VueWrapper): ReturnType<VueWrapper["findAll"]> {
    return sheet(w).findAll("button").filter(button => !button.element.closest("[data-testid='action-sheet-reactions']"));
}

function itemLabels(w: VueWrapper): string[] {
    return items(w).map(button => button.text().trim());
}

function item(w: VueWrapper, label: string): ReturnType<VueWrapper["get"]> {
    const found = items(w).find(button => button.text().trim() === label);
    if (!found) {
        throw new Error(`No item ${label}`);
    }
    return found;
}

/** The row's buttons: the six reactions, then the plus. */
function reactions(w: VueWrapper): ReturnType<VueWrapper["findAll"]> {
    return w.get("[data-testid='action-sheet-reactions']").findAll("button");
}

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
});

describe("the sheet's items", () => {
    test("for the person's own message: Reply in thread, Quote, Edit, Copy, the server's actions, then Delete", async () => {
        const w = await open({ message: own({ actions: [approve] }), isOwn: true });

        expect(itemLabels(w)).toEqual(["Reply in thread", "Quote", "Edit message", "Copy text", "Approve request", "Delete message"]);
        expect(item(w, "Delete message").classes()).toContain("is-danger");
    });

    test("for another person's message: Reply in thread, Quote and Copy only", async () => {
        const w = await open({ message: message() });

        expect(itemLabels(w)).toEqual(["Reply in thread", "Quote", "Copy text"]);
    });

    test("leave out Reply in thread inside a thread", async () => {
        const w = await open({ message: own({ parent_id: 40 }), isOwn: true, isInThread: true });

        expect(itemLabels(w)).toEqual(["Quote", "Edit message", "Copy text", "Delete message"]);
    });

    test("leave out Edit on the person's own block post, which cannot be edited", async () => {
        const w = await open({ message: own({ blocks: [{ type: "text", text: "Card", fallback: { type: "text", text: "Card" } }] as TimelineMessage["blocks"] }), isOwn: true });

        expect(itemLabels(w)).toEqual(["Reply in thread", "Quote", "Copy text", "Delete message"]);
    });

    test("each asks as the bar would, and closes the sheet", async () => {
        const target = own({ actions: [approve] });
        const w = await open({ message: target, isOwn: true });

        for (const label of ["Reply in thread", "Quote", "Edit message", "Copy text", "Approve request", "Delete message"]) {
            await item(w, label).trigger("click");
        }

        expect(w.emitted("replyInThread")).toEqual([[target]]);
        expect(w.emitted("quote")).toEqual([[target]]);
        expect(w.emitted("edit")).toEqual([[target]]);
        expect(w.emitted("copy")).toEqual([[target]]);
        expect(w.emitted("serverAction")).toEqual([[approve]]);
        expect(w.emitted("delete")).toEqual([[target]]);
        expect(w.emitted("close")).toHaveLength(6);
    });
});

describe("opening and closing", () => {
    test("is a modal dialog named Message actions, with focus on its first reaction", async () => {
        const w = await open({ message: message() });

        expect(sheet(w).attributes("role")).toBe("dialog");
        expect(sheet(w).attributes("aria-modal")).toBe("true");
        expect(sheet(w).attributes("aria-label")).toBe("Message actions");
        expect(document.activeElement).toBe(reactions(w)[0].element);
        expect(reactions(w)[0].attributes("aria-label")).toBe("React with thumbs up");
    });

    test("draws nothing while closed", async () => {
        const w = await open({ message: message(), isOpen: false });

        expect(w.find("[data-testid='action-sheet']").exists()).toBe(false);
    });

    test("Escape closes it", async () => {
        const w = await open({ message: message() });

        await sheet(w).trigger("keydown", { key: "Escape" });

        expect(w.emitted("close")).toHaveLength(1);
    });

    test("a tap on the scrim closes it; a tap inside the sheet off its items does not", async () => {
        const w = await open({ message: message() });

        await w.get("[data-testid='action-sheet']").trigger("click");
        expect(w.emitted("close")).toBeUndefined();

        await w.get("[data-testid='action-sheet-scrim']").trigger("click");
        expect(w.emitted("close")).toHaveLength(1);
    });

    test("Tab stays inside the sheet, from its last item back to its first reaction", async () => {
        const w = await open({ message: message() });
        (item(w, "Copy text").element as HTMLElement).focus();

        await sheet(w).trigger("keydown", { key: "Tab" });

        expect(document.activeElement).toBe(reactions(w)[0].element);
    });
});

// The phone has no hover bar, so the sheet carries the reactions: the church's first six
// favorites, each a toggle of the reader's own, and a plus for the full picker.
describe("the sheet's reactions", () => {
    const yourPray = { code: "pray", count: 2, mine: true, emoji: "🙏", image_url: null, label: "folded hands" };

    test("are the first six favorites, then a plus for the full picker", async () => {
        const w = await open({ message: message({ reactions: [yourPray] } as Partial<TimelineMessage>) });

        expect(reactions(w).map(b => b.text().trim()).slice(0, 6)).toEqual(["👍", "❤️", "😂", "😮", "😢", "🙏"]);
        expect(reactions(w).map(b => b.attributes("aria-label"))).toEqual([
            "React with thumbs up",
            "React with red heart",
            "React with face with tears of joy",
            "React with face with open mouth",
            "React with crying face",
            "Remove your folded hands",
            "More reactions"
        ]);
        expect(reactions(w).slice(0, 6).map(b => b.attributes("aria-pressed"))).toEqual(["false", "false", "false", "false", "false", "true"]);
    });

    test("a reaction asks to react to the message with it, and closes the sheet", async () => {
        const target = message();
        const w = await open({ message: target });

        await reactions(w)[1].trigger("click");

        expect(w.emitted("react")).toEqual([[target, "heart"]]);
        expect(w.emitted("close")).toHaveLength(1);
    });

    // The sheet has already offered six quick reactions, so its plus skips the picker's quick tray.
    test("the plus asks for the full picker for the message, past the quick tray", async () => {
        const target = message();
        const w = await open({ message: target });
        const plus = reactions(w)[6];

        await plus.trigger("click");

        expect(w.emitted("openPicker")?.[0]).toEqual([target, plus.element, { startsOn: "full" }]);
        expect(w.emitted("react")).toBeUndefined();
    });
});
