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
// Where the dialog stands in the chat's frame. A question stands centred; a dialog whose body
// grows and shrinks while it is open, as settings does when a note comes and goes, is anchored at
// the top, so its title and its first rows never move under the person's eye.
import { mount, VueWrapper } from "@vue/test-utils";
import Dialog from "../../../../../src/Communication/Chat/ChatShell/ui/dialog.partial.obs";
import { providedSession } from "../sessionShapes";
import { ruleOf, styleOf } from "./partStyle";

let wrapper: VueWrapper | null = null;

function draw(props: Record<string, unknown> = {}): VueWrapper {
    wrapper = mount(Dialog, {
        props: { isOpen: true, title: "Chat settings", ...props },
        global: { provide: { ...providedSession() } },
        attachTo: document.body
    });
    return wrapper;
}

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
});

describe("the dialog's place", () => {
    test("stands centred unless told otherwise", () => {
        const scrim = draw().get("[data-testid='dialog-scrim']");

        expect(scrim.classes()).not.toContain("is-placed-top");
        expect(ruleOf(styleOf("dialog.partial.obs"), ".chat-modal")).toMatch(/place-items:\s*center/);
    });

    test("anchored at the top, keeps its top edge where it opened however tall its body grows", () => {
        const scrim = draw({ placement: "top" }).get("[data-testid='dialog-scrim']");

        expect(scrim.classes()).toContain("is-placed-top");
        expect(ruleOf(styleOf("dialog.partial.obs"), ".chat-modal.is-placed-top")).toMatch(/place-items:\s*start center/);
    });

    test("rises from the bottom as a sheet on a phone wherever it would stand on a wider screen", () => {
        wrapper = mount(Dialog, {
            props: { isOpen: true, title: "Chat settings", placement: "top", sheet: "phone" },
            global: { provide: { ...providedSession({ isPhone: true }) } },
            attachTo: document.body
        });
        const scrim = wrapper.get("[data-testid='dialog-scrim']");

        expect(scrim.classes()).toContain("is-sheet");
        expect(scrim.classes()).not.toContain("is-placed-top");
    });
});
