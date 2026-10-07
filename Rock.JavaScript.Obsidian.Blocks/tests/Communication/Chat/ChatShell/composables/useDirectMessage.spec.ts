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
// Starting a direct message: the New message button for a person who may start one, a picker of
// up to eight people from the platform's search, the conversation those people already have or a
// draft, and the draft's first message, which is what makes Rock create the conversation.
import {
    createDirectMessages,
    DirectMessageDependencies,
    DoorResult,
    isNewMessageShown,
    isTextKeptOnSend,
    maxOthers,
    PersonRow
} from "../../../../../src/Communication/Chat/ChatShell/composables/useDirectMessage.partial";
import { ChatError } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    const promise = new Promise<T>(r => resolve = r);
    return { promise, resolve };
}

function person(n: number): PersonRow {
    return {
        person_alias_guid: `a000000${n.toString(16)}-0000-4000-8000-000000000000`,
        nick_name: `Person${n}`,
        last_name: "Picked",
        avatar_url: null
    };
}

const existing = "c0000003-0000-4000-8000-000000000000";
const created = "c0000009-0000-4000-8000-000000000000";

/** How a first message the platform has not taken yet waits before it is tried again. */
type Waiting = {
    /** Waits, so a test can say when. */
    sleep: (ms: number) => Promise<void>;

    /** The first wait and the longest. */
    retryDelay: () => { baseMs: number, capMs: number };

    /** Spreads the waits of many clients apart. */
    random: () => number;
};

/** A recorder of every call the module makes outside itself, in order. */
function setup(overrides: Partial<DirectMessageDependencies & Waiting> = {}): {
    calls: string[],
    dependencies: DirectMessageDependencies
} {
    const calls: string[] = [];
    const dependencies: DirectMessageDependencies & Partial<Waiting> = {
        search: async query => {
            calls.push(`search ${query}`);
            return { ok: true, people: [person(1), person(2)] };
        },
        findConversation: async people => {
            calls.push(`find ${people.length}`);
            return { ok: true, channelId: null };
        },
        startConversation: async people => {
            calls.push(`start ${people.length}`);
            return { code: "ok", channelGuid: created, isPending: false, message: null, personAliasGuid: null };
        },
        openChannel: channelId => {
            calls.push(`open ${channelId}`);
            return true;
        },
        send: async (channelId, body) => {
            calls.push(`send ${channelId} ${body}`);
            return true;
        },
        ...overrides
    };

    return { calls, dependencies };
}

describe("the New message button", () => {
    test("shows only for a person who may start direct messages", () => {
        expect(isNewMessageShown({ canStartDm: true })).toBe(true);
        expect(isNewMessageShown({ canStartDm: false })).toBe(false);
    });
});

describe("the picker", () => {
    test("lists exactly the people the platform's search answers", async () => {
        const { dependencies } = setup();
        const dm = createDirectMessages(dependencies);

        expect(await dm.search("pe")).toEqual([person(1), person(2)]);
    });

    test("takes at most eight people", () => {
        const { dependencies } = setup();
        const dm = createDirectMessages(dependencies);

        for (let n = 1; n <= maxOthers; n++) {
            expect(dm.choose(person(n))).toBe(true);
        }

        expect(dm.choose(person(9))).toBe(false);
        expect(dm.chosen.length).toBe(8);
        expect(maxOthers).toBe(8);
    });

    test("a person chosen twice is chosen once", () => {
        const { dependencies } = setup();
        const dm = createDirectMessages(dependencies);

        dm.choose(person(1));
        dm.choose(person(1));

        expect(dm.chosen).toEqual([person(1)]);
    });
});

describe("opening", () => {
    test("people who already have a conversation open it", async () => {
        const { calls, dependencies } = setup({
            findConversation: async people => {
                calls.push(`find ${people.length}`);
                return { ok: true, channelId: existing };
            }
        });
        const dm = createDirectMessages(dependencies);
        dm.choose(person(1));

        await dm.open();

        expect(calls).toEqual(["find 1", `open ${existing}`]);
        expect(dm.draft).toBeNull();
    });

    test("people with no conversation get a draft, and nothing is created", async () => {
        const { calls, dependencies } = setup();
        const dm = createDirectMessages(dependencies);
        dm.choose(person(1));
        dm.choose(person(2));

        await dm.open();

        expect(calls).toEqual(["find 2"]);
        expect(dm.draft?.people).toEqual([person(1), person(2)]);
    });
});

describe("the draft's first message", () => {
    async function draftOf(dependencies: DirectMessageDependencies): Promise<ReturnType<typeof createDirectMessages>> {
        const dm = createDirectMessages(dependencies);
        dm.choose(person(1));
        await dm.open();
        return dm;
    }

    test("asks Rock for the conversation, then opens it, then sends", async () => {
        const { calls, dependencies } = setup();
        const dm = await draftOf(dependencies);
        calls.length = 0;

        expect(await dm.sendFirst("hello")).toBe(true);

        expect(calls).toEqual(["start 1", `open ${created}`, `send ${created} hello`]);
        expect(dm.draft).toBeNull();
    });

    test("blank text asks Rock for nothing", async () => {
        const { calls, dependencies } = setup();
        const dm = await draftOf(dependencies);
        calls.length = 0;

        expect(await dm.sendFirst("   ")).toBe(false);
        expect(calls).toEqual([]);
    });

    test("a conversation the platform has not taken yet keeps the text and sends when the person's membership arrives", async () => {
        const { calls, dependencies } = setup({
            startConversation: async people => {
                calls.push(`start ${people.length}`);
                return { code: "ok", channelGuid: created, isPending: true, message: null, personAliasGuid: null };
            }
        });
        const dm = await draftOf(dependencies);
        calls.length = 0;

        expect(await dm.sendFirst("hello")).toBe(false);
        expect(calls).toEqual(["start 1"]);
        expect(dm.draft?.body).toBe("hello");
        expect(dm.draft?.status).toBe("starting");

        await dm.membershipChanged();

        expect(calls).toEqual(["start 1", `open ${created}`, `send ${created} hello`]);
        expect(dm.draft).toBeNull();
    });

    test("a membership signal with no conversation starting does nothing", async () => {
        const { calls, dependencies } = setup();
        const dm = await draftOf(dependencies);
        calls.length = 0;

        await dm.membershipChanged();

        expect(calls).toEqual([]);
    });

    test("a refusal shows Rock's sentence, sends nothing and keeps the draft", async () => {
        const refusal: DoorResult = {
            code: "door.target_not_eligible",
            channelGuid: null,
            isPending: false,
            message: "Person1 can't be messaged.",
            personAliasGuid: person(1).person_alias_guid
        };
        const { calls, dependencies } = setup({
            startConversation: async people => {
                calls.push(`start ${people.length}`);
                return refusal;
            }
        });
        const dm = await draftOf(dependencies);
        calls.length = 0;

        expect(await dm.sendFirst("hello")).toBe(false);

        expect(calls).toEqual(["start 1"]);
        expect(dm.error).toBe("Person1 can't be messaged.");
        expect(dm.draft?.body).toBe("hello");
        expect(dm.draft?.status).toBe("draft");
    });

    test("a second send while Rock is still answering the first asks for nothing more", async () => {
        const answer = deferred<DoorResult>();
        const { calls, dependencies } = setup({
            startConversation: people => {
                calls.push(`start ${people.length}`);
                return answer.promise;
            }
        });
        const dm = await draftOf(dependencies);
        calls.length = 0;

        const first = dm.sendFirst("hello");
        expect(await dm.sendFirst("hello again")).toBe(false);

        answer.resolve({ code: "ok", channelGuid: created, isPending: false, message: null, personAliasGuid: null });
        expect(await first).toBe(true);

        expect(calls).toEqual(["start 1", `open ${created}`, `send ${created} hello`]);
    });

    test("the composer keeps a draft's first message in its box, and empties it for an ordinary send", async () => {
        const { dependencies } = setup();
        const dm = await draftOf(dependencies);

        // A refused first message has no failed row in a feed to keep it on, so the box keeps it
        // until the draft goes away; an ordinary send's failure stays on its own row.
        expect(isTextKeptOnSend(dm.draft)).toBe(true);
        expect(isTextKeptOnSend(null)).toBe(false);
    });

    test("the same text refused twice is still the draft's text after the second refusal", async () => {
        const { dependencies } = setup({
            startConversation: async () => ({ code: "door.unavailable", channelGuid: null, isPending: false, message: "Try again.", personAliasGuid: null })
        });
        const dm = await draftOf(dependencies);

        expect(await dm.sendFirst("hello")).toBe(false);
        expect(await dm.sendFirst("hello")).toBe(false);

        expect(dm.draft?.body).toBe("hello");
        expect(dm.draft?.status).toBe("draft");
        expect(isTextKeptOnSend(dm.draft)).toBe(true);
    });
});

// Waits stand in for the retry's backoff; each resolves when the test lets it, and each records
// how long it was asked to be.
function waits(): Waiting & { list: Array<Deferred<void>>, asked: number[] } {
    const list: Array<Deferred<void>> = [];
    const asked: number[] = [];
    return {
        list,
        asked,
        sleep: (ms: number): Promise<void> => {
            const wait = deferred<void>();
            asked.push(ms);
            list.push(wait);
            return wait.promise;
        },
        retryDelay: () => ({ baseMs: 1000, capMs: 30000 }),
        random: () => 1
    };
}

async function settle(): Promise<void> {
    for (let i = 0; i < 20; i++) {
        await new Promise(r => setTimeout(r, 0));
    }
}

/** A door that made the conversation in Rock before the platform took it. */
function pendingDoor(calls: string[]): (people: string[]) => Promise<DoorResult> {
    return async people => {
        calls.push(`start ${people.length}`);
        return { code: "ok", channelGuid: created, isPending: true, message: null, personAliasGuid: null };
    };
}

const sends = (calls: string[]): string[] => calls.filter(c => c.startsWith("send "));
const starts = (calls: string[]): string[] => calls.filter(c => c.startsWith("start "));

describe("a first message the platform has not taken yet", () => {
    async function pendingDraft(dependencies: DirectMessageDependencies): Promise<ReturnType<typeof createDirectMessages>> {
        const dm = createDirectMessages(dependencies);
        dm.choose(person(1));
        await dm.open();
        expect(await dm.sendFirst("hello")).toBe(false);
        return dm;
    }

    test("is tried again after a wait when no membership signal comes, without asking Rock again", async () => {
        const backoff = waits();
        const { calls, dependencies } = setup(backoff);
        dependencies.startConversation = pendingDoor(calls);
        await pendingDraft(dependencies);
        await settle();

        expect(backoff.list.length).toBe(1);
        expect(sends(calls)).toEqual([]);

        backoff.list.shift()?.resolve();
        await settle();

        expect(sends(calls)).toEqual([`send ${created} hello`]);
        expect(starts(calls)).toEqual(["start 1"]);
    });

    test("a retried send that fails waits longer and tries again, and stops once it is sent", async () => {
        const backoff = waits();
        let attempts = 0;
        const { calls, dependencies } = setup({
            ...backoff,
            send: async (channelId, body) => {
                calls.push(`send ${channelId} ${body}`);
                return ++attempts > 1;
            }
        });
        dependencies.startConversation = pendingDoor(calls);
        await pendingDraft(dependencies);
        await settle();

        backoff.list.shift()?.resolve();
        await settle();
        expect(sends(calls).length).toBe(1);

        // the send was refused, so it waits again, longer than the first time
        expect(backoff.list.length).toBe(1);
        expect(backoff.asked.length).toBe(2);
        expect(backoff.asked[1]).toBeGreaterThan(backoff.asked[0]);

        backoff.list.shift()?.resolve();
        await settle();

        expect(sends(calls)).toEqual([`send ${created} hello`, `send ${created} hello`]);
        expect(starts(calls)).toEqual(["start 1"]);
        expect(backoff.list.length).toBe(0);
    });

    test("stops after five retried sends fail, with no sixth, and leaves the text to the sender as failed", async () => {
        const backoff = waits();
        const lastTries: boolean[] = [];
        const { calls, dependencies } = setup({
            ...backoff,
            send: async (channelId, body, retry?: { isLast: boolean }) => {
                calls.push(`send ${channelId} ${body}`);
                lastTries.push(retry?.isLast === true);
                return false;
            }
        });
        dependencies.startConversation = pendingDoor(calls);
        await pendingDraft(dependencies);
        await settle();

        for (let attempt = 1; attempt <= 5; attempt++) {
            expect(backoff.list.length).toBe(1);
            backoff.list.shift()?.resolve();
            await settle();
            expect(sends(calls).length).toBe(attempt);
        }

        // no sixth wait and no sixth send: the fifth try told the sender it was the last, so its
        // row stays as an ordinary failed message the person can send again or discard
        expect(backoff.list.length).toBe(0);
        expect(lastTries).toEqual([false, false, false, false, true]);
        await settle();
        expect(sends(calls).length).toBe(5);
        expect(starts(calls)).toEqual(["start 1"]);
    });

    test("keeps the draft on screen, opening nothing, until a retried send is confirmed, then opens the conversation", async () => {
        const backoff = waits();
        let attempts = 0;
        const { calls, dependencies } = setup({
            ...backoff,
            send: async (channelId, body) => {
                calls.push(`send ${channelId} ${body}`);
                return ++attempts > 1;
            }
        });
        dependencies.startConversation = pendingDoor(calls);
        const dm = await pendingDraft(dependencies);
        await settle();

        expect(dm.draft?.status).toBe("starting");
        expect(calls).not.toContain(`open ${created}`);

        // the platform refused the first try, so opening it would have been refused too
        backoff.list.shift()?.resolve();
        await settle();
        expect(sends(calls).length).toBe(1);
        expect(calls).not.toContain(`open ${created}`);
        expect(dm.draft?.status).toBe("starting");
        expect(dm.draft?.body).toBe("hello");

        backoff.list.shift()?.resolve();
        await settle();
        expect(calls.slice(-2)).toEqual([`send ${created} hello`, `open ${created}`]);
        expect(dm.draft).toBeNull();
    });

    test("opens the conversation after the last retried send fails, where the failed message is", async () => {
        const backoff = waits();
        const { calls, dependencies } = setup({
            ...backoff,
            send: async (channelId, body) => {
                calls.push(`send ${channelId} ${body}`);
                return false;
            }
        });
        dependencies.startConversation = pendingDoor(calls);
        const dm = await pendingDraft(dependencies);
        await settle();

        for (let attempt = 1; attempt <= 5; attempt++) {
            backoff.list.shift()?.resolve();
            await settle();
        }

        expect(calls.slice(-2)).toEqual([`send ${created} hello`, `open ${created}`]);
        expect(calls.filter(c => c.startsWith("open "))).toEqual([`open ${created}`]);
        expect(dm.draft).toBeNull();
    });

    test("a membership signal while it waits sends it once, and the wait ending sends nothing more", async () => {
        const backoff = waits();
        const { calls, dependencies } = setup(backoff);
        dependencies.startConversation = pendingDoor(calls);
        const dm = await pendingDraft(dependencies);
        await settle();
        expect(backoff.list.length).toBe(1);

        await dm.membershipChanged(created);
        await settle();
        backoff.list.shift()?.resolve();
        await settle();

        expect(sends(calls)).toEqual([`send ${created} hello`]);
        expect(backoff.list.length).toBe(0);
    });

    test("a refusal from Rock is not tried again", async () => {
        const backoff = waits();
        const { calls, dependencies } = setup({
            ...backoff,
            startConversation: async people => {
                calls.push(`start ${people.length}`);
                return { code: "door.target_not_eligible", channelGuid: null, isPending: false, message: "No.", personAliasGuid: null };
            }
        });
        const dm = createDirectMessages(dependencies);
        dm.choose(person(1));
        await dm.open();

        expect(await dm.sendFirst("hello")).toBe(false);
        await settle();

        expect(backoff.list.length).toBe(0);
        expect(sends(calls)).toEqual([]);
    });
});

describe("leaving a draft", () => {
    test("a draft nothing was sent from is dropped, asking Rock for nothing and sending nothing", async () => {
        const backoff = waits();
        const { calls, dependencies } = setup(backoff);
        const dm = createDirectMessages(dependencies);
        dm.choose(person(1));
        await dm.open();
        calls.length = 0;

        dm.close();
        await dm.membershipChanged(created);
        await settle();

        expect(dm.draft).toBeNull();
        expect(calls).toEqual([]);
        expect(backoff.list.length).toBe(0);
    });

    test("a first message whose conversation Rock made is still sent after the draft closes, without taking the screen back", async () => {
        const backoff = waits();
        const { calls, dependencies } = setup(backoff);
        dependencies.startConversation = pendingDoor(calls);
        const dm = createDirectMessages(dependencies);
        dm.choose(person(1));
        await dm.open();
        await dm.sendFirst("hello");

        // the person opened another channel
        dm.close();
        expect(dm.draft).toBeNull();

        await dm.membershipChanged(created);
        await settle();

        expect(sends(calls)).toEqual([`send ${created} hello`]);
        expect(calls).not.toContain(`open ${created}`);
    });

    test("a closed first message with no membership signal is still sent once its wait ends", async () => {
        const backoff = waits();
        const { calls, dependencies } = setup(backoff);
        dependencies.startConversation = pendingDoor(calls);
        const dm = createDirectMessages(dependencies);
        dm.choose(person(1));
        await dm.open();
        await dm.sendFirst("hello");
        dm.close();
        await settle();

        backoff.list.shift()?.resolve();
        await settle();

        expect(sends(calls)).toEqual([`send ${created} hello`]);
    });

    test("a draft closed while Rock is still answering is sent once Rock answers", async () => {
        const answer = deferred<DoorResult>();
        const { calls, dependencies } = setup({
            ...waits(),
            startConversation: people => {
                calls.push(`start ${people.length}`);
                return answer.promise;
            }
        });
        const dm = createDirectMessages(dependencies);
        dm.choose(person(1));
        await dm.open();

        const first = dm.sendFirst("hello");
        dm.close();
        answer.resolve({ code: "ok", channelGuid: created, isPending: false, message: null, personAliasGuid: null });
        await first;
        await settle();

        expect(sends(calls)).toEqual([`send ${created} hello`]);
        expect(calls).not.toContain(`open ${created}`);
    });

    test("a new draft opened while a first message waits leaves that message to be sent and starts empty", async () => {
        const { calls, dependencies } = setup(waits());
        dependencies.startConversation = pendingDoor(calls);
        const dm = createDirectMessages(dependencies);
        dm.choose(person(1));
        await dm.open();
        await dm.sendFirst("hello");

        dm.choose(person(2));
        await dm.open();
        await dm.membershipChanged(created);
        await settle();

        expect(sends(calls)).toEqual([`send ${created} hello`]);
        expect(dm.draft?.people).toEqual([person(2)]);
        expect(dm.draft?.status).toBe("draft");
        expect(dm.draft?.body).toBe("");
    });
});

describe("a refusal after the person left the draft", () => {
    const refusal: DoorResult = {
        code: "door.target_not_eligible",
        channelGuid: null,
        isPending: false,
        message: "Person1 can't be messaged.",
        personAliasGuid: person(1).person_alias_guid
    };

    /** A draft for person 1 whose first message is waiting on Rock's answer, with every report kept. */
    async function waitingOnRock(): Promise<{
        calls: string[],
        reports: ChatError[],
        answer: Deferred<DoorResult>,
        dm: ReturnType<typeof createDirectMessages>,
        first: Promise<boolean>
    }> {
        const answer = deferred<DoorResult>();
        const reports: ChatError[] = [];
        const { calls, dependencies } = setup({
            ...waits(),
            startConversation: people => {
                calls.push(`start ${people.length}`);
                return answer.promise;
            }
        });
        const dm = createDirectMessages({ ...dependencies, report: (error: ChatError) => reports.push(error) } as DirectMessageDependencies);
        dm.choose(person(1));
        await dm.open();
        calls.length = 0;
        const first = dm.sendFirst("hello");
        return { calls, reports, answer, dm, first };
    }

    test("tells the person once, with Rock's sentence, and sends nothing", async () => {
        const { calls, reports, answer, dm, first } = await waitingOnRock();

        dm.close();
        answer.resolve(refusal);
        expect(await first).toBe(false);
        await settle();

        expect(reports.length).toBe(1);
        expect(reports[0].text).toContain("Person1");
        expect(reports[0].text).toContain("could not be sent");
        expect(reports[0].text).toContain("Person1 can't be messaged.");
        expect(sends(calls)).toEqual([]);
        expect(dm.error).toBeNull();
    });

    test("tells the person when a new draft for other people is on screen, and leaves that draft alone", async () => {
        const { calls, reports, answer, dm, first } = await waitingOnRock();

        dm.choose(person(2));
        await dm.open();
        answer.resolve(refusal);
        await first;
        await settle();

        expect(reports.length).toBe(1);
        expect(reports[0].text).toContain("Person1 can't be messaged.");
        expect(dm.draft?.people).toEqual([person(2)]);
        expect(dm.draft?.body).toBe("");
        expect(dm.error).toBeNull();
        expect(sends(calls)).toEqual([]);
    });

    test("keeps the text: the next draft for the same people opens with it", async () => {
        const { answer, dm, first } = await waitingOnRock();

        dm.close();
        answer.resolve(refusal);
        await first;
        await settle();

        dm.choose(person(1));
        await dm.open();

        expect(dm.draft?.people).toEqual([person(1)]);
        expect(dm.draft?.body).toBe("hello");
    });

    test("a refusal arriving after the shell stopped tells nobody and keeps nothing", async () => {
        const { calls, reports, answer, dm, first } = await waitingOnRock();

        dm.close();
        dm.stop();
        answer.resolve(refusal);
        expect(await first).toBe(false);
        await settle();

        expect(reports).toEqual([]);
        expect(sends(calls)).toEqual([]);

        dm.choose(person(1));
        await dm.open();
        expect(dm.draft?.body).toBe("");
    });

    test("a refusal while the draft is still showing goes on the draft, not to the toast", async () => {
        const { reports, answer, dm, first } = await waitingOnRock();

        answer.resolve(refusal);
        await first;
        await settle();

        expect(reports).toEqual([]);
        expect(dm.error).toBe("Person1 can't be messaged.");
        expect(dm.draft?.body).toBe("hello");
    });
});

describe("a first message whose conversation cannot be shown yet", () => {
    /** An open the platform refuses, because it does not hold the conversation for the person yet. */
    const refusedOpen = (calls: string[]) => (channelId: string): boolean => {
        calls.push(`open ${channelId}`);
        return false;
    };

    /** A send that fails until it is let through, recording the row each try names. */
    function gatedSend(calls: string[]): { send: DirectMessageDependencies["send"], rows: Array<string | undefined>, letThrough: () => void } {
        const rows: Array<string | undefined> = [];
        let isLetThrough = false;
        return {
            rows,
            letThrough: () => isLetThrough = true,
            send: async (channelId, body, attempt) => {
                calls.push(`send ${channelId} ${body}`);
                rows.push(attempt?.localId);
                return isLetThrough;
            }
        };
    }

    /** A draft for person 1 whose first message Rock answered pending, every call in one list. */
    async function pendingWith(calls: string[], overrides: Partial<DirectMessageDependencies> = {}): Promise<{
        reports: ChatError[],
        backoff: ReturnType<typeof waits>,
        dm: ReturnType<typeof createDirectMessages>
    }> {
        const backoff = waits();
        const reports: ChatError[] = [];
        const { dependencies } = setup(backoff);
        const dm = createDirectMessages({
            ...dependencies,
            findConversation: async people => {
                calls.push(`find ${people.length}`);
                return { ok: true, channelId: null };
            },
            openChannel: channelId => {
                calls.push(`open ${channelId}`);
                return true;
            },
            startConversation: pendingDoor(calls),
            report: (error: ChatError) => reports.push(error),
            ...overrides
        } as DirectMessageDependencies);
        dm.choose(person(1));
        await dm.open();
        expect(await dm.sendFirst("hello")).toBe(false);
        await settle();
        return { reports, backoff, dm };
    }

    async function failFiveTimes(backoff: ReturnType<typeof waits>): Promise<void> {
        for (let attempt = 1; attempt <= 5; attempt++) {
            backoff.list.shift()?.resolve();
            await settle();
        }
    }

    test("a membership signal whose open is refused keeps the draft, its text and its status while the send is tried", async () => {
        const calls: string[] = [];
        const gate = gatedSend(calls);
        const h = await pendingWith(calls, { send: gate.send, openChannel: refusedOpen(calls) });

        await h.dm.membershipChanged(created);
        await settle();

        expect(sends(calls)).toEqual([`send ${created} hello`]);
        expect(h.dm.draft?.status).toBe("starting");
        expect(h.dm.draft?.body).toBe("hello");
    });

    test("after the fifth failure, a refused open keeps the draft on screen with its text and an error, ready to send again", async () => {
        const calls: string[] = [];
        const gate = gatedSend(calls);
        const h = await pendingWith(calls, { send: gate.send, openChannel: refusedOpen(calls) });

        await failFiveTimes(h.backoff);

        expect(sends(calls).length).toBe(5);
        expect(calls).toContain(`open ${created}`);
        expect(h.dm.draft?.body).toBe("hello");
        expect(h.dm.draft?.status).toBe("draft");
        expect(h.dm.draft?.channelGuid).toBe(created);
        expect(h.dm.error).toEqual(expect.any(String));
        expect(h.reports).toEqual([]);
    });

    test("a send from that draft tries the same row again under the same key, without asking Rock, then opens the conversation", async () => {
        const calls: string[] = [];
        const gate = gatedSend(calls);
        let isOpenable = false;
        const h = await pendingWith(calls, {
            send: gate.send,
            openChannel: channelId => {
                calls.push(`open ${channelId}`);
                return isOpenable;
            }
        });
        await failFiveTimes(h.backoff);

        gate.letThrough();
        isOpenable = true;
        expect(await h.dm.sendFirst("hello")).toBe(true);

        expect(starts(calls)).toEqual(["start 1"]);
        expect(gate.rows.length).toBe(6);
        expect(new Set(gate.rows).size).toBe(1);
        expect(gate.rows[0]).toEqual(expect.any(String));
        expect(calls.slice(-2)).toEqual([`send ${created} hello`, `open ${created}`]);
        expect(h.dm.draft).toBeNull();
        expect(h.dm.error).toBeNull();
    });

    test("after the fifth failure with the person gone, the toast names the people, nothing opens, and their next draft holds the text and its row", async () => {
        const calls: string[] = [];
        const gate = gatedSend(calls);
        const h = await pendingWith(calls, { send: gate.send });
        h.dm.close();

        await failFiveTimes(h.backoff);

        expect(h.reports.length).toBe(1);
        expect(h.reports[0].text).toContain("Person1");
        expect(h.reports[0].text).toContain("could not be sent");
        expect(calls).not.toContain(`open ${created}`);

        h.dm.choose(person(1));
        await h.dm.open();
        expect(h.dm.draft?.body).toBe("hello");

        gate.letThrough();
        expect(await h.dm.sendFirst("hello")).toBe(true);
        expect(starts(calls)).toEqual(["start 1"]);
        expect(new Set(gate.rows).size).toBe(1);
    });

    test("a failed draft the person leaves keeps its text for their next draft to the same people", async () => {
        const calls: string[] = [];
        const gate = gatedSend(calls);
        const h = await pendingWith(calls, { send: gate.send, openChannel: refusedOpen(calls) });
        await failFiveTimes(h.backoff);

        h.dm.close();
        h.dm.choose(person(1));
        await h.dm.open();

        expect(h.dm.draft?.body).toBe("hello");
        expect(h.dm.draft?.channelGuid).toBe(created);
        expect(h.dm.error).toBeNull();
    });

    test("kept text is dropped once the people's conversation can be opened, where its failed row is", async () => {
        const calls: string[] = [];
        const gate = gatedSend(calls);
        let shared: string | null = null;
        const h = await pendingWith(calls, {
            send: gate.send,
            findConversation: async people => {
                calls.push(`find ${people.length}`);
                return { ok: true, channelId: shared };
            }
        });
        h.dm.close();
        await failFiveTimes(h.backoff);

        shared = created;
        h.dm.choose(person(1));
        await h.dm.open();
        expect(calls.slice(-1)).toEqual([`open ${created}`]);

        shared = null;
        h.dm.choose(person(1));
        await h.dm.open();
        expect(h.dm.draft?.body).toBe("");
    });

    test("a held answer whose open is refused keeps the draft and waits, sending nothing into a conversation not on screen", async () => {
        const backoff = waits();
        const { calls, dependencies } = setup({ ...backoff });
        dependencies.openChannel = refusedOpen(calls);
        const dm = createDirectMessages(dependencies);
        dm.choose(person(1));
        await dm.open();

        expect(await dm.sendFirst("hello")).toBe(false);
        await settle();

        expect(calls).toContain(`open ${created}`);
        expect(sends(calls)).toEqual([]);
        expect(dm.draft?.status).toBe("starting");
        expect(dm.draft?.body).toBe("hello");
        expect(backoff.list.length).toBe(1);
    });

    test("a stop while a held answer's conversation opens sends nothing", async () => {
        const opening = deferred<boolean>();
        const { calls, dependencies } = setup({ ...waits() });
        dependencies.openChannel = channelId => {
            calls.push(`open ${channelId}`);
            return opening.promise;
        };
        const dm = createDirectMessages(dependencies);
        dm.choose(person(1));
        await dm.open();

        const first = dm.sendFirst("hello");
        await settle();
        dm.stop();
        opening.resolve(true);
        expect(await first).toBe(false);
        await settle();

        expect(sends(calls)).toEqual([]);
    });

    test("a stop while the last failure's conversation opens changes nothing on screen and tells nobody", async () => {
        const calls: string[] = [];
        const gate = gatedSend(calls);
        const opening = deferred<boolean>();
        const h = await pendingWith(calls, {
            send: gate.send,
            openChannel: channelId => {
                calls.push(`open ${channelId}`);
                return opening.promise;
            }
        });

        await failFiveTimes(h.backoff);
        h.dm.stop();
        opening.resolve(false);
        await settle();

        expect(h.dm.draft?.status).toBe("starting");
        expect(h.dm.error).toBeNull();
        expect(h.reports).toEqual([]);
    });

    test("a retry from a failed draft sends the text as last edited", async () => {
        const calls: string[] = [];
        const gate = gatedSend(calls);
        const h = await pendingWith(calls, { send: gate.send, openChannel: refusedOpen(calls) });
        await failFiveTimes(h.backoff);

        gate.letThrough();
        expect(await h.dm.sendFirst("hello again")).toBe(true);

        expect(sends(calls).slice(-1)).toEqual([`send ${created} hello again`]);
        expect(new Set(gate.rows).size).toBe(1);
    });

    test("a failed draft the person edits and leaves without sending keeps the text as it is in the box", async () => {
        const calls: string[] = [];
        const gate = gatedSend(calls);
        const h = await pendingWith(calls, { send: gate.send, openChannel: refusedOpen(calls) });
        await failFiveTimes(h.backoff);

        h.dm.edit("hello, edited");
        h.dm.close();
        h.dm.choose(person(1));
        await h.dm.open();

        expect(h.dm.draft?.body).toBe("hello, edited");
        expect(sends(calls).length).toBe(5);
    });

    test("two first messages to the same people that both fail after the person left are both kept, and each is retried as itself, oldest first, under its own row", async () => {
        const calls: string[] = [];
        const gate = gatedSend(calls);
        const h = await pendingWith(calls, { send: gate.send });
        h.dm.close();
        h.dm.choose(person(1));
        await h.dm.open();
        expect(h.dm.draft?.body).toBe("");
        expect(await h.dm.sendFirst("second")).toBe(false);
        await settle();
        h.dm.close();

        // Two waits are queued, one per message; ten wait ends are five failed tries each.
        for (let attempt = 1; attempt <= 10; attempt++) {
            h.backoff.list.shift()?.resolve();
            await settle();
        }

        expect(h.reports.length).toBe(2);
        const [firstRow, secondRow] = [...new Set(gate.rows)];

        // Either may have reached the platform before its confirmation was lost, so neither is
        // folded into the other's key: the newest is the draft's retry, the older goes first.
        h.dm.choose(person(1));
        await h.dm.open();
        expect(h.dm.draft?.body).toBe("second");
        expect(h.dm.draft?.channelGuid).toBe(created);
        const triesBefore = gate.rows.length;

        gate.letThrough();
        expect(await h.dm.sendFirst("second")).toBe(true);

        expect(firstRow).not.toBe(secondRow);
        expect(sends(calls).slice(triesBefore)).toEqual([`send ${created} hello`, `send ${created} second`]);
        expect(gate.rows.slice(triesBefore)).toEqual([firstRow, secondRow]);
        expect(starts(calls)).toEqual(["start 1", "start 1"]);
    });

    test("a refused text and a failed message kept for the same people: only the refused text is in the box, and the failed one is retried as itself under its own row", async () => {
        const calls: string[] = [];
        const gate = gatedSend(calls);
        const refusal = deferred<DoorResult>();
        let asked = 0;
        const h = await pendingWith(calls, {
            send: gate.send,
            startConversation: people => {
                calls.push(`start ${people.length}`);
                asked++;
                if (asked === 2) {
                    return refusal.promise;
                }
                return Promise.resolve({ code: "ok", channelGuid: created, isPending: asked === 1, message: null, personAliasGuid: null });
            }
        });
        h.dm.close();

        // A second message to the same people, which Rock refuses after the person left it.
        h.dm.choose(person(1));
        await h.dm.open();
        const second = h.dm.sendFirst("second");
        h.dm.close();
        refusal.resolve({ code: "door.unavailable", channelGuid: null, isPending: false, message: "Try again.", personAliasGuid: null });
        await second;

        // The first one fails for good after the person left.
        await failFiveTimes(h.backoff);
        expect(h.reports.length).toBe(2);
        const [failedRow] = new Set(gate.rows);
        const triesBefore = gate.rows.length;

        h.dm.choose(person(1));
        await h.dm.open();
        expect(h.dm.draft?.body).toBe("second");

        gate.letThrough();
        expect(await h.dm.sendFirst("second")).toBe(true);

        expect(sends(calls).slice(triesBefore)).toEqual([`send ${created} hello`, `send ${created} second`]);
        expect(gate.rows[triesBefore]).toBe(failedRow);
        expect(gate.rows[triesBefore + 1]).not.toBe(failedRow);
        expect(starts(calls)).toEqual(["start 1", "start 1", "start 1"]);
    });

    test("a refused text kept for people who now share a conversation is put in the box when it opens", async () => {
        const answer = deferred<DoorResult>();
        const { calls, dependencies } = setup();
        let shared: string | null = null;
        dependencies.startConversation = people => {
            calls.push(`start ${people.length}`);
            return answer.promise;
        };
        dependencies.findConversation = async people => {
            calls.push(`find ${people.length}`);
            return { ok: true, channelId: shared };
        };
        const dm = createDirectMessages(dependencies);
        dm.choose(person(1));
        await dm.open();
        const first = dm.sendFirst("hello");
        dm.close();
        answer.resolve({ code: "door.unavailable", channelGuid: null, isPending: false, message: "Try again.", personAliasGuid: null });
        await first;

        shared = existing;
        dm.choose(person(1));
        await dm.open();

        expect(calls.slice(-1)).toEqual([`open ${existing}`]);
        expect(dm.draft).toBeNull();
        expect(dm.held?.body).toBe("hello");

        // Handed over once: the next draft to the same people opens empty.
        shared = null;
        dm.choose(person(1));
        await dm.open();
        expect(dm.held).toBeNull();
        expect(dm.draft?.body).toBe("");
    });

    test("a send that rejects while the platform is waited for counts as a failed try, and the fifth leaves the draft ready to send again", async () => {
        const calls: string[] = [];
        const h = await pendingWith(calls, {
            send: async (channelId, body) => {
                calls.push(`send ${channelId} ${body}`);
                throw new Error("network");
            },
            openChannel: refusedOpen(calls)
        });

        await failFiveTimes(h.backoff);

        expect(sends(calls).length).toBe(5);
        expect(h.dm.draft?.status).toBe("draft");
        expect(h.dm.draft?.body).toBe("hello");
        expect(h.dm.error).toEqual(expect.any(String));
    });

    test("a retry from a failed draft whose send rejects puts the draft back with its text and an error", async () => {
        const calls: string[] = [];
        let isRejecting = false;
        const gate = gatedSend(calls);
        const h = await pendingWith(calls, {
            send: async (channelId, body, attempt) => {
                if (isRejecting) {
                    calls.push(`send ${channelId} ${body}`);
                    throw new Error("network");
                }
                return gate.send(channelId, body, attempt);
            },
            openChannel: refusedOpen(calls)
        });
        await failFiveTimes(h.backoff);

        isRejecting = true;
        expect(await h.dm.sendFirst("hello")).toBe(false);

        expect(sends(calls).length).toBe(6);
        expect(h.dm.draft?.status).toBe("draft");
        expect(h.dm.draft?.body).toBe("hello");
        expect(h.dm.error).toEqual(expect.any(String));
    });
});

describe("a call that rejects instead of answering", () => {
    const rejected = async (): Promise<never> => {
        throw new Error("network");
    };

    test("Rock's door rejecting while the draft shows puts the draft back with its text and an error", async () => {
        const { calls, dependencies } = setup();
        dependencies.startConversation = async people => {
            calls.push(`start ${people.length}`);
            return rejected();
        };
        const dm = createDirectMessages(dependencies);
        dm.choose(person(1));
        await dm.open();

        expect(await dm.sendFirst("hello")).toBe(false);

        expect(dm.draft?.status).toBe("draft");
        expect(dm.draft?.body).toBe("hello");
        expect(dm.error).toEqual(expect.any(String));
        expect(sends(calls)).toEqual([]);
    });

    test("Rock's door rejecting after the person left tells them in the toast and keeps the text", async () => {
        let reject!: (reason: unknown) => void;
        const reports: ChatError[] = [];
        const { calls, dependencies } = setup();
        dependencies.startConversation = people => {
            calls.push(`start ${people.length}`);
            return new Promise<DoorResult>((_resolve, r) => reject = r);
        };
        dependencies.report = error => reports.push(error);
        const dm = createDirectMessages(dependencies);
        dm.choose(person(1));
        await dm.open();

        const first = dm.sendFirst("hello");
        dm.close();
        reject(new Error("network"));
        expect(await first).toBe(false);
        await settle();

        expect(reports.length).toBe(1);
        expect(reports[0].text).toContain("Person1");
        expect(reports[0].text).toContain("could not be sent");
        expect(sends(calls)).toEqual([]);

        dm.choose(person(1));
        await dm.open();
        expect(dm.draft?.body).toBe("hello");
    });

    test("a send that rejects on a held conversation resolves as not sent rather than rejecting", async () => {
        const { calls, dependencies } = setup();
        dependencies.send = async (channelId, body) => {
            calls.push(`send ${channelId} ${body}`);
            return rejected();
        };
        const dm = createDirectMessages(dependencies);
        dm.choose(person(1));
        await dm.open();

        await expect(dm.sendFirst("hello")).resolves.toBe(false);
        expect(sends(calls)).toEqual([`send ${created} hello`]);
        expect(dm.draft).toBeNull();
    });
});

describe("kept text for the same people", () => {
    test("two first messages refused after the person left are both kept, in order, for the next draft", async () => {
        const answers = [deferred<DoorResult>(), deferred<DoorResult>()];
        const reports: ChatError[] = [];
        const { calls, dependencies } = setup();
        let asked = 0;
        dependencies.startConversation = people => {
            calls.push(`start ${people.length}`);
            return answers[asked++].promise;
        };
        dependencies.report = error => reports.push(error);
        const dm = createDirectMessages(dependencies);
        const refusal: DoorResult = { code: "door.unavailable", channelGuid: null, isPending: false, message: "Try again.", personAliasGuid: null };

        dm.choose(person(1));
        await dm.open();
        const first = dm.sendFirst("hello");
        dm.close();
        dm.choose(person(1));
        await dm.open();
        const second = dm.sendFirst("second");
        dm.close();

        answers[0].resolve(refusal);
        answers[1].resolve(refusal);
        await Promise.all([first, second]);
        await settle();

        expect(reports.length).toBe(2);
        dm.choose(person(1));
        await dm.open();
        expect(dm.draft?.body).toBe("hello\n\nsecond");
        expect(sends(calls)).toEqual([]);
    });
});
