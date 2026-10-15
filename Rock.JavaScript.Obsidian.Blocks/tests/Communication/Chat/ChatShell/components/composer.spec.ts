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
