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
// The first load's veil over a region. Until the region is revealed it lies over it, and the
// region under it is out of reach (the frame's own rule). From the moment the region is revealed
// the veil is leaving: it fades out over the content coming in, and takes no input while it does,
// so a first click or key on the revealed region reaches it rather than the veil.
//
// The component these tests are written to, components/pageVeil.partial.obs:
//
//   props: region: "sidebar" | "main", isRevealed: boolean, isMarksShown: boolean, kind: RoomLook
//   marks: page-veil, the veil, while it is drawn
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import PageVeil from "../../../../../src/Communication/Chat/ChatShell/components/pageVeil.partial.obs";

let wrapper: VueWrapper | null = null;

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
});

/** The veil with Vue's own transition running, as it does on a page, so its way out is seen. */
function drawVeil(region: "sidebar" | "main"): VueWrapper {
    wrapper = mount(PageVeil, {
        props: { region, isRevealed: false, isMarksShown: true, kind: "channel" },
        attachTo: document.body,
        global: { stubs: { transition: false } }
    });
    return wrapper;
}

describe("the page veil", () => {
    test.each(["sidebar", "main"] as const)("over the %s, stands in until the region is revealed", region => {
        const w = drawVeil(region);

        const veil = w.find("[data-testid='page-veil']");
        expect(veil.exists()).toBe(true);
        expect(veil.attributes("inert")).toBeUndefined();
    });

    test.each(["sidebar", "main"] as const)("over the %s, takes no input from the moment the region is revealed, while it fades", async region => {
        const w = drawVeil(region);

        await w.setProps({ isRevealed: true });
        await nextTick();

        const leaving = document.querySelector("[data-testid='page-veil']");
        expect(leaving).not.toBeNull();
        expect(leaving?.hasAttribute("inert")).toBe(true);
    });
});
