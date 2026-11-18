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
// A settings row: its icon, its label with at most a line under it, and at its end its control, or
// its value and, for a row that leads to a page, a chevron. A row leaving its list fades and shrinks
// in place, the Leave motion, and never closes up its own height.
import { mount } from "@vue/test-utils";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";
import SettingRow from "../../../../../src/Communication/Chat/ChatShell/ui/settingRow.partial.obs";
import { ruleOf, styleOf } from "./partStyle";

describe("a settings row", () => {
    test("shows its value at its end, muted, with no chevron while it leads nowhere", () => {
        const row = mount(SettingRow, { props: { icon: "users", title: "Members", value: "12" } });

        expect(row.element.tagName).toBe("DIV");
        expect(row.get("[data-testid='setting-value']").text()).toBe("12");
        expect(row.findAllComponents(Icon).map(icon => icon.props("name"))).toEqual(["users"]);
    });

    test("a row that leads to a page is one button, its value then a chevron at its end", async () => {
        const row = mount(SettingRow, { props: { icon: "ban", title: "Blocked people", value: "None", isLink: true } });

        expect(row.element.tagName).toBe("BUTTON");
        expect(row.classes()).toContain("is-button");
        const value = row.get("[data-testid='setting-value']");
        expect(value.text()).toBe("None");
        expect(row.findAllComponents(Icon).map(icon => icon.props("name"))).toEqual(["ban", "right"]);
        expect(value.element.contains(row.findAllComponents(Icon)[1].element)).toBe(true);

        await row.trigger("click");
        expect(row.emitted("click")).toHaveLength(1);
    });

    // Blocked people has no count while its list loads or after it failed, and still leads to its page.
    test("a row that leads to a page shows its chevron with no value yet", () => {
        const row = mount(SettingRow, { props: { icon: "ban", title: "Blocked people", isLink: true } });

        expect(row.findAllComponents(Icon).map(icon => icon.props("name"))).toEqual(["ban", "right"]);
        expect(row.get("[data-testid='setting-value']").text()).toBe("");
    });

    test("draws no value where it has none", () => {
        const row = mount(SettingRow, { props: { title: "This browser" } });

        expect(row.find("[data-testid='setting-value']").exists()).toBe(false);
    });

    test("a row asking a question in place is set apart by the hover fill, its consequence in the secondary text colour so it reads before the answer", () => {
        const row = mount(SettingRow, { props: { title: "Unblock Sam?", sub: "They can message you, and you'll see their messages.", isAsking: true } });

        expect(row.classes()).toContain("is-asking");
        expect(ruleOf(styleOf("settingRow.partial.obs"), ".chat-sg-row.is-asking")).toMatch(/background:\s*var\(--chat-hover\)/);
        expect(row.find(".chat-sg-sub").text()).toBe("They can message you, and you'll see their messages.");
        expect(ruleOf(styleOf("settingRow.partial.obs"), ".chat-sg-row.is-asking .chat-sg-sub")).toMatch(/color:\s*var\(--chat-text-2\)/);
    });

    // Taken out of the flow at once, the row let its group's box shrink before it faded, and the box
    // clipped a last row away unseen; so it fades where it stands, still holding its place.
    test("leaves by the Leave motion: in its place in the flow, fading and shrinking to the popover's scale over the fast time, its height never animated, taking no press", () => {
        const style = styleOf("settingRow.partial.obs");
        const leaving = ruleOf(style, ".chat-sg-row.is-leaving");
        const left = ruleOf(style, ".chat-sg-row.is-left");

        expect(leaving).not.toMatch(/position/);
        expect(leaving).toMatch(/transition:[^;]*opacity var\(--chat-t-fast\)/);
        expect(leaving).toMatch(/transition:[^;]*transform var\(--chat-t-fast\)/);
        expect(leaving).toMatch(/pointer-events:\s*none/);
        expect(left).toMatch(/opacity:\s*0/);
        expect(left).toMatch(/transform:\s*scale\(var\(--chat-pop-scale\)\)/);
        expect(`${leaving};${left}`).not.toMatch(/height/);
    });

    // The rows below wait on no delay of their own: the list starts their glide once the row has
    // gone, which settingGroup.spec plays in order.
    test("the rows below glide into the gap by the one move once the leaving row has faded", () => {
        expect(ruleOf(styleOf("settingRow.partial.obs"), ".chat-sg-row.chat-move")).not.toMatch(/transition-delay/);
    });
});
