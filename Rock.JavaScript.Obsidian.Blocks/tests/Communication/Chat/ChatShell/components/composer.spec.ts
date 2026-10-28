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
import { readFileSync } from "fs";
import { join } from "path";
import { flushPromises, mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import Composer from "../../../../../src/Communication/Chat/ChatShell/components/composer.partial.obs";
import ChipEditor from "../../../../../src/Communication/Chat/ChatShell/ui/chipEditor.partial.obs";
import { AttachmentTray, createAttachments } from "../../../../../src/Communication/Chat/ChatShell/composables/useAttachments.partial";
import { harness, Harness, picked, settle } from "../attachmentFakes";

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

/**
 * Mounts the composer with an owner keeping its text, as the shell keeps each conversation's:
 * each change the box tells comes back to it as its text.
 */
function mountComposer(options: Record<string, unknown> = {}): VueWrapper {
    const props = (options.props ?? {}) as Record<string, unknown>;
    const wrapper: VueWrapper = mount(Composer, {
        ...options,
        props: {
            text: "",
            "onUpdate:text": (text: string) => wrapper.setProps({ text }),
            ...props
        }
    });
    return wrapper;
}

/** The wire text the box holds now, as a send would hand it over. */
function boxText(wrapper: VueWrapper): string {
    return (wrapper.findComponent(ChipEditor).vm as unknown as { getText: () => string }).getText();
}

async function sendFrom(isTextKeptOnSend: boolean): Promise<{ box: string, sent: unknown[] }> {
    const wrapper = mountComposer({ props: { isTextKeptOnSend } });
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
    test("tells its owner each change to its text, so the draft holds what the person sees", async () => {
        const wrapper = mountComposer();
        await type(wrapper, "hello, edited");

        expect(wrapper.emitted("update:text")?.slice(-1)).toEqual([["hello, edited"]]);
    });

    test("tells its owner each chip drawn, with the name and photo it shows", async () => {
        const wrapper = mountComposer({ attachTo: document.body });
        (wrapper.findComponent(ChipEditor).vm as unknown as { insertMention: (token: string, name: string, photo: string | null) => void })
            .insertMention("<@a0000002-0000-4000-8000-000000000000>", "Anna Kim", "https://x/anna.jpg");
        await nextTick();

        expect(wrapper.emitted("mention")).toEqual([[{ type: "mention", text: "@Anna Kim", person_alias_guid: "a0000002-0000-4000-8000-000000000000", avatar_url: "https://x/anna.jpg" }]]);
        wrapper.unmount();
    });
});

// The shell warms the @ picker's people on this, so a person who only reads costs nothing.
describe("the composer's focus", () => {
    test("is told once each time focus comes into the box, not as it moves within it", async () => {
        const outside = document.createElement("button");
        document.body.append(outside);
        const wrapper = mountComposer({ attachTo: document.body });
        const emoji = wrapper.get("[data-testid='composer-emoji']").element as HTMLElement;

        editorOf(wrapper).focus();
        emoji.focus();
        editorOf(wrapper).focus();
        expect(wrapper.emitted("focus")).toHaveLength(1);

        outside.focus();
        editorOf(wrapper).focus();
        expect(wrapper.emitted("focus")).toHaveLength(2);

        wrapper.unmount();
        outside.remove();
    });

    // The shell's drafts leave the text of a box being typed in alone when another tab writes it.
    test("is told once each time focus leaves the box, not as it moves within it", async () => {
        const outside = document.createElement("button");
        document.body.append(outside);
        const wrapper = mountComposer({ attachTo: document.body });
        const emoji = wrapper.get("[data-testid='composer-emoji']").element as HTMLElement;

        editorOf(wrapper).focus();
        emoji.focus();
        expect(wrapper.emitted("blur")).toBeUndefined();

        outside.focus();
        expect(wrapper.emitted("blur")).toHaveLength(1);

        wrapper.unmount();
        outside.remove();
    });
});

describe("the composer's text, kept by its owner", () => {
    test("is the text of the place the box types into: another place's text replaces it, empty included", async () => {
        const wrapper = mountComposer({ props: { conversationKey: "team" } });
        await type(wrapper, "typed in a channel");

        await wrapper.setProps({ text: "", conversationKey: "anna" });
        expect(boxText(wrapper)).toBe("");
        expect(editorOf(wrapper).textContent).toBe("");

        await wrapper.setProps({ text: "typed in a channel", conversationKey: "team" });
        expect(boxText(wrapper)).toBe("typed in a channel");
    });

    test("draws a kept chip with the name and photo it was chosen with", async () => {
        const anna = "a0000002-0000-4000-8000-000000000000";
        const wrapper = mountComposer({
            props: {
                text: `Thanks <@${anna}>`,
                mentions: [{ type: "mention", text: "@Anna Kim", person_alias_guid: anna, avatar_url: "https://x/anna.jpg" }]
            }
        });

        const chip = editorOf(wrapper).querySelector("[data-testid='editor-chip']");
        expect(chip?.textContent).toContain("Anna Kim");
        expect(chip?.querySelector("img")?.getAttribute("src")).toBe("https://x/anna.jpg");
        expect(boxText(wrapper)).toBe(`Thanks <@${anna}>`);
    });
});

describe("the composer's placeholder", () => {
    test("names a channel plainly, with no hash character, since the hash is an icon", () => {
        const wrapper = mountComposer({ props: { title: "young-adults", isChannel: true } });

        expect(wrapper.get("[data-testid='composer-input']").attributes("data-placeholder")).toBe("Message young-adults");
        expect(wrapper.get("[data-testid='composer-input']").attributes("aria-label")).toBe("Message young-adults");
    });

    test("names the other person of a direct message", () => {
        const wrapper = mountComposer({ props: { title: "Anna Reyes" } });

        expect(wrapper.get("[data-testid='composer-input']").attributes("data-placeholder")).toBe("Message Anna Reyes");
        expect(wrapper.get("[data-testid='composer-input']").attributes("aria-label")).toBe("Message Anna Reyes");
    });
});

describe("the composer's key hint", () => {
    test("says how to send and how to start a new line", () => {
        const wrapper = mountComposer({ props: { title: "Anna Reyes" } });

        expect(wrapper.get("[data-testid='composer-hint']").text()).toBe("Enter to send, Shift + Enter for a new line");
    });

    test("is left out on a phone, which has no Enter key worth naming", () => {
        const wrapper = mountComposer({ props: { title: "Anna Reyes", isPhone: true } });

        expect(wrapper.find("[data-testid='composer-hint']").exists()).toBe(false);
    });
});

describe("the composer's keys", () => {
    test("Enter sends the text", async () => {
        const wrapper = mountComposer();
        const input = wrapper.get("[data-testid='composer-input']");
        await type(wrapper, "hello");

        await input.trigger("keydown", { key: "Enter" });

        expect(wrapper.emitted("send")).toEqual([["hello"]]);
        expect(boxText(wrapper)).toBe("");
    });

    test("Shift and Enter does not send, leaving the new line to the box", async () => {
        const wrapper = mountComposer();
        const input = wrapper.get("[data-testid='composer-input']");
        await type(wrapper, "hello");

        await input.trigger("keydown", { key: "Enter", shiftKey: true });

        expect(wrapper.emitted("send")).toBeUndefined();
        expect(boxText(wrapper)).toBe("hello\n");
    });
});

describe("the composer's send button", () => {
    test("is ready, in the church's fill, only once there is text to send", async () => {
        const wrapper = mountComposer();
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
        const wrapper = mountComposer({ attachTo: document.body });

        await type(wrapper, "one\ntwo\nthree\nfour");

        const editor = editorOf(wrapper);
        expect(boxText(wrapper)).toBe("one\ntwo\nthree\nfour");
        expect(editor.querySelectorAll("br")).toHaveLength(3);
        expect(editor.style.height).toBe("");
        wrapper.unmount();
    });

    test("goes back to one line once it is emptied by a send", async () => {
        const wrapper = mountComposer({ attachTo: document.body });
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
        const wrapper = mountComposer({ props: { title: "young-adults", isChannel: true, isLocked: true } });

        expect(wrapper.get("[data-testid='composer-locked']").text()).toBe("Sending is paused for maintenance.");
        expect(wrapper.find("[data-testid='composer-input']").exists()).toBe(false);
    });
});

// Up in an empty composer asks to edit the person's newest message they could edit, which the
// shell picks from the conversation; with text in the box Up only moves the caret. The composer
// says so by emitting editLast.
describe("the composer's Up key", () => {
    test("in an empty box asks to edit the newest own message, and keeps the key from moving the caret", async () => {
        const wrapper = mountComposer({ props: { title: "Anna Reyes" } });

        const event = new KeyboardEvent("keydown", { key: "ArrowUp", bubbles: true, cancelable: true });
        wrapper.get("[data-testid='composer-input']").element.dispatchEvent(event);

        expect(wrapper.emitted("editLast")).toEqual([[]]);
        expect(event.defaultPrevented).toBe(true);
    });

    test("with text in the box asks for nothing", async () => {
        const wrapper = mountComposer({ props: { title: "Anna Reyes" } });
        await wrapper.setProps({ text: "draft" });

        await wrapper.get("[data-testid='composer-input']").trigger("keydown", { key: "ArrowUp" });
        expect(wrapper.emitted("editLast")).toBeUndefined();

        await wrapper.setProps({ text: "" });
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
        const quoting = mountComposer({ props: { title: "Anna Reyes", quote: annasMessage } as Record<string, unknown> });
        expect(quoting.find("[data-testid='composer-quote']").exists()).toBe(true);

        const wrapper = mountComposer({ props: { title: "Anna Reyes", quote: null } as Record<string, unknown> });

        expect(wrapper.find("[data-testid='composer-quote']").exists()).toBe(false);
    });

    test("names the person quoted over a preview of the message", () => {
        const wrapper = mountComposer({ props: { title: "Anna Reyes", quote: annasMessage } as Record<string, unknown> });

        const strip = wrapper.get("[data-testid='composer-quote']");
        expect(strip.text()).toContain("Quoting Anna Reyes");
        expect(strip.get("[data-testid='composer-quote-preview']").text()).toBe("Morning all");
    });

    test("says Quoting yourself for the person's own message", () => {
        const own = { ...annasMessage, person_alias_guid: "a0000001-0000-4000-8000-000000000000", sender_nick_name: "Jordan", sender_last_name: "Ellis" };
        const wrapper = mountComposer({ props: { title: "Anna Reyes", quote: own, isQuoteOwn: true } as Record<string, unknown> });

        expect(wrapper.get("[data-testid='composer-quote']").text()).toContain("Quoting yourself");
        expect(wrapper.get("[data-testid='composer-quote']").text()).not.toContain("Jordan");
    });

    test("its cancel button says what it does and drops the quote", async () => {
        const wrapper = mountComposer({ props: { title: "Anna Reyes", quote: annasMessage } as Record<string, unknown> });

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
        mounted = mountComposer({ props: { title: "Anna Reyes", emojiSource } as Record<string, unknown>, attachTo: document.body });
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
        mounted = mountComposer({ props: { title: "Anna Reyes", emojiSource } as Record<string, unknown>, attachTo: document.body });
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
        mounted = mountComposer({ props: { title: "Anna Reyes", emojiSource } as Record<string, unknown>, attachTo: document.body });
        await typeWithCaret(mounted, "Welcome ", 8);

        await mounted.get("[data-testid='composer-emoji']").trigger("click");
        await settle();

        // The picker draws one section at a time; the church's own emoji is under Objects.
        document.querySelector<HTMLElement>("[role='tab'][aria-label='Objects']")?.click();
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
        mounted = mountComposer({ props: { title: "young-adults", isChannel: true, mentionSource } as Record<string, unknown>, attachTo: document.body });

        await typeToEnd(mounted, "Hi @an");

        expect(document.querySelector("[data-testid='mention-picker']")).not.toBeNull();
        expect(mounted.emitted("mentionQuery")?.slice(-1)).toEqual([["an"]]);
    });

    test("takes Enter while open: the chip goes in and nothing is sent", async () => {
        mounted = mountComposer({ props: { title: "young-adults", isChannel: true, mentionSource } as Record<string, unknown>, attachTo: document.body });
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
        const wrapper = mountComposer({ props: { lockedReason: "Only leaders can post in Announcements." } });

        const bar = wrapper.get("[data-testid='composer-locked']");
        expect(bar.text()).toBe("Only leaders can post in Announcements.");
        expect(wrapper.find("[data-testid='composer-input']").exists()).toBe(false);
    });

    test("is the box again once there is no reason", async () => {
        const wrapper = mountComposer({ props: { lockedReason: "Only leaders can post in Announcements." } });

        await wrapper.setProps({ lockedReason: null });

        expect(wrapper.find("[data-testid='composer-locked']").exists()).toBe(false);
        expect(wrapper.find("[data-testid='composer-input']").exists()).toBe(true);
    });
});

describe("the composer's counter", () => {
    test("is silent until the last 500 characters of the limit", async () => {
        const wrapper = mountComposer({ props: { maxLength: 5000 } });
        await type(wrapper, "x".repeat(4500));

        expect(wrapper.get("[data-testid='composer-count']").text()).toBe("");
    });

    test("counts down in the warning ink inside the last 500", async () => {
        const wrapper = mountComposer({ props: { maxLength: 5000 } });
        await type(wrapper, "x".repeat(4812));

        const count = wrapper.get("[data-testid='composer-count']");
        expect(count.text()).toBe("188 left");
        expect(count.attributes("data-tone")).toBe("warn");
    });

    test("says how far over in the danger ink past the limit", async () => {
        const wrapper = mountComposer({ props: { maxLength: 5000 } });
        await type(wrapper, "x".repeat(5012));

        const count = wrapper.get("[data-testid='composer-count']");
        expect(count.text()).toBe("12 over the limit");
        expect(count.attributes("data-tone")).toBe("over");
    });

    test("holds a send over the limit back, keeping the text, and says why", async () => {
        const wrapper = mountComposer({ props: { maxLength: 5000 } });
        await type(wrapper, "x".repeat(5001));
        await wrapper.get("form").trigger("submit");

        expect(wrapper.emitted("send")).toBeUndefined();
        expect(wrapper.emitted("overLimit")).toEqual([[5000]]);
        expect(boxText(wrapper)).toBe("x".repeat(5001));
    });

    test("counts an emoji as one character, as the platform does", async () => {
        const wrapper = mountComposer({ props: { maxLength: 5000 } });
        await type(wrapper, "x".repeat(4998) + "\u{1F600}\u{1F600}");

        expect(wrapper.get("[data-testid='composer-count']").text()).toBe("0 left");
        await wrapper.get("form").trigger("submit");
        expect(wrapper.emitted("send")).toHaveLength(1);
    });

    test("sends at exactly the limit", async () => {
        const wrapper = mountComposer({ props: { maxLength: 5000 } });
        await type(wrapper, "x".repeat(5000));
        await wrapper.get("form").trigger("submit");

        expect(wrapper.emitted("send")).toHaveLength(1);
        expect(wrapper.get("[data-testid='composer-count']").text()).toBe("");
    });
});

// A thread's reply can also go to the conversation. The choice is the chat's own switch, the one
// Chat settings draws, at its small size in the foot line, named by the words beside it, and a
// press on the words toggles it as a label does. It is per message: each send turns it off.
describe("the reply composer's Also send switch", () => {
    function alsoSwitch(wrapper: VueWrapper): ReturnType<VueWrapper["get"]> {
        return wrapper.get("[data-testid='composer-also'] [role='switch']");
    }

    test("is the chat's small switch, off, named by the words beside it", () => {
        const wrapper = mountComposer({ props: { isThread: true, isChannel: true, title: "young-adults" } });
        const control = alsoSwitch(wrapper);

        expect(wrapper.find("[data-testid='composer-also'] input").exists()).toBe(false);
        expect(control.element.tagName).toBe("BUTTON");
        expect(control.attributes("type")).toBe("button");
        expect(control.classes()).toEqual(expect.arrayContaining(["chat-switch", "is-sm"]));
        expect(control.attributes("aria-checked")).toBe("false");
        // the label around it gives it its name
        expect(wrapper.get("[data-testid='composer-also']").element.tagName).toBe("LABEL");
        expect(wrapper.get("[data-testid='composer-also']").text().replace(/\s+/g, " ").trim()).toBe("Also send to young-adults");
        expect(wrapper.get("[data-testid='composer-also'] b").text()).toBe("young-adults");
    });

    test("turns on and off from the switch, and the reply carries the choice", async () => {
        const wrapper = mountComposer({ props: { isThread: true, isChannel: true, title: "young-adults" } });

        await alsoSwitch(wrapper).trigger("click");
        expect(alsoSwitch(wrapper).attributes("aria-checked")).toBe("true");
        await alsoSwitch(wrapper).trigger("click");
        expect(alsoSwitch(wrapper).attributes("aria-checked")).toBe("false");
        await alsoSwitch(wrapper).trigger("click");

        await type(wrapper, "Count me in");
        await wrapper.get("form").trigger("submit");

        expect(wrapper.emitted("send")).toEqual([["Count me in", true]]);
    });

    test("turns off again after each send", async () => {
        const wrapper = mountComposer({ props: { isThread: true, isChannel: true, title: "young-adults" } });
        await alsoSwitch(wrapper).trigger("click");
        await type(wrapper, "Count me in");
        await wrapper.get("form").trigger("submit");

        expect(alsoSwitch(wrapper).attributes("aria-checked")).toBe("false");

        await type(wrapper, "And Ben");
        await wrapper.get("form").trigger("submit");
        expect(wrapper.emitted("send")?.[1]).toEqual(["And Ben", false]);
    });

    test("a press on the words toggles it, as a label does", async () => {
        const wrapper = mountComposer({ props: { isThread: true, isChannel: true, title: "young-adults" } });

        await wrapper.get("[data-testid='composer-also'] b").trigger("click");

        expect(alsoSwitch(wrapper).attributes("aria-checked")).toBe("true");
    });

    test("a direct message's thread names the conversation", () => {
        const wrapper = mountComposer({ props: { isThread: true, title: "Anna Reyes" } });

        expect(wrapper.get("[data-testid='composer-also']").text().replace(/\s+/g, " ").trim()).toBe("Also send to the conversation");
    });
});

// A send that failed is marked on its message; what to do about it waits here, in a strip over the
// box the person types in: what happened, the newest reason, and real buttons. One strip speaks for
// every failed send in the place the box types into, by count, and its buttons act on them all.
// A failure is said aloud once, assertively, when it happens; a strip already there when the
// person comes back to the room, or opens the box, is not said again.
describe("the composer's strip for a failed send", () => {
    const offline = { count: 1, reason: { alone: "You're offline. Try again when you're back.", besideRetry: "You're offline.", canRetry: true }, canRetry: true, localIds: ["k1"] };

    function strip(wrapper: VueWrapper): ReturnType<VueWrapper["get"]> {
        return wrapper.get("[data-testid='composer-failed']");
    }

    function announced(wrapper: VueWrapper): string {
        return wrapper.get("[data-testid='composer-failed-announcement']").text();
    }

    test("is not drawn while nothing failed", () => {
        const wrapper = mountComposer({ props: { title: "young-adults", isChannel: true } });

        expect(wrapper.find("[data-testid='composer-failed']").exists()).toBe(false);
    });

    test("says what happened and why above the box, with Try again and Delete as real buttons", () => {
        const wrapper = mountComposer({ props: { title: "young-adults", isChannel: true, sendFailure: offline } });

        expect(strip(wrapper).get("[data-testid='composer-failed-lead']").text()).toBe("Your message wasn't sent");
        expect(strip(wrapper).get("[data-testid='composer-failed-reason']").text()).toBe("You're offline.");
        expect(strip(wrapper).find(".chat-fail-mark").exists()).toBe(true);
        const buttons = strip(wrapper).findAll("button");
        expect(buttons.map(button => button.text())).toEqual(["Try again", "Delete"]);
        expect(buttons.every(button => button.attributes("type") === "button")).toBe(true);
        // over the box, never under it
        const form = wrapper.get("form").element;
        expect(strip(wrapper).element.compareDocumentPosition(form) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
    });

    test("a failure that said nothing has no reason line", () => {
        const wrapper = mountComposer({ props: { sendFailure: { count: 1, reason: null, canRetry: true, localIds: ["k1"] } } });

        expect(strip(wrapper).find("[data-testid='composer-failed-reason']").exists()).toBe(false);
    });

    test("several are counted, and its buttons act on them all", () => {
        const wrapper = mountComposer({ props: { sendFailure: { count: 3, reason: { alone: "You're offline. Try again when you're back.", besideRetry: "You're offline.", canRetry: true }, canRetry: true, localIds: ["k1", "k2", "k3"] } } });

        expect(strip(wrapper).get("[data-testid='composer-failed-lead']").text()).toBe("3 messages weren't sent");
        expect(strip(wrapper).findAll("button").map(button => button.text())).toEqual(["Try again", "Delete all"]);
    });

    test("a send refused for good shows its sentence and Delete alone", () => {
        const wrapper = mountComposer({ props: { sendFailure: { count: 1, reason: { alone: "You can't mention everyone in this channel.", besideRetry: "You can't mention everyone in this channel.", canRetry: false }, canRetry: false, localIds: ["k1"] } } });

        expect(strip(wrapper).get("[data-testid='composer-failed-reason']").text()).toBe("You can't mention everyone in this channel.");
        expect(strip(wrapper).findAll("button").map(button => button.text())).toEqual(["Delete"]);
    });

    // Beside Try again the button asks, so the reason only says what happened; where the box is
    // locked and Try again waits, the reason says when to try again itself.
    test("its reason asks to try again only where the strip offers no Try again", () => {
        const wrapper = mountComposer({ props: { sendFailure: offline, isLocked: true } });

        expect(strip(wrapper).get("[data-testid='composer-failed-reason']").text()).toBe("You're offline. Try again when you're back.");
        expect(strip(wrapper).findAll("button").map(button => button.text())).toEqual(["Delete"]);
    });

    test("Try again and Delete reach the owner, and focus goes back to the box", async () => {
        const wrapper = mountComposer({ props: { sendFailure: offline }, attachTo: document.body });

        await strip(wrapper).get("[data-testid='composer-failed-retry']").trigger("click");
        await flushPromises();
        expect(wrapper.emitted("retryFailed")).toHaveLength(1);
        expect(document.activeElement).toBe(editorOf(wrapper));

        await strip(wrapper).get("[data-testid='composer-failed-discard']").trigger("click");
        await flushPromises();
        expect(wrapper.emitted("discardFailed")).toHaveLength(1);
        expect(document.activeElement).toBe(editorOf(wrapper));
        wrapper.unmount();
    });

    test("stays over a locked box with Delete alone, so a failed send can still be deleted and nothing is tried that cannot go", () => {
        const locked = mountComposer({ props: { isLocked: true, sendFailure: offline } });
        const refused = mountComposer({ props: { lockedReason: "You can't post in this conversation.", sendFailure: offline } });

        expect(locked.find("[data-testid='composer-locked']").exists()).toBe(true);
        expect(strip(locked).findAll("button").map(button => button.text())).toEqual(["Delete"]);
        expect(strip(refused).findAll("button").map(button => button.text())).toEqual(["Delete"]);
    });

    // The box is not there to take focus over a locked composer, and takes none while a new
    // conversation starts, so focus goes where the person can carry on rather than to the page.
    test("over a locked box, Delete leaves focus on the lock's line once the strip goes", async () => {
        const wrapper = mountComposer({
            props: { isLocked: true, sendFailure: offline, onDiscardFailed: () => wrapper.setProps({ sendFailure: null }) },
            attachTo: document.body
        });

        await strip(wrapper).get("[data-testid='composer-failed-discard']").trigger("click");
        await flushPromises();

        expect(document.activeElement).toBe(wrapper.get("[data-testid='composer-locked']").element);
        wrapper.unmount();
    });

    test("over a locked box, Delete leaves focus on what is left of the strip while it stays", async () => {
        const wrapper = mountComposer({ props: { isLocked: true, sendFailure: { ...offline, count: 2, localIds: ["k1", "k2"] } }, attachTo: document.body });

        await strip(wrapper).get("[data-testid='composer-failed-discard']").trigger("click");
        await flushPromises();

        expect(document.activeElement).toBe(strip(wrapper).get("[data-testid='composer-failed-discard']").element);
        wrapper.unmount();
    });

    test("while the box takes no typing, Try again leaves focus on the box, which says it is unavailable", async () => {
        const wrapper = mountComposer({
            props: { sendFailure: offline, onRetryFailed: () => wrapper.setProps({ sendFailure: null, isDisabled: true }) },
            attachTo: document.body
        });

        await strip(wrapper).get("[data-testid='composer-failed-retry']").trigger("click");
        await flushPromises();

        expect(document.activeElement).toBe(editorOf(wrapper));
        expect(editorOf(wrapper).getAttribute("aria-disabled")).toBe("true");
        // a browser focuses an element that is not editable only when it carries a tab index
        expect(editorOf(wrapper).getAttribute("tabindex")).toBe("-1");
        wrapper.unmount();
    });

    // A failure is said once, as it happens: the sender names the message whose try just failed,
    // and the strip holding that message says it.
    test("says a failure aloud once, assertively, as it happens here", async () => {
        const wrapper = mountComposer({ props: { conversationKey: "c1" } });
        const region = wrapper.get("[data-testid='composer-failed-announcement']");
        expect(region.attributes("aria-live")).toBe("assertive");
        expect(announced(wrapper)).toBe("");

        await wrapper.setProps({ sendFailure: offline, lastFailure: { localId: "k1", count: 1 } });
        expect(announced(wrapper)).toBe("Your message wasn't sent. You're offline.");

        // the same failures again, as another send of the room changes, say nothing new
        await wrapper.setProps({ sendFailure: { ...offline } });
        expect(announced(wrapper)).toBe("Your message wasn't sent. You're offline.");

        // a second failure is said
        await wrapper.setProps({ sendFailure: { count: 2, reason: null, canRetry: true, localIds: ["k1", "k2"] }, lastFailure: { localId: "k2", count: 2 } });
        expect(announced(wrapper)).toBe("2 messages weren't sent.");
    });

    test("says a failure again when a try again fails", async () => {
        const wrapper = mountComposer({ props: { conversationKey: "c1" } });
        await wrapper.setProps({ sendFailure: offline, lastFailure: { localId: "k1", count: 1 } });
        const first = wrapper.get("[data-testid='composer-failed-announcement'] span").element;

        await wrapper.setProps({ sendFailure: null });
        await wrapper.setProps({ sendFailure: offline, lastFailure: { localId: "k1", count: 2 } });

        // drawn afresh, so a screen reader reads the same words again
        expect(announced(wrapper)).toBe("Your message wasn't sent. You're offline.");
        expect(wrapper.get("[data-testid='composer-failed-announcement'] span").element).not.toBe(first);
    });

    test("does not say failures already there when the box opens or the person comes back to the room", async () => {
        const wrapper = mountComposer({ props: { conversationKey: "c1", sendFailure: offline, lastFailure: { localId: "k1", count: 1 } } });
        expect(announced(wrapper)).toBe("");

        await wrapper.setProps({ conversationKey: "c2", sendFailure: null });
        await wrapper.setProps({ conversationKey: "c1", sendFailure: offline });

        expect(announced(wrapper)).toBe("");
    });

    test("does not say a failure another strip already said, when it comes to this one", async () => {
        const wrapper = mountComposer({ props: { conversationKey: "c1", lastFailure: { localId: "k1", count: 1 } } });

        // the thread's drawer closed, and the reply it held failed is now this strip's
        await wrapper.setProps({ sendFailure: offline });

        expect(announced(wrapper)).toBe("");
    });

    test("does not say a failure that happened in another place", async () => {
        const wrapper = mountComposer({ props: { conversationKey: "c1", sendFailure: offline, lastFailure: { localId: "k1", count: 1 } } });

        await wrapper.setProps({ lastFailure: { localId: "elsewhere", count: 2 } });

        expect(announced(wrapper)).toBe("");
    });

    // Its going takes the space it held back smoothly, so the feed above does not jump.
    test("leaves through its own exit, which only fades under reduced motion", async () => {
        const wrapper = mountComposer({ props: { sendFailure: offline }, global: { stubs: { transition: false } } });

        await wrapper.setProps({ sendFailure: null });

        const leaving = wrapper.find("[data-testid='composer-failed']");
        expect(leaving.exists()).toBe(true);
        expect(leaving.element.parentElement?.classList.contains("chat-strip-leave-active")).toBe(true);

        const source = readFileSync(join(__dirname, "../../../../../src/Communication/Chat/ChatShell/components/composer.partial.obs"), "utf8");
        const style = source.slice(source.indexOf("<style"), source.indexOf("</style>"));
        const reduced = style.slice(style.indexOf("@media (prefers-reduced-motion: reduce)"));
        expect(style).toMatch(/\.chat-strip-leave-active\s*\{[^}]*grid-template-rows var\(--chat-t-base\)/);
        expect(reduced).toMatch(/\.chat-strip-leave-to\s*\{[^}]*grid-template-rows: 1fr/);
    });

    // The strip belongs to the place it was raised in: moving to another place takes it away with
    // that place rather than playing its exit over the next one.
    test("is gone at once when the conversation changes, with no exit over the next one", async () => {
        const wrapper = mountComposer({ props: { conversationKey: "c1", sendFailure: offline }, global: { stubs: { transition: false } } });

        await wrapper.setProps({ conversationKey: "c2", sendFailure: null });

        expect(wrapper.find("[data-testid='composer-failed']").exists()).toBe(false);
    });
});

/** A paste carrying these files and these kinds of text, as a browser's clipboard lists them. */
function pasteOf(files: File[], texts: Record<string, string>): Event {
    const pasted = new Event("paste", { bubbles: true, cancelable: true }) as Event & { clipboardData: unknown };
    pasted.clipboardData = { files, types: [...Object.keys(texts), ...(files.length ? ["Files"] : [])], getData: (type: string) => texts[type] ?? "" };
    return pasted;
}

// Files ride with the box: picked, dropped or pasted, each uploads at once into the strip above the
// box, with its progress, a remove and a failure with Try again; Send waits until every one is up.
describe("the composer's files", () => {
    const place = "c0000001-0000-4000-8000-000000000000";

    function trayed(props: Record<string, unknown> = {}): { wrapper: VueWrapper, tray: AttachmentTray, h: Harness } {
        const h = harness();
        const tray = createAttachments(h.dependencies).trayFor(place, place);
        const wrapper = mountComposer({ props: { title: "Anna Reyes", tray, ...props } });
        return { wrapper, tray, h };
    }

    async function pick(wrapper: VueWrapper, files: File[]): Promise<void> {
        const input = wrapper.get("[data-testid='composer-file-input']");
        Object.defineProperty(input.element, "files", { value: files, configurable: true });
        await input.trigger("change");
        await settle();
    }

    test("offers Attach files only where files can go", () => {
        expect(mountComposer().find("[data-testid='composer-attach']").exists()).toBe(false);

        const { wrapper } = trayed();
        expect(wrapper.get("[data-testid='composer-attach']").attributes("aria-label")).toBe("Attach files");
    });

    test("a picked file shows in the strip with its progress, and Send waits until it is up", async () => {
        const { wrapper, h } = trayed();
        await type(wrapper, "the notes");

        await pick(wrapper, [picked("notes.pdf", "application/pdf")]);
        h.requests[0].progress(400, 1000);
        await nextTick();

        const row = wrapper.get("[data-testid='attachment-tray-file']");
        expect(row.get("[data-testid='attachment-tray-name']").text()).toBe("notes.pdf");
        expect(row.get("[role='progressbar']").attributes("aria-valuenow")).toBe("40");
        expect(wrapper.get("[data-testid='composer-send']").attributes("disabled")).toBeDefined();
        await wrapper.get("form").trigger("submit");
        expect(wrapper.emitted("send")).toBeUndefined();

        h.requests[0].respond(200);
        await settle();
        await nextTick();
        expect(wrapper.get("[data-testid='composer-send']").attributes("disabled")).toBeUndefined();
        await wrapper.get("form").trigger("submit");
        expect(wrapper.emitted("send")).toEqual([["the notes"]]);
    });

    test("files with no text send", async () => {
        const { wrapper, h } = trayed();
        await pick(wrapper, [picked("a.pdf", "application/pdf")]);
        h.requests[0].respond(200);
        await settle();
        await nextTick();

        expect(wrapper.get("[data-testid='composer-send']").classes()).toContain("is-ready");
        await wrapper.get("form").trigger("submit");
        expect(wrapper.emitted("send")).toEqual([[""]]);
    });

    test("a failed upload offers Try again on its own file, and Remove takes a file out", async () => {
        const { wrapper, h, tray } = trayed();
        await pick(wrapper, [picked("a.pdf", "application/pdf"), picked("b.pdf", "application/pdf")]);
        h.requests[0].fail();
        await settle();
        await nextTick();

        const [first, second] = wrapper.findAll("[data-testid='attachment-tray-file']");
        expect(first.attributes("data-status")).toBe("failed");
        expect(first.get("[data-testid='attachment-tray-failure']").text()).toBe("Couldn't upload.");
        expect(second.find("[data-testid='attachment-tray-retry']").exists()).toBe(false);

        await first.get("[data-testid='attachment-tray-retry']").trigger("click");
        await settle();
        expect(h.requests).toHaveLength(3);

        await second.get("[data-testid='attachment-tray-remove']").trigger("click");
        expect(tray.files.map(f => f.name)).toEqual(["a.pdf"]);
        expect(second.get("[data-testid='attachment-tray-remove']").attributes("aria-label")).toBe("Remove b.pdf");
    });

    test("files dropped on the composer or pasted into it go into the strip", async () => {
        const { wrapper, tray } = trayed();

        const dropped = new Event("drop", { bubbles: true, cancelable: true }) as Event & { dataTransfer: unknown };
        dropped.dataTransfer = { files: [picked("a.pdf", "application/pdf")], types: ["Files"] };
        wrapper.element.dispatchEvent(dropped);
        await settle();
        expect(dropped.defaultPrevented).toBe(true);

        const pasted = pasteOf([picked("image.png", "image/png")], {});
        wrapper.get("[data-testid='composer-input']").element.dispatchEvent(pasted);
        await settle();

        expect(tray.files.map(f => f.name)).toEqual(["a.pdf", "image.png"]);
        expect(pasted.defaultPrevented).toBe(true);
    });

    test("a paste from Word or Excel, its rich text beside a picture of it, is left to the editor", async () => {
        const { wrapper, tray } = trayed();

        const pasted = pasteOf([picked("image.png", "image/png")], { "text/plain": "Q3 totals", "text/html": "<table><tr><td>Q3 totals</td></tr></table>", "text/rtf": "{\\rtf1 Q3 totals}" });
        wrapper.get("[data-testid='composer-input']").element.dispatchEvent(pasted);
        await settle();

        expect(tray.files).toEqual([]);
    });

    test("a picture copied from a web page, which also puts its markup on the clipboard, is attached", async () => {
        const { wrapper, tray } = trayed();

        const pasted = pasteOf([picked("image.png", "image/png")], { "text/html": "<img src=\"https://example.org/image.png\">" });
        wrapper.get("[data-testid='composer-input']").element.dispatchEvent(pasted);
        await settle();

        expect(tray.files.map(f => f.name)).toEqual(["image.png"]);
        expect(pasted.defaultPrevented).toBe(true);
    });

    test("a file copied from a file manager, which also puts its name as plain text, is attached", async () => {
        const { wrapper, tray } = trayed();

        const pasted = pasteOf([picked("budget.pdf", "application/pdf")], { "text/plain": "budget.pdf" });
        wrapper.get("[data-testid='composer-input']").element.dispatchEvent(pasted);
        await settle();

        expect(tray.files.map(f => f.name)).toEqual(["budget.pdf"]);
        expect(pasted.defaultPrevented).toBe(true);
    });
});

describe("the composer's quote of files alone", () => {
    test("reads as its files", () => {
        const pdf = { id: "d1", key: "t/d1", kind: "file" as const, name: "Q3.pdf", size: 9, type: "application/pdf", width: null, height: null };
        const quote = { id: 40, person_alias_guid: "a0000002-0000-4000-8000-000000000000", sender_nick_name: "Anna", sender_last_name: "Reyes", message_type: "text", body: null, attachments: [pdf], created_at: "2026-10-15T09:00:00Z" };
        const wrapper = mountComposer({ props: { title: "Anna Reyes", quote } as Record<string, unknown> });

        expect(wrapper.get("[data-testid='composer-quote-preview']").text()).toBe("File: Q3.pdf");
    });
});
