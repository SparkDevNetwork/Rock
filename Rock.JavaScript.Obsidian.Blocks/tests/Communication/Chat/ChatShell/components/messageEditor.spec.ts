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
// Edit in place: the message's text in the shared chip box, with Cancel and Save under it. Where
// there is a keyboard, a line beside them names its keys; a phone has neither Escape nor a
// separate Enter, so there the line is not drawn, and the box is as wide as the message column.
//
// The words of an edit left unfinished are the message actions' to keep: the editor tells them
// each change and opens with what they kept for its message. An empty editor saves nothing and
// stays open, as an empty composer sends nothing.
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import MessageEditor from "../../../../../src/Communication/Chat/ChatShell/components/messageEditor.partial.obs";
import { EditTexts, editTextsKey } from "../../../../../src/Communication/Chat/ChatShell/composables/useMessageActions.partial";
import { TimelineMessage } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { providedSession } from "../sessionShapes";

const message = { id: 42, body: "See you Sunday", pieces: null } as unknown as TimelineMessage;

let wrapper: VueWrapper | null = null;

function draw(isPhone: boolean, editTexts?: EditTexts): VueWrapper {
    wrapper = mount(MessageEditor, {
        props: { message },
        global: { provide: { ...providedSession({ isPhone }), ...(editTexts ? { [editTextsKey as symbol]: editTexts } : {}) } },
        attachTo: document.body
    });
    return wrapper;
}

/** The message actions' keeping, as a spec sees it: what was kept, by message. */
function keeping(kept: Record<number, string> = {}): EditTexts & { keepEditText: jest.Mock } {
    return {
        keptEditText: (messageId: number): string | null => kept[messageId] ?? null,
        keepEditText: jest.fn()
    };
}

function box(w: VueWrapper): HTMLElement {
    return w.get("[data-testid='edit-input']").element as HTMLElement;
}

/** Replaces the box's words, as typing over them would. */
async function typeOver(w: VueWrapper, text: string): Promise<void> {
    box(w).replaceChildren(...(text ? [document.createTextNode(text)] : []));
    box(w).dispatchEvent(new InputEvent("input", { bubbles: true }));
    await nextTick();
}

async function enter(w: VueWrapper): Promise<void> {
    box(w).dispatchEvent(new KeyboardEvent("keydown", { key: "Enter", bubbles: true, cancelable: true }));
    await nextTick();
}

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
});

describe("the editor's foot", () => {
    test("names Escape and Enter beside Cancel and Save where there is a keyboard", () => {
        const foot = draw(false).get("[data-testid='message-editor-foot']");

        expect(foot.text()).toContain("Esc to cancel, Enter to save");
        expect(foot.find("[data-testid='edit-cancel']").exists()).toBe(true);
        expect(foot.find("[data-testid='edit-save']").exists()).toBe(true);
    });

    test("draws no key names on a phone, only Cancel and Save", () => {
        const foot = draw(true).get("[data-testid='message-editor-foot']");

        expect(foot.find("kbd").exists()).toBe(false);
        expect(foot.text()).not.toContain("to save");
        expect(foot.find("[data-testid='edit-cancel']").exists()).toBe(true);
        expect(foot.find("[data-testid='edit-save']").exists()).toBe(true);
    });
});

describe("an empty editor", () => {
    test("Enter saves nothing and does not close it", async () => {
        const w = draw(false);
        await typeOver(w, "");

        await enter(w);

        expect(w.emitted("save")).toBeUndefined();
        expect(w.emitted("cancel")).toBeUndefined();
    });

    test("only spaces count as empty", async () => {
        const w = draw(false);
        await typeOver(w, "   ");

        await enter(w);

        expect(w.emitted("save")).toBeUndefined();
        expect(w.emitted("cancel")).toBeUndefined();
    });

    test("holds Save until there are words again", async () => {
        const w = draw(false);
        expect(w.get("[data-testid='edit-save']").attributes("disabled")).toBeUndefined();

        await typeOver(w, "");
        expect(w.get("[data-testid='edit-save']").attributes("disabled")).toBeDefined();

        await typeOver(w, "See you Monday");
        expect(w.get("[data-testid='edit-save']").attributes("disabled")).toBeUndefined();
    });

    test("Escape still cancels", async () => {
        const w = draw(false);
        await typeOver(w, "");

        box(w).dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true, cancelable: true }));
        await nextTick();

        expect(w.emitted("cancel")).toHaveLength(1);
    });
});

describe("an unfinished edit's words", () => {
    test("are told to the message actions as they change, for this message", async () => {
        const kept = keeping();
        const w = draw(false, kept);

        await typeOver(w, "See you Monday");

        expect(kept.keepEditText).toHaveBeenLastCalledWith(42, "See you Monday");
    });

    test("are what the editor opens with when the actions kept some for this message", async () => {
        const w = draw(false, keeping({ 42: "See you Monday instead" }));
        await nextTick();

        expect(box(w).textContent).toBe("See you Monday instead");
    });

    test("open with the message's own words when nothing was kept for it", async () => {
        const w = draw(false, keeping({ 7: "another message's words" }));
        await nextTick();

        expect(box(w).textContent).toBe("See you Sunday");
    });

    // the kept words differ from the message, so Enter on them is a change to save, not a cancel
    test("are saved by Enter as they stand", async () => {
        const w = draw(false, keeping({ 42: "See you Monday instead" }));
        await nextTick();

        await enter(w);

        expect(w.emitted("save")).toEqual([["See you Monday instead"]]);
    });

    test("typed back to the message's own words only close the editor on Enter", async () => {
        const w = draw(false, keeping({ 42: "See you Monday instead" }));
        await typeOver(w, "See you Sunday");

        await enter(w);

        expect(w.emitted("save")).toBeUndefined();
        expect(w.emitted("cancel")).toHaveLength(1);
    });
});

// A phone has the message column's width to give, as the composer takes the screen's: the editor
// is drawn to fill it rather than at a share of the screen.
describe("the editor's width", () => {
    test("fills the message column on a phone", () => {
        expect(draw(true).get("[data-testid='message-editor']").classes()).toContain("is-phone");
    });

    test("keeps its own width where there is a keyboard", () => {
        expect(draw(false).get("[data-testid='message-editor']").classes()).not.toContain("is-phone");
    });
});
