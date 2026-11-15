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
// What the device tells the page about itself, read in one place: whether the person asked for less
// motion, and whether the pointer can hover. A part asks at the moment it decides (before a scroll,
// a focus, an animation), or follows the answer live where what is drawn must change with it. A
// browser or test page without matchMedia answers no to both, as a page with no preference would.
import { effectScope, nextTick } from "vue";
import { isHoverless, isMotionReduced, useIsMotionReduced } from "../../../../../src/Communication/Chat/ChatShell/ui/viewport.partial";

type Listener = (event: { matches: boolean }) => void;

/** A page whose media answers are set by the test, with the listeners each query was given. */
function fakeMedia(answers: Record<string, boolean>): { listeners: Map<string, Listener[]>, removed: Listener[] } {
    const listeners = new Map<string, Listener[]>();
    const removed: Listener[] = [];
    window.matchMedia = ((query: string) => ({
        media: query,
        get matches() {
            return answers[query] ?? false;
        },
        addEventListener: (_type: string, listener: Listener) => listeners.set(query, [...(listeners.get(query) ?? []), listener]),
        removeEventListener: (_type: string, listener: Listener) => removed.push(listener)
    })) as unknown as typeof window.matchMedia;
    return { listeners, removed };
}

const original = window.matchMedia;

afterEach(() => {
    window.matchMedia = original;
});

describe("asking at the moment of deciding", () => {
    test("less motion is asked for exactly when the page's reduced-motion query matches", () => {
        fakeMedia({ "(prefers-reduced-motion: reduce)": true });
        expect(isMotionReduced()).toBe(true);

        fakeMedia({ "(prefers-reduced-motion: reduce)": false });
        expect(isMotionReduced()).toBe(false);
    });

    test("a pointer that cannot hover, a finger, is told by the hover-none query alone", () => {
        fakeMedia({ "(hover: none)": true });
        expect(isHoverless()).toBe(true);
        expect(isMotionReduced()).toBe(false);

        fakeMedia({ "(prefers-reduced-motion: reduce)": true });
        expect(isHoverless()).toBe(false);
    });

    test("each ask reads the page as it is now, so a setting changed between two asks is seen", () => {
        const answers = { "(prefers-reduced-motion: reduce)": false };
        fakeMedia(answers);
        expect(isMotionReduced()).toBe(false);

        answers["(prefers-reduced-motion: reduce)"] = true;
        expect(isMotionReduced()).toBe(true);
    });

    test("a page without matchMedia answers no to both, as a page with no preference does", () => {
        delete (window as { matchMedia?: unknown }).matchMedia;

        expect(isMotionReduced()).toBe(false);
        expect(isHoverless()).toBe(false);
    });
});

describe("following less motion live", () => {
    test("the flag starts at the page's answer and moves when the setting is turned on with the chat open", async () => {
        const { listeners } = fakeMedia({ "(prefers-reduced-motion: reduce)": false });
        const scope = effectScope();
        const isReduced = scope.run(() => useIsMotionReduced())!;
        expect(isReduced.value).toBe(false);

        listeners.get("(prefers-reduced-motion: reduce)")!.forEach(listener => listener({ matches: true }));
        await nextTick();

        expect(isReduced.value).toBe(true);
        scope.stop();
    });

    test("the listener ends with the part that asked, so a closed part is never told again", () => {
        const { listeners, removed } = fakeMedia({});
        const scope = effectScope();
        scope.run(() => useIsMotionReduced());
        const added = listeners.get("(prefers-reduced-motion: reduce)")!;

        scope.stop();

        expect(removed).toEqual(added);
    });

    test("a page whose media list cannot be listened to still gives its first answer", () => {
        window.matchMedia = ((query: string) => ({ media: query, matches: true })) as unknown as typeof window.matchMedia;
        const scope = effectScope();

        const isReduced = scope.run(() => useIsMotionReduced())!;

        expect(isReduced.value).toBe(true);
        expect(() => scope.stop()).not.toThrow();
    });

    test("a page without matchMedia follows nothing and reads no", () => {
        delete (window as { matchMedia?: unknown }).matchMedia;
        const scope = effectScope();

        const isReduced = scope.run(() => useIsMotionReduced())!;

        expect(isReduced.value).toBe(false);
        scope.stop();
    });
});
