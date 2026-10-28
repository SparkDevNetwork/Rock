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
// The panel host's entrance. Every list in the panel column, and a thread's messages, waterfall in
// when the panel opens with its first page: the host marks its body entering, and the chat's
// sheet plays each marked row's rise from that mark. The host is told the keys of the rows it
// draws, in order, or null while the first page is still loading. It enters once, when the first
// rows are drawn. A row drawn after that, a live arrival, a later page or a filter cleared, is
// marked settled and does not rise, while the first page's rows keep rising; the entrance ends
// once every first-page row has risen or gone. A list that loads again from nothing, as a tab
// switch or a retry does, enters again.
import { mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, h, PropType } from "vue";
import PanelHost from "../../../../../src/Communication/Chat/ChatShell/ui/panelHost.partial.obs";

let wrapper: VueWrapper | null = null;

/** A panel drawing one entering row for each key, as the lists do. */
const panel = defineComponent({
    props: {
        keys: {
            type: Array as PropType<readonly string[] | null>,
            default: null
        }
    },
    setup(props) {
        return () => h(PanelHost, { title: "Threads", entranceKeys: props.keys }, {
            default: () => h("ul", (props.keys ?? []).map(key => h("li", { key, class: "chat-enter", "data-key": key }, key)))
        });
    }
});

function draw(keys: readonly string[] | null): VueWrapper {
    wrapper = mount(panel, { props: { keys }, attachTo: document.body });
    return wrapper;
}

async function show(w: VueWrapper, keys: readonly string[] | null): Promise<void> {
    await w.setProps({ keys });
    await w.vm.$nextTick();
}

function isEntering(w: VueWrapper): boolean {
    return w.get(".chat-panel-body").classes().includes("is-entering");
}

/** The keys of the rows that rise while the panel is entering: those not marked settled. */
function risingKeys(w: VueWrapper): string[] {
    return w.findAll(".chat-panel-body li").filter(li => li.attributes("data-chat-settled") === undefined).map(li => li.attributes("data-key") ?? "");
}

/** A row's rise ending, as the browser reports it from the row up through the body. */
function riseEnds(w: VueWrapper, key: string, name = "chat-enter"): void {
    const event = new Event("animationend", { bubbles: true });
    Object.defineProperty(event, "animationName", { value: name });
    w.get(`.chat-panel-body li[data-key='${key}']`).element.dispatchEvent(event);
}

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
});

describe("the panel's entrance", () => {
    test("nothing enters while the first page is loading, and the page enters as it is drawn", async () => {
        const w = draw(null);
        expect(isEntering(w)).toBe(false);

        await show(w, ["a", "b", "c"]);
        expect(isEntering(w)).toBe(true);
        expect(risingKeys(w)).toEqual(["a", "b", "c"]);
    });

    test("a panel opened with its first page already held enters at once", async () => {
        const w = draw(["a", "b"]);
        await w.vm.$nextTick();

        expect(isEntering(w)).toBe(true);
        expect(risingKeys(w)).toEqual(["a", "b"]);
    });

    test("a row added after the first page, live or by a later page, does not rise, and the first page's rows keep rising", async () => {
        const w = draw(["a", "b"]);
        await w.vm.$nextTick();

        await show(w, ["c", "a", "b"]);

        expect(isEntering(w)).toBe(true);
        expect(risingKeys(w)).toEqual(["a", "b"]);
    });

    test("a filter keeps the rows it leaves rising, and clearing it does not replay the rest", async () => {
        const w = draw(["a", "b", "c"]);
        await w.vm.$nextTick();

        await show(w, ["b"]);
        expect(isEntering(w)).toBe(true);
        expect(risingKeys(w)).toEqual(["b"]);

        await show(w, ["a", "b", "c"]);
        expect(risingKeys(w)).toEqual(["b"]);
    });

    test("a redraw of the same rows keeps the entrance playing", async () => {
        const w = draw(["a", "b"]);
        await w.vm.$nextTick();

        await show(w, ["a", "b"]);
        expect(isEntering(w)).toBe(true);
        expect(risingKeys(w)).toEqual(["a", "b"]);
    });

    test("an empty first page does not enter, nor do rows that arrive into it later", async () => {
        const w = draw(null);

        await show(w, []);
        expect(isEntering(w)).toBe(false);
        await show(w, ["a"]);
        expect(isEntering(w)).toBe(false);
    });

    test("a list that loads again from nothing enters again, every row of it", async () => {
        const w = draw(["a", "b"]);
        await w.vm.$nextTick();
        await show(w, ["a", "b", "c"]);

        await show(w, null);
        expect(isEntering(w)).toBe(false);
        await show(w, ["c", "x"]);
        expect(isEntering(w)).toBe(true);
        expect(risingKeys(w)).toEqual(["c", "x"]);
    });

    // Once every row has risen the mark goes, so a row's own later motion, such as a member row
    // collapsing as it is removed, is never held under the finished entrance.
    test("the entrance ends once every first-page row has risen, and other animations do not count", async () => {
        const w = draw(["a", "b"]);
        await w.vm.$nextTick();

        riseEnds(w, "a", "chat-skeleton");
        riseEnds(w, "a");
        await w.vm.$nextTick();
        expect(isEntering(w)).toBe(true);

        riseEnds(w, "b");
        await w.vm.$nextTick();
        expect(isEntering(w)).toBe(false);
    });

    test("a first-page row that goes before it has risen leaves the entrance to the rest", async () => {
        const w = draw(["a", "b", "c"]);
        await w.vm.$nextTick();
        await show(w, ["a", "c", "d"]);

        riseEnds(w, "a");
        await w.vm.$nextTick();
        expect(isEntering(w)).toBe(true);

        riseEnds(w, "c");
        await w.vm.$nextTick();
        expect(isEntering(w)).toBe(false);
    });
});
