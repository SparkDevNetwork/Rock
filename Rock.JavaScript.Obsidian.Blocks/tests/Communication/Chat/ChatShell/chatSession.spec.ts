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
// The session context: what the whole chat shares, provided once by the shell and read by any part
// however deep, so no layer between carries it. A part drawn alone reads a session in which no one
// is signed in and the width is not a phone's.
import { mount } from "@vue/test-utils";
import { computed, defineComponent, h, nextTick, ref } from "vue";
import { ChatMe, SessionContext, provideChatSession, useChatSession } from "../../../../src/Communication/Chat/ChatShell/chatSession.partial";

describe("the session context", () => {
    let read: SessionContext;

    const Deep = defineComponent({
        setup() {
            read = useChatSession();
            return () => h("span", read.isPhone.value ? "phone" : "wide");
        }
    });

    it("is read several layers below where it was provided", async () => {
        const isPhone = ref(false);
        const me = ref<ChatMe>({ personAliasGuid: "a1", name: "Ann", photoUrl: null });
        const Middle = defineComponent({ setup: () => () => h("div", [h(Deep)]) });
        const Shell = defineComponent({
            setup() {
                provideChatSession({ isPhone, me: computed(() => me.value), church: { name: "Grace" }, emojiSource: null, mentionSource: () => null });
                return () => h(Middle);
            }
        });

        const wrapper = mount(Shell);
        expect(read.me.value.personAliasGuid).toBe("a1");
        expect(read.church.name).toBe("Grace");
        expect(wrapper.text()).toBe("wide");

        // followed as it changes, as the window narrows or a birthdate lets a new session in
        isPhone.value = true;
        me.value = { personAliasGuid: "a2", name: "Ann", photoUrl: null };
        await nextTick();
        expect(wrapper.text()).toBe("phone");
        expect(read.me.value.personAliasGuid).toBe("a2");
    });

    it("reads as no one signed in, at a wide width, for a part drawn alone", () => {
        mount(Deep);
        expect(read.isPhone.value).toBe(false);
        expect(read.me.value.personAliasGuid).toBe("");
        expect(read.church.name).toBe("");
        expect(read.emojiSource).toBeNull();
        expect(read.mentionSource(null)).toBeNull();
    });
});
