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
// The announcer, the chat's one voice for a screen reader: what a part says without moving focus,
// such as how many results came back. It is provided once by the shell, so two live regions never
// talk over each other, and it speaks from inside an open modal surface, which a screen reader
// would otherwise keep it out of.
import { mount, VueWrapper } from "@vue/test-utils";
import { defineComponent, h, nextTick, ref } from "vue";
import { Announcer, provideAnnouncer, useAnnouncer, useAnnouncerModal } from "../../../../../src/Communication/Chat/ChatShell/ui/announcer.partial";

async function settle(): Promise<void> {
    await nextTick();
    await new Promise(resolve => setTimeout(resolve, 0));
}

function spoken(politeness: "polite" | "assertive" = "polite"): string[] {
    return Array.from(document.querySelectorAll<HTMLElement>(`[aria-live='${politeness}']`)).map(region => region.textContent ?? "").filter(text => text !== "");
}

describe("the announcer", () => {
    let wrapper: VueWrapper | null = null;
    let announcer: Announcer;

    afterEach(() => {
        wrapper?.unmount();
        wrapper = null;
        document.body.innerHTML = "";
    });

    /** A shell root that provides the announcer, and a part under it that reads it. */
    function shell(withModal = false): void {
        const isModalOpen = ref(withModal);
        const Part = defineComponent({
            setup() {
                announcer = useAnnouncer();
                const modal = ref<HTMLElement>();
                useAnnouncerModal(modal);
                return () => isModalOpen.value ? h("div", { ref: modal, role: "dialog", "aria-modal": "true", "data-testid": "modal" }) : null;
            }
        });
        const Root = defineComponent({
            setup() {
                const root = ref<HTMLElement>();
                provideAnnouncer(root);
                return () => h("div", { ref: root, "data-testid": "root" }, [h(Part)]);
            }
        });
        wrapper = mount(Root, { attachTo: document.body });
    }

    it("speaks politely from one region in the chat's root", async () => {
        shell();
        await settle();
        announcer.announce("4 threads you follow");
        await settle();
        expect(spoken()).toEqual(["4 threads you follow"]);
        const root = document.querySelector("[data-testid='root']") as HTMLElement;
        expect(root.querySelectorAll("[aria-live='polite']")).toHaveLength(1);
    });

    it("speaks a failure assertively, from its own region", async () => {
        shell();
        await settle();
        announcer.announce("Couldn't load mentions", "assertive");
        await settle();
        expect(spoken("assertive")).toEqual(["Couldn't load mentions"]);
        expect(spoken()).toEqual([]);
    });

    it("says the same words again when they are announced again", async () => {
        shell();
        await settle();
        announcer.announce("No matches");
        await settle();
        announcer.announce("No matches");
        // cleared first, so a screen reader hears a change rather than nothing
        expect(spoken()).toEqual([]);
        await settle();
        expect(spoken()).toEqual(["No matches"]);
    });

    it("speaks from inside an open modal surface, and from the root again once it closes", async () => {
        shell(true);
        await settle();
        announcer.announce("2 people found");
        await settle();
        const modal = document.querySelector("[data-testid='modal']") as HTMLElement;
        const region = Array.from(document.querySelectorAll<HTMLElement>("[aria-live='polite']")).find(one => one.textContent === "2 people found");
        expect(region && modal.contains(region)).toBe(true);
    });

    it("works for a part drawn alone, with no shell to provide it", async () => {
        const Alone = defineComponent({
            setup() {
                announcer = useAnnouncer();
                return () => h("div");
            }
        });
        wrapper = mount(Alone, { attachTo: document.body });
        announcer.announce("Saved");
        await settle();
        expect(spoken()).toEqual(["Saved"]);
    });
});
