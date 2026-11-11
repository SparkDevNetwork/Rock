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
// The words a status is read in: the few a screen reader hears after a name, the tooltip and the
// card's line, the person's own words for their own state, and the line a sender reads above the
// box while the other person is Busy. One set of functions writes them all, so a label, a tip and
// a card never say a person's state two ways. A time is the viewer's own clock: the same day reads
// as a time, the next day as tomorrow, and a later day by its month and day.
import { busyNoteWords, ownStatusWords, statusWords } from "../../../../../src/Communication/Chat/ChatShell/ui/face.partial";

const locale = "en-US";

/** Times as a person reads them; some ICU builds put a narrow no-break space before AM and PM. */
function plain<T extends Record<string, string | null>>(words: T): T {
    return Object.fromEntries(Object.entries(words).map(([key, value]) => [key, value === null ? null : value.replace(/\s+/g, " ")])) as T;
}

/** Tuesday, November 10, 2026, 9 p.m. in the viewer's own time. */
const evening = new Date(2026, 10, 10, 21, 0);

describe("someone else's status in words", () => {
    test("online", () => {
        expect(statusWords("online", null, evening, locale)).toEqual({ label: ", online", tip: "Online" });
    });

    test("away", () => {
        expect(statusWords("away", null, evening, locale)).toEqual({ label: ", away", tip: "Away" });
    });

    test("Busy until a time the same day", () => {
        const afternoon = new Date(2026, 10, 10, 12, 0);

        expect(plain(statusWords("busy", new Date(2026, 10, 10, 15, 0), afternoon, locale))).toEqual({ label: ", busy until 3:00 PM", tip: "Busy until 3:00 PM" });
    });

    test("Busy until tomorrow morning", () => {
        expect(plain(statusWords("busy", new Date(2026, 10, 11, 8, 0), evening, locale))).toEqual({ label: ", busy until tomorrow, 8:00 AM", tip: "Busy until tomorrow, 8:00 AM" });
    });

    test("Busy until a day past tomorrow, by its month and day", () => {
        expect(plain(statusWords("busy", new Date(2026, 10, 12, 8, 0), evening, locale))).toEqual({ label: ", busy until Nov 12, 8:00 AM", tip: "Busy until Nov 12, 8:00 AM" });
    });

    test("Busy with no end", () => {
        expect(statusWords("busy", null, evening, locale)).toEqual({ label: ", busy", tip: "Busy" });
    });

    test("at 1 a.m. an end of 8:00 that morning is the same day, not tomorrow", () => {
        const night = new Date(2026, 10, 11, 1, 0);

        expect(plain(statusWords("busy", new Date(2026, 10, 11, 8, 0), night, locale)).tip).toBe("Busy until 8:00 AM");
    });
});

describe("the person's own status in words", () => {
    test.each([
        ["online", "Online"],
        ["away", "Away"],
        ["busy", "Busy, notifications paused"],
        ["hidden", "Appear offline"]
    ] as const)("%s with no end reads %s", (state, words) => {
        expect(ownStatusWords(state, null, evening, locale)).toBe(words);
    });

    test("Busy with an end says when notifications come back", () => {
        expect(ownStatusWords("busy", new Date(2026, 10, 11, 8, 0), evening, locale).replace(/\s+/g, " ")).toBe("Busy, notifications paused until tomorrow, 8:00 AM");
    });
});

describe("the line above the box while the other person is Busy", () => {
    const afternoon = new Date(2026, 10, 10, 12, 0);

    test("names her, the end, and that she won't be notified, in parts the line can shorten one at a time", () => {
        expect(plain(busyNoteWords("Cindy", new Date(2026, 10, 10, 15, 0), afternoon, locale))).toEqual({
            name: "Cindy",
            until: " until 3:00 PM",
            sentence: "Cindy is busy until 3:00 PM and won't be notified."
        });
    });

    test("with no end", () => {
        expect(busyNoteWords("Grace", null, afternoon, locale)).toEqual({
            name: "Grace",
            until: null,
            sentence: "Grace is busy and won't be notified."
        });
    });

    test("until tomorrow", () => {
        expect(plain(busyNoteWords("Alexandrina-Josephine", new Date(2026, 10, 11, 8, 0), afternoon, locale)).sentence)
            .toBe("Alexandrina-Josephine is busy until tomorrow, 8:00 AM and won't be notified.");
    });
});
