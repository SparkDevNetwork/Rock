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
// Drafts: what a person typed and has not sent, one per conversation and one per thread, kept on
// this device across a reload for the signed-in person. A draft is written to the device once
// typing pauses, and when the page goes; it is read from the device the first time its place is
// asked for, so the box draws it on its first frame. An emptied box, a send included, removes it.
// Drafts older than 30 days are dropped, and past 50 the oldest go. Rock's sign-out takes away a
// mark the block sets, and a block that finds the mark gone forgets every draft on the device
// before reading any. Storage that is missing, blocked or full keeps drafts for the page only.
import { createDrafts, DraftStorage } from "../../../../../src/Communication/Chat/ChatShell/composables/useDrafts.partial";

const tenant = "T0000001-0000-4000-8000-000000000000";
const jordan = "A0000001-0000-4000-8000-000000000000";
const anna = "a0000002-0000-4000-8000-000000000000";
const team = "c0000001-0000-4000-8000-000000000000";
const youth = "c0000002-0000-4000-8000-000000000000";

const day = 24 * 60 * 60 * 1000;

/** The device's storage as a browser keeps it, with every call counted. */
function memoryStorage(): DraftStorage & { items: Map<string, string>, writes: number } {
    const items = new Map<string, string>();
    const storage = {
        items,
        writes: 0,
        get length(): number {
            return items.size;
        },
        key: (index: number): string | null => Array.from(items.keys())[index] ?? null,
        getItem: (key: string): string | null => items.get(key) ?? null,
        setItem: (key: string, value: string): void => {
            storage.writes++;
            items.set(key, value);
        },
        removeItem: (key: string): void => {
            items.delete(key);
        }
    };
    return storage;
}

/** The sign-out mark, present unless a test says the person signed out. */
function mark(isSet = true): { isSet: () => boolean, set: jest.Mock } {
    let present = isSet;
    return {
        isSet: () => present,
        set: jest.fn(() => {
            present = true;
        })
    };
}

function storedKey(place: string, person = jordan): string {
    return `rock-chat:draft:${tenant.toLowerCase()}:${person.toLowerCase()}:${place}`;
}

let now = Date.UTC(2026, 9, 27, 12);

function drafts(storage: DraftStorage | null, signInMark = mark()): ReturnType<typeof createDrafts> {
    return createDrafts({ storage, tenantId: tenant, personAliasGuid: jordan, signInMark, now: () => now });
}

beforeEach(() => {
    jest.useFakeTimers();
    now = Date.UTC(2026, 9, 27, 12);
});

afterEach(() => {
    jest.useRealTimers();
});

describe("a draft in memory", () => {
    test("belongs to its place: a conversation's text is not another's, nor its thread's", () => {
        const d = drafts(memoryStorage());

        d.write(team, "hello team");

        expect(d.of(team).text).toBe("hello team");
        expect(d.of(youth).text).toBe("");
        expect(d.of(`${team}/40`).text).toBe("");
    });

    test("keeps each chip's name and photo with its text, once per person", () => {
        const d = drafts(memoryStorage());

        d.addMention(team, { type: "mention", text: "@Anna Kim", person_alias_guid: anna, avatar_url: "https://x/anna.jpg" });
        d.addMention(team, { type: "mention", text: "@Anna Kim", person_alias_guid: anna.toUpperCase(), avatar_url: "https://x/anna.jpg" });
        d.write(team, `Thanks <@${anna}>`);

        expect(d.of(team).mentions).toEqual([{ type: "mention", text: "@Anna Kim", person_alias_guid: anna, avatar_url: "https://x/anna.jpg" }]);
    });

    test("takes held text after what is already there, a blank line between", () => {
        const d = drafts(memoryStorage());

        d.append(team, "refused once");
        expect(d.of(team).text).toBe("refused once");

        d.append(team, "refused again");
        expect(d.of(team).text).toBe("refused once\n\nrefused again");
    });
});

describe("a draft on the device", () => {
    test("is written once typing pauses, not on each keystroke", () => {
        const storage = memoryStorage();
        const d = drafts(storage);

        d.write(team, "h");
        d.write(team, "he");
        d.write(team, "hel");
        jest.advanceTimersByTime(299);
        expect(storage.writes).toBe(0);

        jest.advanceTimersByTime(1);
        expect(storage.writes).toBe(1);
        expect(JSON.parse(storage.items.get(storedKey(team)) ?? "null")).toMatchObject({ text: "hel", at: now });
    });

    test("is written at once when the page goes", () => {
        const storage = memoryStorage();
        const listeners = new Map<string, () => void>();
        const page = { addEventListener: (type: string, listener: () => void) => listeners.set(type, listener), removeEventListener: jest.fn() };
        const d = createDrafts({ storage, tenantId: tenant, personAliasGuid: jordan, signInMark: mark(), now: () => now, window: page });

        d.write(team, "hello");
        listeners.get("pagehide")?.();

        expect(JSON.parse(storage.items.get(storedKey(team)) ?? "null")?.text).toBe("hello");
    });

    test("is written at once when the shell stops", () => {
        const storage = memoryStorage();
        const d = drafts(storage);

        d.write(team, "hello");
        d.stop();

        expect(JSON.parse(storage.items.get(storedKey(team)) ?? "null")?.text).toBe("hello");
    });

    test("comes back on the next page, chips and all, read only when its place is first asked for", () => {
        const storage = memoryStorage();
        const first = drafts(storage);
        first.addMention(team, { type: "mention", text: "@Anna Kim", person_alias_guid: anna, avatar_url: null });
        first.write(team, `Thanks <@${anna}>`);
        first.stop();

        const read = jest.spyOn(storage, "getItem");
        const next = drafts(storage);
        expect(read).not.toHaveBeenCalled();

        expect(next.of(team)).toEqual({ text: `Thanks <@${anna}>`, mentions: [{ type: "mention", text: "@Anna Kim", person_alias_guid: anna, avatar_url: null }] });
        next.of(team);
        expect(read).toHaveBeenCalledTimes(1);
    });

    test("keeps only the chips its text still holds", () => {
        const storage = memoryStorage();
        const d = drafts(storage);
        d.addMention(team, { type: "mention", text: "@Anna Kim", person_alias_guid: anna, avatar_url: null });
        d.addMention(team, { type: "mention", text: "@all", all: true });
        d.write(team, "<@all> rehearsal moved");
        d.stop();

        expect(JSON.parse(storage.items.get(storedKey(team)) ?? "null")?.mentions).toEqual([{ type: "mention", text: "@all", all: true }]);
    });

    test("is removed when the box is emptied, as a send empties it", () => {
        const storage = memoryStorage();
        const d = drafts(storage);
        d.write(team, "hello");
        jest.advanceTimersByTime(300);

        d.write(team, "");
        jest.advanceTimersByTime(300);

        expect(storage.items.has(storedKey(team))).toBe(false);
    });

    test("of white space alone is no draft", () => {
        const storage = memoryStorage();
        const d = drafts(storage);
        d.write(team, "  \n ");
        d.stop();

        expect(storage.items.has(storedKey(team))).toBe(false);
    });

    test("is kept for its person: another person on the device does not see it", () => {
        const storage = memoryStorage();
        const d = drafts(storage);
        d.write(team, "hello");
        d.stop();

        const other = createDrafts({ storage, tenantId: tenant, personAliasGuid: anna, signInMark: mark(), now: () => now });
        expect(other.of(team).text).toBe("");
    });

    test("older than 30 days is dropped as it is read", () => {
        const storage = memoryStorage();
        storage.items.set(storedKey(team), JSON.stringify({ text: "old", mentions: [], at: now - 31 * day }));

        expect(drafts(storage).of(team).text).toBe("");
        expect(storage.items.has(storedKey(team))).toBe(false);
    });

    test("past 50, the oldest go, and any older than 30 days, when the page first writes one", () => {
        const storage = memoryStorage();
        for (let i = 0; i < 50; i++) {
            storage.items.set(storedKey(`room-${i}`), JSON.stringify({ text: `draft ${i}`, mentions: [], at: now - (50 - i) * 60_000 }));
        }
        storage.items.set(storedKey("stale"), JSON.stringify({ text: "stale", mentions: [], at: now - 40 * day }));
        storage.items.set("rock-chat:last-channel:x:y", team);

        const d = drafts(storage);
        d.write(team, "newest");
        jest.advanceTimersByTime(300);

        const kept = Array.from(storage.items.keys()).filter(key => key.startsWith("rock-chat:draft:"));
        expect(kept).toHaveLength(50);
        expect(kept).toContain(storedKey(team));
        expect(kept).not.toContain(storedKey("room-0"));
        expect(kept).not.toContain(storedKey("stale"));
        expect(storage.items.get("rock-chat:last-channel:x:y")).toBe(team);
    });
});

describe("after Rock's sign-out", () => {
    test("every draft on the device is forgotten before any is read, and the mark is set again", () => {
        const storage = memoryStorage();
        storage.items.set(storedKey(team), JSON.stringify({ text: "the last person's", mentions: [], at: now }));
        storage.items.set(storedKey(team, anna), JSON.stringify({ text: "someone else's", mentions: [], at: now }));
        storage.items.set("rock-chat:last-channel:x:y", team);
        const signInMark = mark(false);

        const d = drafts(storage, signInMark);

        expect(Array.from(storage.items.keys())).toEqual(["rock-chat:last-channel:x:y"]);
        expect(signInMark.set).toHaveBeenCalledTimes(1);
        expect(d.of(team).text).toBe("");
    });

    test("with the mark kept, nothing is forgotten", () => {
        const storage = memoryStorage();
        storage.items.set(storedKey(team), JSON.stringify({ text: "mine", mentions: [], at: now }));

        expect(drafts(storage, mark(true)).of(team).text).toBe("mine");
    });
});

describe("another person signed in on this device", () => {
    test("forgets every draft not this person's, with the mark still set, as a sign-out Rock never saw", () => {
        const storage = memoryStorage();
        storage.items.set(storedKey(team, anna), JSON.stringify({ text: "the last person's", mentions: [], at: now }));
        storage.items.set(`rock-chat:draft:other-church:${jordan.toLowerCase()}:${team}`, JSON.stringify({ text: "another church's", mentions: [], at: now }));
        storage.items.set(storedKey(youth), JSON.stringify({ text: "mine", mentions: [], at: now }));
        storage.items.set("rock-chat:last-channel:x:y", team);

        const d = drafts(storage, mark(true));

        expect(Array.from(storage.items.keys()).sort()).toEqual(["rock-chat:last-channel:x:y", storedKey(youth)].sort());
        expect(d.of(youth).text).toBe("mine");
    });
});

describe("in two tabs of the same person", () => {
    /** The page's window, whose storage events a test sends as another tab's writes arrive. */
    function page(): { addEventListener: jest.Mock, removeEventListener: jest.Mock, send: (key: string | null, newValue: string | null) => void } {
        const listeners = new Map<string, Set<(event: unknown) => void>>();
        return {
            addEventListener: jest.fn((type: string, listener: (event: unknown) => void) => {
                if (!listeners.has(type)) {
                    listeners.set(type, new Set());
                }
                listeners.get(type)?.add(listener);
            }),
            removeEventListener: jest.fn((type: string, listener: (event: unknown) => void) => {
                listeners.get(type)?.delete(listener);
            }),
            send: (key, newValue) => listeners.get("storage")?.forEach(listener => listener({ key, newValue }))
        };
    }

    function tab(storage: DraftStorage, window: ReturnType<typeof page>): ReturnType<typeof createDrafts> {
        return createDrafts({ storage, tenantId: tenant, personAliasGuid: jordan, signInMark: mark(), window, now: () => now });
    }

    test("a draft the other tab changes is shown here while its box is not focused", () => {
        const storage = memoryStorage();
        const window = page();
        const d = tab(storage, window);
        expect(d.of(team).text).toBe("");

        window.send(storedKey(team), JSON.stringify({ text: `from the other tab <@${anna}>`, mentions: [{ type: "mention", text: "@Anna Kim", person_alias_guid: anna }], at: now }));

        expect(d.of(team).text).toBe(`from the other tab <@${anna}>`);
        expect(d.of(team).mentions).toEqual([{ type: "mention", text: "@Anna Kim", person_alias_guid: anna }]);
        expect(storage.writes).toBe(0);
    });

    test("a draft the other tab sent is dropped here, so it is never sent twice", () => {
        const storage = memoryStorage();
        storage.items.set(storedKey(team), JSON.stringify({ text: "hello", mentions: [], at: now }));
        const window = page();
        const d = tab(storage, window);
        expect(d.of(team).text).toBe("hello");

        storage.items.delete(storedKey(team));
        window.send(storedKey(team), null);

        expect(d.of(team).text).toBe("");
    });

    test("the box being typed in keeps its text, and its next write wins", () => {
        const storage = memoryStorage();
        const window = page();
        const d = tab(storage, window);
        d.focus(team);
        d.write(team, "typing here");

        window.send(storedKey(team), JSON.stringify({ text: "from the other tab", mentions: [], at: now }));
        expect(d.of(team).text).toBe("typing here");

        jest.advanceTimersByTime(300);
        expect(JSON.parse(storage.items.get(storedKey(team)) ?? "null")?.text).toBe("typing here");
    });

    test("once the box loses focus, the other tab's changes show again", () => {
        const storage = memoryStorage();
        const window = page();
        const d = tab(storage, window);
        expect(d.of(team).text).toBe("");
        d.focus(team);
        d.focus(null);

        window.send(storedKey(team), JSON.stringify({ text: "from the other tab", mentions: [], at: now }));

        expect(d.of(team).text).toBe("from the other tab");
    });

    test("another person's or another place's keys change nothing here", () => {
        const storage = memoryStorage();
        const window = page();
        const d = tab(storage, window);
        d.write(team, "mine");

        window.send(storedKey(team, anna), JSON.stringify({ text: "anna's", mentions: [], at: now }));
        window.send("rock-chat:last-channel:x:y", youth);

        expect(d.of(team).text).toBe("mine");
    });

    test("keeps at most 50 between them: the oldest go by what storage holds, not what this tab wrote", () => {
        const storage = memoryStorage();
        const d = tab(storage, page());
        d.write(team, "first");
        jest.advanceTimersByTime(300);

        // The other tab writes fifty more, each newer than this tab's first.
        for (let i = 0; i < 50; i++) {
            storage.items.set(storedKey(`room-${i}`), JSON.stringify({ text: `draft ${i}`, mentions: [], at: now + i + 1 }));
        }
        now += 100;
        d.write(youth, "second");
        jest.advanceTimersByTime(300);

        const kept = Array.from(storage.items.keys()).filter(key => key.startsWith("rock-chat:draft:"));
        expect(kept).toHaveLength(50);
        expect(kept).toContain(storedKey(youth));
        expect(kept).not.toContain(storedKey(team));
        expect(kept).not.toContain(storedKey("room-0"));
    });

    test("stops listening when the drafts stop", () => {
        const window = page();
        const d = tab(memoryStorage(), window);

        d.stop();

        expect(window.removeEventListener).toHaveBeenCalledWith("storage", expect.any(Function));
    });
});

describe("without the device's storage", () => {
    test("drafts are kept for the page when storage refuses every call", () => {
        const refusing: DraftStorage = {
            get length(): number {
                throw new Error("blocked");
            },
            key: () => {
                throw new Error("blocked");
            },
            getItem: () => {
                throw new Error("blocked");
            },
            setItem: () => {
                throw new Error("full");
            },
            removeItem: () => {
                throw new Error("blocked");
            }
        };
        const d = drafts(refusing, mark(false));

        expect(() => {
            d.write(team, "hello");
            jest.advanceTimersByTime(300);
            d.stop();
        }).not.toThrow();
        expect(d.of(team).text).toBe("hello");
    });

    test("drafts are kept for the page where there is no storage or no sign-out mark", () => {
        const storage = memoryStorage();
        const d = createDrafts({ storage, tenantId: tenant, personAliasGuid: jordan, signInMark: null, now: () => now });

        d.write(team, "hello");
        d.stop();

        expect(d.of(team).text).toBe("hello");
        expect(storage.writes).toBe(0);
        expect(drafts(null).of(team).text).toBe("");
    });
});
