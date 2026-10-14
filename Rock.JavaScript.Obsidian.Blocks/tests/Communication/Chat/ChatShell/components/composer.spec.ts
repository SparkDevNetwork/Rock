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
// The composer itself, mounted: what is left in its box after a send is what the person sees,
// so it is read from the box rather than from the rule the box is meant to follow.
import { mount } from "@vue/test-utils";
import Composer from "../../../../../src/Communication/Chat/ChatShell/components/composer.partial.obs";

async function sendFrom(isTextKeptOnSend: boolean): Promise<{ box: string, sent: unknown[] }> {
    const wrapper = mount(Composer, { props: { isTextKeptOnSend } });
    await wrapper.get("[data-testid='composer-input']").setValue("hello");
    await wrapper.get("form").trigger("submit");

    return {
        box: (wrapper.get("[data-testid='composer-input']").element as HTMLTextAreaElement).value,
        sent: wrapper.emitted("send") ?? []
    };
}

describe("the composer", () => {
    test("keeps the text in its box when the shell keeps it, as a draft's first message does", async () => {
        const { box, sent } = await sendFrom(true);

        expect(sent).toEqual([["hello"]]);
        expect(box).toBe("hello");
    });

    test("empties its box after an ordinary send", async () => {
        const { box, sent } = await sendFrom(false);

        expect(sent).toEqual([["hello"]]);
        expect(box).toBe("");
    });

    // With no cryptographic generator the send makes no row, so a failed row cannot keep the
    // text; only the box can.
    test("keeps the text in its box in a browser that cannot make a send key", async () => {
        const original = Object.getOwnPropertyDescriptor(globalThis, "crypto");
        Object.defineProperty(globalThis, "crypto", { value: undefined, configurable: true, writable: true });
        try {
            const { box, sent } = await sendFrom(false);

            expect(sent).toEqual([["hello"]]);
            expect(box).toBe("hello");
        }
        finally {
            if (original) {
                Object.defineProperty(globalThis, "crypto", original);
            }
        }
    });
});

describe("the composer's box", () => {
    test("tells the shell each change to its text, so a draft holds what the person sees", async () => {
        const wrapper = mount(Composer);
        await wrapper.get("[data-testid='composer-input']").setValue("hello, edited");

        expect(wrapper.emitted("edit")?.slice(-1)).toEqual([["hello, edited"]]);
    });
});

describe("the composer's held text", () => {
    test("is put back in the box whenever what holds it changes, so a new empty draft does not show text typed in a channel", async () => {
        const wrapper = mount(Composer, { props: { heldText: "", heldBy: null } });
        await wrapper.get("[data-testid='composer-input']").setValue("typed in a channel");

        await wrapper.setProps({ heldText: "", heldBy: { people: [] } });

        expect((wrapper.get("[data-testid='composer-input']").element as HTMLTextAreaElement).value).toBe("");
    });
});

describe("the composer's placeholder", () => {
    test("names a channel plainly, with no hash character, since the hash is an icon", () => {
        const wrapper = mount(Composer, { props: { title: "young-adults", isChannel: true } });

        expect(wrapper.get("[data-testid='composer-input']").attributes("placeholder")).toBe("Message young-adults");
    });

    test("names the other person of a direct message", () => {
        const wrapper = mount(Composer, { props: { title: "Anna Reyes" } });

        expect(wrapper.get("[data-testid='composer-input']").attributes("placeholder")).toBe("Message Anna Reyes");
    });
});

describe("the composer's key hint", () => {
    test("says how to send and how to start a new line", () => {
        const wrapper = mount(Composer, { props: { title: "Anna Reyes" } });

        expect(wrapper.get("[data-testid='composer-hint']").text()).toBe("Enter to send, Shift + Enter for a new line");
    });

    test("is left out on a phone, which has no Enter key worth naming", () => {
        const wrapper = mount(Composer, { props: { title: "Anna Reyes", isPhone: true } });

        expect(wrapper.find("[data-testid='composer-hint']").exists()).toBe(false);
    });
});

describe("the composer's keys", () => {
    test("Enter sends the text", async () => {
        const wrapper = mount(Composer);
        const input = wrapper.get("[data-testid='composer-input']");
        await input.setValue("hello");

        await input.trigger("keydown", { key: "Enter" });

        expect(wrapper.emitted("send")).toEqual([["hello"]]);
    });

    test("Shift and Enter does not send, leaving the new line to the box", async () => {
        const wrapper = mount(Composer);
        const input = wrapper.get("[data-testid='composer-input']");
        await input.setValue("hello");

        await input.trigger("keydown", { key: "Enter", shiftKey: true });

        expect(wrapper.emitted("send")).toBeUndefined();
    });
});

describe("the composer's send button", () => {
    test("is ready, in the church's fill, only once there is text to send", async () => {
        const wrapper = mount(Composer);
        const send = wrapper.get("[data-testid='composer-send']");
        expect(send.classes()).not.toContain("is-ready");
        expect(send.attributes("aria-label")).toBe("Send");

        await wrapper.get("[data-testid='composer-input']").setValue("hello");

        expect(send.classes()).toContain("is-ready");
    });
});

describe("the composer's text box", () => {
    test("grows to the height of what is typed, which the sheet caps before it scrolls", async () => {
        const wrapper = mount(Composer, { attachTo: document.body });
        const input = wrapper.get("[data-testid='composer-input']").element as HTMLTextAreaElement;
        Object.defineProperty(input, "scrollHeight", { configurable: true, get: () => 98 });

        await wrapper.get("[data-testid='composer-input']").setValue("one\ntwo\nthree\nfour");

        expect(input.style.height).toBe("98px");
        wrapper.unmount();
    });

    test("goes back to one line once it is emptied by a send", async () => {
        const wrapper = mount(Composer, { attachTo: document.body });
        const input = wrapper.get("[data-testid='composer-input']").element as HTMLTextAreaElement;
        let contentHeight = 98;
        Object.defineProperty(input, "scrollHeight", { configurable: true, get: () => contentHeight });
        await wrapper.get("[data-testid='composer-input']").setValue("one\ntwo\nthree\nfour");

        contentHeight = 35;
        await wrapper.get("form").trigger("submit");

        expect(input.style.height).toBe("35px");
        wrapper.unmount();
    });
});

describe("the composer while the service is paused", () => {
    test("is a locked bar saying sending is paused, with no box to type into", () => {
        const wrapper = mount(Composer, { props: { title: "young-adults", isChannel: true, isLocked: true } });

        expect(wrapper.get("[data-testid='composer-locked']").text()).toBe("Sending is paused for maintenance.");
        expect(wrapper.find("[data-testid='composer-input']").exists()).toBe(false);
    });
});
