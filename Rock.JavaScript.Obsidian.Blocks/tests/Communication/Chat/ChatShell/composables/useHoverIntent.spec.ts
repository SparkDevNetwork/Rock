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
// The timing a hover-only control of a list of rows shares between the message action bar and the
// members list's Message button: it shows once the pointer has rested, with a fade; follows the
// pointer from row to row at once; and, when the pointer is on no row, fades out after a grace.
// The composable tests are written to, in useHoverIntent.partial.ts:
//
//   useHoverIntent<TKey>(timingElement): { shownKey, isShown, isInstant, follow, show, hide, stop }

import { useHoverIntent } from "../../../../../src/Communication/Chat/ChatShell/composables/useHoverIntent.partial";

describe("useHoverIntent", () => {
    beforeEach(() => jest.useFakeTimers());
    afterEach(() => jest.useRealTimers());

    function setup(): ReturnType<typeof useHoverIntent<number>> {
        // no element, so the rest and grace are the design's own, 150 and 220 ms
        return useHoverIntent<number>(() => null);
    }

    test("shows on a row only once the pointer has rested on it for 150 ms, with a fade", () => {
        const hover = setup();

        hover.follow(1);
        jest.advanceTimersByTime(149);
        expect(hover.isShown.value).toBe(false);

        jest.advanceTimersByTime(1);
        expect(hover.isShown.value).toBe(true);
        expect(hover.shownKey.value).toBe(1);
        expect(hover.isInstant.value).toBe(false);
    });

    test("a pointer that leaves before resting shows nothing", () => {
        const hover = setup();

        hover.follow(1);
        jest.advanceTimersByTime(100);
        hover.follow(null);
        jest.advanceTimersByTime(1000);

        expect(hover.isShown.value).toBe(false);
        expect(hover.shownKey.value).toBeNull();
    });

    test("a move to another row while shown is at once and instant, with no rest", () => {
        const hover = setup();
        hover.show(1);

        hover.follow(2);

        expect(hover.shownKey.value).toBe(2);
        expect(hover.isShown.value).toBe(true);
        expect(hover.isInstant.value).toBe(true);
    });

    test("with the pointer on no row, a shown control stays for 220 ms and then fades", () => {
        const hover = setup();
        hover.show(1);

        hover.follow(null);
        jest.advanceTimersByTime(219);
        expect(hover.isShown.value).toBe(true);

        jest.advanceTimersByTime(1);
        expect(hover.isShown.value).toBe(false);
        expect(hover.isInstant.value).toBe(false);
        expect(hover.shownKey.value).toBe(1);
    });

    test("a pointer back on a row within the grace keeps the control, and the grace does not hide it later", () => {
        const hover = setup();
        hover.show(1);

        hover.follow(null);
        jest.advanceTimersByTime(200);
        hover.follow(2);
        jest.advanceTimersByTime(1000);

        expect(hover.isShown.value).toBe(true);
        expect(hover.shownKey.value).toBe(2);
    });

    // A move skips the fade, but leaving always fades, however the control got to where it is.
    test("withdrawing the pointer after a move fades", () => {
        const hover = setup();
        hover.show(1);
        hover.follow(2);
        expect(hover.isInstant.value).toBe(true);

        hover.follow(null);
        jest.advanceTimersByTime(220);

        expect(hover.isShown.value).toBe(false);
        expect(hover.isInstant.value).toBe(false);
    });

    test("hide skips the fade only when asked to", () => {
        const hover = setup();

        hover.show(1);
        hover.hide(true);
        expect(hover.isShown.value).toBe(false);
        expect(hover.isInstant.value).toBe(true);

        hover.show(1);
        expect(hover.isInstant.value).toBe(false);
        hover.hide(false);
        expect(hover.isInstant.value).toBe(false);
    });

    test("stop clears a rest that is pending", () => {
        const hover = setup();

        hover.follow(1);
        hover.stop();
        jest.advanceTimersByTime(1000);

        expect(hover.isShown.value).toBe(false);
    });

    test("stop clears a grace that is pending", () => {
        const hover = setup();
        hover.show(1);

        hover.follow(null);
        hover.stop();
        jest.advanceTimersByTime(1000);

        expect(hover.isShown.value).toBe(true);
    });
});
