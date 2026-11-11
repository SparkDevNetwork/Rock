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
// The status menu's morning end across a daylight saving change, in the zone the zone run pins: on
// the evening before the clocks move, the next 8:00 is the next day's 8:00 by the calendar, 14 hours
// on as the clocks fall back and 12 as they spring forward, never a day's length from today's 8:00.

import { mount, VueWrapper } from "@vue/test-utils";
import { nextTick } from "vue";
import StatusMenu from "../../../../../src/Communication/Chat/ChatShell/components/statusMenu.partial.obs";
import { presenceStatusKey } from "../../../../../src/Communication/Chat/ChatShell/composables/usePresenceStatus.partial";
import { StatusChoice } from "../../../../../src/Communication/Chat/ChatShell/composables/usePrefs.partial";

let wrapper: VueWrapper | null = null;

async function settle(): Promise<void> {
    await nextTick();
    await nextTick();
}

function morningItem(): HTMLElement {
    return document.querySelector<HTMLElement>("[role='menu'] [data-key='busy-morning']")!;
}

async function openMenu(choice: StatusChoice): Promise<void> {
    wrapper?.unmount();
    wrapper = mount(StatusMenu, {
        props: { choice, trigger: "change" },
        global: { provide: { [presenceStatusKey as symbol]: { statusOf: () => null, register: () => () => undefined, now: () => Date.now() } } },
        attachTo: document.body
    });
    (wrapper.get("[data-testid='status-change']").element as HTMLElement).click();
    await settle();
}

beforeAll(() => {
    // jsdom has no ResizeObserver, and floating placement keeps its menu beside the anchor with one.
    if (typeof window.ResizeObserver === "undefined") {
        window.ResizeObserver = class {
            observe(): void { /* jsdom lays nothing out, so there is nothing to watch. */ }
            unobserve(): void { /* As above. */ }
            disconnect(): void { /* As above. */ }
        } as unknown as typeof ResizeObserver;
    }
});

afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
    document.body.innerHTML = "";
    jest.useRealTimers();
});

test.each([
    ["falls back", new Date(2026, 9, 31, 19, 0), new Date(2026, 10, 1, 8, 0), 14],
    ["springs forward", new Date(2026, 2, 7, 19, 0), new Date(2026, 2, 8, 8, 0), 12]
])("the evening before the clocks %s, the morning item ends at the next day's 8:00", async (_change, evening, morning, hours) => {
    // the run's zone changes its clocks that night, so the cell cannot pass in a zone with no change
    expect(evening.getTimezoneOffset()).not.toBe(morning.getTimezoneOffset());
    expect(morning.getTime() - evening.getTime()).toBe(hours * 3_600_000);
    jest.useFakeTimers({ now: evening, doNotFake: ["nextTick", "queueMicrotask", "setImmediate", "setTimeout", "clearTimeout", "setInterval", "clearInterval", "requestAnimationFrame", "cancelAnimationFrame"] });

    await openMenu({ busy_until: null, appear_offline: false });
    expect(morningItem().textContent!.replace(/\s+/g, " ").trim()).toBe("Until tomorrow, 8:00 AM");
    morningItem().click();
    await settle();
    const told = (wrapper!.emitted("choose") as Array<[StatusChoice]>)[0][0];
    expect(told).toEqual({ busy_until: morning.toISOString(), appear_offline: false });

    // and that end, chosen, is the morning item's check
    await openMenu(told);
    expect(morningItem().getAttribute("aria-checked")).toBe("true");
});
