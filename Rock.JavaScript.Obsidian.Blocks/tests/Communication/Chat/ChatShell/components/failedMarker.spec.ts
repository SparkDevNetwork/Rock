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
// The Not sent marker of a failed message, drawn once for a feed row, a thread's reply and a
// conversation's first message: a button that opens that message's own Try again and Delete, with
// the reason it failed as its one tooltip and its description for a screen reader, and a menu that
// opens as a sheet on a phone as the composer's menus do.
import { readFileSync } from "fs";
import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick, ref } from "vue";
import FailedMarker from "../../../../../src/Communication/Chat/ChatShell/components/failedMarker.partial.obs";
import IconButton from "../../../../../src/Communication/Chat/ChatShell/ui/iconButton.partial.obs";
import { isPhoneKey } from "../../../../../src/Communication/Chat/ChatShell/ui/viewport.partial";

const css = readFileSync(`${__dirname}/../../../../../../RockWeb/Styles/Blocks/Communication/Chat/chat.css`, "utf8");

function draw(props: { canRetry?: boolean, reason?: string | null } = {}, isPhone = false): VueWrapper {
    return mount(FailedMarker, { props, attachTo: document.body, global: { provide: { [isPhoneKey as symbol]: ref(isPhone) } } });
}

const button = (wrapper: VueWrapper): HTMLElement => wrapper.get("[data-testid='message-failed-actions']").element as HTMLElement;

async function open(wrapper: VueWrapper): Promise<void> {
    await wrapper.get("[data-testid='message-failed-actions']").trigger("click");
    await nextTick();
}

describe("the Not sent marker", () => {
    afterEach(() => {
        document.body.innerHTML = "";
    });

    test("is a menu button saying Not sent, named for what it offers", () => {
        const wrapper = draw();

        expect(button(wrapper).tagName).toBe("BUTTON");
        expect(button(wrapper).textContent).toContain("Not sent");
        expect(button(wrapper).getAttribute("aria-label")).toBe("Not sent. Try again or delete");
        expect(button(draw({ canRetry: false })).getAttribute("aria-label")).toBe("Not sent. Delete");
    });

    test("offers Try again and Delete, or Delete alone for a message refused for good", async () => {
        const both = draw();
        await open(both);
        expect(both.findAll("[role='menuitem']").map(i => i.text())).toEqual(["Try again", "Delete"]);
        document.body.innerHTML = "";

        const only = draw({ canRetry: false });
        await open(only);
        expect(only.findAll("[role='menuitem']").map(i => i.text())).toEqual(["Delete"]);
    });

    test("says which it chose, and nothing else", async () => {
        const wrapper = draw();
        await open(wrapper);
        await wrapper.findAll("[role='menuitem']")[0].trigger("click");
        await open(wrapper);
        await wrapper.findAll("[role='menuitem']")[1].trigger("click");

        expect(wrapper.emitted("retry")).toHaveLength(1);
        expect(wrapper.emitted("discard")).toHaveLength(1);
    });

    test("carries its reason as its one tooltip and its description, never a title", () => {
        const reason = "You're offline. Try again when you're back.";
        const wrapper = draw({ reason });
        const described = button(wrapper).getAttribute("aria-describedby");

        expect(described).toBeTruthy();
        expect(document.getElementById(described as string)?.textContent).toBe(reason);
        expect(wrapper.findComponent(IconButton).props("tooltip")).toBe(reason);
        expect(document.body.querySelector("[title]")).toBeNull();
    });

    test("has no description or tooltip of its own where nothing was said", () => {
        const wrapper = draw();

        expect(button(wrapper).hasAttribute("aria-describedby")).toBe(false);
        expect(wrapper.findComponent(IconButton).props("tooltip")).toBe("");
    });

    test("opens beside its button on a wide screen and as a sheet on a phone", async () => {
        const wide = draw();
        await open(wide);
        expect(document.querySelector(".chat-menu")).not.toBeNull();
        expect(document.querySelector("[data-testid='menu-scrim']")).toBeNull();
        document.body.innerHTML = "";

        const phone = draw({}, true);
        await open(phone);
        expect(document.querySelector(".chat-sheet")).not.toBeNull();
        expect(document.querySelector("[data-testid='menu-scrim']")).not.toBeNull();
    });

    // A finger or a pointer needs 24 by 24 CSS pixels, and the words are one short line, so the
    // button takes the height and a matching negative margin keeps the line from growing.
    test("is at least 24 pixels tall, with a margin that leaves its line as it was", () => {
        const rule = [...css.replace(/\/\*[\s\S]*?\*\//g, "").matchAll(/([^{}]+)\{([^}]*)\}/g)]
            .filter(match => match[1].trim() === "button.chat-icon-btn.chat-fail-btn")
            .map(match => match[2])
            .join("");

        expect(rule).toMatch(/min-height:\s*24px/);
        expect(rule).toMatch(/align-self:\s*baseline/);
        expect(rule).toMatch(/margin-block:\s*-\d/);
    });
});
