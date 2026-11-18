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
// The profile: the card reads its person for the room it was opened from, keeping only the newest
// card's answer, and Rock's half beside it. Once stopped, nothing more is asked and no answer still
// on its way reaches the screen.
//
// The fakes answer in the platform's and Rock's own shapes: chat_get_person's object and
// ChatProfileDetailsBag.
/* eslint-disable @typescript-eslint/naming-convention */
import { ChatProfileDetailsBag } from "@Obsidian/ViewModels/Blocks/Communication/Chat/ChatShell/chatProfileDetailsBag";
import { createProfile, PersonProfileResult, profileDetailsWait } from "../../../../../src/Communication/Chat/ChatShell/composables/useProfile.partial";
import { PersonProfile } from "../../../../../src/Communication/Chat/ChatShell/types.partial";

// #region Fixtures

const team = "c0000003-0000-4000-8000-000000000000";

type Deferred<T> = { promise: Promise<T>, resolve: (value: T) => void, reject: (error: unknown) => void };

function deferred<T>(): Deferred<T> {
    let resolve!: (value: T) => void;
    let reject!: (error: unknown) => void;
    const promise = new Promise<T>((res, rej) => {
        resolve = res;
        reject = rej;
    });
    return { promise, resolve, reject };
}

async function settle(): Promise<void> {
    for (let i = 0; i < 20; i++) {
        await Promise.resolve();
    }
}

type Setup = {
    profile: ReturnType<typeof createProfile>;
    personCalls: Array<{ personAliasGuid: string, channelId: string, answer: Deferred<PersonProfileResult> }>;
    detailsCalls: Array<{ personAliasGuid: string, answer: Deferred<ChatProfileDetailsBag | null> }>;
};

function setup(options: { hasDetailsDoor?: boolean } = {}): Setup {
    const personCalls: Setup["personCalls"] = [];
    const detailsCalls: Setup["detailsCalls"] = [];

    const profile = createProfile({
        getPerson: (personAliasGuid, channelId) => {
            const answer = deferred<PersonProfileResult>();
            personCalls.push({ personAliasGuid, channelId, answer });
            return answer.promise;
        },
        getProfileDetails: options.hasDetailsDoor === false
            ? undefined
            : personAliasGuid => {
                const answer = deferred<ChatProfileDetailsBag | null>();
                detailsCalls.push({ personAliasGuid, answer });
                return answer.promise;
            }
    });

    return { profile, personCalls, detailsCalls };
}

// #endregion Fixtures

describe("the profile card", () => {
    const anna = "a0000002-0000-4000-8000-000000000000";
    const ben = "a0000003-0000-4000-8000-000000000000";

    /** chat_get_person's answer, exactly its keys. */
    function profile(personAliasGuid: string, nickName: string, extra: Partial<PersonProfile> = {}): PersonProfile {
        return {
            person_alias_guid: personAliasGuid,
            nick_name: nickName,
            last_name: "Reyes",
            avatar_url: null,
            campus_id: null,
            is_self: false,
            is_leader: false,
            details: null,
            can_message: true,
            dm_channel_id: null,
            ...extra
        };
    }

    test("opening asks for the person in that room, and the card is drawn once they are read", async () => {
        const s = setup();

        s.profile.openCard(anna, team);
        expect(s.personCalls.map(c => [c.personAliasGuid, c.channelId])).toEqual([[anna, team]]);
        expect(s.profile.state.card).toEqual({ personAliasGuid: anna, channelId: team, person: null, rock: { kind: "loading" } });

        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_leader: true }) });
        await settle();

        expect(s.profile.state.card).toEqual({ personAliasGuid: anna, channelId: team, person: profile(anna, "Anna", { is_leader: true }), rock: { kind: "loading" } });
    });

    test("a card opened over another drops the first one's answer, whichever lands first", async () => {
        const s = setup();
        s.profile.openCard(anna, team);
        s.profile.openCard(ben, team);
        const [first, second] = s.personCalls;

        first.answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();
        expect(s.profile.state.card).toEqual({ personAliasGuid: ben, channelId: team, person: null, rock: { kind: "loading" } });

        second.answer.resolve({ ok: true, person: profile(ben, "Ben") });
        await settle();
        expect(s.profile.state.card?.person?.nick_name).toBe("Ben");
    });

    test("an older card's answer landing after the newer one's changes nothing", async () => {
        const s = setup();
        s.profile.openCard(anna, team);
        s.profile.openCard(ben, team);
        const [first, second] = s.personCalls;

        second.answer.resolve({ ok: true, person: profile(ben, "Ben") });
        await settle();
        first.answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();

        expect(s.profile.state.card?.person?.nick_name).toBe("Ben");
    });

    test("a person the platform does not list draws no card", async () => {
        const s = setup();
        s.profile.openCard(anna, team);

        s.personCalls[0].answer.resolve({ ok: false, error: { code: "rpc.person_not_found", severity: "failed", text: "This person isn't available." } });
        await settle();

        expect(s.profile.state.card).toBeNull();
    });

    test("a read that fails draws no card either", async () => {
        const s = setup();
        s.profile.openCard(anna, team);

        s.personCalls[0].answer.resolve({ ok: false, error: { code: "rpc.transport", severity: "failed" } });
        await settle();

        expect(s.profile.state.card).toBeNull();
    });

    test("closing drops an answer still on its way", async () => {
        const s = setup();
        s.profile.openCard(anna, team);

        s.profile.closeCard();
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();

        expect(s.profile.state.card).toBeNull();
    });
});

// What Rock adds to the card, its email, phone and template, is read beside the platform's person
// and never copied to chat. A person showing details waits a moment for Rock's answer, so the card
// usually draws once, whole; past that it draws with Rock's rows reserved. Each person's answer is
// kept for the visit, so a card opened again draws whole at once.
describe("the profile card's details from Rock", () => {
    const anna = "a0000002-0000-4000-8000-000000000000";
    const ben = "a0000003-0000-4000-8000-000000000000";

    /** chat_get_person's answer for someone showing details, exactly its keys. */
    function profile(personAliasGuid: string, nickName: string, extra: Partial<PersonProfile> = {}): PersonProfile {
        return {
            person_alias_guid: personAliasGuid,
            nick_name: nickName,
            last_name: "Reyes",
            avatar_url: null,
            campus_id: null,
            is_self: false,
            is_leader: false,
            details: { badge_keys: [] },
            can_message: true,
            dm_channel_id: null,
            ...extra
        };
    }

    function shown(email: string): ChatProfileDetailsBag {
        return { isShown: true, email, mobilePhone: "(602) 555-0142", mobilePhoneUri: "tel:+16025550142", lavaHtml: null };
    }

    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    test("opening asks the platform and Rock together", () => {
        const s = setup();

        s.profile.openCard(anna, team);

        expect(s.personCalls.map(c => c.personAliasGuid)).toEqual([anna]);
        expect(s.detailsCalls.map(c => c.personAliasGuid)).toEqual([anna]);
    });

    test("Rock answering first is kept, and the card draws once, whole", async () => {
        const s = setup();
        s.profile.openCard(anna, team);

        s.detailsCalls[0].answer.resolve(shown("anna@example.org"));
        await settle();
        expect(s.profile.state.card?.person).toBeNull();

        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();

        expect(s.profile.state.card).toEqual({ personAliasGuid: anna, channelId: team, person: profile(anna, "Anna"), rock: { kind: "answered", details: shown("anna@example.org") } });
    });

    test("someone showing details waits for Rock, and Rock answering in time draws the card once, whole", async () => {
        const s = setup();
        s.profile.openCard(anna, team);

        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();
        expect(s.profile.state.card?.person).toBeNull();

        jest.advanceTimersByTime(profileDetailsWait - 1);
        s.detailsCalls[0].answer.resolve(shown("anna@example.org"));
        await settle();

        expect(s.profile.state.card?.person?.nick_name).toBe("Anna");
        expect(s.profile.state.card?.rock).toEqual({ kind: "answered", details: shown("anna@example.org") });
    });

    test("past the wait the card draws with Rock's rows still loading, and fills them when it answers", async () => {
        const s = setup();
        s.profile.openCard(anna, team);
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();

        jest.advanceTimersByTime(profileDetailsWait);
        await settle();
        expect(s.profile.state.card?.person?.nick_name).toBe("Anna");
        expect(s.profile.state.card?.rock).toEqual({ kind: "loading" });

        s.detailsCalls[0].answer.resolve(shown("anna@example.org"));
        await settle();
        expect(s.profile.state.card?.rock).toEqual({ kind: "answered", details: shown("anna@example.org") });
    });

    test("the wait runs from the open, so a slow platform is not waited on twice", async () => {
        const s = setup();
        s.profile.openCard(anna, team);

        jest.advanceTimersByTime(profileDetailsWait);
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();

        expect(s.profile.state.card?.person?.nick_name).toBe("Anna");
        expect(s.profile.state.card?.rock).toEqual({ kind: "loading" });
    });

    test("someone keeping details private draws at once, Rock or not", async () => {
        const s = setup();
        s.profile.openCard(anna, team);

        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { details: null }) });
        await settle();

        expect(s.profile.state.card?.person?.nick_name).toBe("Anna");
    });

    test("Rock failing, or answering nothing, leaves its rows out", async () => {
        const s = setup();
        s.profile.openCard(anna, team);
        s.detailsCalls[0].answer.reject(new Error("unreachable"));
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();
        expect(s.profile.state.card?.person?.nick_name).toBe("Anna");
        expect(s.profile.state.card?.rock).toEqual({ kind: "failed" });

        s.profile.openCard(ben, team);
        s.detailsCalls[1].answer.resolve(null);
        s.personCalls[1].answer.resolve({ ok: true, person: profile(ben, "Ben") });
        await settle();
        expect(s.profile.state.card?.person?.nick_name).toBe("Ben");
        expect(s.profile.state.card?.rock).toEqual({ kind: "failed" });
    });

    test("an older card's answer from Rock never lands on a newer card", async () => {
        const s = setup();
        s.profile.openCard(anna, team);
        s.profile.openCard(ben, team);

        s.detailsCalls[0].answer.resolve(shown("anna@example.org"));
        await settle();

        expect(s.profile.state.card?.personAliasGuid).toBe(ben);
        expect(s.profile.state.card?.rock).toEqual({ kind: "loading" });
    });

    test("each person's answer is kept for the visit, so their card opens whole without asking again", async () => {
        const s = setup();
        s.profile.openCard(anna, team);
        s.detailsCalls[0].answer.resolve(shown("anna@example.org"));
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();
        s.profile.closeCard();

        s.profile.openCard(anna.toUpperCase(), team);
        expect(s.detailsCalls).toHaveLength(1);
        expect(s.profile.state.card?.rock).toEqual({ kind: "answered", details: shown("anna@example.org") });

        s.personCalls[1].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();
        expect(s.profile.state.card?.person?.nick_name).toBe("Anna");
    });

    test("a failed answer is not kept, so the next open asks again", async () => {
        const s = setup();
        s.profile.openCard(anna, team);
        s.detailsCalls[0].answer.reject(new Error("unreachable"));
        await settle();
        s.profile.closeCard();

        s.profile.openCard(anna, team);

        expect(s.detailsCalls).toHaveLength(2);
    });

    test("after the person's own switch is saved, their open card asks Rock again and keeps the new answer", async () => {
        const s = setup();
        s.profile.openCard(anna, team);
        s.detailsCalls[0].answer.resolve({ isShown: false });
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true, details: null }) });
        await settle();

        const saved = deferred<void>();
        s.profile.refreshCardDetails(saved.promise);
        expect(s.profile.state.card?.rock).toEqual({ kind: "loading" });
        expect(s.detailsCalls).toHaveLength(1);

        saved.resolve();
        await settle();
        expect(s.detailsCalls).toHaveLength(2);

        s.detailsCalls[1].answer.resolve(shown("anna@example.org"));
        await settle();
        expect(s.profile.state.card?.rock).toEqual({ kind: "answered", details: shown("anna@example.org") });

        s.profile.closeCard();
        s.profile.openCard(anna, team);
        expect(s.profile.state.card?.rock).toEqual({ kind: "answered", details: shown("anna@example.org") });
    });

    test("an answer that shows nothing is not kept, so a person who turns details on is asked again at the next open", async () => {
        const s = setup();
        s.profile.openCard(anna, team);
        s.detailsCalls[0].answer.resolve({ isShown: false });
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { details: null }) });
        await settle();
        s.profile.closeCard();

        s.profile.openCard(anna, team);

        expect(s.detailsCalls).toHaveLength(2);
        expect(s.profile.state.card?.rock).toEqual({ kind: "loading" });
    });

    test("only the newest ask after quick changes of the switch is drawn and kept, however the answers arrive", async () => {
        const s = setup();
        s.profile.openCard(anna, team);
        s.detailsCalls[0].answer.resolve(shown("anna@example.org"));
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true }) });
        await settle();

        s.profile.refreshCardDetails(Promise.resolve());
        await settle();
        s.profile.refreshCardDetails(Promise.resolve());
        await settle();
        expect(s.detailsCalls).toHaveLength(3);

        s.detailsCalls[2].answer.resolve({ isShown: false });
        await settle();
        s.detailsCalls[1].answer.resolve(shown("old@example.org"));
        await settle();

        expect(s.profile.state.card?.rock).toEqual({ kind: "answered", details: { isShown: false } });

        s.profile.closeCard();
        s.profile.openCard(anna, team);
        expect(s.detailsCalls).toHaveLength(4);
    });

    test("an answer to an open asked before the switch changed is neither drawn nor kept", async () => {
        const s = setup();
        s.profile.openCard(anna, team);
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true }) });
        jest.advanceTimersByTime(profileDetailsWait);
        await settle();

        s.profile.refreshCardDetails(Promise.resolve());
        await settle();
        s.detailsCalls[0].answer.resolve(shown("old@example.org"));
        await settle();

        expect(s.profile.state.card?.rock).toEqual({ kind: "loading" });

        s.detailsCalls[1].answer.resolve({ isShown: false });
        await settle();
        s.profile.closeCard();
        s.profile.openCard(anna, team);
        expect(s.detailsCalls).toHaveLength(3);
    });

    test("after the person's own switch is saved, their card reads them again from the platform, so their badges show", async () => {
        const s = setup();
        s.profile.openCard(anna, team);
        s.detailsCalls[0].answer.resolve({ isShown: false });
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true, details: null }) });
        await settle();

        const saved = deferred<void>();
        s.profile.refreshCardDetails(saved.promise);
        await settle();
        expect(s.personCalls).toHaveLength(1);

        saved.resolve();
        await settle();
        expect(s.personCalls.map(c => [c.personAliasGuid, c.channelId])).toEqual([[anna, team], [anna, team]]);

        s.personCalls[1].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true, details: { badge_keys: ["serving"] } }) });
        await settle();
        expect(s.profile.state.card?.person?.details).toEqual({ badge_keys: ["serving"] });
    });

    test("only the newest save's platform read is drawn, however the answers arrive", async () => {
        const s = setup();
        s.profile.openCard(anna, team);
        s.detailsCalls[0].answer.resolve({ isShown: false });
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true, details: null }) });
        await settle();

        s.profile.refreshCardDetails(Promise.resolve());
        await settle();
        s.profile.refreshCardDetails(Promise.resolve());
        await settle();
        expect(s.personCalls).toHaveLength(3);

        s.personCalls[2].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true, details: null }) });
        await settle();
        s.personCalls[1].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true, details: { badge_keys: ["serving"] } }) });
        await settle();

        expect(s.profile.state.card?.person?.details).toBeNull();
    });

    test("a failed read after the save keeps the person the card already shows", async () => {
        const s = setup();
        s.profile.openCard(anna, team);
        s.detailsCalls[0].answer.resolve({ isShown: false });
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna", { is_self: true, details: null }) });
        await settle();

        s.profile.refreshCardDetails(Promise.resolve());
        await settle();
        s.personCalls[1].answer.resolve({ ok: false, error: { code: "rpc.transport", severity: "failed" } });
        await settle();

        expect(s.profile.state.card?.person?.nick_name).toBe("Anna");
    });

    test("without Rock's door the card draws at once with nothing from Rock", async () => {
        const s = setup({ hasDetailsDoor: false });
        s.profile.openCard(anna, team);
        s.personCalls[0].answer.resolve({ ok: true, person: profile(anna, "Anna") });
        await settle();

        expect(s.profile.state.card?.person?.nick_name).toBe("Anna");
        expect(s.profile.state.card?.rock).toEqual({ kind: "failed" });
    });

    test("once stopped Rock is not asked again and its answer never lands", async () => {
        const s = setup();
        s.profile.openCard(anna, team);
        s.profile.stop();
        s.detailsCalls[0].answer.resolve(shown("anna@example.org"));
        jest.advanceTimersByTime(profileDetailsWait);
        await settle();

        expect(s.profile.state.card?.rock).toEqual({ kind: "loading" });

        s.profile.openCard(ben, team);
        s.profile.refreshCardDetails(Promise.resolve());
        await settle();
        expect(s.detailsCalls).toHaveLength(1);
    });
});

describe("the card once stopped", () => {
    test("nothing still on its way reaches the screen and nothing more is asked", async () => {
        const s = setup();
        s.profile.openCard("a0000002-0000-4000-8000-000000000000", team);

        s.profile.stop();
        s.personCalls[0].answer.resolve({
            ok: true,
            person: { person_alias_guid: "a0000002-0000-4000-8000-000000000000", nick_name: "Anna", last_name: "Reyes", avatar_url: null, campus_id: null, is_self: false, is_leader: false, details: null, can_message: true, dm_channel_id: null }
        });
        await settle();

        expect(s.profile.state.card?.person ?? null).toBeNull();

        s.profile.openCard("a0000003-0000-4000-8000-000000000000", team);
        expect(s.personCalls).toHaveLength(1);
    });
});
