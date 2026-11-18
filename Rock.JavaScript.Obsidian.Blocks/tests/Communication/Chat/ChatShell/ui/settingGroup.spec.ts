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
// A settings group: its label over a rounded group of rows. A group that is a list of people is a
// list, and a row taken out of it leaves by the Leave motion: it fades and shrinks where it stands,
// still holding its place, and only once it has gone does the box close and the rows below glide
// up by the list's one move. The last row's going is told, so an empty list's words follow it.
import { mount } from "@vue/test-utils";
import { defineComponent, h, nextTick, ref, TransitionGroup } from "vue";
import SettingGroup from "../../../../../src/Communication/Chat/ChatShell/ui/settingGroup.partial.obs";
import SettingRow from "../../../../../src/Communication/Chat/ChatShell/ui/settingRow.partial.obs";
import { ruleOf, styleOf } from "./partStyle";

/** A list group of named rows, the names its own to change. */
function mountList(names: string[]): { names: ReturnType<typeof ref<string[]>>, wrapper: ReturnType<typeof mount> } {
    const shown = ref(names);
    const wrapper = mount(defineComponent({
        setup: () => () => h(SettingGroup, { label: "Blocked people", isList: true }, {
            default: () => shown.value!.map(name => h(SettingRow, { key: name, title: name, "data-name": name }))
        })
    }), { global: { stubs: { transition: false, "transition-group": false } } });

    return { names: shown, wrapper };
}

describe("a settings group", () => {
    test("a list group is a list, its rows its items", () => {
        const { wrapper } = mountList(["Sam", "Ben"]);

        const list = wrapper.get("[role='list']");
        expect(list.classes()).toContain("chat-sg");
        expect(list.findAll("[data-name]").every(row => row.attributes("role") === "listitem")).toBe(true);
    });

    // The rows below glide by the list's one move only once a row has gone, so the transition group
    // moves nothing of itself: a row whose ask opens or closes as it starts to leave neither glides
    // itself nor sets the rows below gliding while it fades.
    test("a row taken out of a list leaves by the Leave motion, the transition group moving nothing of itself", async () => {
        const { names, wrapper } = mountList(["Sam", "Ben", "Ana"]);

        expect(wrapper.getComponent(TransitionGroup).props("moveClass")).toBeUndefined();

        names.value = ["Ben", "Ana"];
        await nextTick();

        const leaving = wrapper.get("[data-name='Sam']");
        expect(leaving.classes()).toContain("is-leaving");
    });

    test("holds its leaving rows inside it while they fade", () => {
        expect(ruleOf(styleOf("settingGroup.partial.obs"), ".chat-sg")).toMatch(/position:\s*relative/);
    });

    describe("the Leave motion's order", () => {
        /** A browser's layout in one respect: each row 44 px tall, stacked from the group's top. */
        const rowHeight = 44;

        beforeEach(() => {
            jest.spyOn(HTMLElement.prototype, "offsetTop", "get").mockImplementation(function (this: HTMLElement) {
                return Array.from(this.parentElement?.children ?? []).indexOf(this) * rowHeight;
            });
        });

        afterEach(() => {
            jest.restoreAllMocks();
        });

        /** Waits out a leave: Vue ends it a frame after its last class goes on. */
        async function leaveEnds(): Promise<void> {
            for (let frame = 0; frame < 3; frame++) {
                await new Promise<void>(resolve => requestAnimationFrame(() => resolve()));
            }
            await nextTick();
        }

        function rowOf(wrapper: ReturnType<typeof mount>, name: string): HTMLElement | null {
            return wrapper.element.querySelector<HTMLElement>(`[data-name='${name}']`);
        }

        test("a middle row fades holding its place, and only once it has gone do the rows below glide up", async () => {
            const { names, wrapper } = mountList(["Sam", "Ben", "Ana"]);

            names.value = ["Sam", "Ana"];
            await nextTick();
            // while it fades it is still a row of the list, and nothing has moved
            expect(rowOf(wrapper, "Ben")?.classList).toContain("is-leaving");
            expect(rowOf(wrapper, "Ana")?.classList).not.toContain("chat-move");

            await leaveEnds();
            expect(rowOf(wrapper, "Ben")).toBeNull();
            expect(rowOf(wrapper, "Ana")?.classList).toContain("chat-move");
            expect(rowOf(wrapper, "Sam")?.classList).not.toContain("chat-move");
            expect(wrapper.getComponent(SettingGroup).emitted("empty")).toBeUndefined();
        });

        test("the last row fades holding its place, and nothing glides once it has gone", async () => {
            const { names, wrapper } = mountList(["Sam", "Ben", "Ana"]);

            names.value = ["Sam", "Ben"];
            await nextTick();
            expect(rowOf(wrapper, "Ana")?.classList).toContain("is-leaving");

            await leaveEnds();
            expect(rowOf(wrapper, "Ana")).toBeNull();
            expect(wrapper.element.querySelectorAll(".chat-move")).toHaveLength(0);
            expect(wrapper.getComponent(SettingGroup).emitted("empty")).toBeUndefined();
        });

        test("the only row fades holding its place, and the list says it is empty only once it has gone", async () => {
            const { names, wrapper } = mountList(["Sam"]);

            names.value = [];
            await nextTick();
            expect(rowOf(wrapper, "Sam")?.classList).toContain("is-leaving");
            expect(wrapper.getComponent(SettingGroup).emitted("empty")).toBeUndefined();

            await leaveEnds();
            expect(rowOf(wrapper, "Sam")).toBeNull();
            expect(wrapper.getComponent(SettingGroup).emitted("empty")).toHaveLength(1);
        });

        // A sheet held at the screen's bottom closes from its top: as the box closes by the row,
        // the list's top comes down by it and the rows below stay where they are seen, so nothing
        // is drawn away from its place to glide back.
        test("in a surface held at its bottom, the rows below a row that has gone stay where they are seen and do not glide", async () => {
            const { names, wrapper } = mountList(["Sam", "Ben", "Ana"]);
            const list = wrapper.get("[role='list']").element as HTMLElement;
            jest.spyOn(list, "getBoundingClientRect").mockImplementation(() => {
                const top = 500 + (3 - list.children.length) * rowHeight;
                return { x: 0, y: top, left: 0, top, right: 300, bottom: top + list.children.length * rowHeight, width: 300, height: list.children.length * rowHeight, toJSON: () => ({}) } as DOMRect;
            });

            names.value = ["Sam", "Ana"];
            await nextTick();
            await leaveEnds();

            const ana = rowOf(wrapper, "Ana") as HTMLElement;
            expect(rowOf(wrapper, "Ben")).toBeNull();
            expect(ana.classList).not.toContain("chat-move");
            expect(ana.style.transform).toBe("");
        });

        // Where less motion is asked for, the move takes no time and no transition ever ends, so a
        // row given the move would keep its class and its listener for good. The rows below simply
        // stand in their new places.
        test("where less motion is asked for, the rows below stand in their places with no move left on them", async () => {
            const matchMedia = window.matchMedia;
            window.matchMedia = ((query: string) => ({ media: query, matches: query === "(prefers-reduced-motion: reduce)" })) as unknown as typeof window.matchMedia;
            const listening = jest.spyOn(HTMLElement.prototype, "addEventListener");
            try {
                const { names, wrapper } = mountList(["Sam", "Ben", "Ana"]);

                names.value = ["Sam", "Ana"];
                await nextTick();
                await leaveEnds();

                const ana = rowOf(wrapper, "Ana") as HTMLElement;
                expect(ana.classList).not.toContain("chat-move");
                expect(ana.style.transform).toBe("");
                expect(ana.style.transition).toBe("");
                // the leaving row's own fade listens for its end; the row that stayed must not
                const anaListens = listening.mock.calls.filter(([type], call) => type === "transitionend" && listening.mock.contexts[call] === ana);
                expect(anaListens).toHaveLength(0);
            }
            finally {
                window.matchMedia = matchMedia;
            }
        });
    });

    // A group spaces itself from what comes before it, so a page that opens on a group, as Blocked
    // people does, starts at the surface's own padding rather than a gap twice as tall.
    test("keeps its space above only after something else, none at the top of what holds it", () => {
        const style = styleOf("settingGroup.partial.obs");

        expect(ruleOf(style, ".chat-sg-sec")).not.toMatch(/margin-top/);
        expect(ruleOf(style, ".chat-sg-sec:not(:first-child)")).toMatch(/margin-top:\s*16px/);
    });
});
