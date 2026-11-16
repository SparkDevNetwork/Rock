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
// The inline notice: one soft box for every note or failure inside a view. A note is grey, a
// failure a red tint with its mark in the danger ink; both carry an icon, their words and, for a
// failure that can be asked again, Try again at the end. Every screen that says such a thing in
// place draws it through this one part.
import { mount } from "@vue/test-utils";
import Button from "../../../../../src/Communication/Chat/ChatShell/ui/button.partial.obs";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import SettingNote from "../../../../../src/Communication/Chat/ChatShell/ui/settingNote.partial.obs";
import { partRuleBreaks, ruleOf, styleOf } from "./partStyle";

describe("the inline notice", () => {
    test("a note says its words beside an info mark, with nothing to press", () => {
        const note = mount(SettingNote, { props: { note: { kind: "info", text: "Only leaders can post in this channel." } } });

        expect(note.attributes("data-kind")).toBe("info");
        expect(note.findComponent(Icon).props("name")).toBe("info");
        expect(note.get(".chat-set-note-words").text()).toBe("Only leaders can post in this channel.");
        expect(note.findComponent(Button).exists()).toBe(false);
    });

    test("a failure carries the alert mark, and one that can be asked again offers Try again at its end", () => {
        const note = mount(SettingNote, { props: { note: { kind: "error", text: "Couldn't save the name.", retry: () => undefined } } });
        const retry = note.findComponent(Button);

        expect(note.attributes("data-kind")).toBe("error");
        expect(note.findComponent(Icon).props("name")).toBe("alert");
        expect(retry.text()).toBe("Try again");
        expect(retry.attributes("data-testid")).toBe("note-retry");
        // Try again is drawn one way across the chat: a quiet small button with the refresh mark
        expect(retry.props("kind")).toBe("ghost");
        expect(retry.props("size")).toBe("sm");
        expect(retry.props("icon")).toBe("refresh");
        // after the words, never inside them, so the sentence reads whole
        expect(note.element.lastElementChild?.contains(retry.element)).toBe(true);
        expect(note.get(".chat-set-note-words").element.contains(retry.element)).toBe(false);
    });

    test("a failure that cannot be asked again offers nothing to press", () => {
        const note = mount(SettingNote, { props: { note: { kind: "error", text: "He doesn't accept messages from everyone." } } });

        expect(note.findComponent(Button).exists()).toBe(false);
    });

    test("pressing Try again asks again once, tells the holder so it can hand focus back, and is not the row's press", async () => {
        const retry = jest.fn();
        const rowPress = jest.fn();
        const host = mount({
            components: { SettingNote },
            data: () => ({ note: { kind: "error", text: "Couldn't save the name.", retry } }),
            methods: { rowPress },
            template: `<div @click="rowPress"><SettingNote :note="note" @retry="$emit('handed')" /></div>`
        }, { attachTo: document.body });

        await host.get("[data-testid='note-retry']").trigger("click");

        expect(retry).toHaveBeenCalledTimes(1);
        expect(host.findComponent(SettingNote).emitted("retry")).toHaveLength(1);
        expect(rowPress).not.toHaveBeenCalled();
        host.unmount();
    });

    test("a holder may write its own words in place of the text, the mark and Try again still the notice's", () => {
        const note = mount(SettingNote, {
            props: { note: { kind: "error", text: "Not sent.", retry: () => undefined } },
            slots: { default: "<b>Not sent.</b> <span>You're offline.</span>" }
        });

        expect(note.get(".chat-set-note-words").html()).toContain("<b>Not sent.</b>");
        expect(note.findComponent(Icon).props("name")).toBe("alert");
        expect(note.get("[data-testid='note-retry']").text()).toBe("Try again");
    });

    test("a holder's own actions follow Try again at the end", () => {
        const note = mount(SettingNote, {
            props: { note: { kind: "error", text: "Not sent.", retry: () => undefined } },
            slots: { end: "<button data-testid='x-delete'>Delete</button>" }
        });
        const ends = [...note.element.lastElementChild!.children].map(child => child.getAttribute("data-testid"));

        expect(ends).toEqual(["note-retry", "x-delete"]);
    });

    test("takes its test id and id from where it is drawn", () => {
        const note = mount(SettingNote, { props: { note: { kind: "info", text: "Hi" } }, attrs: { "data-testid": "x-note", id: "x-1" } });

        expect(note.attributes("data-testid")).toBe("x-note");
        expect(note.attributes("id")).toBe("x-1");
    });
});

describe("the inline notice's look", () => {
    const style = styleOf("settingNote.partial.obs");

    test("is one soft box: 12px corners, the interface's size, words in the text colour", () => {
        const box = ruleOf(style, ".chat-set-note");

        expect(box).toMatch(/border-radius:\s*var\(--chat-r-surface\)/);
        expect(box).toMatch(/font-size:\s*var\(--chat-fs-ui\)/);
        expect(box).toMatch(/background:\s*var\(--chat-bubble\)/);
        expect(box).not.toMatch(/border:/);
    });

    test("a note is grey with secondary words; a failure is the danger tint, red only in its mark", () => {
        expect(ruleOf(style, ".chat-set-note")).toMatch(/color:\s*var\(--chat-text-2\)/);
        expect(ruleOf(style, ".chat-set-note[data-kind=\"error\"]")).toMatch(/background:\s*var\(--chat-danger-soft\)/);
        expect(ruleOf(style, ".chat-set-note[data-kind=\"error\"]")).toMatch(/color:\s*var\(--chat-text\)/);
        expect(ruleOf(style, ".chat-set-note-ic")).toMatch(/color:\s*var\(--chat-icon\)/);
        expect(ruleOf(style, ".chat-set-note[data-kind=\"error\"] .chat-set-note-ic")).toMatch(/color:\s*var\(--chat-danger-ink\)/);
    });

    test("holds no motion, priority, reach or capitals of its own", () => {
        expect(partRuleBreaks("settingNote.partial.obs")).toEqual([]);
    });
});
