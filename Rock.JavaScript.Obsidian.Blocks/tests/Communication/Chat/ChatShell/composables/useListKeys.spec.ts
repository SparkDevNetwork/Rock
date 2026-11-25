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
// A list's keys, the one way: the list is one tab stop, the row last focused holds it (the first
// row while none has been), and Up, Down, Home and End move between rows, stopping at the ends.
// The Mentions and Threads panels each kept a verbatim copy; these cells hold the one.
import { mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, h, nextTick, PropType, ref } from "vue";
import { provideRowKeys, useListKeys, useRowKeys } from "../../../../../src/Communication/Chat/ChatShell/composables/useListKeys.partial";

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

function row(id: number): HTMLElement {
    return document.querySelector<HTMLElement>(`[data-row='${id}']`) as HTMLElement;
}

function press(key: string): KeyboardEvent {
    const event = new KeyboardEvent("keydown", { key, bubbles: true, cancelable: true });
    (document.activeElement ?? document.body).dispatchEvent(event);
    return event;
}

describe("a list's keys", () => {
    let wrapper: VueWrapper | null = null;
    const onPick = jest.fn();

    beforeAll(() => {
        Element.prototype.scrollIntoView = jest.fn();
    });

    afterEach(() => {
        wrapper?.unmount();
        wrapper = null;
        onPick.mockReset();
    });

    function list(ids: number[]): VueWrapper {
        const List = defineComponent({
            props: { ids: { type: Array as PropType<number[]>, required: true } },
            setup(props) {
                const keys = useListKeys(() => props.ids, { onPick });
                return () => h("ul", props.ids.map(id => h("li", [h("button", { "data-row": id, ...keys.itemAttrs(id), ref: keys.itemRef(id) }, `row ${id}`)])));
            }
        });
        wrapper = mount(List, { props: { ids }, attachTo: document.body });
        return wrapper;
    }

    it("is one tab stop, on the first row while none has been focused", async () => {
        list([1, 2, 3]);
        await settle();
        expect([1, 2, 3].map(id => row(id).tabIndex)).toEqual([0, -1, -1]);
    });

    it("moves the tab stop to the row last focused", async () => {
        list([1, 2, 3]);
        await settle();
        row(2).focus();
        await settle();
        expect([1, 2, 3].map(id => row(id).tabIndex)).toEqual([-1, 0, -1]);
    });

    it("gives the tab stop back to the first row when the row holding it is gone", async () => {
        const drawn = list([1, 2, 3]);
        await settle();
        row(2).focus();
        await drawn.setProps({ ids: [1, 3] });
        await settle();
        expect(row(1).tabIndex).toBe(0);
    });

    it("moves down and up by one, stopping at the ends rather than wrapping", async () => {
        list([1, 2, 3]);
        await settle();
        row(1).focus();
        expect(press("ArrowDown").defaultPrevented).toBe(true);
        expect(document.activeElement).toBe(row(2));
        press("ArrowDown");
        press("ArrowDown");
        expect(document.activeElement).toBe(row(3));
        press("ArrowUp");
        expect(document.activeElement).toBe(row(2));
    });

    it("goes to the first row on Home and the last on End, keeping the moved-to row in view", async () => {
        list([1, 2, 3]);
        await settle();
        row(2).focus();
        press("End");
        expect(document.activeElement).toBe(row(3));
        expect(row(3).scrollIntoView).toHaveBeenCalledWith({ block: "nearest" });
        press("Home");
        expect(document.activeElement).toBe(row(1));
    });

    it("leaves every other key to the row", async () => {
        list([1, 2]);
        await settle();
        row(1).focus();
        expect(press("ArrowRight").defaultPrevented).toBe(false);
        expect(document.activeElement).toBe(row(1));
    });

    it("picks the focused row on Enter, where a list asks to be told", async () => {
        list([1, 2]);
        await settle();
        row(2).focus();
        press("Enter");
        expect(onPick).toHaveBeenCalledWith(2);
    });
});

// The feed's messages are one list of hundreds of rows, each its own component. The stop is held
// per key, so a row reads whether it is the stop and nothing else: an arrow press wakes the two
// rows the stop moves between, and a message arriving wakes none of the rows already drawn.
describe("a list of row components, as the feed draws its messages", () => {
    let wrapper: VueWrapper | null = null;
    const renders: number[] = [];

    beforeAll(() => {
        Element.prototype.scrollIntoView = jest.fn();
    });

    afterEach(() => {
        wrapper?.unmount();
        wrapper = null;
        renders.length = 0;
    });

    const rowPart = defineComponent({
        props: { id: { type: Number, required: true } },
        setup(props) {
            const keys = useRowKeys();
            return () => {
                renders.push(props.id);
                const attrs = keys ? { ...keys.itemAttrs(String(props.id)), ref: keys.itemRef(String(props.id)) } : { tabindex: -1 };
                return h("div", { "data-row": props.id, ...attrs }, [h("button", { "data-inner": props.id }, "inner")]);
            };
        }
    });

    function feed(ids: number[], isNewestDefault = true): VueWrapper {
        const feedPart = defineComponent({
            props: { ids: { type: Array as PropType<number[]>, required: true } },
            setup(props) {
                const keys = (): string[] => props.ids.map(String);
                provideRowKeys(useListKeys(keys, {
                    isRowOnly: true,
                    ...(isNewestDefault ? { defaultKey: () => keys()[keys().length - 1] ?? null } : {})
                }));
                return () => h("div", props.ids.map(id => h(rowPart, { key: id, id })));
            }
        });
        wrapper = mount(feedPart, { props: { ids }, attachTo: document.body });
        return wrapper;
    }

    test("holds the stop on the newest row while none has been focused, where the list says so", async () => {
        feed([1, 2, 3]);
        await settle();
        expect([1, 2, 3].map(id => row(id).tabIndex)).toEqual([-1, -1, 0]);
    });

    test("an arrow press redraws only the row the stop leaves and the row it reaches", async () => {
        feed([1, 2, 3, 4]);
        await settle();
        row(2).focus();
        await settle();
        renders.length = 0;

        press("ArrowDown");
        await settle();

        expect(document.activeElement).toBe(row(3));
        expect([...renders].sort()).toEqual([2, 3]);
    });

    test("a row arriving redraws none of the rows drawn while a focused row holds the stop", async () => {
        const drawn = feed([1, 2, 3]);
        await settle();
        row(2).focus();
        await settle();
        renders.length = 0;

        await drawn.setProps({ ids: [1, 2, 3, 4] });
        await settle();

        expect(renders).toEqual([4]);
        expect(row(2).tabIndex).toBe(0);
        expect(row(4).tabIndex).toBe(-1);
    });

    test("keys pressed in something inside a row are that thing's, never a move between rows", async () => {
        feed([1, 2]);
        await settle();
        document.querySelector<HTMLElement>("[data-inner='1']")?.focus();

        expect(press("ArrowDown").defaultPrevented).toBe(false);
        expect(document.activeElement).toBe(document.querySelector("[data-inner='1']"));
    });

    test("Down reaches a row that moved into the group above it, as a direct message first drawn as a channel joins a quote to its run", async () => {
        // Each group is its own keyed element, so the moved row is drawn anew in the group it joins
        // before its old group, and the row in it, is taken away.
        const groupsPart = defineComponent({
            props: { groups: { type: Array as PropType<number[][]>, required: true } },
            setup(props) {
                const keys = (): string[] => props.groups.flat().map(String);
                provideRowKeys(useListKeys(keys, { isRowOnly: true, defaultKey: () => keys()[keys().length - 1] ?? null }));
                return () => h("div", props.groups.map(group => h("div", { key: `g${group[0]}` }, group.map(id => h(rowPart, { key: id, id })))));
            }
        });
        wrapper = mount(groupsPart, { props: { groups: [[1], [2], [3]] }, attachTo: document.body });
        await settle();
        await wrapper.setProps({ groups: [[1, 2], [3]] });
        await settle();

        row(1).focus();
        await settle();
        press("ArrowDown");
        await settle();

        expect(document.activeElement).toBe(row(2));
        expect(row(2).tabIndex).toBe(0);
    });

    test("a row drawn with no list around it is focusable but no stop", async () => {
        wrapper = mount(rowPart, { props: { id: 7 }, attachTo: document.body });
        await settle();
        expect(row(7).tabIndex).toBe(-1);
    });
});
