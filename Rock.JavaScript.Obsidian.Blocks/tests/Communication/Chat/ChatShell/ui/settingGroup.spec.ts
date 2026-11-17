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
// list, and a row taken out of it leaves by the Leave motion: it fades and shrinks where it stood
// while the rows below glide up by the list's one move.
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

    test("a row taken out of a list leaves by the Leave motion, the rows below moving by the one move", async () => {
        const { names, wrapper } = mountList(["Sam", "Ben", "Ana"]);

        expect(wrapper.getComponent(TransitionGroup).props("moveClass")).toBe("chat-move");

        names.value = ["Ben", "Ana"];
        await nextTick();

        const leaving = wrapper.get("[data-name='Sam']");
        expect(leaving.classes()).toContain("is-leaving");
    });

    test("holds its leaving rows inside it while they fade", () => {
        expect(ruleOf(styleOf("settingGroup.partial.obs"), ".chat-sg")).toMatch(/position:\s*relative/);
    });

    // A group spaces itself from what comes before it, so a page that opens on a group, as Blocked
    // people does, starts at the surface's own padding rather than a gap twice as tall.
    test("keeps its space above only after something else, none at the top of what holds it", () => {
        const style = styleOf("settingGroup.partial.obs");

        expect(ruleOf(style, ".chat-sg-sec")).not.toMatch(/margin-top/);
        expect(ruleOf(style, ".chat-sg-sec:not(:first-child)")).toMatch(/margin-top:\s*16px/);
    });
});
