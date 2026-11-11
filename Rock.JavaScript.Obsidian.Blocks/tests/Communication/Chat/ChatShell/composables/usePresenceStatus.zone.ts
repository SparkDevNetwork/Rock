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
// Others' online status across a daylight saving change, in the zone the zone run pins: the wake
// for Busy's end words comes at the viewer's next midnight by the calendar, not 24 hours after the
// last, so the 25-hour day that leaves daylight saving turns the words once, at its end.

import { computed } from "vue";
import { createPresenceStatus } from "../../../../../src/Communication/Chat/ChatShell/composables/usePresenceStatus.partial";
import { readSettings } from "../../../../../src/Communication/Chat/ChatShell/composables/useSession.partial";
import { busy, readerPerson, serverNow } from "../statusShapes";

const me = "a0000007-0000-4000-8000-000000000000";
const anna = "a0000001-0000-4000-8000-000000000000";

beforeEach(() => {
    jest.useFakeTimers();
});

afterEach(() => {
    jest.useRealTimers();
});

test("a Busy ending two days on, told just after midnight on the day daylight saving ends, reads tomorrow from the next midnight, waking once", () => {
    // the run's zone leaves daylight saving that night, so the cell cannot pass in a zone with no change
    expect(new Date(2026, 10, 1, 0, 30).getTimezoneOffset()).not.toBe(new Date(2026, 10, 2, 12).getTimezoneOffset());
    const start = new Date(2026, 10, 1, 0, 30).getTime();
    const midnight = new Date(2026, 10, 2).getTime();
    expect(midnight - start).toBe(24.5 * 3_600_000);

    jest.setSystemTime(start);
    const page = { visibilityState: "visible", addEventListener: () => undefined, removeEventListener: () => undefined };
    const status = createPresenceStatus({
        settings: () => readSettings({ flags: { online: true } }),
        personAliasGuid: me,
        me: () => ({ busy_until: null, appear_offline: false }),
        isBlocked: () => false,
        isStopped: () => false,
        targets: { document: page, window: page },
        leave: () => undefined,
        session: { refresh: async () => true, nextRefreshAt: () => null, tokenLifeSeconds: () => 300, inFlight: () => null }
    });
    status.clockFrom(start / 1000);
    const end = new Date(2026, 10, 3, 8, 0).getTime() - serverNow;
    // a wake that finds its midnight still ahead sets itself again at once, so a storm is stopped here rather than hanging the run
    const setTimer = global.setTimeout;
    const timers = jest.spyOn(global, "setTimeout").mockImplementation(((handler: () => void, ms?: number) => {
        if (timers.mock.calls.length > 10) {
            throw new Error("the status timer was set again and again");
        }
        return setTimer(handler, ms);
    }) as typeof setTimeout);
    // a row that lasts past the change, so only the words can move
    status.told([readerPerson(anna, busy(end, start - serverNow, end))]);
    const drawn = computed(() => status.statusOf(anna));
    expect(drawn.value?.tip).toMatch(/^Busy until Nov 3, 8:00\sAM$/u);

    jest.advanceTimersByTime(midnight - start - 1);
    expect(drawn.value?.tip).toMatch(/^Busy until Nov 3, 8:00\sAM$/u);
    jest.advanceTimersByTime(1);

    expect(drawn.value?.tip).toMatch(/^Busy until tomorrow, 8:00\sAM$/u);
    // one at the write and one at the wake: a midnight counted as 24 hours would wake an hour early and again and again
    expect(timers).toHaveBeenCalledTimes(2);
    timers.mockRestore();
    status.stop();
});

test("her own Busy ending two days on, chosen just after midnight on the day daylight saving ends, reads tomorrow from the next midnight with no request", () => {
    const start = new Date(2026, 10, 1, 0, 30).getTime();
    const midnight = new Date(2026, 10, 2).getTime();
    jest.setSystemTime(start);
    const page = { visibilityState: "visible", addEventListener: () => undefined, removeEventListener: () => undefined };
    const refreshes: string[] = [];
    const status = createPresenceStatus({
        settings: () => readSettings({ flags: { online: true } }),
        personAliasGuid: me,
        me: () => ({ busy_until: new Date(2026, 10, 3, 8, 0).toISOString(), appear_offline: false }),
        isBlocked: () => false,
        isStopped: () => false,
        targets: { document: page, window: page },
        leave: () => undefined,
        session: {
            refresh: async reason => {
                refreshes.push(reason);
                return true;
            },
            nextRefreshAt: () => null,
            tokenLifeSeconds: () => 300,
            inFlight: () => null
        }
    });
    status.clockFrom(start / 1000);
    const setTimer = global.setTimeout;
    const timers = jest.spyOn(global, "setTimeout").mockImplementation(((handler: () => void, ms?: number) => {
        if (timers.mock.calls.length > 10) {
            throw new Error("the status timer was set again and again");
        }
        return setTimer(handler, ms);
    }) as typeof setTimeout);
    const drawn = computed(() => status.statusOf(me));
    expect(drawn.value?.tip).toMatch(/^Busy, notifications paused until Nov 3, 8:00\sAM$/u);

    jest.advanceTimersByTime(midnight - start - 1);
    expect(drawn.value?.tip).toMatch(/^Busy, notifications paused until Nov 3, 8:00\sAM$/u);
    jest.advanceTimersByTime(1);

    expect(drawn.value?.tip).toMatch(/^Busy, notifications paused until tomorrow, 8:00\sAM$/u);
    expect(refreshes).toEqual([]);
    timers.mockRestore();
    status.stop();
});
