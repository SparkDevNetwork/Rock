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
// Everyone who reacted to a message, behind a chip's See all or a long press: a dialog, a sheet on
// a phone, with a tab of every emoji first and one per chip in chip order, opened on the chip's
// own. A chip's tab draws the people its chip carries at once and asks for the rest after them;
// rows the count says are coming are held so nothing moves when they land; a page at a time as
// the list nears its end, the emoji part of the cursor.
//
// The component these tests are written to, components/reactorsList.partial.obs:
//
//   props: {
//       isOpen: boolean,
//       reactions: Reaction[],            the message's chips, each with its newest three
//       code: string,                     the tab it opens on
//       reader: { personAliasGuid, name, photoUrl },
//       listReactors: (code | null, after: { reacted_at, reactor_person_alias_guid, code } | null) => Promise<ReactorsPage | null>,
//       isPhone: boolean
//   }
//   emits: close()
//   marks: tabs role="tab" data-testid="reactors-tab" with data-code; the list
//          data-testid="reactors-list"; rows "reactors-row"; held rows "reactors-reserved".
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import ReactorsList from "../../../../../src/Communication/Chat/ChatShell/components/reactorsList.partial.obs";

const me = "a0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";
const david = "a0000003-0000-4000-8000-000000000000";
const cara = "a0000004-0000-4000-8000-000000000000";

const reader = { personAliasGuid: me, name: "Jordan Ellis", photoUrl: "https://rock.example.org/jordan.jpg" };

const person = (alias: string, nick: string | null, last: string | null, at: string): Record<string, unknown> => ({
    person_alias_guid: alias, nick_name: nick, last_name: last, avatar_url: null, reacted_at: at, reactor_person_alias_guid: alias
});

const annaAt = person(anna, "Anna", "Reyes", "2026-10-16T09:03:00Z");
const davidAt = person(david, "David", "Okafor", "2026-10-16T09:02:00Z");

const chips = [
    { code: "pray", count: 5, mine: true, emoji: "🙏", image_url: null, label: "folded hands", reactors: [person(me, "Jordan", "Ellis", "2026-10-16T09:04:00Z"), annaAt, davidAt] },
    { code: "heart", count: 1, mine: false, emoji: "❤️", image_url: null, label: "red heart", reactors: [person(cara, "Cara", "Lin", "2026-10-16T09:05:00Z")] }
];

let wrapper: VueWrapper | null = null;

function draw(props: Record<string, unknown>): VueWrapper {
    wrapper = mount(ReactorsList, { props: { isOpen: true, reactions: chips, code: "pray", reader, listReactors: jest.fn(async () => ({ reactors: [], has_more: false })), ...props } as never, attachTo: document.body });
    return wrapper;
}

async function settle(): Promise<void> {
    for (let i = 0; i < 5; i++) {
        await Promise.resolve();
        await nextTick();
    }
}

function names(w: VueWrapper): string[] {
    return w.findAll("[data-testid='reactors-row'] .chat-rl-name").map(name => name.text());
}

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
});

describe("the dialog", () => {
    test("is a panel: its title and Close in a head, the list under it, no actions row", () => {
        const w = draw({});

        const dialog = w.get("[role='dialog']");
        expect(dialog.classes()).toContain("is-panel");
        expect(dialog.get(".chat-dialog-head").text()).toContain("Reactions");
        expect(dialog.get(".chat-dialog-head [data-testid='reactors-close']").exists()).toBe(true);
        expect(dialog.find(".chat-dialog-actions").exists()).toBe(false);
    });
});

describe("the tabs", () => {
    test("are every emoji first, then one per chip in chip order with its count, opened on the chip's own", () => {
        const w = draw({});

        const tabs = w.findAll("[data-testid='reactors-tab']");
        expect(tabs.map(tab => tab.attributes("aria-label"))).toEqual(["All, 6", "folded hands, 5", "red heart, 1"]);
        expect(w.get("[role='tablist']").exists()).toBe(true);
        expect(tabs[1].attributes("aria-selected")).toBe("true");
    });
});

describe("a chip's tab", () => {
    test("draws the people its chip carries at once, the reader as You, and asks for the rest after the last of them", async () => {
        const listReactors = jest.fn(() => new Promise<never>(() => undefined));
        const w = draw({ listReactors });

        expect(names(w)).toEqual(["You", "Anna Reyes", "David Okafor"]);
        expect(listReactors).toHaveBeenCalledWith("pray", { reacted_at: davidAt.reacted_at, reactor_person_alias_guid: david, code: "pray" });

        // Five reacted and three are drawn, so two rows are held for the rest.
        await settle();
        expect(w.findAll("[data-testid='reactors-reserved']")).toHaveLength(2);
    });

    test("asks for nothing when its chip carries everyone", () => {
        const listReactors = jest.fn(async () => null);
        draw({ code: "heart", listReactors });

        expect(listReactors).not.toHaveBeenCalled();
    });

    test("lays the rest in once they land, the held rows gone", async () => {
        const w = draw({ listReactors: async () => ({ reactors: [{ ...person(cara, "Cara", "Lin", "2026-10-16T09:01:00Z"), code: "pray" }, { ...person("a0000005-0000-4000-8000-000000000000", null, null, "2026-10-16T09:00:00Z"), code: "pray" }], has_more: false }) });
        await settle();

        expect(names(w)).toEqual(["You", "Anna Reyes", "David Okafor", "Cara Lin", ""]);
        expect(w.findAll("[data-testid='reactors-reserved']")).toHaveLength(0);
    });
});

describe("the tab of every emoji", () => {
    test("holds rows by the count until the first page lands, then draws each person with their emoji", async () => {
        let answer!: (page: unknown) => void;
        const listReactors = jest.fn((code: string | null) => code === null ? new Promise(resolve => answer = resolve) : new Promise<never>(() => undefined));
        const w = draw({ listReactors });

        await w.findAll("[data-testid='reactors-tab']")[0].trigger("click");
        expect(listReactors).toHaveBeenLastCalledWith(null, null);
        expect(w.findAll("[data-testid='reactors-reserved']")).toHaveLength(6);

        answer({ reactors: [{ ...person(cara, "Cara", "Lin", "2026-10-16T09:05:00Z"), code: "heart" }, { ...person(anna, "Anna", "Reyes", "2026-10-16T09:03:00Z"), code: "pray" }], has_more: true });
        await settle();

        expect(names(w)).toEqual(["Cara Lin", "Anna Reyes"]);
        expect(w.findAll("[data-testid='reactors-row']").map(row => row.find(".chat-rl-e").text())).toEqual(["❤️", "🙏"]);
    });

    test("asks for the next page after the last row, emoji and all, as the list nears its end", async () => {
        const listReactors = jest.fn(async (code: string | null) => code === null
            ? { reactors: [{ ...person(anna, "Anna", "Reyes", "2026-10-16T09:03:00Z"), code: "pray" }], has_more: true }
            : { reactors: [], has_more: false });
        const w = draw({ listReactors });
        await w.findAll("[data-testid='reactors-tab']")[0].trigger("click");
        await settle();

        const list = w.get("[data-testid='reactors-list']").element as HTMLElement;
        Object.defineProperty(list, "scrollHeight", { value: 400, configurable: true });
        Object.defineProperty(list, "clientHeight", { value: 320, configurable: true });
        list.scrollTop = 20;
        await w.get("[data-testid='reactors-list']").trigger("scroll");

        expect(listReactors).toHaveBeenLastCalledWith(null, { reacted_at: "2026-10-16T09:03:00Z", reactor_person_alias_guid: anna, code: "pray" });
    });
});

describe("the dialog", () => {
    test("rises as a sheet on a phone", () => {
        draw({ isPhone: true });

        expect(document.querySelector("[data-testid='dialog-scrim']")?.classList.contains("is-sheet")).toBe(true);
    });

    test("closes from its close button", async () => {
        const w = draw({});

        await w.get("[data-testid='reactors-close']").trigger("click");

        expect(w.emitted("close")).toHaveLength(1);
    });
});
