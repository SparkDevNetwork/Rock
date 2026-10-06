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
    maxOthers,
    PersonRow
} from "../../../../../src/Communication/Chat/ChatShell/composables/useDirectMessage.partial";

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

/** A recorder of every call the module makes outside itself, in order. */
function setup(overrides: Partial<DirectMessageDependencies> = {}): {
    calls: string[],
    dependencies: DirectMessageDependencies
} {
    const calls: string[] = [];
    const dependencies: DirectMessageDependencies = {
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
});
