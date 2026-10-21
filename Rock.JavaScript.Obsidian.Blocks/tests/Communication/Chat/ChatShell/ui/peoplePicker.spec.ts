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
// The people picker New message and Add people share: chips over a search field, and the search's
// answer listed under it. Its owner keeps the people chosen and the name typed and runs the search;
// the picker draws them and owns the keys. Over the page, the list opens only while a name is typed
// in the focused field and Escape closes it first; inline, it is always there, saying what to type
// and when the cap is reached. Someone the search marks as in the room is dimmed with the reason,
// listed last, and can be neither picked nor reached with the arrows. Enter with a name only ever
// picks, waiting for an answer still out; Enter on an empty field is the owner's next step.
/* eslint-disable @typescript-eslint/naming-convention */
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import PeoplePicker from "../../../../../src/Communication/Chat/ChatShell/ui/peoplePicker.partial.obs";
import { PersonRow } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

/** One person as chat_search_people answers them, exactly its keys. */
function person(n: number, nickName: string, lastName: string, inRoom?: boolean): PersonRow {
    return { person_alias_guid: `a00000${10 + n}-0000-4000-8000-000000000000`, nick_name: nickName, last_name: lastName, avatar_url: null, campus_id: null, ...(inRoom === undefined ? {} : { in_room: inRoom }) };
}

const aaron = person(1, "Aaron", "Park", false);
const anna = person(2, "Anna", "Reyes", true);
const andre = person(3, "Andre", "Lin", false);

let wrapper: VueWrapper | null = null;

function draw(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(PeoplePicker, {
        props: {
            chosen: [],
            query: "",
            results: null,
            max: 8,
            label: "Add",
            prefix: "pp",
            inputTestId: "pp-input",
            listTestId: "pp-list",
            ...props
        },
        attachTo: document.body
    });
    return wrapper;
}

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
});

function input(w: VueWrapper): HTMLInputElement {
    return w.get("[data-testid='pp-input']").element as HTMLInputElement;
}

async function press(w: VueWrapper, key: string): Promise<KeyboardEvent> {
    const event = new KeyboardEvent("keydown", { key, bubbles: true, cancelable: true });
    input(w).dispatchEvent(event);
    await nextTick();
    return event;
}

/** The owner's answer for a name, as it hands it down. */
async function answer(w: VueWrapper, query: string, results: PersonRow[]): Promise<void> {
    await w.setProps({ query, results, isSearching: false });
}

function optionNames(w: VueWrapper): string[] {
    return w.findAll("[data-testid='pp-option'] b").map(b => b.text());
}

describe("over the page", () => {
    test("the list opens only while a name is typed in the focused field", async () => {
        const w = draw({ query: "a", results: [aaron] });
        expect(w.find("[data-testid='pp-list']").exists()).toBe(false);

        input(w).focus();
        await w.get("[data-testid='pp-input']").trigger("focus");
        expect(w.find("[data-testid='pp-list']").exists()).toBe(true);
        expect(input(w).getAttribute("aria-expanded")).toBe("true");

        await w.setProps({ query: "" });
        expect(w.find("[data-testid='pp-list']").exists()).toBe(false);
    });

    test("Escape closes the open list first, stopped there; the next is the owner's", async () => {
        const w = draw({ query: "a", results: [aaron] });
        input(w).focus();
        await w.get("[data-testid='pp-input']").trigger("focus");

        const first = await press(w, "Escape");
        expect(first.defaultPrevented).toBe(true);
        expect(w.find("[data-testid='pp-list']").exists()).toBe(false);
        expect(w.emitted("escape")).toBeUndefined();

        await press(w, "Escape");
        expect(w.emitted("escape")).toHaveLength(1);
    });

    test("marks the part of each name the text matched, when asked", async () => {
        const w = draw({ isMatchMarked: true });
        input(w).focus();
        await w.get("[data-testid='pp-input']").trigger("focus");
        await answer(w, "par", [aaron]);

        expect(w.get("[data-testid='pp-option'] mark").text()).toBe("Par");
    });
});

describe("inline", () => {
    test("the list is always there, saying what to type, and at the cap says so in place of the results", async () => {
        const w = draw({ isInline: true, idleText: "Type a name.", capText: "You can add up to 1 person.", max: 1 });
        expect(w.get("[data-testid='pp-idle']").text()).toBe("Type a name.");

        await answer(w, "a", [aaron]);
        expect(optionNames(w)).toEqual(["Aaron Park"]);

        await w.setProps({ chosen: [aaron] });
        expect(w.get("[data-testid='pp-cap']").text()).toBe("You can add up to 1 person.");
        expect(w.findAll("[data-testid='pp-option']")).toHaveLength(0);
    });

    test("Escape is left to what holds it", async () => {
        const w = draw({ isInline: true });
        await answer(w, "a", [aaron]);

        const event = await press(w, "Escape");
        expect(event.defaultPrevented).toBe(false);
        expect(w.emitted("escape")).toHaveLength(1);
    });
});

describe("someone in the room", () => {
    test("is dimmed with the reason, listed last, and a press does not pick them", async () => {
        const w = draw({ isInline: true, inRoomReason: "Already in young-adults" });
        await answer(w, "a", [anna, aaron]);

        expect(optionNames(w)).toEqual(["Aaron Park", "Anna Reyes"]);
        const dimmed = w.findAll("[data-testid='pp-option']")[1];
        expect(dimmed.attributes("aria-disabled")).toBe("true");
        expect(dimmed.get("[data-testid='pp-option-why']").text()).toBe("Already in young-adults");

        await dimmed.trigger("mousedown");
        expect(w.emitted("choose")).toBeUndefined();
    });

    test("the arrows pass them by, and Enter picks the one marked", async () => {
        const w = draw({ isInline: true, inRoomReason: "Already here" });
        await answer(w, "a", [aaron, anna, andre]);

        await press(w, "ArrowDown");
        expect(input(w).getAttribute("aria-activedescendant")).toContain(andre.person_alias_guid);
        await press(w, "ArrowDown");
        expect(input(w).getAttribute("aria-activedescendant")).toContain(aaron.person_alias_guid);

        await press(w, "ArrowUp");
        await press(w, "Enter");
        expect(w.emitted("choose")).toEqual([[andre]]);
    });

    test("is pickable where no reason is given", async () => {
        const w = draw({ isInline: true });
        await answer(w, "a", [anna]);

        await w.get("[data-testid='pp-option']").trigger("mousedown");
        expect(w.emitted("choose")).toEqual([[anna]]);
    });
});

describe("Enter", () => {
    test("with a name while the search is out waits, and picks once the answer comes", async () => {
        const w = draw({ isInline: true, query: "aa", isSearching: true });

        await press(w, "Enter");
        expect(w.emitted("choose")).toBeUndefined();

        await answer(w, "aa", [aaron]);
        expect(w.emitted("choose")).toEqual([[aaron]]);
    });

    test("on an empty field is the owner's next step", async () => {
        const w = draw({ isInline: true });

        await press(w, "Enter");
        expect(w.emitted("submit")).toHaveLength(1);
        expect(w.emitted("choose")).toBeUndefined();
    });

    test("with a name no one can be picked for says so through the owner, and at the cap says that", async () => {
        const w = draw({ isInline: true, inRoomReason: "Already here", max: 1 });
        await answer(w, "anna", [anna]);

        await press(w, "Enter");
        expect(w.emitted("noMatch")).toEqual([["anna"]]);

        await w.setProps({ chosen: [aaron] });
        await press(w, "Enter");
        expect(w.emitted("full")).toHaveLength(1);
    });
});

describe("the chips", () => {
    test("Backspace on an empty field takes out the last, and a chip's X takes out its person", async () => {
        const w = draw({ chosen: [aaron, andre] });

        await press(w, "Backspace");
        expect(w.emitted("unchoose")).toEqual([[andre]]);

        await w.findAll("[data-testid='pp-chip-remove']")[0].trigger("click");
        expect(w.emitted("unchoose")?.[1]).toEqual([aaron]);
    });

    test("cannot be taken out while the owner's work is under way", async () => {
        const w = draw({ chosen: [aaron], isDisabled: true });

        expect(w.get("[data-testid='pp-chip-remove']").attributes("disabled")).toBeDefined();
        expect(input(w).disabled).toBe(true);
    });
});

test("an answer is said: how many can be picked, or that no one was found", async () => {
    const w = draw({ isInline: true, inRoomReason: "Already here" });

    await answer(w, "a", [aaron, anna]);
    await answer(w, "zz", []);

    expect(w.emitted("announce")).toEqual([["1 person found"], ["No one found"]]);
});
