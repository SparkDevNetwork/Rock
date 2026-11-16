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
import { useListKeys } from "../../../../../src/Communication/Chat/ChatShell/composables/useListKeys.partial";

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
