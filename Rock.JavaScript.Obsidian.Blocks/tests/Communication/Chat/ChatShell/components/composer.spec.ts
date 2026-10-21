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
// The composer itself, mounted: what is left in its box after a send is what the person sees,
// so it is read from the box rather than from the rule the box is meant to follow.
//
// The box is the chip editor, an editable element marked data-testid="composer-input". A test
// types into it as a browser does, drawing the lines and firing input, and reads back the wire
// text it holds, a mention as its token.
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import Composer from "../../../../../src/Communication/Chat/ChatShell/components/composer.partial.obs";
import ChipEditor from "../../../../../src/Communication/Chat/ChatShell/ui/chipEditor.partial.obs";

function editorOf(wrapper: VueWrapper): HTMLElement {
    return wrapper.get("[data-testid='composer-input']").element as HTMLElement;
}

/** Types into the editor as a browser does: each line a text node, a line break between, then input. */
async function type(wrapper: VueWrapper, text: string): Promise<void> {
    const editor = editorOf(wrapper);
    const nodes: Node[] = [];
    text.split("\n").forEach((line, index) => {
        if (index > 0) {
            nodes.push(document.createElement("br"));
        }
        if (line) {
            nodes.push(document.createTextNode(line));
        }
    });
    editor.replaceChildren(...nodes);
    editor.dispatchEvent(new Event("input", { bubbles: true }));
    await nextTick();
}

/** The wire text the box holds now, as a send would hand it over. */
function boxText(wrapper: VueWrapper): string {
    return (wrapper.findComponent(ChipEditor).vm as unknown as { getText: () => string }).getText();
}

async function sendFrom(isTextKeptOnSend: boolean): Promise<{ box: string, sent: unknown[] }> {
    const wrapper = mount(Composer, { props: { isTextKeptOnSend } });
    await type(wrapper, "hello");
    await wrapper.get("form").trigger("submit");

    return {
        box: boxText(wrapper),
        sent: wrapper.emitted("send") ?? []
    };
}

describe("the composer", () => {
    test("keeps the text in its box when the shell keeps it, as a draft's first message does", async () => {
        const { box, sent } = await sendFrom(true);

        expect(sent).toEqual([["hello"]]);
        expect(box).toBe("hello");
    });

    test("empties its box after an ordinary send", async () => {
        const { box, sent } = await sendFrom(false);

        expect(sent).toEqual([["hello"]]);
        expect(box).toBe("");
    });

    // With no cryptographic generator the send makes no row, so a failed row cannot keep the
    // text; only the box can.
    test("keeps the text in its box in a browser that cannot make a send key", async () => {
        const original = Object.getOwnPropertyDescriptor(globalThis, "crypto");
        Object.defineProperty(globalThis, "crypto", { value: undefined, configurable: true, writable: true });
        try {
            const { box, sent } = await sendFrom(false);

            expect(sent).toEqual([["hello"]]);
            expect(box).toBe("hello");
        }
        finally {
            if (original) {
                Object.defineProperty(globalThis, "crypto", original);
            }
        }
    });
});

describe("the composer's box", () => {
    test("tells the shell each change to its text, so a draft holds what the person sees", async () => {
        const wrapper = mount(Composer);
        await type(wrapper, "hello, edited");

        expect(wrapper.emitted("edit")?.slice(-1)).toEqual([["hello, edited"]]);
    });
});

describe("the composer's held text", () => {
    test("is put back in the box whenever what holds it changes, so a new empty draft does not show text typed in a channel", async () => {
        const wrapper = mount(Composer, { props: { heldText: "", heldBy: null } });
        await type(wrapper, "typed in a channel");

        await wrapper.setProps({ heldText: "", heldBy: { people: [] } });

        expect(boxText(wrapper)).toBe("");
        expect(editorOf(wrapper).textContent).toBe("");
    });
});

describe("the composer's placeholder", () => {
    test("names a channel plainly, with no hash character, since the hash is an icon", () => {
        const wrapper = mount(Composer, { props: { title: "young-adults", isChannel: true } });

        expect(wrapper.get("[data-testid='composer-input']").attributes("data-placeholder")).toBe("Message young-adults");
        expect(wrapper.get("[data-testid='composer-input']").attributes("aria-label")).toBe("Message young-adults");
    });

    test("names the other person of a direct message", () => {
        const wrapper = mount(Composer, { props: { title: "Anna Reyes" } });

        expect(wrapper.get("[data-testid='composer-input']").attributes("data-placeholder")).toBe("Message Anna Reyes");
        expect(wrapper.get("[data-testid='composer-input']").attributes("aria-label")).toBe("Message Anna Reyes");
    });
});

describe("the composer's key hint", () => {
    test("says how to send and how to start a new line", () => {
        const wrapper = mount(Composer, { props: { title: "Anna Reyes" } });

        expect(wrapper.get("[data-testid='composer-hint']").text()).toBe("Enter to send, Shift + Enter for a new line");
    });

    test("is left out on a phone, which has no Enter key worth naming", () => {
        const wrapper = mount(Composer, { props: { title: "Anna Reyes", isPhone: true } });

        expect(wrapper.find("[data-testid='composer-hint']").exists()).toBe(false);
    });
});

describe("the composer's keys", () => {
    test("Enter sends the text", async () => {
        const wrapper = mount(Composer);
        const input = wrapper.get("[data-testid='composer-input']");
        await type(wrapper, "hello");

        await input.trigger("keydown", { key: "Enter" });

        expect(wrapper.emitted("send")).toEqual([["hello"]]);
        expect(boxText(wrapper)).toBe("");
    });

    test("Shift and Enter does not send, leaving the new line to the box", async () => {
        const wrapper = mount(Composer);
        const input = wrapper.get("[data-testid='composer-input']");
        await type(wrapper, "hello");

        await input.trigger("keydown", { key: "Enter", shiftKey: true });

        expect(wrapper.emitted("send")).toBeUndefined();
        expect(boxText(wrapper)).toBe("hello\n");
    });
});

describe("the composer's send button", () => {
    test("is ready, in the church's fill, only once there is text to send", async () => {
        const wrapper = mount(Composer);
        const send = wrapper.get("[data-testid='composer-send']");
        expect(send.classes()).not.toContain("is-ready");
        expect(send.attributes("aria-label")).toBe("Send");

        await type(wrapper, "hello");

        expect(send.classes()).toContain("is-ready");
    });
});

// The editor is a block that grows with its lines between the sheet's one-line minimum and its
// cap, past which it scrolls; jsdom lays nothing out, so a cell reads what decides that height:
// the lines drawn, and no fixed height set over them.
describe("the composer's text box", () => {
    test("grows to the height of what is typed, which the sheet caps before it scrolls", async () => {
        const wrapper = mount(Composer, { attachTo: document.body });

        await type(wrapper, "one\ntwo\nthree\nfour");

        const editor = editorOf(wrapper);
        expect(boxText(wrapper)).toBe("one\ntwo\nthree\nfour");
        expect(editor.querySelectorAll("br")).toHaveLength(3);
        expect(editor.style.height).toBe("");
        wrapper.unmount();
    });

    test("goes back to one line once it is emptied by a send", async () => {
        const wrapper = mount(Composer, { attachTo: document.body });
        await type(wrapper, "one\ntwo\nthree\nfour");

        await wrapper.get("form").trigger("submit");

        // Empty to the last node: a line break left behind would hold a second line open.
        const editor = editorOf(wrapper);
        expect(boxText(wrapper)).toBe("");
        expect(editor.childNodes).toHaveLength(0);
        expect(editor.style.height).toBe("");
        wrapper.unmount();
    });
});

describe("the composer while the service is paused", () => {
    test("is a locked bar saying sending is paused, with no box to type into", () => {
        const wrapper = mount(Composer, { props: { title: "young-adults", isChannel: true, isLocked: true } });

        expect(wrapper.get("[data-testid='composer-locked']").text()).toBe("Sending is paused for maintenance.");
        expect(wrapper.find("[data-testid='composer-input']").exists()).toBe(false);
    });
});

// Up in an empty composer asks to edit the person's newest message they could edit, which the
// shell picks from the conversation; with text in the box Up only moves the caret. The composer
// says so by emitting editLast.
describe("the composer's Up key", () => {
    test("in an empty box asks to edit the newest own message, and keeps the key from moving the caret", async () => {
        const wrapper = mount(Composer, { props: { title: "Anna Reyes" } });

        const event = new KeyboardEvent("keydown", { key: "ArrowUp", bubbles: true, cancelable: true });
        wrapper.get("[data-testid='composer-input']").element.dispatchEvent(event);

        expect(wrapper.emitted("editLast")).toEqual([[]]);
        expect(event.defaultPrevented).toBe(true);
    });

    test("with text in the box asks for nothing", async () => {
        const wrapper = mount(Composer, { props: { title: "Anna Reyes", heldText: "", heldBy: null } });
        await wrapper.setProps({ heldText: "draft", heldBy: { draft: 1 } });

        await wrapper.get("[data-testid='composer-input']").trigger("keydown", { key: "ArrowUp" });
        expect(wrapper.emitted("editLast")).toBeUndefined();

        await wrapper.setProps({ heldText: "", heldBy: { draft: 2 } });
        await wrapper.get("[data-testid='composer-input']").trigger("keydown", { key: "ArrowUp" });
        expect(wrapper.emitted("editLast")).toEqual([[]]);
    });
});

// The message being quoted sits in a strip above the box: who is quoted and a one-line preview,
// with a cancel. The composer takes it as quote, the message as a page carries it, and
// isQuoteOwn, whether it is the person's own; its cancel emits cancelQuote. The strip is marked
// data-testid="composer-quote", its preview data-testid="composer-quote-preview".
describe("the composer's quote strip", () => {
    const annasMessage = {
        id: 40,
        person_alias_guid: "a0000002-0000-4000-8000-000000000000",
        sender_nick_name: "Anna",
        sender_last_name: "Reyes",
        message_type: "text",
        body: "Morning all",
        pieces: null,
        created_at: "2026-10-15T09:00:00Z"
    };

    test("is not drawn with nothing quoted", () => {
        const quoting = mount(Composer, { props: { title: "Anna Reyes", quote: annasMessage } as Record<string, unknown> });
        expect(quoting.find("[data-testid='composer-quote']").exists()).toBe(true);

        const wrapper = mount(Composer, { props: { title: "Anna Reyes", quote: null } as Record<string, unknown> });

        expect(wrapper.find("[data-testid='composer-quote']").exists()).toBe(false);
    });

    test("names the person quoted over a preview of the message", () => {
        const wrapper = mount(Composer, { props: { title: "Anna Reyes", quote: annasMessage } as Record<string, unknown> });

        const strip = wrapper.get("[data-testid='composer-quote']");
        expect(strip.text()).toContain("Quoting Anna Reyes");
        expect(strip.get("[data-testid='composer-quote-preview']").text()).toBe("Morning all");
    });

    test("says Quoting yourself for the person's own message", () => {
        const own = { ...annasMessage, person_alias_guid: "a0000001-0000-4000-8000-000000000000", sender_nick_name: "Jordan", sender_last_name: "Ellis" };
        const wrapper = mount(Composer, { props: { title: "Anna Reyes", quote: own, isQuoteOwn: true } as Record<string, unknown> });

        expect(wrapper.get("[data-testid='composer-quote']").text()).toContain("Quoting yourself");
        expect(wrapper.get("[data-testid='composer-quote']").text()).not.toContain("Jordan");
    });

    test("its cancel button says what it does and drops the quote", async () => {
        const wrapper = mount(Composer, { props: { title: "Anna Reyes", quote: annasMessage } as Record<string, unknown> });

        await wrapper.get("[data-testid='composer-quote'] button[aria-label='Cancel quote']").trigger("click");

        expect(wrapper.emitted("cancelQuote")).toEqual([[]]);
    });
});

// The emoji button opens the picker in insert mode, anchored on the button: a Unicode emoji goes
// into the box as its glyph at the caret, an image emoji as its :code: text, which the message
// draws. The composer takes the picker's data as emojiSource, { favorites, catalog, recents,
// churchName }, the same the shell hands every picker; it emits emojiPickerOpen() as the picker
// opens, so the shell can fetch the catalog, and emojiChosen(code) for the recently used. The
// button is "Add emoji", data-testid="composer-emoji"; the picker is the emoji picker component.
describe("the composer's emoji button", () => {
    const emojiSource = {
        favorites: [
            { code: "thumbsup", label: "thumbs up", emoji: "👍", image_url: null },
            { code: "pray", label: "folded hands", emoji: "🙏", image_url: null }
        ],
        catalog: [
            { code: "thumbsup", label: "thumbs up", emoji: "👍", image_url: null, category: "people", keywords: [] },
            { code: "pray", label: "folded hands", emoji: "🙏", image_url: null, category: "people", keywords: ["pray"] },
            { code: "cedar_hill", label: "cedar hill", emoji: null, image_url: "https://cdn.example.org/cedar.png", category: "objects", keywords: [] }
        ],
        recents: [],
        churchName: "Cedar Hill"
    };

    let mounted: VueWrapper | null = null;

    beforeAll(() => {
        // jsdom has no ResizeObserver, and floating placement keeps the picker beside its anchor with one.
        if (typeof window.ResizeObserver === "undefined") {
            window.ResizeObserver = class {
                observe(): void { /* jsdom lays nothing out, so there is nothing to watch. */ }
                unobserve(): void { /* As above. */ }
                disconnect(): void { /* As above. */ }
            } as unknown as typeof ResizeObserver;
        }
    });

    afterEach(() => {
        mounted?.unmount();
        mounted = null;
        document.body.replaceChildren();
    });

    async function settle(): Promise<void> {
        for (let i = 0; i < 3; i++) {
            await Promise.resolve();
            await nextTick();
        }
    }

    /** Types the text, then puts the caret after its first characters, as a click there would. */
    async function typeWithCaret(w: VueWrapper, text: string, caretAt: number): Promise<void> {
        await type(w, text);
        const node = editorOf(w).firstChild as Text;
        const range = document.createRange();
        range.setStart(node, caretAt);
        range.collapse(true);
        const selection = window.getSelection() as Selection;
        selection.removeAllRanges();
        selection.addRange(range);
        document.dispatchEvent(new Event("selectionchange"));
        await nextTick();
    }

    function pickerCell(code: string): HTMLElement {
        const cell = document.querySelector<HTMLElement>(`[role='gridcell'][data-code='${code}']`);
        if (!cell) {
            throw new Error(`No picker cell ${code}`);
        }
        return cell;
    }

    test("is Add emoji, and opens the picker in insert mode", async () => {
        mounted = mount(Composer, { props: { title: "Anna Reyes", emojiSource } as Record<string, unknown>, attachTo: document.body });
        const button = mounted.get("[data-testid='composer-emoji']");
        expect(button.attributes("aria-label")).toBe("Add emoji");
        expect(document.querySelector("[role='grid']")).toBeNull();

        await button.trigger("click");
        await settle();

        expect(document.querySelector("[role='grid']")).not.toBeNull();
        expect(mounted.emitted("emojiPickerOpen")).toEqual([[]]);
        expect(document.querySelector("[role='gridcell'][aria-pressed='true']")).toBeNull();
    });

    test("puts the chosen emoji's glyph in the box at the caret, and leaves the person typing", async () => {
        mounted = mount(Composer, { props: { title: "Anna Reyes", emojiSource } as Record<string, unknown>, attachTo: document.body });
        await typeWithCaret(mounted, "Hi there", 2);

        await mounted.get("[data-testid='composer-emoji']").trigger("click");
        await settle();
        pickerCell("pray").click();
        await settle();

        expect(boxText(mounted)).toBe("Hi🙏 there");
        expect(mounted.emitted("emojiChosen")).toEqual([["pray"]]);
        expect(document.querySelector("[role='grid']")).toBeNull();
        expect(document.activeElement).toBe(editorOf(mounted));
    });

    test("puts an image emoji in the box as its code", async () => {
        mounted = mount(Composer, { props: { title: "Anna Reyes", emojiSource } as Record<string, unknown>, attachTo: document.body });
        await typeWithCaret(mounted, "Welcome ", 8);

        await mounted.get("[data-testid='composer-emoji']").trigger("click");
        await settle();
        pickerCell("cedar_hill").click();
        await settle();

        expect(boxText(mounted)).toBe("Welcome :cedar_hill:");
        expect(mounted.emitted("emojiChosen")).toEqual([["cedar_hill"]]);
    });
});

describe("the composer's @ picker", () => {
    const anna = "a0000002-0000-4000-8000-000000000000";
    const mentionSource = { canMention: true, people: [{ person_alias_guid: anna, nick_name: "Anna", last_name: "Reyes" }], isAllOffered: false };

    let mounted: VueWrapper | null = null;

    beforeAll(() => {
        if (typeof window.ResizeObserver === "undefined") {
            window.ResizeObserver = class {
                observe(): void { /* jsdom lays nothing out, so there is nothing to watch. */ }
                unobserve(): void { /* As above. */ }
                disconnect(): void { /* As above. */ }
            } as unknown as typeof ResizeObserver;
        }
    });

    afterEach(() => {
        mounted?.unmount();
        mounted = null;
        document.body.replaceChildren();
    });

    /** Types the text with the caret at its end, as a person typing leaves it. */
    async function typeToEnd(w: VueWrapper, text: string): Promise<void> {
        const editor = editorOf(w);
        const node = document.createTextNode(text);
        editor.replaceChildren(node);
        const range = document.createRange();
        range.setStart(node, text.length);
        range.collapse(true);
        const selection = window.getSelection() as Selection;
        selection.removeAllRanges();
        selection.addRange(range);
        editor.dispatchEvent(new Event("input", { bubbles: true }));
        await nextTick();
        await nextTick();
    }

    test("appears above the box once @ is typed, and tells the shell the query", async () => {
        mounted = mount(Composer, { props: { title: "young-adults", isChannel: true, mentionSource } as Record<string, unknown>, attachTo: document.body });

        await typeToEnd(mounted, "Hi @an");

        expect(document.querySelector("[data-testid='mention-picker']")).not.toBeNull();
        expect(mounted.emitted("mentionQuery")?.slice(-1)).toEqual([["an"]]);
    });

    test("takes Enter while open: the chip goes in and nothing is sent", async () => {
        mounted = mount(Composer, { props: { title: "young-adults", isChannel: true, mentionSource } as Record<string, unknown>, attachTo: document.body });
        await typeToEnd(mounted, "Hi @an");
        expect(document.querySelector("[data-testid='mention-picker']")).not.toBeNull();

        await mounted.get("[data-testid='composer-input']").trigger("keydown", { key: "Enter" });
        await nextTick();

        expect(mounted.emitted("send")).toBeUndefined();
        expect(boxText(mounted)).toBe(`Hi <@${anna}> `);
        expect(document.querySelector("[data-testid='mention-picker']")).toBeNull();
        expect(mounted.emitted("mentionQuery")?.slice(-1)).toEqual([[null]]);
    });
});

// A room the person cannot post in gives the box way to a locked bar carrying the server's own
// sentence, as the sidebar row's can_post_reason says it; the join case has its join bar instead,
// which the block draws in the composer's place, and a paused service its own words above. The box counts down in the last 500 characters of
// the room's max_length and over it, and a send over the limit is held back with a toast's word.
describe("the composer where the person cannot post", () => {
    test("is a locked bar carrying the server's sentence, with no box", () => {
        const wrapper = mount(Composer, { props: { lockedReason: "Only leaders can post in Announcements." } });

        const bar = wrapper.get("[data-testid='composer-locked']");
        expect(bar.text()).toBe("Only leaders can post in Announcements.");
        expect(wrapper.find("[data-testid='composer-input']").exists()).toBe(false);
    });

    test("is the box again once there is no reason", async () => {
        const wrapper = mount(Composer, { props: { lockedReason: "Only leaders can post in Announcements." } });

        await wrapper.setProps({ lockedReason: null });

        expect(wrapper.find("[data-testid='composer-locked']").exists()).toBe(false);
        expect(wrapper.find("[data-testid='composer-input']").exists()).toBe(true);
    });
});

describe("the composer's counter", () => {
    test("is silent until the last 500 characters of the limit", async () => {
        const wrapper = mount(Composer, { props: { maxLength: 5000 } });
        await type(wrapper, "x".repeat(4500));

        expect(wrapper.get("[data-testid='composer-count']").text()).toBe("");
    });

    test("counts down in the warning ink inside the last 500", async () => {
        const wrapper = mount(Composer, { props: { maxLength: 5000 } });
        await type(wrapper, "x".repeat(4812));

        const count = wrapper.get("[data-testid='composer-count']");
        expect(count.text()).toBe("188 left");
        expect(count.attributes("data-tone")).toBe("warn");
    });

    test("says how far over in the danger ink past the limit", async () => {
        const wrapper = mount(Composer, { props: { maxLength: 5000 } });
        await type(wrapper, "x".repeat(5012));

        const count = wrapper.get("[data-testid='composer-count']");
        expect(count.text()).toBe("12 over the limit");
        expect(count.attributes("data-tone")).toBe("over");
    });

    test("holds a send over the limit back, keeping the text, and says why", async () => {
        const wrapper = mount(Composer, { props: { maxLength: 5000 } });
        await type(wrapper, "x".repeat(5001));
        await wrapper.get("form").trigger("submit");

        expect(wrapper.emitted("send")).toBeUndefined();
        expect(wrapper.emitted("overLimit")).toEqual([[5000]]);
        expect(boxText(wrapper)).toBe("x".repeat(5001));
    });

    test("counts an emoji as one character, as the platform does", async () => {
        const wrapper = mount(Composer, { props: { maxLength: 5000 } });
        await type(wrapper, "x".repeat(4998) + "\u{1F600}\u{1F600}");

        expect(wrapper.get("[data-testid='composer-count']").text()).toBe("0 left");
        await wrapper.get("form").trigger("submit");
        expect(wrapper.emitted("send")).toHaveLength(1);
    });

    test("sends at exactly the limit", async () => {
        const wrapper = mount(Composer, { props: { maxLength: 5000 } });
        await type(wrapper, "x".repeat(5000));
        await wrapper.get("form").trigger("submit");

        expect(wrapper.emitted("send")).toHaveLength(1);
        expect(wrapper.get("[data-testid='composer-count']").text()).toBe("");
    });
});
