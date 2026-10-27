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
// rows are drawn, and stops as soon as the list changes under it (a live arrival, a later page, a
// filter) or every row has risen, so nothing after the first page rises with it. A list that
// loads again from nothing, as a tab switch or a retry does, enters again.
import { mount, VueWrapper } from "@vue/test-utils";
import PanelHost from "../../../../../src/Communication/Chat/ChatShell/ui/panelHost.partial.obs";

let wrapper: VueWrapper | null = null;

function draw(entranceKeys: readonly string[] | null): VueWrapper {
    wrapper = mount(PanelHost, {
        props: { title: "Threads", entranceKeys },
        slots: { default: "<ul><li class=\"chat-enter\">One</li></ul>" },
        attachTo: document.body
    });
    return wrapper;
}

function isEntering(w: VueWrapper): boolean {
    return w.get(".chat-panel-body").classes().includes("is-entering");
}

/** A row's rise ending, as the browser reports it from the row up through the body. */
function riseEnds(w: VueWrapper, name = "chat-enter"): void {
    const event = new Event("animationend", { bubbles: true });
    Object.defineProperty(event, "animationName", { value: name });
    w.get(".chat-panel-body li").element.dispatchEvent(event);
}

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
});

describe("the panel's entrance", () => {
    test("nothing enters while the first page is loading, and the page enters as it is drawn", async () => {
        const w = draw(null);
        expect(isEntering(w)).toBe(false);

        await w.setProps({ entranceKeys: ["a", "b", "c"] });
        expect(isEntering(w)).toBe(true);
    });

    test("a panel opened with its first page already held enters at once", () => {
        expect(isEntering(draw(["a", "b"]))).toBe(true);
    });

    test("a row added after the first page, live or by a later page, ends the entrance so it does not rise", async () => {
        const w = draw(["a", "b"]);

        await w.setProps({ entranceKeys: ["a", "b", "c"] });
        expect(isEntering(w)).toBe(false);
    });

    test("a filter that changes the rows ends the entrance, and clearing it does not replay it", async () => {
        const w = draw(["a", "b", "c"]);

        await w.setProps({ entranceKeys: ["b"] });
        expect(isEntering(w)).toBe(false);
        await w.setProps({ entranceKeys: ["a", "b", "c"] });
        expect(isEntering(w)).toBe(false);
    });

    test("a redraw of the same rows keeps the entrance playing", async () => {
        const w = draw(["a", "b"]);

        await w.setProps({ entranceKeys: ["a", "b"] });
        expect(isEntering(w)).toBe(true);
    });

    test("an empty first page does not enter, nor do rows that arrive into it later", async () => {
        const w = draw(null);

        await w.setProps({ entranceKeys: [] });
        expect(isEntering(w)).toBe(false);
        await w.setProps({ entranceKeys: ["a"] });
        expect(isEntering(w)).toBe(false);
    });

    test("a list that loads again from nothing enters again", async () => {
        const w = draw(["a", "b"]);
        await w.setProps({ entranceKeys: ["a", "b", "c"] });

        await w.setProps({ entranceKeys: null });
        expect(isEntering(w)).toBe(false);
        await w.setProps({ entranceKeys: ["x", "y"] });
        expect(isEntering(w)).toBe(true);
    });

    // Once every row has risen the mark goes, so a row's own later motion, such as a member row
    // collapsing as it is removed, is never held under the finished entrance.
    test("the entrance ends once every row has risen, and other animations do not count", async () => {
        const w = draw(["a", "b"]);

        riseEnds(w, "chat-skeleton");
        riseEnds(w);
        await w.vm.$nextTick();
        expect(isEntering(w)).toBe(true);

        riseEnds(w);
        await w.vm.$nextTick();
        expect(isEntering(w)).toBe(false);
    });
});
