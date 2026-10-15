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
// The chip editor, mounted: the text box the composer and edit in place share. A message is sent
// as plain text in which a mention is a token, <@guid> or <@all>; the editor draws each token as a
// chip with the person's name, one unit the person selects, deletes and copies whole, and hands
// back exactly the text it was given, so an edit never loses or rewrites a mention. Copy writes
// the token, so a chip pasted elsewhere in chat is the same mention; paste is plain text only.
//
// The component these tests are written to, ui/chipEditor.partial.obs:
//
//   props: {
//       text: string,                     the wire text to load, tokens and all
//       pieces: DisplayPiece[] | null     the server's pieces for that text; a mention piece
//                                         { type: "mention", text: "@Lee Leader", person_alias_guid }
//                                         names its chip, as the message body draws it
//   }
//   emits: send(text)                     Enter, with the wire text
//
// The editable element is marked data-testid="chip-editor"; each chip data-testid="editor-chip",
// not editable itself.
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import ChipEditor from "../../../../../src/Communication/Chat/ChatShell/ui/chipEditor.partial.obs";

const lee = "a0000007-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";

const mentionLee = { type: "mention", text: "@Lee Leader", person_alias_guid: lee };
const mentionAnna = { type: "mention", text: "@Anna Reyes", person_alias_guid: anna };
const mentionAll = { type: "mention", text: "@all", all: true };

let wrapper: VueWrapper | null = null;

function draw(text: string, pieces: unknown[] | null): VueWrapper {
    wrapper = mount(ChipEditor, { props: { text, pieces }, attachTo: document.body });
    return wrapper;
}

function editor(w: VueWrapper): HTMLElement {
    return w.get("[data-testid='chip-editor']").element as HTMLElement;
}

function chips(w: VueWrapper): HTMLElement[] {
    return w.findAll("[data-testid='editor-chip']").map(chip => chip.element as HTMLElement);
}

/** Puts the caret at a point in the editor, as a click there would. */
function caretAt(set: (range: Range) => void): void {
    const range = document.createRange();
    set(range);
    range.collapse(true);
    const selection = window.getSelection()!;
    selection.removeAllRanges();
    selection.addRange(range);
}

/** Puts the caret at the editor's end. */
function caretAtEnd(w: VueWrapper): void {
    caretAt(range => range.selectNodeContents(editor(w)));
    window.getSelection()!.collapseToEnd();
}

/** Selects from one point to another. */
function select(set: (range: Range) => void): void {
    const range = document.createRange();
    set(range);
    const selection = window.getSelection()!;
    selection.removeAllRanges();
    selection.addRange(range);
}

async function key(w: VueWrapper, name: string, extra: KeyboardEventInit = {}): Promise<KeyboardEvent> {
    const event = new KeyboardEvent("keydown", { key: name, bubbles: true, cancelable: true, ...extra });
    editor(w).dispatchEvent(event);
    await nextTick();
    return event;
}

/** The wire text the editor hands over, read by pressing Enter. */
async function sent(w: VueWrapper): Promise<string | undefined> {
    await key(w, "Enter");
    const sends = w.emitted("send") ?? [];
    return sends[sends.length - 1]?.[0] as string | undefined;
}

/** A clipboard event jsdom can carry: the data is a plain store the test reads or fills. */
function clipboardEvent(type: "copy" | "cut" | "paste", data: Record<string, string> = {}): Event & { store: Record<string, string> } {
    const event = new Event(type, { bubbles: true, cancelable: true }) as Event & { store: Record<string, string>, clipboardData: unknown };
    event.store = { ...data };
    Object.defineProperty(event, "clipboardData", {
        value: {
            getData: (format: string) => event.store[format] ?? "",
            setData: (format: string, value: string) => {
                event.store[format] = value;
            },
            types: Object.keys(data)
        }
    });
    return event;
}

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
});

describe("loading a body", () => {
    test("is an editable box", () => {
        const w = draw("hello", null);

        expect(editor(w).getAttribute("contenteditable")).toBe("true");
        expect(editor(w).textContent).toBe("hello");
    });

    test("draws each mention token as a chip with the person's name, never the token", () => {
        const w = draw(`Hi <@${lee}> and <@all>`, [{ type: "text", text: "Hi " }, mentionLee, { type: "text", text: " and " }, mentionAll]);

        expect(chips(w)).toHaveLength(2);
        expect(chips(w)[0].textContent).toContain("Lee Leader");
        expect(chips(w)[1].textContent).toContain("@all");
        expect(editor(w).textContent).not.toContain("<@");
    });

    test("a chip is one unit, not editable inside", () => {
        const w = draw(`<@${lee}>`, [mentionLee]);
        expect(chips(w)).toHaveLength(1);

        expect(chips(w)[0].getAttribute("contenteditable")).toBe("false");
    });

    test("a mention token in any letter case finds its name", () => {
        const w = draw(`Hi <@${lee.toUpperCase()}>`, [{ type: "text", text: "Hi " }, mentionLee]);

        expect(chips(w)).toHaveLength(1);
        expect(chips(w)[0].textContent).toContain("Lee Leader");
    });
});

describe("the wire text", () => {
    test.each([
        ["plain text", "Doors open at 6", null],
        ["a mention between words", `Hi <@${lee}>, see you`, [{ type: "text", text: "Hi " }, mentionLee, { type: "text", text: ", see you" }]],
        ["two mentions side by side", `<@${lee}><@${anna}>`, [mentionLee, mentionAnna]],
        ["the room's mention and new lines", `<@all>\nline two\n\nline four`, [mentionAll, { type: "text", text: "\nline two\n\nline four" }]],
        ["characters that look like markup", "a <b>tag</b> & more", null]
    ])("comes back exactly as loaded: %s", async (_what, text, pieces) => {
        const w = draw(text, pieces as unknown[] | null);

        expect(await sent(w)).toBe(text);
    });
});

describe("a chip is deleted whole", () => {
    test("Backspace right after a chip removes the chip and its token, and nothing else", async () => {
        const w = draw(`Hi <@${lee}> there`, [{ type: "text", text: "Hi " }, mentionLee, { type: "text", text: " there" }]);
        expect(chips(w)).toHaveLength(1);
        const [chip] = chips(w);
        caretAt(range => range.setStartAfter(chip));

        const event = await key(w, "Backspace");

        expect(event.defaultPrevented).toBe(true);
        expect(chips(w)).toHaveLength(0);
        expect(await sent(w)).toBe("Hi  there");
    });

    test("Delete right before a chip removes it whole too", async () => {
        const w = draw(`Hi <@${lee}> there`, [{ type: "text", text: "Hi " }, mentionLee, { type: "text", text: " there" }]);
        expect(chips(w)).toHaveLength(1);
        const [chip] = chips(w);
        caretAt(range => range.setStartBefore(chip));

        await key(w, "Delete");

        expect(chips(w)).toHaveLength(0);
        expect(await sent(w)).toBe("Hi  there");
    });
});

describe("copy and paste", () => {
    test("copying a selection with a chip writes the token, so pasting it sends the same mention", async () => {
        const w = draw(`Hi <@${lee}> there`, [{ type: "text", text: "Hi " }, mentionLee, { type: "text", text: " there" }]);
        expect(chips(w)).toHaveLength(1);
        const [chip] = chips(w);
        select(range => {
            range.setStart(editor(w), 0);
            range.setEndAfter(chip);
        });

        const event = clipboardEvent("copy");
        editor(w).dispatchEvent(event);

        expect(event.defaultPrevented).toBe(true);
        expect(event.store["text/plain"]).toBe(`Hi <@${lee}>`);
    });

    test("paste puts in the plain text only, never markup", async () => {
        const w = draw("Note: ", null);
        caretAtEnd(w);

        const event = clipboardEvent("paste", { "text/plain": "bring a coat", "text/html": "<b>bring</b> a <i>coat</i>" });
        editor(w).dispatchEvent(event);
        await nextTick();

        expect(event.defaultPrevented).toBe(true);
        expect(editor(w).querySelector("b, i")).toBeNull();
        expect(await sent(w)).toBe("Note: bring a coat");
    });
});

describe("the keys", () => {
    test("Enter sends the wire text and does not add a line", async () => {
        const w = draw(`Hi <@${lee}>`, [{ type: "text", text: "Hi " }, mentionLee]);

        const event = await key(w, "Enter");

        expect(event.defaultPrevented).toBe(true);
        expect(w.emitted("send")).toEqual([[`Hi <@${lee}>`]]);
    });

    test("Shift and Enter adds a new line where the caret is, and sends nothing", async () => {
        const w = draw("ab", null);
        const text = editor(w).firstChild as Text;
        caretAt(range => range.setStart(text, 1));

        await key(w, "Enter", { shiftKey: true });

        expect(w.emitted("send")).toBeUndefined();
        expect(await sent(w)).toBe("a\nb");
    });

    test("Enter while an input method is composing does not send", async () => {
        const w = draw("ab", null);

        await key(w, "Enter", { isComposing: true });

        expect(w.emitted("send")).toBeUndefined();
        expect(await sent(w)).toBe("ab");
    });
});
