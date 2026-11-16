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
// The empty state: a few words, centred, with no tile. An error is the same words in the same
// place with a Try again under them, never a strip above a list that says the opposite.
import { mount } from "@vue/test-utils";
import Button from "../../../../../src/Communication/Chat/ChatShell/ui/button.partial.obs";
import EmptyState from "../../../../../src/Communication/Chat/ChatShell/ui/emptyState.partial.obs";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import { partRuleBreaks, ruleOf, styleOf } from "./partStyle";

describe("the empty state", () => {
    test("says its title and its line, and draws no tile or icon", () => {
        const empty = mount(EmptyState, { props: { title: "No threads yet", text: "Reply to a message to start one." } });

        expect(empty.get(".chat-empty-title").text()).toBe("No threads yet");
        expect(empty.get(".chat-empty-text").text()).toBe("Reply to a message to start one.");
        expect(empty.findComponent(Icon).exists()).toBe(false);
        expect(empty.classes()).toEqual(expect.arrayContaining(["chat-empty", "is-empty"]));
    });

    test("leaves out the line it is not given", () => {
        expect(mount(EmptyState, { props: { title: "No members" } }).find(".chat-empty-text").exists()).toBe(false);
    });

    test("an error is marked as one, and offers its action as a button", async () => {
        const error = mount(EmptyState, { props: { title: "Couldn't load members", tone: "error", actionLabel: "Try again", actionIcon: "refresh", actionAttrs: { "data-testid": "x-retry" } } });
        const action = error.findComponent(Button);

        expect(error.classes()).toContain("is-error");
        expect(action.text()).toBe("Try again");
        expect(action.props("icon")).toBe("refresh");
        expect(action.attributes("data-testid")).toBe("x-retry");

        await action.trigger("click");
        expect(error.emitted("action")).toHaveLength(1);
    });

    test("offers no button without an action", () => {
        expect(mount(EmptyState, { props: { title: "No members" } }).findComponent(Button).exists()).toBe(false);
    });

    test("takes its test id and data from where it is drawn", () => {
        const empty = mount(EmptyState, { props: { title: "No members" }, attrs: { "data-testid": "x-state", "data-state": "error" } });

        expect(empty.attributes("data-testid")).toBe("x-state");
        expect(empty.attributes("data-state")).toBe("error");
    });
});

describe("the empty state's look", () => {
    const style = styleOf("emptyState.partial.obs");

    test("is centred words: the title in the text colour at the display size, its line muted in the interface's", () => {
        expect(ruleOf(style, ".chat-empty")).toMatch(/text-align:\s*center/);
        expect(ruleOf(style, ".chat-empty-title")).toMatch(/font-size:\s*var\(--chat-fs-display\)/);
        expect(ruleOf(style, ".chat-empty-title")).toMatch(/color:\s*var\(--chat-text\)/);
        expect(ruleOf(style, ".chat-empty-text")).toMatch(/font-size:\s*var\(--chat-fs-ui\)/);
        expect(ruleOf(style, ".chat-empty-text")).toMatch(/color:\s*var\(--chat-muted\)/);
    });

    // The type scale's display size is an empty state's; a failure was always one line in the
    // interface's size, so an error's title keeps it.
    test("an empty state's title draws at the display size, and an error's at the interface's", () => {
        expect(ruleOf(style, ".chat-empty-title")).toMatch(/font-size:\s*var\(--chat-fs-display\)/);
        expect(ruleOf(style, ".chat-empty.is-error .chat-empty-title")).toMatch(/font-size:\s*var\(--chat-fs-ui\)/);
    });

    // A short last line, such as one word left alone under a paragraph, reads as an accident; the
    // part balances its lines, so every empty state gets it.
    test("balances the lines of its title and its text", () => {
        expect(ruleOf(style, ".chat-empty-title")).toMatch(/text-wrap:\s*balance/);
        expect(ruleOf(style, ".chat-empty-text")).toMatch(/text-wrap:\s*balance/);
    });

    test("holds no motion, priority, reach or capitals of its own", () => {
        expect(partRuleBreaks("emptyState.partial.obs")).toEqual([]);
    });
});
