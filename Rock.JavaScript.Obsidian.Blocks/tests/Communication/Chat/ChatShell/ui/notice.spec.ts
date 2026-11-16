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
// Floating notices: one raised pill each, stacked in one place above the composer, spoken through
// one status region that is on the page before anything is said in it. Any part raises one through
// the session's notices; a passing notice goes after the sheet's toast life, a failure stays until
// it is let go, and one that offers an action stays until its owner takes it back.
import { mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, h, nextTick } from "vue";
import Button from "../../../../../src/Communication/Chat/ChatShell/ui/button.partial.obs";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import Notice from "../../../../../src/Communication/Chat/ChatShell/ui/notice.partial.obs";
import { NoticeItem, Notices, provideNotices, useNotices } from "../../../../../src/Communication/Chat/ChatShell/ui/notices.partial";
import { partRuleBreaks, ruleOf, styleOf } from "./partStyle";

/** The host, drawn under a parent that provides the notices, and the notices it reads. */
function drawHost(): { wrapper: VueWrapper, notices: Notices } {
    let notices!: Notices;
    const Parent = defineComponent({
        setup() {
            notices = provideNotices();
            return () => h(Notice, { "data-testid": "notices" });
        }
    });

    return { wrapper: mount(Parent, { attachTo: document.body }), notices };
}

function pill(wrapper: VueWrapper, key: string): ReturnType<VueWrapper["find"]> {
    return wrapper.find(`[data-key='${key}']`);
}

describe("the notices", () => {
    let drawn: VueWrapper | null = null;

    afterEach(() => {
        drawn?.unmount();
        drawn = null;
        jest.useRealTimers();
    });

    test("speak through one status region, there before anything is said in it", async () => {
        const { wrapper, notices } = drawHost();
        drawn = wrapper;
        const region = wrapper.get("[data-testid='notices']");
        expect(region.attributes("role")).toBe("status");

        notices.show({ key: "online", code: "back-online", text: "You're back online", icon: "check" });
        await nextTick();

        expect(region.get("[data-key='online']").text()).toBe("You're back online");
        expect(region.get("[data-key='online']").attributes("data-code")).toBe("back-online");
        expect(wrapper.findComponent(Icon).props("name")).toBe("check");
    });

    test("a notice raised again under its key replaces itself rather than stacking", async () => {
        const { wrapper, notices } = drawHost();
        drawn = wrapper;

        notices.show({ key: "limit", text: "Too long." });
        notices.show({ key: "limit", text: "Still too long." });
        await nextTick();

        expect(wrapper.findAll("[data-key='limit']")).toHaveLength(1);
        expect(pill(wrapper, "limit").text()).toBe("Still too long.");
    });

    test("a passing notice goes after the toast life, telling its owner once", async () => {
        jest.useFakeTimers();
        const onDismiss = jest.fn();
        const { wrapper, notices } = drawHost();
        drawn = wrapper;

        notices.show({ key: "online", text: "You're back online", onDismiss });
        await nextTick();

        jest.advanceTimersByTime(2599);
        expect(onDismiss).not.toHaveBeenCalled();

        jest.advanceTimersByTime(1);
        await nextTick();
        expect(onDismiss).toHaveBeenCalledTimes(1);
        expect(notices.items.value).toEqual([]);
    });

    test("a failure stays until it is let go, from its own dismiss button that a keyboard can reach", async () => {
        jest.useFakeTimers();
        const onDismiss = jest.fn();
        const { wrapper, notices } = drawHost();
        drawn = wrapper;

        notices.show({ key: "rate", text: "Slow down a little.", tone: "failure", onDismiss });
        await nextTick();
        jest.advanceTimersByTime(60_000);
        await nextTick();

        const failure = pill(wrapper, "rate");
        expect(failure.classes()).toContain("is-failure");
        expect(wrapper.findComponent(Icon).props("name")).toBe("alert");
        const dismiss = failure.get("button[aria-label='Dismiss']");
        expect(dismiss.attributes("type")).toBe("button");

        await dismiss.trigger("click");

        expect(onDismiss).toHaveBeenCalledTimes(1);
        expect(notices.items.value).toEqual([]);
    });

    test("a passing notice offers no dismiss button unless asked", async () => {
        const { wrapper, notices } = drawHost();
        drawn = wrapper;

        notices.show({ key: "a", text: "Copied." });
        notices.show({ key: "b", text: "Saved.", isDismissible: true });
        await nextTick();

        expect(pill(wrapper, "a").find("button").exists()).toBe(false);
        expect(pill(wrapper, "b").find("button[aria-label='Dismiss']").exists()).toBe(true);
    });

    test("an action is a button in the pill, and its notice stays until its owner takes it back", async () => {
        jest.useFakeTimers();
        const onPress = jest.fn();
        const onDismiss = jest.fn();
        const { wrapper, notices } = drawHost();
        drawn = wrapper;

        notices.show({ key: "undo", text: "Message deleted.", action: { label: "Undo", onPress }, onDismiss });
        await nextTick();
        jest.advanceTimersByTime(60_000);
        await nextTick();

        const action = pill(wrapper, "undo").findComponent(Button);
        expect(action.text()).toBe("Undo");
        await action.trigger("click");
        expect(onPress).toHaveBeenCalledTimes(1);

        notices.withdraw("undo");
        await nextTick();
        expect(onDismiss).not.toHaveBeenCalled();
        expect(notices.items.value).toEqual([]);
    });

    test("a notice its owner takes back goes quietly, and its timer with it", async () => {
        jest.useFakeTimers();
        const onDismiss = jest.fn();
        const { wrapper, notices } = drawHost();
        drawn = wrapper;

        notices.show({ key: "online", text: "You're back online", onDismiss });
        await nextTick();
        notices.withdraw("online");
        await nextTick();
        jest.advanceTimersByTime(5000);

        expect(onDismiss).not.toHaveBeenCalled();
    });

    test("a part drawn alone, with no session above it, holds notices of its own", () => {
        let notices!: Notices;
        const Alone = defineComponent({
            setup() {
                notices = useNotices();
                return () => h("div");
            }
        });
        drawn = mount(Alone);

        notices.show({ key: "x", text: "Hello." } as NoticeItem);

        expect(notices.items.value.map(item => item.key)).toEqual(["x"]);
    });

    test("a part under the session reads the session's notices", () => {
        let provided!: Notices;
        let read!: Notices;
        const Child = defineComponent({
            setup() {
                read = useNotices();
                return () => h("div");
            }
        });
        const Parent = defineComponent({
            setup() {
                provided = provideNotices();
                return () => h(Child);
            }
        });
        drawn = mount(Parent);

        expect(read).toBe(provided);
    });
});

describe("the notices' look", () => {
    const style = styleOf("notice.partial.obs");

    test("stack centred 14px above the foot of the place the frame gives them, over the conversation", () => {
        const host = ruleOf(style, ".chat-notices");

        expect(host).toMatch(/position:\s*absolute/);
        expect(host).toMatch(/bottom:\s*14px/);
        expect(host).toMatch(/z-index:\s*var\(--chat-z-notice\)/);
        expect(host).toMatch(/align-items:\s*center/);
    });

    test("each is a 34px raised pill in the interface's type, the same in both themes, its status in its icon alone", () => {
        const notice = ruleOf(style, ".chat-notice");

        expect(notice).toMatch(/height:\s*34px/);
        expect(notice).toMatch(/background:\s*var\(--chat-raised\)/);
        expect(notice).toMatch(/color:\s*var\(--chat-text\)/);
        expect(notice).toMatch(/border-radius:\s*var\(--chat-r-full\)/);
        expect(notice).toMatch(/box-shadow:\s*var\(--chat-shadow-pop\)/);
        expect(notice).toMatch(/font-size:\s*var\(--chat-fs-ui\)/);
        expect(ruleOf(style, ".chat-notice.is-failure > .chat-ic")).toMatch(/color:\s*var\(--chat-danger-ink\)/);
    });

    test("rises in and sinks out with the set's pop, which reduced motion leaves a fade", () => {
        expect(ruleOf(style, ".chat-notice-enter-active")).toMatch(/animation:\s*chat-pop-in var\(--chat-d-toast-in\) var\(--chat-ease-in\)/);
        expect(ruleOf(style, ".chat-notice-leave-active")).toMatch(/animation:\s*chat-pop-out var\(--chat-d-toast-out\) var\(--chat-ease-out\) forwards/);
    });

    test("holds no motion, priority, reach or capitals of its own", () => {
        expect(partRuleBreaks("notice.partial.obs")).toEqual([]);
    });
});
