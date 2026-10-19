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
// The @ picker. It opens when the person types @ at the start of a line or after a space, never
// right after a chip or inside a word, and lists the room's people under "People in this channel",
// each with their face and name, the first row active; with nobody to list it says "No one in this
// channel matches." The editor keeps focus and acts as the combobox: it carries aria-expanded,
// aria-controls naming the listbox and aria-activedescendant naming the active row. Up and Down
// move, Enter or Tab inserts the chip and a space, Escape closes, and Enter never sends while the
// picker is open. @all is its first row only where the composer is told it is offered, and alone
// where the person may not see who is in the room; with neither, there is no picker at all.
//
// The component these tests are written to, components/mentionPicker.partial.obs:
//
//   props: target: HTMLElement | null      the editor it lists for: its keys and its combobox attributes
//          people: MentionPerson[]         chat_search_mentionable's people, at most six drawn
//          isAllOffered: boolean           the @all row leads the list
//   emits: choose(token: string, name: string), the row's token (<@guid> or <@all>) and its chip's name
//          close                            Escape
//   marks: mention-picker (the popover), mention-option (each row, role="option")
//   exports: MentionPickerSource = { canMention: boolean, people: MentionPerson[], isAllOffered: boolean }
//
// And the composer, which hosts it:
//
//   props: mentionSource: MentionPickerSource | null    null offers no picker
//   emits: mentionQuery(query: string | null)           the @ query at the caret after each change, null for none
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import MentionPicker from "../../../../../src/Communication/Chat/ChatShell/components/mentionPicker.partial.obs";
import Composer from "../../../../../src/Communication/Chat/ChatShell/components/composer.partial.obs";
import ChipEditor from "../../../../../src/Communication/Chat/ChatShell/ui/chipEditor.partial.obs";

// #region Fixtures

const anna = "a0000002-0000-4000-8000-000000000000";
const ben = "a0000003-0000-4000-8000-000000000000";

const annaPerson = { person_alias_guid: anna, nick_name: "Anna", last_name: "Reyes" };
const benPerson = { person_alias_guid: ben, nick_name: "Ben", last_name: "Okafor" };

/** Eight people, more than the picker draws. */
const many = Array.from({ length: 8 }, (_, index) => ({
    person_alias_guid: `a00000${10 + index}-0000-4000-8000-000000000000`,
    nick_name: `Person${index}`,
    last_name: "Lee"
}));

let wrapper: VueWrapper | null = null;

/** The picker on its own, over a stand-in editor. */
function drawPicker(props: Record<string, unknown> = {}): { w: VueWrapper, target: HTMLElement } {
    const target = document.createElement("div");
    target.setAttribute("contenteditable", "true");
    document.body.append(target);
    wrapper = mount(MentionPicker, {
        props: { target, people: [annaPerson, benPerson], isAllOffered: false, ...props },
        attachTo: document.body
    });
    return { w: wrapper, target };
}

function picker(): HTMLElement | null {
    return document.querySelector<HTMLElement>("[data-testid='mention-picker']");
}

function options(): HTMLElement[] {
    return Array.from(document.querySelectorAll<HTMLElement>("[data-testid='mention-option']"));
}

function activeOption(): HTMLElement | undefined {
    return options().find(o => o.getAttribute("aria-selected") === "true");
}

/** A row's name, the text it reads with its face's initials taken out. */
function nameOf(option: HTMLElement): string {
    const face = option.querySelector("[data-testid='avatar']")?.textContent ?? "";
    return (option.textContent ?? "").replace(face, "").replace(/\s+/g, " ").trim();
}

async function press(target: HTMLElement, key: string, extra: KeyboardEventInit = {}): Promise<KeyboardEvent> {
    const event = new KeyboardEvent("keydown", { key, bubbles: true, cancelable: true, ...extra });
    target.dispatchEvent(event);
    await nextTick();
    return event;
}

/** The composer with a picker source. */
function drawComposer(source: Record<string, unknown> | null): VueWrapper {
    wrapper = mount(Composer, { props: { mentionSource: source }, attachTo: document.body });
    return wrapper;
}

function editorOf(w: VueWrapper): HTMLElement {
    return w.get("[data-testid='composer-input']").element as HTMLElement;
}

/**
 * Types into the composer as a browser does: the nodes, the caret after the last of them, then
 * input. A node given as a string is text; anything else goes in as it is, a chip for one.
 */
async function typeNodes(w: VueWrapper, nodes: Array<string | Node>): Promise<void> {
    const editor = editorOf(w);
    editor.replaceChildren(...nodes.map(node => typeof node === "string" ? document.createTextNode(node) : node));
    const last = editor.lastChild;
    const range = document.createRange();
    if (last && last.nodeType === Node.TEXT_NODE) {
        range.setStart(last, (last as Text).data.length);
    }
    else {
        range.selectNodeContents(editor);
        range.collapse(false);
    }
    range.collapse(true);
    const selection = window.getSelection()!;
    selection.removeAllRanges();
    selection.addRange(range);
    editor.dispatchEvent(new Event("input", { bubbles: true }));
    await nextTick();
    await nextTick();
}

/** A chip as the editor draws one: one unit carrying its token. */
function chip(token: string, name: string): HTMLElement {
    const element = document.createElement("span");
    element.className = "chat-mn";
    element.setAttribute("contenteditable", "false");
    element.dataset.token = token;
    element.textContent = name;
    return element;
}

/** The wire text the box holds now. */
function boxText(w: VueWrapper): string {
    return (w.findComponent(ChipEditor).vm as unknown as { getText: () => string }).getText();
}

/** The last @ query the composer told of. */
function lastQuery(w: VueWrapper): string | null | undefined {
    const queries = w.emitted("mentionQuery") ?? [];
    return queries[queries.length - 1]?.[0] as string | null | undefined;
}

const people = { canMention: true, people: [annaPerson, benPerson], isAllOffered: false };

beforeAll(() => {
    window.ResizeObserver ??= class {
        observe(): void { /* jsdom lays nothing out. */ }
        unobserve(): void { /* As above. */ }
        disconnect(): void { /* As above. */ }
    } as unknown as typeof ResizeObserver;
    Element.prototype.scrollIntoView ??= function scrollIntoView(): void { /* jsdom scrolls nothing. */ };
});

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
});

// #endregion Fixtures

describe("the picker's rows", () => {
    test("are headed People in this channel, each a face and the person's name, with no line under it", () => {
        drawPicker();

        expect(picker()?.textContent).toContain("People in this channel");
        expect(options()).toHaveLength(2);
        expect(options().map(nameOf)).toEqual(["Anna Reyes", "Ben Okafor"]);
        expect(options()[0].querySelector("[data-testid='avatar']")).not.toBeNull();
    });

    test("are at most six", () => {
        drawPicker({ people: many });

        expect(options()).toHaveLength(6);
    });

    test("start with the first row active, the only one selected", () => {
        drawPicker();

        expect(options().map(o => o.getAttribute("aria-selected"))).toEqual(["true", "false"]);
        expect(options()[0].classList.contains("is-active")).toBe(true);
        expect(options()[1].classList.contains("is-active")).toBe(false);
    });

    test("say No one in this channel matches. when there is nobody to list", () => {
        drawPicker({ people: [] });

        expect(options()).toHaveLength(0);
        expect(picker()?.textContent?.trim()).toBe("No one in this channel matches.");
    });

    test("choose a person by a press too", async () => {
        const { w } = drawPicker();
        expect(options()).toHaveLength(2);

        options()[1].dispatchEvent(new MouseEvent("mousedown", { bubbles: true, cancelable: true }));
        options()[1].click();
        await nextTick();

        expect(w.emitted("choose")?.[0]).toEqual([`<@${ben}>`, "Ben Okafor"]);
    });
});

describe("the @all row", () => {
    test("leads the list when offered: @all over Notify everyone in this channel", () => {
        drawPicker({ isAllOffered: true });

        const [all, first] = options();
        expect(options()).toHaveLength(3);
        expect(all.textContent).toContain("@all");
        expect(all.textContent).toContain("Notify everyone in this channel");
        expect(nameOf(first)).toBe("Anna Reyes");
        expect(all.getAttribute("aria-selected")).toBe("true");
    });

    test("is not drawn unless offered", () => {
        drawPicker();

        expect(options().some(o => o.textContent?.includes("@all"))).toBe(false);
        expect(picker()?.textContent).not.toContain("Notify everyone in this channel");
    });

    test("stands alone, with no empty line, where it is offered and nobody is listed", () => {
        drawPicker({ people: [], isAllOffered: true });

        expect(options()).toHaveLength(1);
        expect(picker()?.textContent).not.toContain("No one in this channel matches.");
    });

    test("inserts the @all token", async () => {
        const { w, target } = drawPicker({ isAllOffered: true });

        await press(target, "Enter");

        expect(w.emitted("choose")?.[0]).toEqual(["<@all>", "@all"]);
    });
});

describe("the editor as the picker's combobox", () => {
    test("is expanded, controls the listbox and points at the active row", () => {
        const { target } = drawPicker();

        const listbox = document.querySelector("[role='listbox']");
        expect(listbox).not.toBeNull();
        expect(target.getAttribute("aria-expanded")).toBe("true");
        expect(target.getAttribute("aria-controls")).toBe(listbox?.id);
        expect(listbox?.id).toBeTruthy();
        expect(options().every(o => o.getAttribute("role") === "option" && !!o.id)).toBe(true);
        expect(target.getAttribute("aria-activedescendant")).toBe(options()[0].id);
    });

    test("is collapsed again, pointing at nothing, once the picker goes", () => {
        const { w, target } = drawPicker();
        expect(target.getAttribute("aria-expanded")).toBe("true");

        w.unmount();
        wrapper = null;

        expect(target.getAttribute("aria-expanded")).not.toBe("true");
        expect(target.hasAttribute("aria-activedescendant")).toBe(false);
    });
});

describe("the picker in a direct message or group", () => {
    test("says conversation where a channel says channel: its heading, the @all row and the empty line", () => {
        drawPicker({ isConversation: true, isAllOffered: true });

        expect(picker()?.textContent).toContain("People in this conversation");
        expect(options()[0].textContent).toContain("Notify everyone in this conversation");
        expect(picker()?.textContent).not.toContain("channel");

        wrapper?.unmount();
        drawPicker({ isConversation: true, people: [] });
        expect(picker()?.textContent?.trim()).toBe("No one in this conversation matches.");
    });

    test("is told so by its composer, which is not a channel's, and the @all chip it inserts says so too", async () => {
        wrapper = mount(Composer, { props: { mentionSource: { ...people, isAllOffered: true }, isChannel: false }, attachTo: document.body });
        await typeNodes(wrapper, ["@"]);

        expect(picker()?.textContent).toContain("People in this conversation");

        await press(editorOf(wrapper), "Enter");
        expect(editorOf(wrapper).querySelector("[data-testid='editor-chip']")?.getAttribute("aria-label")).toBe("@all, everyone in this conversation");
    });

    test("a channel's composer keeps saying channel", async () => {
        wrapper = mount(Composer, { props: { mentionSource: { ...people, isAllOffered: true }, isChannel: true }, attachTo: document.body });
        await typeNodes(wrapper, ["@"]);

        expect(picker()?.textContent).toContain("People in this channel");

        await press(editorOf(wrapper), "Enter");
        expect(editorOf(wrapper).querySelector("[data-testid='editor-chip']")?.getAttribute("aria-label")).toBe("@all, everyone in this channel");
    });
});

describe("the picker's keys, pressed in the editor", () => {
    test("Down and Up move the active row, wrapping at the ends", async () => {
        const { target } = drawPicker();

        const down = await press(target, "ArrowDown");
        expect(down.defaultPrevented).toBe(true);
        expect(activeOption()).toBe(options()[1]);
        expect(target.getAttribute("aria-activedescendant")).toBe(options()[1].id);

        await press(target, "ArrowDown");
        expect(activeOption()).toBe(options()[0]);

        await press(target, "ArrowUp");
        expect(activeOption()).toBe(options()[1]);
    });

    test.each(["Enter", "Tab"])("%s chooses the active row and keeps the key from the editor", async key => {
        const { w, target } = drawPicker();
        await press(target, "ArrowDown");

        const event = await press(target, key);

        expect(event.defaultPrevented).toBe(true);
        expect(w.emitted("choose")).toEqual([[`<@${ben}>`, "Ben Okafor"]]);
    });

    test("Escape closes, and keeps the key from the editor", async () => {
        const { w, target } = drawPicker();

        const event = await press(target, "Escape");

        expect(event.defaultPrevented).toBe(true);
        expect(w.emitted("close")).toHaveLength(1);
        expect(w.emitted("choose")).toBeUndefined();
    });
});

describe("the picker in the composer", () => {
    test.each([
        ["at the start of the box", ["@an"], "an"],
        ["after a space", ["Hi @an"], "an"],
        ["after a new line", ["Hi", document.createElement("br"), "@an"], "an"],
        ["as a bare @", ["Hi @"], ""]
    ])("opens on @ %s, with the query after it", async (_where, nodes, query) => {
        const w = drawComposer(people);

        await typeNodes(w, nodes as Array<string | Node>);

        expect(lastQuery(w)).toBe(query);
        expect(picker()).not.toBeNull();
        expect(options().map(nameOf)).toEqual(["Anna Reyes", "Ben Okafor"]);
    });

    test("does not open on @ right after a chip", async () => {
        const w = drawComposer(people);
        await typeNodes(w, ["Hi @"]);
        expect(picker()).not.toBeNull();

        await typeNodes(w, [chip(`<@${anna}>`, "Anna Reyes"), "@"]);

        expect(lastQuery(w)).toBeNull();
        expect(picker()).toBeNull();
    });

    test("does not open on @ inside a word", async () => {
        const w = drawComposer(people);
        await typeNodes(w, ["@"]);
        expect(picker()).not.toBeNull();

        await typeNodes(w, ["mail me at anna@"]);

        expect(lastQuery(w)).toBeNull();
        expect(picker()).toBeNull();
    });

    test("closes once the caret leaves the query", async () => {
        const w = drawComposer(people);
        await typeNodes(w, ["Hi @an"]);
        expect(picker()).not.toBeNull();

        await typeNodes(w, ["Hi @an there"]);

        expect(picker()).toBeNull();
    });

    test("makes the editor its combobox while open", async () => {
        const w = drawComposer(people);

        await typeNodes(w, ["@an"]);

        const editor = editorOf(w);
        const listbox = document.querySelector("[role='listbox']");
        expect(editor.getAttribute("aria-expanded")).toBe("true");
        expect(editor.getAttribute("aria-controls")).toBe(listbox?.id);
        expect(editor.getAttribute("aria-activedescendant")).toBe(options()[0].id);
    });

    test.each(["Enter", "Tab"])("%s inserts the active person's chip and a space in place of the query, and sends nothing", async key => {
        const w = drawComposer(people);
        await typeNodes(w, ["Hi @an"]);

        await press(editorOf(w), key);
        await nextTick();

        expect(w.emitted("send")).toBeUndefined();
        expect(boxText(w)).toBe(`Hi <@${anna}> `);
        expect(editorOf(w).querySelectorAll("[data-testid='editor-chip']")).toHaveLength(1);
        expect(picker()).toBeNull();
    });

    test("Escape closes it and keeps the text, and the next Enter sends", async () => {
        const w = drawComposer(people);
        await typeNodes(w, ["Hi @an"]);
        expect(picker()).not.toBeNull();

        await press(editorOf(w), "Escape");
        expect(picker()).toBeNull();
        expect(boxText(w)).toBe("Hi @an");

        await press(editorOf(w), "Enter");
        expect(w.emitted("send")).toEqual([["Hi @an"]]);
    });

    test("is not offered where the person may not see who is in the room and holds no @all right", async () => {
        const w = drawComposer({ canMention: false, people: [], isAllOffered: false });

        await typeNodes(w, ["@"]);
        expect(picker()).toBeNull();

        await press(editorOf(w), "Enter");
        expect(w.emitted("send")).toEqual([["@"]]);

        // the same room once the person may see who is in it
        await w.setProps({ mentionSource: people });
        await typeNodes(w, ["@"]);
        expect(picker()).not.toBeNull();
    });

    test("is not offered with no source at all", async () => {
        const w = drawComposer(null);

        await typeNodes(w, ["@"]);
        expect(picker()).toBeNull();

        await w.setProps({ mentionSource: people });
        await typeNodes(w, ["@"]);
        expect(picker()).not.toBeNull();
    });

    test("offers @all alone where the person may not see who is in the room but holds the right", async () => {
        const w = drawComposer({ canMention: false, people: [], isAllOffered: true });

        await typeNodes(w, ["@"]);

        expect(options()).toHaveLength(1);
        expect(options()[0].textContent).toContain("@all");

        await press(editorOf(w), "Enter");
        expect(w.emitted("send")).toBeUndefined();
        expect(boxText(w)).toBe("<@all> ");
    });
});
