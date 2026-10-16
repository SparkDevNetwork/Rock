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
// bar does. It has no reactions. It opens with focus on its first item, and Escape or a tap on
// the scrim closes it.
//
// The component these tests are written to, components/actionSheet.partial.obs:
//
//   props: {
//       isOpen: boolean,
//       message: TimelineMessage | null,   the message acted on, with the actions the server listed
//       isOwn: boolean,                    the person's own message, which they may edit and delete
//       isInThread: boolean                inside the thread drawer, where Reply in thread has no place
//   }
//   emits: replyInThread(messageId), quote(message), edit(message), copy(message), delete(message),
//          serverAction(action), close()
//
// The scrim is data-testid="action-sheet-scrim", the sheet data-testid="action-sheet".
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

let wrapper: VueWrapper | null = null;

async function open(props: Record<string, unknown>): Promise<VueWrapper> {
    wrapper = mount(ActionSheet, { props: { isOpen: true, isOwn: false, isInThread: false, ...props }, attachTo: document.body });
    await nextTick();
    await nextTick();
    return wrapper;
}

function sheet(w: VueWrapper): ReturnType<VueWrapper["get"]> {
    return w.get("[data-testid='action-sheet']");
}

function itemLabels(w: VueWrapper): string[] {
    return sheet(w).findAll("button").map(button => button.text().trim());
}

function item(w: VueWrapper, label: string): ReturnType<VueWrapper["get"]> {
    const found = sheet(w).findAll("button").find(button => button.text().trim() === label);
    if (!found) {
        throw new Error(`No item ${label}`);
    }
    return found;
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

    test("draw no reactions", async () => {
        const w = await open({ message: message() });

        expect(sheet(w).findAll("[aria-label^='React']")).toHaveLength(0);
        expect(sheet(w).findAll("[aria-label='More reactions']")).toHaveLength(0);
    });

    test("each asks as the bar would, and closes the sheet", async () => {
        const target = own({ actions: [approve] });
        const w = await open({ message: target, isOwn: true });

        for (const label of ["Reply in thread", "Quote", "Edit message", "Copy text", "Approve request", "Delete message"]) {
            await item(w, label).trigger("click");
        }

        expect(w.emitted("replyInThread")).toEqual([[42]]);
        expect(w.emitted("quote")).toEqual([[target]]);
        expect(w.emitted("edit")).toEqual([[target]]);
        expect(w.emitted("copy")).toEqual([[target]]);
        expect(w.emitted("serverAction")).toEqual([[approve]]);
        expect(w.emitted("delete")).toEqual([[target]]);
        expect(w.emitted("close")).toHaveLength(6);
    });
});

describe("opening and closing", () => {
    test("is a modal dialog named Message actions, with focus on its first item", async () => {
        const w = await open({ message: message() });

        expect(sheet(w).attributes("role")).toBe("dialog");
        expect(sheet(w).attributes("aria-modal")).toBe("true");
        expect(sheet(w).attributes("aria-label")).toBe("Message actions");
        expect(document.activeElement).toBe(item(w, "Reply in thread").element);
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

    test("Tab stays inside the sheet, from its last item back to its first", async () => {
        const w = await open({ message: message() });
        (item(w, "Copy text").element as HTMLElement).focus();

        await sheet(w).trigger("keydown", { key: "Tab" });

        expect(document.activeElement).toBe(item(w, "Reply in thread").element);
    });
});
