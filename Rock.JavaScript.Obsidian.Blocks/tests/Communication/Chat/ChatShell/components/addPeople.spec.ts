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
// Add people: a wide dialog over the frame with the people picker inline. A channel takes up to
// eight people a call; a group holds nine people in all, so it takes fewer as it fills. The search
// lists who the room may take, and someone already in it is shown dimmed with the reason and cannot
// be picked. The picked show as chips; a note counts what is left. A warning says new people see
// the history. Add waits for a pick, names one person or counts several, and says Adding while the
// door works; the door's refusal shows in a box, and taking out the person it named clears it.
//
// The component these tests are written to, addPeople.partial.obs:
//
//   props: isOpen, isChannel, channelName, memberCount: number | null, query: string,
//          people: PersonRow[] | null (the search's answer for the query), isSearching,
//          error: ChatError | null, isAdding, refusal: { text, personAliasGuid } | null,
//          campusNames: Record<string, string>
//   emits: close, query(text), add(people: PersonRow[])
//   marks: add-people (the dialog), ap-close, ap-sub, ap-input, ap-results, ap-option,
//          ap-option-why, ap-idle, ap-searching, ap-none, ap-search-error, ap-cap, ap-chip,
//          ap-chip-remove, ap-note, ap-warn, ap-refusal, ap-cancel, ap-add
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import AddPeople from "../../../../../src/Communication/Chat/ChatShell/components/addPeople.partial.obs";
import { PersonRow } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

const downtown = "d0000001-0000-4000-8000-000000000000";

/** One person as chat_search_people answers them for a room, exactly its keys. */
function person(n: number, nickName: string, lastName: string, inRoom = false, campusId: string | null = null): PersonRow {
    return { person_alias_guid: `a00000${10 + n}-0000-4000-8000-000000000000`, nick_name: nickName, last_name: lastName, avatar_url: null, campus_id: campusId, in_room: inRoom };
}

const aaron = person(1, "Aaron", "Park", false, downtown);
const anna = person(2, "Anna", "Reyes", true);
const luis = person(3, "Luis", "Romero");
const many = Array.from({ length: 9 }, (_, i) => person(20 + i, `P${i}`, "Zed"));

let wrapper: VueWrapper | null = null;

function draw(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(AddPeople, {
        props: {
            isOpen: true,
            isChannel: true,
            channelName: "young-adults",
            memberCount: 42,
            query: "",
            people: null,
            isSearching: false,
            error: null,
            isAdding: false,
            refusal: null,
            campusNames: { [downtown]: "Downtown" },
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

/** What a person reads of an element, each element's words apart; a face's initials and icons are hidden from them. */
function spaced(node: Node): string {
    if (node instanceof Element && node.getAttribute("aria-hidden") === "true") {
        return "";
    }
    return node.nodeType === Node.TEXT_NODE ? node.textContent ?? "" : Array.from(node.childNodes).map(child => ` ${spaced(child)} `).join("");
}

const read = (element: Element): string => spaced(element).replace(/\s+/g, " ").replace(/ ([.,;?])/g, "$1").trim();
const text = (w: VueWrapper, mark: string): string => read(w.get(`[data-testid='${mark}']`).element);

/** Answers a search: the name typed and the people found, as the shell would pass them. */
async function answer(w: VueWrapper, query: string, people: PersonRow[]): Promise<void> {
    await w.setProps({ query, people, isSearching: false });
}

/** Picks the person in the results by clicking their option. */
async function pick(w: VueWrapper, who: PersonRow): Promise<void> {
    const option = w.findAll("[data-testid='ap-option']").find(o => o.attributes("data-person") === who.person_alias_guid);
    await option!.trigger("mousedown");
}

describe("the dialog", () => {
    test("a channel's head, subtitle, idle line and history warning", () => {
        const w = draw();

        expect(w.get(".chat-dialog-title").text()).toBe("Add people to young-adults");
        expect(w.get("[data-testid='ap-close']").attributes("aria-label")).toBe("Close");
        expect(text(w, "ap-sub")).toBe("young-adults has 42 members. A leader can add up to 8 people at a time.");
        expect(text(w, "ap-idle")).toBe("Type a name to find someone to add to young-adults.");
        expect(text(w, "ap-warn")).toBe("New members will see the history of young-adults.");
        expect(w.get("[data-testid='ap-input']").attributes("placeholder")).toBe("Search for people");
    });

    test("a group's head, subtitle, idle line and history warning", () => {
        const w = draw({ isChannel: false, channelName: "Hike planning", memberCount: 4 });

        expect(w.get(".chat-dialog-title").text()).toBe("Add people to Hike planning");
        expect(text(w, "ap-sub")).toBe("Hike planning has 4 people. A group can have up to 9.");
        expect(text(w, "ap-idle")).toBe("Type a name. You can add people who accept messages from everyone, and people in your private groups.");
        expect(text(w, "ap-warn")).toBe("New people will see this conversation's history, back to the first message.");
    });

    test("typing asks for the name; while it searches it says so, and nothing found says so", async () => {
        const w = draw();

        await w.get("[data-testid='ap-input']").setValue("zz");
        expect(w.emitted("query")).toEqual([["zz"]]);

        await w.setProps({ query: "zz", isSearching: true });
        expect(text(w, "ap-searching")).toBe("Searching…");

        await answer(w, "zz", []);
        expect(text(w, "ap-none")).toBe("No one found for “zz”.");
    });

    test("a search that failed says so", async () => {
        const w = draw({ query: "an", error: { code: "rpc.transport", severity: "failed" } });

        expect(text(w, "ap-search-error")).toBe("Couldn't search right now.");
    });

    test("someone already in the room is dimmed with the reason and cannot be picked", async () => {
        const w = draw();
        await answer(w, "a", [aaron, anna]);

        const options = w.findAll("[data-testid='ap-option']");
        expect(options.map(o => read(o.element))).toEqual(["Aaron Park Downtown", "Anna Reyes Already in young-adults"]);
        expect(options[1].attributes("aria-disabled")).toBe("true");
        expect(text(w, "ap-option-why")).toBe("Already in young-adults");

        await pick(w, anna);
        expect(w.findAll("[data-testid='ap-chip']")).toHaveLength(0);
    });

    test("in a group, the reason says this conversation", async () => {
        const w = draw({ isChannel: false, channelName: "Hike planning", memberCount: 4 });
        await answer(w, "a", [anna]);

        expect(text(w, "ap-option-why")).toBe("Already in this conversation");
    });
});

describe("picking", () => {
    test("a pick becomes a chip, clears the name, and Add names them", async () => {
        const w = draw();
        await answer(w, "a", [aaron, luis]);

        await pick(w, aaron);
        expect(w.findAll("[data-testid='ap-chip']").map(c => read(c.element))).toEqual(["Aaron Park"]);
        expect(w.emitted("query")?.at(-1)).toEqual([""]);
        expect(text(w, "ap-add")).toBe("Add Aaron");
        expect(text(w, "ap-note")).toBe("1 of 8 at a time");
    });

    test("Add is disabled until a pick, and counts several", async () => {
        const w = draw();
        expect(w.get("[data-testid='ap-add']").attributes("disabled")).toBeDefined();
        expect(text(w, "ap-add")).toBe("Add");

        await answer(w, "a", [aaron, luis, person(4, "Mia", "Chen")]);
        await pick(w, aaron);
        await answer(w, "a", [luis, person(4, "Mia", "Chen")]);
        await pick(w, luis);
        await answer(w, "m", [person(4, "Mia", "Chen")]);
        await pick(w, person(4, "Mia", "Chen"));

        expect(w.get("[data-testid='ap-add']").attributes("disabled")).toBeUndefined();
        expect(text(w, "ap-add")).toBe("Add 3 people");
    });

    test("a chip's X takes the person out", async () => {
        const w = draw();
        await answer(w, "a", [aaron]);
        await pick(w, aaron);

        const remove = w.get("[data-testid='ap-chip-remove']");
        expect(remove.attributes("aria-label")).toBe("Remove Aaron Park");
        await remove.trigger("click");
        expect(w.findAll("[data-testid='ap-chip']")).toHaveLength(0);
    });

    test("a channel takes eight at a time, then says so in place of the results", async () => {
        const w = draw();
        for (const p of many.slice(0, 8)) {
            await answer(w, "p", [p]);
            await pick(w, p);
        }

        expect(w.findAll("[data-testid='ap-chip']")).toHaveLength(8);
        expect(text(w, "ap-cap")).toBe("You can add up to 8 people at a time.");
        expect(w.find("[data-testid='ap-option']").exists()).toBe(false);
        expect(text(w, "ap-note")).toBe("");
    });

    test("a group of four has room for five, and says so at the cap", async () => {
        const w = draw({ isChannel: false, channelName: "Hike planning", memberCount: 4 });
        await answer(w, "p", [many[0]]);
        await pick(w, many[0]);
        expect(text(w, "ap-note")).toBe("Room for 4 more");

        for (const p of many.slice(1, 5)) {
            await answer(w, "p", [p]);
            await pick(w, p);
        }
        expect(w.findAll("[data-testid='ap-chip']")).toHaveLength(5);
        expect(text(w, "ap-cap")).toBe("A group can have up to 9 people, so Hike planning has room for 5.");
    });

    test("Enter on the name picks the person found, and Backspace on an empty name takes out the last", async () => {
        const w = draw();
        await answer(w, "aa", [aaron]);

        const input = w.get("[data-testid='ap-input']");
        await input.trigger("keydown", { key: "Enter" });
        expect(w.findAll("[data-testid='ap-chip']")).toHaveLength(1);

        await w.setProps({ query: "" });
        await input.trigger("keydown", { key: "Backspace" });
        expect(w.findAll("[data-testid='ap-chip']")).toHaveLength(0);
    });
});

describe("adding", () => {
    test("Add asks for everyone picked, and says Adding while the door works", async () => {
        const w = draw();
        await answer(w, "a", [aaron, luis]);
        await pick(w, aaron);
        await answer(w, "l", [luis]);
        await pick(w, luis);

        await w.get("[data-testid='ap-add']").trigger("click");
        expect(w.emitted("add")).toEqual([[[aaron, luis]]]);

        await w.setProps({ isAdding: true });
        const add = w.get("[data-testid='ap-add']");
        expect(add.text()).toBe("Adding");
        expect(add.attributes("aria-busy")).toBe("true");
        expect(w.get("[data-testid='ap-input']").attributes("disabled")).toBeDefined();
    });

    test("a refusal shows in its box, and taking out the person it named clears it", async () => {
        const w = draw();
        await answer(w, "a", [aaron, luis]);
        await pick(w, aaron);
        await answer(w, "l", [luis]);
        await pick(w, luis);

        await w.setProps({ refusal: { text: "Luis Romero cannot be added. Remove Luis and try again.", personAliasGuid: luis.person_alias_guid } });
        const refusal = w.get("[data-testid='ap-refusal']");
        expect(read(refusal.element)).toBe("Luis Romero cannot be added. Remove Luis and try again.");
        expect(refusal.attributes("role")).toBe("alert");

        await w.findAll("[data-testid='ap-chip-remove']")[1].trigger("click");
        expect(w.find("[data-testid='ap-refusal']").exists()).toBe(false);
    });

    test("opening again starts with no one picked", async () => {
        const w = draw();
        await answer(w, "a", [aaron]);
        await pick(w, aaron);

        await w.setProps({ isOpen: false });
        await w.setProps({ isOpen: true });
        await nextTick();
        expect(w.findAll("[data-testid='ap-chip']")).toHaveLength(0);
    });

    test("Cancel and the X close", async () => {
        const w = draw();

        await w.get("[data-testid='ap-cancel']").trigger("click");
        await w.get("[data-testid='ap-close']").trigger("click");
        expect(w.emitted("close")).toHaveLength(2);
    });
});
