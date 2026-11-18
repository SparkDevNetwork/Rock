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
// The list panel: the one frame the Mentions and Threads lists are drawn in. It holds what the two
// shared: the panel host and its Refresh, the strip a failed refresh leaves over the rows it kept,
// the skeleton rows while a first page loads, the empty state, what a screen reader hears as a
// page lands or a refresh ends, and the drawn rows' links kept out of the tab order. A list draws
// only its rows.
//
//   props: title, subtitle, name (plural, as "mentions"), itemName (as "mention"), entranceKeys,
//          isLoading, isRefreshing, error (the strip's sentence or null), skeletonCount, isEmpty,
//          emptyIcon, emptyTitle, emptyText, emptyActionLabel, loadedSentence, upToDateSentence, bodyAttrs
//   emits: refresh, close, emptyAction
//   marks: <name>-refresh, <name>-error, <name>-empty, <itemName>-skeleton
import { mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, h, nextTick } from "vue";
import EmptyState from "../../../../../src/Communication/Chat/ChatShell/ui/emptyState.partial.obs";
import ListPanel from "../../../../../src/Communication/Chat/ChatShell/ui/listPanel.partial.obs";

let wrapper: VueWrapper | null = null;

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
});

const base = {
    title: "Mentions",
    name: "mentions",
    itemName: "mention",
    isLoading: false,
    isRefreshing: false,
    error: null,
    isEmpty: false,
    emptyTitle: "No mentions yet",
    emptyText: "When someone @mentions you, it shows up here.",
    loadedSentence: "3 mentions",
    upToDateSentence: "Mentions are up to date"
};

/** The panel, drawing one row whose drawn copy holds a link, as a message body's would. */
function draw(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(ListPanel, {
        props: { ...base, ...props },
        slots: {
            default: () => h("ul", [h("li", { "data-testid": "row" }, [h("button", { type: "button" }, "Open"), h("div", { inert: "" }, [h("a", { href: "https://example.com" }, "a link")])])])
        },
        attachTo: document.body
    });
    return wrapper;
}

const said = (): string | null | undefined => document.querySelector("[data-testid='announcer-polite']")?.textContent;

describe("the list panel", () => {
    test("while a first page loads draws its skeleton rows and says so, and neither rows nor the empty state", () => {
        const w = draw({ isLoading: true, skeletonCount: 3 });

        expect(w.findAll("[data-testid='mention-skeleton']")).toHaveLength(3);
        expect(w.text()).toContain("Loading mentions");
        expect(w.find("[data-testid='row']").exists()).toBe(false);
        expect(w.find("[data-testid='mentions-empty']").exists()).toBe(false);
    });

    test("an empty list draws the shared empty state with its words and action", async () => {
        const w = draw({ isEmpty: true, emptyActionLabel: "See all" });

        const empty = w.get("[data-testid='mentions-empty']");
        expect(empty.get(".chat-empty-title").text()).toBe("No mentions yet");
        expect(empty.get(".chat-empty-text").text()).toBe("When someone @mentions you, it shows up here.");
        expect(w.find("[data-testid='row']").exists()).toBe(false);

        await empty.get("button").trigger("click");
        expect(w.emitted("emptyAction")).toHaveLength(1);
    });

    test("an empty list's state draws the list's own mark in its tile", () => {
        const w = draw({ isEmpty: true, emptyIcon: "at" });

        expect(w.getComponent(EmptyState).props("icon")).toBe("at");
        expect(w.find("[data-testid='mentions-empty'] .chat-empty-tile").exists()).toBe(true);
    });

    test("a failed refresh leaves its sentence over the rows it kept, with Try again", async () => {
        const w = draw({ error: "Couldn't refresh." });

        const strip = w.get("[data-testid='mentions-error']");
        expect(strip.text()).toContain("Couldn't refresh.");
        expect(w.find("[data-testid='row']").exists()).toBe(true);

        await strip.get("button").trigger("click");
        expect(w.emitted("refresh")).toHaveLength(1);
    });

    test("Refresh asks once, and is busy while a refresh is out", async () => {
        const w = draw();

        const refresh = w.get("[data-testid='mentions-refresh']");
        expect(refresh.attributes("aria-label")).toBe("Refresh mentions");
        await refresh.trigger("click");
        expect(w.emitted("refresh")).toHaveLength(1);

        await w.setProps({ isRefreshing: true });
        expect(refresh.attributes("aria-busy")).toBe("true");
        await refresh.trigger("click");
        expect(w.emitted("refresh")).toHaveLength(1);
    });

    test("says what a first page brought as it lands, and that the list is up to date once a refresh ends", async () => {
        const w = draw({ isLoading: true });

        await w.setProps({ isLoading: false });
        expect(said()).toBe("3 mentions");

        await w.setProps({ isRefreshing: true });
        await w.setProps({ isRefreshing: false });
        expect(said()).toBe("Mentions are up to date");
    });

    test("says a failure as it comes, with the strip", async () => {
        const w = draw();

        await w.setProps({ error: "Couldn't refresh." });
        expect(said()).toBe("Couldn't refresh.");
    });

    test("keeps a drawn row's links out of the tab order", async () => {
        const w = draw();
        await nextTick();

        expect((w.get("[inert] a").element as HTMLElement).tabIndex).toBe(-1);
    });

    test("closes from its host's X", async () => {
        const w = draw();

        await w.get("[data-testid='panel-close']").trigger("click");
        expect(w.emitted("close")).toHaveLength(1);
    });

    test("gives its body the attributes the list names it by", () => {
        const w = draw({ bodyAttrs: { "data-testid": "mentions-panel", role: "tabpanel" } });

        expect(w.get("[data-testid='mentions-panel']").attributes("role")).toBe("tabpanel");
        expect(w.get("[data-testid='mentions-panel']").attributes("aria-busy")).toBe("false");
    });
});

// A list's own band under the header, as the Threads tabs, goes above its body.
describe("the band under the header", () => {
    test("is drawn from the top slot", () => {
        const Host = defineComponent({
            setup() {
                return () => h(ListPanel, { ...base }, { top: () => h("div", { "data-testid": "band" }), default: () => h("ul") });
            }
        });
        wrapper = mount(Host, { attachTo: document.body });

        expect(wrapper.find("[data-testid='band']").exists()).toBe(true);
    });
});
