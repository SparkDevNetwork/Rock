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
// The person's settings: a room's notify mode, star and hide, and their own switches. Every change
// shows at once and writes at once, one save per change: a room's through chat_set_prefs, the
// reaction switch through it too with no room, and the two privacy switches through Rock's
// SavePersonSetting door. Hiding unstars and starring unhides in the same one patch. A save that
// lands shows nothing more; one that fails puts the control back to the last value saved, with the
// server's own sentence and, where another try can pass, Try again under it, said aloud. A change
// made from the header's menu says so in a toast instead. An answer to a save the person has
// since changed again moves nothing but what is known saved, and once stopped no answer reaches
// the screen.
//
// The fakes answer in the platform's and the door's own shapes: chat_set_prefs's
// {channel: {channel_id, is_favorite, hidden, notify_mode, notify_mode_effective}, me: {...}},
// its refusals as the shell's one call path classifies them, and ChatPersonSettingResultBag's
// {code, isPending, message}.
/* eslint-disable @typescript-eslint/naming-convention */
import { watch } from "vue";
import { createPrefs, PersonSettingKey, PersonSettingResult, PrefsCallResult } from "../../../../../src/Communication/Chat/ChatShell/composables/usePrefs.partial";
import { StatusChange, StatusChoice, statusSaveOutcome } from "../../../../../src/Communication/Chat/ChatShell/composables/statusTabs.partial";
import { readOnlySentence as readOnlyWordsOfOurs } from "../../../../../src/Communication/Chat/ChatShell/errors.partial";
import { CallResult, createPlatformCall } from "../../../../../src/Communication/Chat/ChatShell/platformCall.partial";
import { createChannelStore } from "../../../../../src/Communication/Chat/ChatShell/stores/channelStore.partial";
import { ChatError, SidebarRow } from "../../../../../src/Communication/Chat/ChatShell/types.partial";
import { createChannelFake, createLockFake, setStatusAnswers, StatusSaveAnswer, statusSaveFailures } from "../statusShapes";

// #region Fixtures

const team = "c0000001-0000-4000-8000-000000000000";
const prayer = "c0000002-0000-4000-8000-000000000000";

const readOnlySentence = "Chat is read only for a few minutes while Cedar Hill updates it. Try again soon.";

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

/** A sidebar row in the exact shape chat_get_bootstrap answers. */
function sidebarRow(channelId: string, extra: Partial<SidebarRow> = {}): SidebarRow {
    return {
        channel_id: channelId,
        name: "Room",
        icon_url: null,
        channel_type: "shared",
        is_public: true,
        always_shown: false,
        is_member: true,
        is_favorite: false,
        notify_mode: "all",
        is_leader: false,
        can_post_announcements: false,
        can_mention_all: false,
        last_message_id: 10,
        last_message_at: "2026-10-19T09:00:00Z",
        last_message_type: "text",
        last_sender_nick_name: "Ada",
        last_sender_last_name: "Member",
        last_sender_avatar_url: null,
        last_sender_listed: true,
        last_message_preview: "old",
        last_message_is_own: false,
        last_message_posted_by_rock: false,
        read_cursor: 10,
        is_unread: false,
        mention_count: 0,
        all_mention_count: 0,
        can_mention: true,
        dm_people: null,
        dm_other_count: null,
        room_kind: "channel",
        title: extra.name ?? "Room",
        preview_line: "",
        ...extra
    };
}

/** chat_set_prefs's answer for a room, as it reads the rows back after writing them. */
function written(channelId: string, channel: { is_favorite?: boolean, hidden?: boolean, notify_mode?: string | null } = {}, me: Record<string, unknown> = {}): PrefsCallResult {
    const mode = channel.notify_mode ?? null;
    return {
        ok: true,
        data: {
            channel: {
                channel_id: channelId,
                is_favorite: channel.is_favorite ?? false,
                hidden: channel.hidden ?? false,
                notify_mode: mode,
                notify_mode_effective: mode ?? "all"
            },
            me: { show_profile_details: true, is_open_dm_allowed: false, reaction_push: true, ...me }
        }
    };
}

/** chat_set_prefs's answer with no room, as the reaction switch is written. */
function writtenMe(me: Record<string, unknown>): PrefsCallResult {
    return { ok: true, data: { channel: null, me: { show_profile_details: true, is_open_dm_allowed: false, reaction_push: true, ...me } } };
}

/** A refusal as the shell's call path hands it on: the platform's code and its own sentence. */
function refused(code = "rpc.read_only", text: string | null = readOnlySentence): PrefsCallResult {
    return { ok: false, error: { code, severity: "failed", text } };
}

/** The door's answer, as ChatPersonSettingResultBag reaches the browser. */
function door(code: string, isPending = false, message: string | null = null): PersonSettingResult {
    return { code, isPending, message };
}

type Setup = {
    prefs: ReturnType<typeof createPrefs>;
    channels: ReturnType<typeof createChannelStore>;
    prefCalls: Array<{ channelId: string | null, changes: Record<string, unknown>, answer: Deferred<PrefsCallResult> }>;
    doorCalls: Array<{ setting: PersonSettingKey, value: boolean, answer: Deferred<PersonSettingResult> }>;
    toasts: Array<{ code: string, text: string, icon: string }>;
    offline: { value: boolean };
};

function setup(rows: SidebarRow[] = [sidebarRow(team), sidebarRow(prayer, { name: "Prayer" })]): Setup {
    const prefCalls: Setup["prefCalls"] = [];
    const doorCalls: Setup["doorCalls"] = [];
    const toasts: Setup["toasts"] = [];
    const offline = { value: false };
    const channels = createChannelStore({ reloadSidebar: () => undefined, retryFirstLoad: () => undefined });
    channels.setSidebar(rows);

    const prefs = createPrefs({
        setPrefs: (channelId, changes) => {
            const answer = deferred<PrefsCallResult>();
            prefCalls.push({ channelId, changes, answer });
            return answer.promise;
        },
        savePersonSetting: (setting, value) => {
            const answer = deferred<PersonSettingResult>();
            doorCalls.push({ setting, value, answer });
            return answer.promise;
        },
        channels,
        isOffline: () => offline.value,
        notify: notice => toasts.push(notice)
    });

    return { prefs, channels, prefCalls, doorCalls, toasts, offline };
}

function rowOf(s: Setup, channelId: string): SidebarRow | undefined {
    return s.channels.rows.find(row => row.channel_id === channelId);
}

// #endregion Fixtures

describe("a room's notify mode", () => {
    test("shows at once and writes one key of chat_set_prefs for the room", async () => {
        const s = setup();

        void s.prefs.setNotifyMode(team, "mentions", false);

        expect(rowOf(s, team)?.notify_mode).toBe("mentions");
        expect(s.prefCalls).toEqual([expect.objectContaining({ channelId: team, changes: { notify_mode: "mentions" } })]);

        s.prefCalls[0].answer.resolve(written(team, { notify_mode: "mentions" }));
        await settle();

        // A save that lands says nothing: the control already shows it.
        expect(rowOf(s, team)?.notify_mode).toBe("mentions");
        expect(s.prefs.roomOf(team)).toEqual({ notes: {} });
        expect(s.prefs.state.announcement).toBeNull();
    });

    test("asks nothing when the mode chosen is the one the room already has", async () => {
        const s = setup();

        await s.prefs.setNotifyMode(team, "all", false);

        expect(s.prefCalls).toEqual([]);
        expect(s.prefs.roomOf(team)).toEqual({ notes: {} });
    });

    test("goes back as it was on a refusal, the server's own sentence under the choices", async () => {
        const s = setup();

        void s.prefs.setNotifyMode(team, "silent", false);
        s.prefCalls[0].answer.resolve(refused());
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("all");
        expect(s.prefs.roomOf(team).notes.notify).toEqual({ kind: "error", text: readOnlySentence });
        expect(s.prefs.state.announcement).toEqual({ text: `Not saved. ${readOnlySentence}`, isAssertive: true, count: expect.any(Number) });
    });

    test("Try again asks the refused choice again, shown at once, and clears the note", async () => {
        const s = setup();

        void s.prefs.setNotifyMode(team, "silent", false);
        s.prefCalls[0].answer.resolve(refused("rpc.busy", "Chat is busy."));
        await settle();

        s.prefs.roomOf(team).notes.notify?.retry?.();

        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(s.prefs.roomOf(team).notes.notify).toBeUndefined();
        expect(s.prefCalls[1].changes).toEqual({ notify_mode: "silent" });

        s.prefCalls[1].answer.resolve(written(team, { notify_mode: "silent" }));
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(s.prefs.roomOf(team).notes).toEqual({});
    });

    test("from the header's menu, says what changed in a toast, and a refusal's sentence in one", async () => {
        const s = setup();

        void s.prefs.setNotifyMode(team, "silent", true);
        s.prefCalls[0].answer.resolve(written(team, { notify_mode: "silent" }));
        await settle();

        expect(s.toasts.map(t => t.text)).toEqual(["Muted. A mention by name still reaches you."]);

        void s.prefs.setNotifyMode(team, "mentions", true);
        s.prefCalls[1].answer.resolve(written(team, { notify_mode: "mentions" }));
        await settle();
        void s.prefs.setNotifyMode(team, "all", true);
        s.prefCalls[2].answer.resolve(refused());
        await settle();

        expect(s.toasts.map(t => t.text)).toEqual([
            "Muted. A mention by name still reaches you.",
            "You'll be notified only when mentioned",
            readOnlySentence
        ]);
        expect(rowOf(s, team)?.notify_mode).toBe("mentions");
    });

    test("an older save's refusal moves nothing once the person has chosen again", async () => {
        const s = setup();

        void s.prefs.setNotifyMode(team, "mentions", false);
        void s.prefs.setNotifyMode(team, "silent", false);
        expect(s.prefCalls).toHaveLength(1);
        s.prefCalls[0].answer.resolve(refused());
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(s.prefs.roomOf(team).notes.notify).toBeUndefined();

        s.prefCalls[1].answer.resolve(written(team, { notify_mode: "silent" }));
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(s.prefs.roomOf(team).notes.notify).toBeUndefined();
    });

    // One save of a setting is out at a time, so the newest is never answered before an older one:
    // it waits, shown, and is asked once the older is answered.
    test("a save made while an older one is out waits for it, the older's landing moves nothing on screen, and the newer's refusal goes back to what the older landed", async () => {
        const s = setup();

        const mentions = s.prefs.setNotifyMode(team, "mentions", false);
        void s.prefs.setNotifyMode(team, "silent", false);

        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(s.prefCalls.map(c => c.changes)).toEqual([{ notify_mode: "mentions" }]);

        s.prefCalls[0].answer.resolve(written(team, { notify_mode: "mentions" }));
        await mentions;
        await settle();

        // The older's answer is not the newest, so it neither draws its mode nor says anything.
        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(s.prefs.roomOf(team).notes.notify).toBeUndefined();
        expect(s.prefCalls.map(c => c.changes)).toEqual([{ notify_mode: "mentions" }, { notify_mode: "silent" }]);

        s.prefCalls[1].answer.resolve(refused());
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("mentions");
        expect(s.prefs.roomOf(team).notes.notify?.kind).toBe("error");
    });

    test("the newest save refused after an older one landed goes back to the older one", async () => {
        const s = setup();

        void s.prefs.setNotifyMode(team, "mentions", false);
        void s.prefs.setNotifyMode(team, "silent", false);
        expect(s.prefCalls).toHaveLength(1);
        s.prefCalls[0].answer.resolve(written(team, { notify_mode: "mentions" }));
        await settle();
        s.prefCalls[1].answer.resolve(refused());
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("mentions");
    });

    test("both of two quick saves refused go back to where it started", async () => {
        const s = setup();

        void s.prefs.setNotifyMode(team, "mentions", false);
        void s.prefs.setNotifyMode(team, "silent", false);
        s.prefCalls[0].answer.resolve(refused());
        await settle();
        s.prefCalls[1].answer.resolve(refused());
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("all");
        expect(s.prefCalls.map(c => c.changes)).toEqual([{ notify_mode: "mentions" }, { notify_mode: "silent" }]);
    });

    test("quick changes send one flight, then only the newest, the skipped ones resolving false without a call", async () => {
        const s = setup();

        const first = s.prefs.setNotifyMode(team, "mentions", false);
        let isSkippedDone = false;
        void s.prefs.setNotifyMode(team, "silent", false).then(() => {
            isSkippedDone = true;
        });
        const newest = s.prefs.setNotifyMode(team, "all", false);
        await settle();

        expect(isSkippedDone).toBe(true);
        s.prefCalls[0].answer.resolve(written(team, { notify_mode: "mentions" }));
        await first;
        await settle();
        s.prefCalls[1].answer.resolve(written(team, { notify_mode: "all" }));
        await newest;

        expect(s.prefCalls.map(c => c.changes)).toEqual([{ notify_mode: "mentions" }, { notify_mode: "all" }]);
        expect(rowOf(s, team)?.notify_mode).toBe("all");
    });

    test("takes the effective mode the platform read back, so a cleared choice shows the room's default", async () => {
        const s = setup([sidebarRow(team, { notify_mode: "mentions" })]);

        void s.prefs.setNotifyMode(team, "silent", false);
        s.prefCalls[0].answer.resolve({
            ok: true,
            data: { channel: { channel_id: team, is_favorite: false, hidden: false, notify_mode: "silent", notify_mode_effective: "silent" }, me: {} }
        });
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("silent");
    });
});

describe("star and hide", () => {
    test("starring writes the room's whole sidebar setting and moves the room into Starred at once", async () => {
        const s = setup();

        void s.prefs.setStarred(prayer, true, false);

        expect(rowOf(s, prayer)?.is_favorite).toBe(true);
        // starred rooms sort first, as the platform's sidebar sorts them
        expect(s.channels.rows[0].channel_id).toBe(prayer);
        expect(s.prefCalls.map(c => c.changes)).toEqual([{ is_favorite: true, hidden: false }]);

        s.prefCalls[0].answer.resolve(written(prayer, { is_favorite: true }));
        await settle();

        expect(rowOf(s, prayer)?.is_favorite).toBe(true);
        expect(s.prefs.roomOf(prayer).notes).toEqual({});
        expect(s.prefs.state.announcement).toBeNull();
    });

    test("hiding a starred room unstars it in the same one patch, and says so under Hide", async () => {
        const s = setup([sidebarRow(team, { is_favorite: true })]);

        void s.prefs.setHidden(team, true);

        expect(s.prefCalls.map(c => c.changes)).toEqual([{ is_favorite: false, hidden: true }]);
        expect(rowOf(s, team)?.is_favorite).toBe(false);
        expect(rowOf(s, team)?.hidden).toBe(true);
        // What hiding means is said with the change, not after its save.
        expect(s.prefs.roomOf(team).notes.hide).toEqual({ kind: "info", text: "Hidden, and taken out of Starred. It leaves your list when you open another conversation." });

        s.prefCalls[0].answer.resolve(written(team, { hidden: true }));
        await settle();

        expect(s.prefs.roomOf(team).notes.hide).toEqual({ kind: "info", text: "Hidden, and taken out of Starred. It leaves your list when you open another conversation." });
        expect(s.prefs.state.announcement).toBeNull();
    });

    test("hiding a room that is not starred writes it hidden and still not starred", async () => {
        const s = setup();

        void s.prefs.setHidden(team, true);

        expect(s.prefCalls.map(c => c.changes)).toEqual([{ is_favorite: false, hidden: true }]);
        expect(s.prefs.roomOf(team).notes.hide).toEqual({ kind: "info", text: "Hidden. It leaves your list when you open another conversation." });
    });

    // The platform reads a key left out of the patch as unchanged, so the bytes sent are what
    // matters, not the object: toEqual would pass an undefined favorite that the wire drops.
    test("a hide and a star send the row's star and hidden both, by value, on the wire", async () => {
        const s = setup();

        void s.prefs.setHidden(team, true);
        s.prefCalls[0].answer.resolve(written(team, { hidden: true }));
        await settle();
        void s.prefs.setStarred(prayer, true, false);

        expect(s.prefCalls.map(c => JSON.stringify(c.changes))).toEqual([
            "{\"is_favorite\":false,\"hidden\":true}",
            "{\"is_favorite\":true,\"hidden\":false}"
        ]);
    });

    test("starring a hidden room unhides it in the same one patch, and says so under Star", async () => {
        const s = setup();
        void s.prefs.setHidden(team, true);
        s.prefCalls[0].answer.resolve(written(team, { hidden: true }));
        await settle();

        void s.prefs.setStarred(team, true, false);

        expect(s.prefCalls[1].changes).toEqual({ is_favorite: true, hidden: false });
        expect(rowOf(s, team)?.hidden).toBe(false);
        expect(s.prefs.roomOf(team).notes.star).toEqual({ kind: "info", text: "Starred, so it's back in your list." });
        expect(s.prefs.roomOf(team).notes.hide).toBeUndefined();
    });

    test("a refusal puts both switches back and carries the server's sentence under the one pressed", async () => {
        const s = setup([sidebarRow(team, { is_favorite: true })]);

        void s.prefs.setHidden(team, true);
        s.prefCalls[0].answer.resolve(refused());
        await settle();

        expect(rowOf(s, team)?.is_favorite).toBe(true);
        expect(rowOf(s, team)?.hidden).toBeFalsy();
        // The note said with the change gives way to the refusal.
        expect(s.prefs.roomOf(team).notes.hide).toEqual({ kind: "error", text: readOnlySentence });
    });

    // The star in the header shows it, so only a refusal is toasted.
    test("from the header, a star that lands says nothing, and a refusal's sentence is toasted", async () => {
        const s = setup();

        void s.prefs.setStarred(team, true, true);
        s.prefCalls[0].answer.resolve(written(team, { is_favorite: true }));
        await settle();
        void s.prefs.setStarred(team, false, true);
        s.prefCalls[1].answer.resolve(written(team));
        await settle();
        void s.prefs.setStarred(team, true, true);
        s.prefCalls[2].answer.resolve(refused());
        await settle();

        expect(s.toasts.map(t => [t.text, t.icon])).toEqual([[readOnlySentence, "alert"]]);
        expect(s.prefs.state.announcement?.text).toBe(`Not saved. ${readOnlySentence}`);
        expect(rowOf(s, team)?.is_favorite).toBe(false);
    });

    test("a lost connection's refusal says the person is offline", async () => {
        const s = setup();
        s.offline.value = true;

        void s.prefs.setStarred(team, true, false);
        s.prefCalls[0].answer.resolve(refused("rpc.transport", null));
        await settle();

        expect(s.prefs.roomOf(team).notes.star).toEqual({ kind: "error", text: "You're offline.", retry: expect.any(Function) });
    });

    test("a call that reached nothing while the connection holds is not called offline", async () => {
        const s = setup();

        void s.prefs.setStarred(team, true, false);
        s.prefCalls[0].answer.resolve(refused("rpc.transport", null));
        await settle();

        expect(s.prefs.roomOf(team).notes.star).toEqual({ kind: "error", text: "Something went wrong in chat.", retry: expect.any(Function) });
    });

    // Star and hide are one save's two halves, so what is known saved is both of them.
    test("a star that lands and then a hide refused goes back to what landed: starred and not hidden", async () => {
        const s = setup();

        void s.prefs.setStarred(team, true, false);
        void s.prefs.setHidden(team, true);
        expect(s.prefCalls).toHaveLength(1);
        s.prefCalls[0].answer.resolve(written(team, { is_favorite: true }));
        await settle();
        expect(s.prefCalls[1].changes).toEqual({ is_favorite: false, hidden: true });
        s.prefCalls[1].answer.resolve(refused());
        await settle();

        expect(rowOf(s, team)?.is_favorite).toBe(true);
        expect(rowOf(s, team)?.hidden).toBe(false);
    });

    // The newest save is the whole value shown, so it lands what is shown whatever an older one
    // does: here the unhide is refused, but the star after it carried the unhide too.
    test("an unhide still out and then a star: the star asks for the room shown, starred and not hidden", async () => {
        const s = setup([sidebarRow(team, { hidden: true }), sidebarRow(prayer, { name: "Prayer" })]);

        void s.prefs.setHidden(team, false);
        void s.prefs.setStarred(team, true, false);
        expect(s.prefCalls).toHaveLength(1);
        s.prefCalls[0].answer.resolve(refused());
        await settle();

        // The unhide's refusal is not the newest, so it puts nothing back while the star waits.
        expect(rowOf(s, team)?.hidden).toBe(false);
        expect(s.prefCalls[1].changes).toEqual({ is_favorite: true, hidden: false });
        s.prefCalls[1].answer.resolve(written(team, { is_favorite: true }));
        await settle();

        expect(rowOf(s, team)?.is_favorite).toBe(true);
        expect(rowOf(s, team)?.hidden).toBe(false);
    });

    test("a save whose call throws goes back, says it couldn't save with Try again, and is no longer out", async () => {
        const s = setup();

        void s.prefs.setStarred(team, true, false);
        s.prefCalls[0].answer.reject(new Error("lost"));
        await settle();

        expect(rowOf(s, team)?.is_favorite).toBe(false);
        expect(s.prefs.roomOf(team).notes.star).toEqual({ kind: "error", text: "Couldn't save.", retry: expect.any(Function) });

        // Nothing is out, so the next load is taken as read.
        s.prefs.setRooms([sidebarRow(team, { is_favorite: true }), sidebarRow(prayer, { name: "Prayer" })]);
        expect(rowOf(s, team)?.is_favorite).toBe(true);
    });
});

describe("a sidebar load", () => {
    test("while a room's save is out leaves that room's setting as the person chose it, in its place in the list", async () => {
        const s = setup();

        void s.prefs.setStarred(prayer, true, false);
        void s.prefs.setNotifyMode(team, "silent", false);
        void s.prefs.setHidden(team, true);

        // A load read before the saves landed.
        s.prefs.setRooms([sidebarRow(team, { last_message_id: 11 }), sidebarRow(prayer, { name: "Prayer" })]);

        expect(rowOf(s, team)?.last_message_id).toBe(11);
        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(rowOf(s, team)?.hidden).toBe(true);
        expect(rowOf(s, prayer)?.is_favorite).toBe(true);
        expect(s.channels.rows[0].channel_id).toBe(prayer);
    });

    test("once the saves are answered takes the rooms as read", async () => {
        const s = setup();

        void s.prefs.setStarred(prayer, true, false);
        void s.prefs.setNotifyMode(team, "silent", false);
        s.prefCalls[0].answer.resolve(written(prayer, { is_favorite: true }));
        s.prefCalls[1].answer.resolve(refused());
        await settle();

        s.prefs.setRooms([sidebarRow(team, { notify_mode: "mentions" }), sidebarRow(prayer, { name: "Prayer" })]);

        expect(rowOf(s, team)?.notify_mode).toBe("mentions");
        expect(rowOf(s, prayer)?.is_favorite).toBe(false);
    });
});

describe("the person's own switches", () => {
    test("start from the bootstrap's me", () => {
        const s = setup();

        s.prefs.setMe({ show_profile_details: false, is_open_dm_allowed: true, reaction_push: false, unread_mentions: [] } as never);

        expect(s.prefs.state.me).toEqual({ show_profile_details: false, is_open_dm_allowed: true, reaction_push: false, busy_until: null, appear_offline: false });
    });

    test("her Busy and Appear offline come from the bootstrap's me at every load, a Busy with no end as infinity, and anything else as none", () => {
        const s = setup();

        s.prefs.setMe({ busy_until: "2026-11-10T16:00:00.000000+00:00", appear_offline: true });
        expect(s.prefs.state.me).toEqual(expect.objectContaining({ busy_until: "2026-11-10T16:00:00.000000+00:00", appear_offline: true }));

        s.prefs.setMe({ busy_until: "infinity", appear_offline: false });
        expect(s.prefs.state.me).toEqual(expect.objectContaining({ busy_until: "infinity", appear_offline: false }));

        s.prefs.setMe({ busy_until: 5, appear_offline: "yes" } as never);
        expect(s.prefs.state.me).toEqual(expect.objectContaining({ busy_until: null, appear_offline: false }));

        s.prefs.setMe({ busy_until: "infinity" });
        s.prefs.setMe({ show_profile_details: true });
        expect(s.prefs.state.me).toEqual(expect.objectContaining({ busy_until: null, appear_offline: false }));
    });

    test("are read only once a me has been stated, and are already hers when that says so", () => {
        const s = setup();
        const seen: Array<boolean | string | null> = [];
        const stop = watch(() => s.prefs.state.isMeRead, read => seen.push(read, s.prefs.state.me.appear_offline, s.prefs.state.me.busy_until), { flush: "sync" });
        expect(s.prefs.state.isMeRead).toBe(false);

        s.prefs.setMe(null);
        expect(s.prefs.state.isMeRead).toBe(false);

        s.prefs.setMe({ busy_until: "infinity", appear_offline: true });
        stop();

        expect(seen).toEqual([true, true, "infinity"]);
    });

    test("the reaction switch writes chat_set_prefs with no room", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setReactionPush(false);

        expect(s.prefs.state.me.reaction_push).toBe(false);
        expect(s.prefCalls).toEqual([expect.objectContaining({ channelId: null, changes: { reaction_push: false } })]);

        s.prefCalls[0].answer.resolve(writtenMe({ reaction_push: false }));
        await settle();

        expect(s.prefs.state.me.reaction_push).toBe(false);
        expect(s.prefs.state.meNotes).toEqual({});
        expect(s.prefs.state.announcement).toBeNull();
    });

    test("one switch refused leaves another switch saved as the person set it", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setReactionPush(false);
        void s.prefs.setPersonSetting("open_dm", true);
        s.doorCalls[0].answer.resolve(door("ok"));
        s.prefCalls[0].answer.resolve(refused());
        await settle();

        expect(s.prefs.state.me).toEqual({ show_profile_details: true, is_open_dm_allowed: true, reaction_push: true, busy_until: null, appear_offline: false });
        expect(Object.keys(s.prefs.state.meNotes)).toEqual(["reaction_push"]);
    });

    test("Try again on a switch asks its refused value again, shown at once", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setPersonSetting("open_dm", true);
        s.doorCalls[0].answer.resolve(door("door.unavailable"));
        await settle();
        expect(s.prefs.state.me.is_open_dm_allowed).toBe(false);

        s.prefs.state.meNotes.open_dm?.retry?.();

        expect(s.prefs.state.me.is_open_dm_allowed).toBe(true);
        expect(s.prefs.state.meNotes.open_dm).toBeUndefined();
        expect(s.doorCalls.map(c => [c.setting, c.value])).toEqual([["open_dm", true], ["open_dm", true]]);
    });

    test("a sidebar load while a switch's save is out leaves that switch as the person set it", () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setPersonSetting("open_dm", true);
        s.prefs.setMe({ show_profile_details: false, is_open_dm_allowed: false, reaction_push: false });

        expect(s.prefs.state.me).toEqual({ show_profile_details: false, is_open_dm_allowed: true, reaction_push: false, busy_until: null, appear_offline: false });
    });

    test("a privacy switch goes through Rock's door, one setting a call", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setPersonSetting("open_dm", true);

        expect(s.prefs.state.me.is_open_dm_allowed).toBe(true);
        expect(s.doorCalls.map(c => [c.setting, c.value])).toEqual([["open_dm", true]]);
        expect(s.prefCalls).toEqual([]);

        s.doorCalls[0].answer.resolve(door("ok"));
        await settle();

        expect(s.prefs.state.me.is_open_dm_allowed).toBe(true);
        expect(s.prefs.state.announcement).toBeNull();
    });

    test("a door that answers pending has saved, so it shows nothing more either", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setPersonSetting("profile_details", false);
        s.doorCalls[0].answer.resolve(door("ok", true));
        await settle();

        expect(s.prefs.state.me.show_profile_details).toBe(false);
        expect(s.prefs.state.meNotes).toEqual({});
        expect(s.prefs.state.announcement).toBeNull();
    });

    test("goes back on the door's refusal with the door's own sentence", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setPersonSetting("profile_details", false);
        s.doorCalls[0].answer.resolve(door("age_restricted", false, "Chat is not available to you right now."));
        await settle();

        expect(s.prefs.state.me.show_profile_details).toBe(true);
        expect(s.prefs.state.meNotes.profile_details).toEqual({ kind: "error", text: "Chat is not available to you right now." });
        expect(s.prefs.state.announcement?.text).toBe("Not saved. Chat is not available to you right now.");
    });

    test("goes back when Rock answered without its door, saying to try again, or offline while the connection is lost", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setPersonSetting("open_dm", true);
        s.doorCalls[0].answer.resolve(door("door.unavailable"));
        await settle();

        expect(s.prefs.state.me.is_open_dm_allowed).toBe(false);
        expect(s.prefs.state.meNotes.open_dm).toEqual({ kind: "error", text: "Couldn't save.", retry: expect.any(Function) });

        s.offline.value = true;
        void s.prefs.setPersonSetting("open_dm", true);
        s.doorCalls[1].answer.resolve(door("door.unavailable"));
        await settle();

        expect(s.prefs.state.meNotes.open_dm).toEqual({ kind: "error", text: "You're offline.", retry: expect.any(Function) });
    });

    test("a page with no door refuses a privacy switch and leaves it as it was", async () => {
        const channels = createChannelStore({ reloadSidebar: () => undefined, retryFirstLoad: () => undefined });
        const prefs = createPrefs({ setPrefs: async () => writtenMe({}), channels, isOffline: () => false, notify: () => undefined });
        prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        await prefs.setPersonSetting("open_dm", true);

        expect(prefs.state.me.is_open_dm_allowed).toBe(false);
        expect(prefs.state.meNotes.open_dm?.kind).toBe("error");
    });
});

// The block hands every room control's choice here as it came: the details say so, the header's
// menus leave it out, which tells the save in a toast; the header's Star turns the room's star
// over from its row. Chat settings' switches go where each is owned.
describe("a room's choice and Chat settings' switches", () => {
    test("a menu's notify mode is told in a toast and the details' is not", async () => {
        const s = setup();

        void s.prefs.choose(team, { kind: "notify", mode: "mentions" });
        s.prefCalls[0].answer.resolve(written(team, { notify_mode: "mentions" }));
        await settle();
        void s.prefs.choose(prayer, { kind: "notify", mode: "silent", isFromMenu: false });
        s.prefCalls[1].answer.resolve(written(prayer, { notify_mode: "silent" }));
        await settle();

        expect(s.prefCalls.map(c => [c.channelId, c.changes])).toEqual([[team, { notify_mode: "mentions" }], [prayer, { notify_mode: "silent" }]]);
        expect(s.toasts.map(t => t.text)).toEqual(["You'll be notified only when mentioned"]);
        expect(s.prefs.roomOf(prayer).notes).toEqual({});
    });

    test("Star with no value turns the room's star over, and a value is kept as given", async () => {
        const s = setup([sidebarRow(team, { is_favorite: true }), sidebarRow(prayer)]);

        void s.prefs.choose(team, { kind: "star" });
        void s.prefs.choose(prayer, { kind: "star" });
        void s.prefs.choose(prayer, { kind: "star", isStarred: true, isFromMenu: false });
        // the second change of one room's star waits for the first's answer
        s.prefCalls[1].answer.resolve(written(prayer, { is_favorite: true }));
        await settle();

        expect(s.prefCalls.map(c => [c.channelId, c.changes])).toEqual([
            [team, { is_favorite: false, hidden: false }],
            [prayer, { is_favorite: true, hidden: false }],
            [prayer, { is_favorite: true, hidden: false }]
        ]);
    });

    test("hide writes Hide, and with no room open nothing is asked", () => {
        const s = setup();

        void s.prefs.choose(null, { kind: "notify", mode: "silent" });
        void s.prefs.choose(team, { kind: "hide", isHidden: true });

        expect(s.prefCalls.map(c => [c.channelId, c.changes])).toEqual([[team, { is_favorite: false, hidden: true }]]);
    });

    test("the reaction switch goes to the platform and the privacy switches to Rock's door", () => {
        const s = setup();

        void s.prefs.setSwitch("reaction_push", false);
        void s.prefs.setSwitch("open_dm", true);
        void s.prefs.setSwitch("profile_details", false);

        expect(s.prefCalls.map(c => [c.channelId, c.changes])).toEqual([[null, { reaction_push: false }]]);
        expect(s.doorCalls.map(c => [c.setting, c.value])).toEqual([["open_dm", true], ["profile_details", false]]);
    });
});

describe("once stopped", () => {
    test("no answer still on its way reaches the screen, and nothing more is asked", async () => {
        const s = setup();
        s.prefs.setMe({ show_profile_details: true, is_open_dm_allowed: false, reaction_push: true });

        void s.prefs.setNotifyMode(team, "silent", false);
        void s.prefs.setPersonSetting("open_dm", true);
        s.prefs.stop();
        s.prefCalls[0].answer.resolve(refused());
        s.doorCalls[0].answer.resolve(door("ok", true));
        await settle();

        expect(rowOf(s, team)?.notify_mode).toBe("silent");
        expect(s.prefs.roomOf(team).notes).toEqual({});
        expect(s.prefs.state.me.is_open_dm_allowed).toBe(true);
        expect(s.prefs.state.announcement).toBeNull();
        expect(s.toasts).toEqual([]);

        await s.prefs.setStarred(team, true, false);
        expect(s.prefCalls).toHaveLength(1);
    });

    test("a change waiting behind a save still out is never asked, and resolves", async () => {
        const s = setup();

        void s.prefs.setNotifyMode(team, "silent", false);
        const waiting = s.prefs.setNotifyMode(team, "mentions", false);
        s.prefs.stop();
        s.prefCalls[0].answer.resolve(written(team, { notify_mode: "silent" }));
        await settle();

        await waiting;
        expect(s.prefCalls).toHaveLength(1);
    });
});

// #region A status save's answer

/** The answer as the shell's one call path reads it off the wire, through the real classification. */
async function statusCallResult(answer: StatusSaveAnswer): Promise<CallResult> {
    const controller = new AbortController();
    const call = createPlatformCall({
        fetch: async (_to, init) => {
            if ("unreached" in answer) {
                if (answer.unreached === "network") {
                    throw new TypeError("Failed to fetch");
                }
                if (answer.unreached === "deadline") {
                    return new Promise<Response>((_, reject) => {
                        init.signal?.addEventListener("abort", () => reject(new DOMException("aborted", "AbortError")));
                        controller.abort();
                    });
                }
                return {
                    ok: true,
                    status: 200,
                    headers: { get: () => null },
                    json: () => new Promise((_, reject) => {
                        init.signal?.addEventListener("abort", () => reject(new DOMException("aborted", "AbortError")));
                        controller.abort();
                    })
                } as unknown as Response;
            }

            const headers = answer.headers ?? {};
            return {
                ok: answer.status >= 200 && answer.status < 300,
                status: answer.status,
                headers: { get: (name: string) => headers[name.toLowerCase()] ?? null },
                json: async () => answer.body === null ? Promise.reject(new SyntaxError("Unexpected token <")) : answer.body
            } as unknown as Response;
        },
        projectUrl: "http://platform",
        publishableKey: "k",
        currentToken: () => "platform-token",
        routes: () => ({ send: "direct" })
    });

    return call.call("chat_set_status", { p_client: null, p_change: { status: "busy" } }, { signal: controller.signal });
}

/** The backoff's wait for a failure count and a random draw, to the millisecond: 10, 20, 40, then every 60 s, times 0.8 to 1.2. */
function backoffMs(failures: number, random: number): number {
    return Math.round(Math.min(60_000, 10_000 * 2 ** (failures - 1)) * (0.8 + 0.4 * random));
}

describe("a status save's answer", () => {
    test("a landing is saved, with what the server read back", () => {
        expect(statusSaveOutcome({ ok: true, data: setStatusAnswers.unchanged }, 0)).toEqual({ isSaved: true, data: setStatusAnswers.unchanged });
    });

    // None of these is the server's answer about the choice: the network, the deadline, the token,
    // the path or the platform, so the choice stays pending and is asked again on the backoff.
    test.each([
        ["no network", "network"],
        ["the deadline's abort", "deadline"],
        ["the deadline's abort while the answer is read", "deadlineWhileReading"],
        ["a 401 for an expired token", "expired"],
        ["a proxy's 408", "proxyTimeout"],
        ["a 500", "serverError"],
        ["a 503 for maintenance", "maintenance"],
        ["the status rule's own 429, which names no Retry-After", "rateLimited"]
    ] as const)("%s is asked again after the backoff for the failure count, spread by its random draw", async (_name, shape) => {
        const result = await statusCallResult(statusSaveFailures[shape]);

        for (const failures of [1, 2, 3, 4, 7]) {
            for (const random of [0, 0.5, 0.999]) {
                const outcome = statusSaveOutcome(result, failures, () => random);
                expect(outcome).toMatchObject({ isSaved: false });
                expect((outcome as { retryInMs?: number }).retryInMs).toBe(backoffMs(failures, random));
            }
        }
    });

    test("the backoff grows 10, 20, 40, then holds at 60 s, each inside 0.8 to 1.2 of its step", async () => {
        const result = await statusCallResult(statusSaveFailures.network);
        const waits = (random: number): number[] => [1, 2, 3, 4, 5].map(n => (statusSaveOutcome(result, n, () => random) as { retryInMs: number }).retryInMs);

        expect(waits(0)).toEqual([8_000, 16_000, 32_000, 48_000, 48_000]);
        expect(waits(0.5)).toEqual([10_000, 20_000, 40_000, 60_000, 60_000]);
        expect(waits(1)).toEqual([12_000, 24_000, 48_000, 72_000, 72_000]);
    });

    test("a gateway's 429 with no code of ours is asked again after the longer of the backoff and its Retry-After", async () => {
        const result = await statusCallResult(statusSaveFailures.gatewayRateLimited);
        expect(result).toMatchObject({ ok: false, status: 429, error: { code: "rpc.unknown" }, retryAfterSeconds: 90 });

        expect(statusSaveOutcome(result, 1, () => 0.5)).toMatchObject({ isSaved: false, retryInMs: 90_000 });
        expect(statusSaveOutcome(result, 4, () => 1)).toMatchObject({ isSaved: false, retryInMs: 90_000 });

        const shortWait = await statusCallResult({ ...statusSaveFailures.gatewayRateLimited, headers: { "retry-after": "5" } });
        expect(statusSaveOutcome(shortWait, 2, () => 0.5)).toMatchObject({ isSaved: false, retryInMs: 20_000 });
    });

    // Read only refuses the save through the writer check while her pages still beat, so asking
    // again would leave her own faces on a choice viewers never see: it is refused, in the words
    // the banner says.
    test.each([
        ["a church's banner", "readOnlyBanner", "Chat is read only tonight while Cedar Hill moves to its new server."],
        ["the server's own sentence", "readOnlyServerSentence", "Chat is read only right now. You can read messages but not send them."],
        ["no sentence at all", "readOnlyNoHint", readOnlyWordsOfOurs]
    ] as const)("a 503 for read only with %s is refused in the banner's words and never asked again", async (_name, shape, words) => {
        const result = await statusCallResult(statusSaveFailures[shape]);

        const outcome = statusSaveOutcome(result, 1, () => 0.5);

        expect(outcome).toEqual({ isSaved: false, failure: { alone: words, besideRetry: words, canRetry: false } });
    });

    test.each([
        ["a Busy end that has passed", "badRequest", "Chat could not understand that request."],
        ["a person who may not set a status", "notLivePerson", "Chat isn't available for your account."]
    ] as const)("the platform's answer about the choice, %s, is refused with its own words", async (_name, shape, words) => {
        const outcome = statusSaveOutcome(await statusCallResult(statusSaveFailures[shape]), 1, () => 0.5);

        expect(outcome).toEqual({ isSaved: false, failure: { alone: words, besideRetry: words, canRetry: false } });
    });

    // Asking again would only meet the same answer: the same schema cache until a reload, the same
    // proxy rule for the same request.
    test.each([
        ["a 404 from a schema cache that has not loaded the function", "notInSchemaCache"],
        ["a proxy's 403", "proxyForbidden"],
        ["a proxy's 413", "proxyTooLarge"]
    ] as const)("%s, with no code of ours, is refused by name with the generic sentence", async (_name, shape) => {
        const outcome = statusSaveOutcome(await statusCallResult(statusSaveFailures[shape]), 1, () => 0.5);

        expect(outcome).toEqual({ isSaved: false, failure: expect.objectContaining({ alone: "Something went wrong in chat. Try again in a moment." }) });
        expect("retryInMs" in outcome).toBe(false);
    });

    // A count another tab posted may be missing or malformed; each reads as the first failure, so
    // the wait is never NaN, which the rule would ask again at once.
    test.each([
        ["missing", undefined],
        ["not a number", NaN],
        ["endless", Infinity],
        ["zero", 0],
        ["below zero", -3]
    ])("a failure count %s waits as the first failure does", async (_name, failures) => {
        const result = await statusCallResult(statusSaveFailures.network);

        expect(statusSaveOutcome(result, failures as number, () => 0.5)).toMatchObject({ isSaved: false, retryInMs: backoffMs(1, 0.5) });
    });

    test("a failure count with a fraction counts its whole failures", async () => {
        const result = await statusCallResult(statusSaveFailures.network);

        expect(statusSaveOutcome(result, 2.7, () => 0.5)).toMatchObject({ isSaved: false, retryInMs: backoffMs(2, 0.5) });
    });

    test("a 429's Retry-After is held to five minutes, however long a proxy names", () => {
        const result: CallResult = { ok: false, status: 429, error: { code: "rpc.unknown", severity: "failed", text: null }, retryAfterSeconds: 3_000_000 };

        expect(statusSaveOutcome(result, 1, () => 0.5)).toMatchObject({ isSaved: false, retryInMs: 300_000 });
    });

    test("a refusal of a code this release does not know, carrying a sentence, is shown in that sentence", () => {
        const outcome = statusSaveOutcome({ ok: false, status: 409, error: { code: "rpc.status_conflict", severity: "failed", text: "Your status changed somewhere else." }, retryAfterSeconds: null }, 1);

        expect(outcome).toEqual({ isSaved: false, failure: expect.objectContaining({ alone: "Your status changed somewhere else." }) });
        expect("retryInMs" in outcome).toBe(false);
    });
});

// #endregion A status save's answer

// #region Her status across her tabs

const her = "A0000001-0000-4000-8000-00000000000A";
const church = "C1000000-0000-4000-8000-000000000001";
const lockName = `rock-chat:status:${church}:${her}`.toLowerCase();
// the channel's name carries the version of its posts, so a release that changes them opens another
const channelName = `rock-chat:me:v1:${church}:${her}`.toLowerCase();
const someoneElse = "a0000099-0000-4000-8000-000000000000";

const automatic: StatusChoice = { busy_until: null, appear_offline: false };
const hidden: StatusChoice = { busy_until: null, appear_offline: true };
const busy: StatusChoice = { busy_until: "infinity", appear_offline: false };

/** chat_set_status's landing for a Busy with no end, and for a choice the server already held. */
const landed: CallResult = { ok: true, data: { status: "busy" } };
const alreadyHeld: CallResult = { ok: true, data: { status: "appear_offline", changed: false } };

type StatusCall = { change: StatusChange, signal: AbortSignal, answer: Deferred<CallResult>, tab: number, at: number };

type StatusTab = {
    n: number;
    prefs: ReturnType<typeof createPrefs>;
    toasts: Array<{ code: string, text: string, icon: string }>;
    refusals: ChatError[];
    person: { value: string | null };
    fire: (type: string, event: unknown) => void;

    /** The tab closes: its locks go as the browser lets them go, and the page runs nothing more. */
    close: () => void;
};

type StatusBrowser = {
    open: () => StatusTab;
    calls: StatusCall[];
    posts: Array<{ name: string, data: unknown }>;
    raw: () => ReturnType<ReturnType<typeof createChannelFake>["open"]>;
    locks: ReturnType<typeof createLockFake>;
    openChannels: () => number;
};

/** One browser of hers: tabs of chat sharing one origin's Web Locks and BroadcastChannel, and every status call any of them asks. */
function statusBrowser(options: { hasLocks?: boolean, locksRefuse?: boolean } = {}): StatusBrowser {
    const locks = createLockFake();
    const channels = createChannelFake();
    const calls: StatusCall[] = [];
    const start = Date.now();
    let tabs = 0;

    function open(): StatusTab {
        const n = ++tabs;
        const tabLocks = locks.tab();
        const listeners = new Map<string, Set<(event: unknown) => void>>();
        const person = { value: her.toLowerCase() as string | null };
        const toasts: StatusTab["toasts"] = [];
        const refusals: ChatError[] = [];
        const store = createChannelStore({ reloadSidebar: () => undefined, retryFirstLoad: () => undefined });
        store.setSidebar([]);
        const prefs = createPrefs({
            setPrefs: () => new Promise(() => undefined),
            channels: store,
            isOffline: () => false,
            notify: notice => toasts.push(notice),
            status: {
                tenantId: church,
                personAliasGuid: her,
                currentPerson: () => person.value,
                saveStatus: (change, signal) => {
                    const answer = deferred<CallResult>();
                    // an aborted request reaches nothing, as the browser's fetch ends one
                    signal.addEventListener("abort", () => void statusCallResult(statusSaveFailures.deadline).then(answer.resolve));
                    calls.push({ change, signal, answer, tab: n, at: Date.now() - start });
                    return answer.promise;
                },
                applyRefusal: error => refusals.push(error),
                locks: options.hasLocks === false
                    ? undefined
                    // a browser that refuses Web Locks to the page, as on an opaque origin
                    : options.locksRefuse ? { request: () => Promise.reject(new DOMException("The request was denied.", "SecurityError")) } : tabLocks,
                channel: name => channels.open(name),
                targets: {
                    addEventListener: (type, listener) => {
                        const set = listeners.get(type) ?? new Set();
                        set.add(listener);
                        listeners.set(type, set);
                    },
                    removeEventListener: (type, listener) => {
                        listeners.get(type)?.delete(listener);
                    }
                }
            }
        });
        return {
            n,
            prefs,
            toasts,
            refusals,
            person,
            fire: (type, event) => listeners.get(type)?.forEach(listener => listener(event)),
            close: () => {
                tabLocks.close();
                prefs.stop();
            }
        };
    }

    return { open, calls, posts: channels.posts, raw: () => channels.open(channelName), locks, openChannels: channels.openCount };
}

/** Moves the page's clocks on, running every timer and what each resolves on the way. */
async function after(ms: number): Promise<void> {
    await jest.advanceTimersByTimeAsync(ms);
}

/**
 * Lets a post and the post it is answered with arrive, each on a later task: the fake clock runs a
 * task made during a tick a millisecond on, so this moves the clock one millisecond.
 */
async function delivered(): Promise<void> {
    await after(1);
}

/** Her settings as a sidebar load reads them, with her status as given. */
function sidebarMe(choice: StatusChoice): Record<string, unknown> {
    return { show_profile_details: false, is_open_dm_allowed: false, reaction_push: true, ...choice };
}

type Shown = { status: StatusChoice, isPending: boolean };

/** What a tab shows of her status and whether it is a choice not yet saved. */
function shown(tab: StatusTab): Shown {
    return { status: tab.prefs.shownStatus(), isPending: tab.prefs.isChoicePending() };
}

const settled = (status: StatusChoice): Shown => ({ status, isPending: false });
const pending = (status: StatusChoice): Shown => ({ status, isPending: true });

/** The kinds of every post on her channel, in order. */
function kinds(b: StatusBrowser): string[] {
    return b.posts.map(p => (p.data as { kind: string }).kind);
}

describe("her status across her tabs", () => {
    beforeEach(() => {
        // The backoff is left its own draws, which also mint each tab's id: no cell waits on a
        // retry's exact time, the first being 8 to 12 s after the failure.
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.restoreAllMocks();
        jest.useRealTimers();
    });

    test("a choice is sent once its pause has passed, and shows as chosen through it", async () => {
        const b = statusBrowser();
        const a = b.open();
        a.prefs.setMe(sidebarMe(automatic));

        await a.prefs.setStatus(hidden);
        expect(shown(a)).toEqual(pending(hidden));
        await after(999);
        expect(b.calls).toHaveLength(0);
        await after(1);

        expect(b.calls.map(c => c.change)).toEqual([{ status: "appear_offline" }]);
        b.calls[0].answer.resolve(alreadyHeld);
        await after(0);
        expect(shown(a)).toEqual(settled(hidden));
    });

    test.each([
        ["Automatic", automatic, { status: "auto" }],
        ["Busy with no end", busy, { status: "busy" }],
        ["Busy to an end", { busy_until: "2026-11-11T17:00:00.000Z", appear_offline: false }, { status: "busy", busy_until: "2026-11-11T17:00:00.000Z" }],
        ["Appear offline", hidden, { status: "appear_offline" }]
    ] as const)("%s is sent as chat_set_status names it", async (_name, choice, change) => {
        const b = statusBrowser();
        const a = b.open();

        await a.prefs.setStatus(choice);
        await after(1000);

        expect(b.calls.map(c => c.change)).toEqual([change]);
    });

    test("a sidebar me landing inside the pause keeps the choice shown, and so does one while it is out", async () => {
        const b = statusBrowser();
        const a = b.open();
        a.prefs.setMe(sidebarMe(automatic));

        await a.prefs.setStatus(hidden);
        await after(500);
        a.prefs.setMe(sidebarMe(automatic));
        expect(shown(a)).toEqual(pending(hidden));

        await after(500);
        a.prefs.setMe(sidebarMe(automatic));
        expect(shown(a)).toEqual(pending(hidden));
        b.calls[0].answer.resolve(landed);
        await after(0);
        expect(shown(a)).toEqual(settled(hidden));
    });

    test("the holder waits out the newest record's pause inside the lock, so a choice made in another tab meanwhile is the one call", async () => {
        const b = statusBrowser();
        const a = b.open();
        const t2 = b.open();
        // a slow holder before them, as a hung save of another tab would be
        let free!: () => void;
        void b.locks.tab().request(lockName, {}, () => new Promise<void>(resolve => free = resolve));

        await a.prefs.setStatus(busy);
        await after(1100);
        await t2.prefs.setStatus(automatic);
        await after(100);
        free();
        await after(0);
        expect(b.calls).toHaveLength(0);

        await after(900);
        expect(b.calls.map(c => [c.tab, c.change, c.at])).toEqual([[a.n, { status: "auto" }, 2100]]);
    });

    test("a record heard during the holder's wait waits its own pause, and the newest is what goes", async () => {
        const b = statusBrowser();
        const a = b.open();
        const t2 = b.open();
        let free!: () => void;
        void b.locks.tab().request(lockName, {}, () => new Promise<void>(resolve => free = resolve));

        await a.prefs.setStatus(busy);
        await after(1100);
        await t2.prefs.setStatus(automatic);
        await after(100);
        free();
        await after(300);
        await t2.prefs.setStatus(hidden);
        await after(999);
        expect(b.calls).toHaveLength(0);
        await after(1);

        expect(b.calls.map(c => [c.tab, c.change, c.at])).toEqual([[a.n, { status: "appear_offline" }, 2500]]);
    });

    test("a record taken from a hello is due 3 s after it was taken, not at once", async () => {
        const b = statusBrowser();
        const a = b.open();
        await a.prefs.setStatus(hidden);
        await after(500);

        const c = b.open();
        await delivered();
        expect(shown(c)).toEqual(pending(hidden));
        // A closes inside its pause, so only C's copy is left to send
        a.close();
        await after(2999);
        expect(b.calls).toHaveLength(0);
        await after(1);

        // taken a millisecond after the open, as the answer to its hello arrived
        expect(b.calls.map(call => [call.tab, call.at])).toEqual([[c.n, 3501]]);
    });

    test("the lock's holder sending another tab's record posts what it sent, and the chooser settles on it", async () => {
        const b = statusBrowser();
        const a = b.open();
        const t2 = b.open();
        // the chooser's session names another person at its grant, so it sends nothing and the other tab's copy goes
        t2.person.value = someoneElse;
        await t2.prefs.setStatus(busy);
        await after(1000);
        t2.person.value = her.toLowerCase();
        await after(2000);

        expect(b.calls.map(c => c.tab)).toEqual([a.n]);
        b.calls[0].answer.resolve({ ok: true, data: { status: "busy", changed: false } });
        await after(0);

        const saved = b.posts.map(p => p.data as Record<string, unknown>).filter(d => d.kind === "saved");
        expect(saved).toEqual([{ kind: "saved", choiceAt: expect.any(Number), me: busy }]);
        expect(shown(t2)).toEqual(settled(busy));
        expect(shown(a)).toEqual(settled(busy));
    });

    test("the saved post goes before the lock is released", async () => {
        const b = statusBrowser();
        const a = b.open();
        await a.prefs.setStatus(busy);
        await after(1000);

        let postsAtGrant: string[] | null = null;
        void b.locks.tab().request(lockName, {}, async () => {
            postsAtGrant = kinds(b);
        });
        b.calls[0].answer.resolve(landed);
        await after(0);

        expect(postsAtGrant).toEqual(["hello", "choice", "saved"]);
    });

    test("a holder whose session now names another person sends nothing", async () => {
        const b = statusBrowser();
        const a = b.open();
        a.person.value = "b0000009-0000-4000-8000-000000000009";

        await a.prefs.setStatus(busy);
        await after(60_000);

        expect(b.calls).toHaveLength(0);
    });

    test("A chooses then closes inside the pause: B sends it once after its hand-off and settles, and C settles from B's saved", async () => {
        const b = statusBrowser();
        const a = b.open();
        const t2 = b.open();
        const c = b.open();
        for (const tab of [a, t2, c]) {
            tab.prefs.setMe(sidebarMe(automatic));
        }

        await a.prefs.setStatus(hidden);
        await after(500);
        a.close();
        expect(shown(t2)).toEqual(pending(hidden));
        await after(2499);
        expect(b.calls).toHaveLength(0);
        await after(1);

        expect(b.calls.map(call => call.tab)).toEqual([t2.n]);
        b.calls[0].answer.resolve(landed);
        await after(0);
        // the lock may reach C before B's post does, and C then sends what the server already holds
        b.calls[1]?.answer.resolve(alreadyHeld);
        await after(0);

        expect(b.calls.filter(call => call.tab === t2.n)).toHaveLength(1);
        expect(shown(t2)).toEqual(settled(hidden));
        expect(shown(c)).toEqual(settled(hidden));
    });

    test("A closes holding the lock with its call out and no saved ever comes: B sends it", async () => {
        const b = statusBrowser();
        const a = b.open();
        const t2 = b.open();

        await a.prefs.setStatus(hidden);
        await after(1000);
        expect(b.calls.map(c => c.tab)).toEqual([a.n]);
        await after(500);
        a.close();
        await after(1500);

        expect(b.calls.map(c => [c.tab, c.change])).toEqual([[a.n, { status: "appear_offline" }], [t2.n, { status: "appear_offline" }]]);
    });

    test("three live tabs, one choice: one call, the two others' copies dropped by the saved post before they ask", async () => {
        const b = statusBrowser();
        const tabs = [b.open(), b.open(), b.open()];

        await tabs[0].prefs.setStatus(busy);
        await after(1000);
        b.calls[0].answer.resolve(landed);
        await after(60_000);

        expect(b.calls).toHaveLength(1);
        expect(tabs.map(shown)).toEqual([settled(busy), settled(busy), settled(busy)]);
    });

    test("a tab that heard a choice shows it pending until a covering saved, and a sidebar me meanwhile does not settle it", async () => {
        const b = statusBrowser();
        const a = b.open();
        const t2 = b.open();
        t2.prefs.setMe(sidebarMe(automatic));

        await a.prefs.setStatus(busy);
        await after(0);
        expect(shown(t2)).toEqual(pending(busy));
        t2.prefs.setMe(sidebarMe(automatic));
        expect(shown(t2)).toEqual(pending(busy));

        await after(1000);
        b.calls[0].answer.resolve(landed);
        await after(0);
        expect(shown(t2)).toEqual(settled(busy));
    });

    test("a tab holding a saved before its record does not queue it", async () => {
        const b = statusBrowser();
        const t2 = b.open();
        const raw = b.raw();
        const at = Date.now();

        raw.postMessage({ kind: "saved", choiceAt: at, me: hidden });
        raw.postMessage({ kind: "choice", record: { choice: hidden, at, page: "another tab" } });
        await after(60_000);

        expect(b.calls).toHaveLength(0);
        expect(shown(t2)).toEqual(settled(hidden));
    });

    describe("without Web Locks", () => {
        test("A chooses Appear offline and closes inside its pause: B never shows it, and nothing is sent", async () => {
            const b = statusBrowser({ hasLocks: false });
            const a = b.open();
            const t2 = b.open();
            t2.prefs.setMe(sidebarMe(automatic));

            await a.prefs.setStatus(hidden);
            await after(0);
            expect(shown(t2)).toEqual(settled(automatic));
            await after(500);
            a.close();
            await after(60_000);

            expect(b.calls).toHaveLength(0);
            expect(shown(t2)).toEqual(settled(automatic));
        });

        test("with A's save landed, B settles on it; no hello is posted", async () => {
            const b = statusBrowser({ hasLocks: false });
            const a = b.open();
            const t2 = b.open();
            t2.prefs.setMe(sidebarMe(automatic));

            await a.prefs.setStatus(hidden);
            await after(1000);
            b.calls[0].answer.resolve(landed);
            await after(0);

            expect(shown(t2)).toEqual(settled(hidden));
            expect(kinds(b)).not.toContain("hello");
        });

        test("each page sends its own record in its own flight", async () => {
            const b = statusBrowser({ hasLocks: false });
            const a = b.open();
            const t2 = b.open();

            await a.prefs.setStatus(busy);
            await after(200);
            await t2.prefs.setStatus(hidden);
            await after(1000);

            expect(b.calls.map(c => [c.tab, c.change, c.at])).toEqual([[a.n, { status: "busy" }, 1000], [t2.n, { status: "appear_offline" }, 1200]]);
        });
    });

    describe("a tab with no token yet", () => {
        test("Appear offline chosen offline, a new tab opened with no token, the first tab closed: sent once the new tab's token arrives", async () => {
            const b = statusBrowser();
            const a = b.open();
            await a.prefs.setStatus(hidden);
            await after(1000);
            b.calls[0].answer.resolve(await statusCallResult(statusSaveFailures.network));
            await after(0);

            const c = b.open();
            c.person.value = null;
            await delivered();
            a.close();
            await after(30_000);
            expect(b.calls).toHaveLength(1);
            expect(shown(c)).toEqual(pending(hidden));

            c.person.value = her.toLowerCase();
            c.prefs.statusWake();
            await after(0);
            expect(b.calls.map(call => [call.tab, call.change])).toEqual([[a.n, { status: "appear_offline" }], [c.n, { status: "appear_offline" }]]);
            b.calls[1].answer.resolve(landed);
            await after(60_000);

            expect(b.calls).toHaveLength(2);
            expect(shown(c)).toEqual(settled(hidden));
        });

        test("a browser that refuses the lock to the page: the choice is sent all the same", async () => {
            const b = statusBrowser({ locksRefuse: true });
            const a = b.open();
            await a.prefs.setStatus(busy);
            await after(1000);

            expect(b.calls.map(call => call.change)).toEqual([{ status: "busy" }]);
        });
    });

    describe("a save the server already held", () => {
        test("A's Appear offline reaches the server and A closes before its answer: B's resend answered changed false settles B and C on it", async () => {
            const b = statusBrowser();
            const a = b.open();
            const t2 = b.open();
            const c = b.open();
            for (const tab of [a, t2, c]) {
                tab.prefs.setMe(sidebarMe(automatic));
            }

            await a.prefs.setStatus(hidden);
            await after(1500);
            a.close();
            await after(1500);
            expect(b.calls.map(call => call.tab)).toEqual([a.n, t2.n]);
            b.calls[1].answer.resolve(alreadyHeld);
            await after(0);
            b.calls[2]?.answer.resolve(alreadyHeld);
            await after(0);

            expect(shown(t2)).toEqual(settled(hidden));
            expect(shown(c)).toEqual(settled(hidden));
        });

        test("Appear offline chosen on her phone first, then here, answered changed false: it stays", async () => {
            const b = statusBrowser();
            const a = b.open();
            a.prefs.setMe(sidebarMe(hidden));

            await a.prefs.setStatus(hidden);
            await after(1000);
            b.calls[0].answer.resolve(alreadyHeld);
            await after(0);

            expect(shown(a)).toEqual(settled(hidden));
        });

        test("a save its deadline ended that had landed, sent again and answered changed false, settles every tab on the choice", async () => {
            const b = statusBrowser();
            const a = b.open();
            const t2 = b.open();
            for (const tab of [a, t2]) {
                tab.prefs.setMe(sidebarMe(automatic));
            }

            await a.prefs.setStatus(hidden);
            await after(1000);
            await after(15_000);
            expect(b.calls[0].signal.aborted).toBe(true);
            expect(shown(t2)).toEqual(pending(hidden));
            // the other tab's copy was waiting on the lock, and goes as the deadline lets it go
            expect(b.calls.map(call => [call.tab, call.at])).toEqual([[a.n, 1000], [t2.n, 16_000]]);
            b.calls[1].answer.resolve(alreadyHeld);
            await after(0);

            expect(shown(a)).toEqual(settled(hidden));
            expect(shown(t2)).toEqual(settled(hidden));
        });
    });

    describe("her settings' status has one owner", () => {
        test("her phone's choice loaded by a sidebar read, then another tab's refusal of an older record: the tab keeps the phone's value", async () => {
            const b = statusBrowser();
            const a = b.open();
            const t2 = b.open();
            for (const tab of [a, t2]) {
                tab.prefs.setMe(sidebarMe(automatic));
            }

            await a.prefs.setStatus(busy);
            await after(1000);
            t2.prefs.setMe(sidebarMe(hidden));
            b.calls[0].answer.resolve(await statusCallResult(statusSaveFailures.badRequest));
            await after(0);

            expect(shown(t2)).toEqual(settled(hidden));
            expect(shown(a)).toEqual(settled(automatic));
        });

        test("C takes a pending Busy from a hello, its sidebar loads Appear offline, the Busy is refused: C shows Appear offline", async () => {
            const b = statusBrowser();
            const a = b.open();
            // each wait at the middle of its range, once A's id is drawn
            jest.spyOn(Math, "random").mockReturnValue(0.5);
            a.prefs.setMe(sidebarMe(automatic));
            await a.prefs.setStatus(busy);
            await after(1000);
            b.calls[0].answer.resolve(await statusCallResult(statusSaveFailures.maintenance));

            const c = b.open();
            await delivered();
            expect(shown(c)).toEqual(pending(busy));
            c.prefs.setMe(sidebarMe(hidden));
            // A asks again at the end of its wait, 10 s after the failure
            await after(9999);
            expect(b.calls.map(call => call.tab)).toEqual([a.n, a.n]);
            b.calls[1].answer.resolve(await statusCallResult(statusSaveFailures.badRequest));
            await after(0);

            expect(shown(c)).toEqual(settled(hidden));
        });
    });

    describe("the hello", () => {
        test("C opens while A's Appear offline is pending, its save failing 503: C shows it and settles on the saved", async () => {
            const b = statusBrowser();
            const a = b.open();
            // each wait at the middle of its range, once A's id is drawn
            jest.spyOn(Math, "random").mockReturnValue(0.5);
            a.prefs.setMe(sidebarMe(automatic));
            await a.prefs.setStatus(hidden);
            await after(1000);
            b.calls[0].answer.resolve(await statusCallResult(statusSaveFailures.maintenance));
            await after(1000);

            const c = b.open();
            c.prefs.setMe(sidebarMe(automatic));
            await delivered();
            expect(shown(c)).toEqual(pending(hidden));

            // A asks again at the end of its wait, 10 s after the failure
            await after(8999);
            expect(b.calls.map(call => [call.tab, call.at])).toEqual([[a.n, 1000], [a.n, 11_000]]);
            b.calls[1].answer.resolve(landed);
            await after(0);
            expect(shown(c)).toEqual(settled(hidden));
            expect(shown(a)).toEqual(settled(hidden));
        });

        test("with nothing pending a hello is answered with what is covered, her saved status and the retry clock, and no record", async () => {
            const b = statusBrowser();
            const a = b.open();
            a.prefs.setMe(sidebarMe(busy));

            const c = b.open();
            await delivered();

            const answers = b.posts.map(p => p.data as Record<string, unknown>).filter(d => d.kind !== "hello");
            expect(answers).toEqual([{ kind: "held", covered: 0, me: busy, meStamp: 0, retryAt: 0, failures: 0, retryPage: null }]);
            expect(shown(c)).toEqual(settled(busy));
        });

        test("a hello after stop is answered with nothing", async () => {
            const b = statusBrowser();
            const a = b.open();
            a.prefs.setMe(sidebarMe(busy));
            a.prefs.stop();

            b.open();
            await after(0);

            expect(kinds(b)).toEqual(["hello", "hello"]);
        });
    });

    test("Appear offline answered read only: no tab shows it, every tab shows its own, the choosing tab alone says why in the banner's words, and nothing is asked again", async () => {
        const b = statusBrowser();
        const a = b.open();
        const t2 = b.open();
        a.prefs.setMe(sidebarMe(automatic));
        t2.prefs.setMe(sidebarMe(busy));

        await a.prefs.setStatus(hidden);
        await after(1000);
        b.calls[0].answer.resolve(await statusCallResult(statusSaveFailures.readOnlyBanner));
        await after(60_000);

        expect(shown(a)).toEqual(settled(automatic));
        expect(shown(t2)).toEqual(settled(busy));
        expect(a.toasts.map(t => t.text)).toEqual(["Chat is read only tonight while Cedar Hill moves to its new server."]);
        expect(t2.toasts).toEqual([]);
        expect(b.calls).toHaveLength(1);
    });

    describe("a refusal another tab sent", () => {
        /** B's choice, sent by A: B's session names another person when the lock reaches it. */
        async function sentByA(choice: StatusChoice): Promise<{ b: StatusBrowser, a: StatusTab, t2: StatusTab, c: StatusTab }> {
            const b = statusBrowser();
            const a = b.open();
            const t2 = b.open();
            const c = b.open();
            t2.person.value = someoneElse;
            await t2.prefs.setStatus(choice);
            await after(1000);
            t2.person.value = her.toLowerCase();
            await after(2000);
            expect(b.calls.map(call => call.tab)).toEqual([a.n]);
            return { b, a, t2, c };
        }

        test("read only with a church banner: B says it and A does not, and B alone takes read only", async () => {
            const { b, a, t2, c } = await sentByA(hidden);
            const refusal = await statusCallResult(statusSaveFailures.readOnlyBanner);
            b.calls[0].answer.resolve(refusal);
            await after(0);
            // C's copy may reach the lock before A's post reaches C, and meets the same answer
            b.calls[1]?.answer.resolve(refusal);
            await after(0);

            expect(t2.toasts.map(t => t.text)).toEqual(["Chat is read only tonight while Cedar Hill moves to its new server."]);
            expect(a.toasts).toEqual([]);
            expect(c.toasts).toEqual([]);
            expect(t2.refusals).toEqual([(refusal as { error: ChatError }).error]);
            expect(a.refusals).toEqual([]);
            expect(c.refusals).toEqual([]);
        });

        test("a bad request with the server's sentence: B says the sentence and no tab takes read only", async () => {
            const { b, a, t2, c } = await sentByA(busy);
            const refusal = await statusCallResult(statusSaveFailures.badRequest);
            b.calls[0].answer.resolve(refusal);
            await after(0);
            b.calls[1]?.answer.resolve(refusal);
            await after(0);

            expect(t2.toasts.map(t => t.text)).toEqual(["Chat could not understand that request."]);
            expect(a.toasts).toEqual([]);
            expect([a, t2, c].flatMap(tab => tab.refusals)).toEqual([]);
        });

        test("B makes r1, A sends it, B makes r2 during A's flight, r1 refused read only: B takes read only once and says nothing for r1; r2's refusal is said once", async () => {
            const { b, t2 } = await sentByA(busy);
            await after(100);
            await t2.prefs.setStatus(hidden);
            const r1Refused = await statusCallResult(statusSaveFailures.readOnlyBanner);
            b.calls[0].answer.resolve(r1Refused);
            await delivered();
            // C's copy of r1 may reach the lock before A's post reaches C, and meets the same answer
            b.calls[1]?.answer.resolve(r1Refused);
            await delivered();

            expect(t2.refusals).toHaveLength(1);
            expect(t2.toasts).toEqual([]);
            await after(999);
            const r2 = b.calls.filter(call => call.change.status === "appear_offline");
            expect(r2).toHaveLength(1);
            r2[0].answer.resolve(await statusCallResult(statusSaveFailures.badRequest));
            await after(60_000);

            expect(t2.refusals).toHaveLength(1);
            expect(t2.toasts.map(t => t.text)).toEqual(["Chat could not understand that request."]);
        });
    });

    describe("the record's time", () => {
        test("Busy at 0 sent at 1.0, Automatic chosen in B at 1.2, Busy lands at 1.4, Automatic lands at 2.6: every tab shows Automatic throughout", async () => {
            const b = statusBrowser();
            const tabs = [b.open(), b.open(), b.open()];
            for (const tab of tabs) {
                tab.prefs.setMe(sidebarMe(automatic));
            }

            await tabs[0].prefs.setStatus(busy);
            await after(1200);
            await tabs[1].prefs.setStatus(automatic);
            await after(0);
            expect(tabs.map(t => shown(t).status)).toEqual([automatic, automatic, automatic]);
            await after(200);
            b.calls[0].answer.resolve(landed);
            await after(0);
            expect(tabs.map(shown)).toEqual([pending(automatic), pending(automatic), pending(automatic)]);

            await after(800);
            expect(b.calls.map(call => [call.tab, call.change])).toEqual([[tabs[0].n, { status: "busy" }], [tabs[1].n, { status: "auto" }]]);
            await after(400);
            b.calls[1].answer.resolve({ ok: true, data: { status: "auto" } });
            await after(0);
            expect(tabs.map(shown)).toEqual([settled(automatic), settled(automatic), settled(automatic)]);
        });

        test("Busy saved here, Automatic set on her phone and loaded, Busy chosen here again: it is sent", async () => {
            const b = statusBrowser();
            const a = b.open();
            a.prefs.setMe(sidebarMe(automatic));
            await a.prefs.setStatus(busy);
            await after(1000);
            b.calls[0].answer.resolve(landed);
            await after(0);

            a.prefs.setMe(sidebarMe(automatic));
            await a.prefs.setStatus(busy);
            await after(1000);

            expect(b.calls.map(call => call.change)).toEqual([{ status: "busy" }, { status: "busy" }]);
        });
    });

    describe("failures across tabs", () => {
        test.each([
            ["no answer", "network"],
            ["a token the refresh could not renew", "expired"],
            ["a proxy giving up", "proxyTimeout"],
            ["maintenance", "maintenance"]
        ] as const)("%s leaves the choice shown and pending in every tab, with no toast", async (_name, shape) => {
            const b = statusBrowser();
            const a = b.open();
            const t2 = b.open();
            for (const tab of [a, t2]) {
                tab.prefs.setMe(sidebarMe(automatic));
            }

            await a.prefs.setStatus(busy);
            await after(1000);
            b.calls[0].answer.resolve(await statusCallResult(statusSaveFailures[shape]));
            await after(0);

            expect([a, t2].map(shown)).toEqual([pending(busy), pending(busy)]);
            expect([...a.toasts, ...t2.toasts]).toEqual([]);
        });

        test("a hung save ends at its deadline, pending with no toast, and the next tab's copy goes", async () => {
            const b = statusBrowser();
            const a = b.open();
            const t2 = b.open();

            await a.prefs.setStatus(busy);
            await after(1000);
            await after(14_999);
            expect(b.calls).toHaveLength(1);
            await after(1);

            expect(b.calls[0].signal.aborted).toBe(true);
            expect(b.calls.map(call => [call.tab, call.at])).toEqual([[a.n, 1000], [t2.n, 16_000]]);
            expect([a, t2].map(shown)).toEqual([pending(busy), pending(busy)]);
            expect([...a.toasts, ...t2.toasts]).toEqual([]);
        });

        test("a refusal the server answered returns every tab to its own, with no request, and the choosing tab alone says why", async () => {
            const b = statusBrowser();
            const a = b.open();
            const t2 = b.open();
            a.prefs.setMe(sidebarMe(automatic));
            t2.prefs.setMe(sidebarMe(automatic));

            await a.prefs.setStatus(busy);
            await after(1000);
            b.calls[0].answer.resolve(await statusCallResult(statusSaveFailures.badRequest));
            await after(60_000);

            expect([a, t2].map(shown)).toEqual([settled(automatic), settled(automatic)]);
            expect(a.toasts.map(t => t.text)).toEqual(["Chat could not understand that request."]);
            expect(t2.toasts).toEqual([]);
            expect(b.calls).toHaveLength(1);
        });

        test("a refusal of an older record leaves a newer one pending, and says nothing", async () => {
            const b = statusBrowser();
            const a = b.open();
            const t2 = b.open();
            a.prefs.setMe(sidebarMe(automatic));
            t2.prefs.setMe(sidebarMe(automatic));

            await a.prefs.setStatus(busy);
            await after(1200);
            await a.prefs.setStatus(hidden);
            b.calls[0].answer.resolve(await statusCallResult(statusSaveFailures.badRequest));
            await after(0);

            expect([a, t2].map(shown)).toEqual([pending(hidden), pending(hidden)]);
            expect(a.toasts).toEqual([]);
        });
    });

    describe("a wake", () => {
        test.each([
            ["the network returning", (tab: StatusTab) => tab.fire("online", {})],
            ["the platform taking her writes again", (tab: StatusTab) => tab.prefs.statusWake()]
        ])("%s asks a save waiting out a failure at once", async (_name, wake) => {
            const b = statusBrowser();
            const a = b.open();
            await a.prefs.setStatus(hidden);
            await after(1000);
            b.calls[0].answer.resolve(await statusCallResult(statusSaveFailures.network));
            await after(3000);

            wake(a);
            await after(0);

            expect(b.calls.map(call => call.at)).toEqual([1000, 4000]);
        });

        test("during a try, the try's failure answered after it is asked again at once", async () => {
            const b = statusBrowser();
            const a = b.open();
            await a.prefs.setStatus(hidden);
            await after(1000);

            a.fire("online", {});
            b.calls[0].answer.resolve(await statusCallResult(statusSaveFailures.network));
            await after(0);

            expect(b.calls.map(call => call.at)).toEqual([1000, 1000]);
        });

        test.each([
            ["a 401 after the refresh inside the call, which had a fresh token already", "expired"],
            ["a 429, which is a pace", "rateLimited"]
        ] as const)("during a try, %s is asked again on the backoff, not at once", async (_name, shape) => {
            const b = statusBrowser();
            const a = b.open();
            jest.spyOn(Math, "random").mockReturnValue(0.5);
            await a.prefs.setStatus(hidden);
            await after(1000);

            // the refresh inside the call landed ok, which wakes her waiting save
            a.prefs.statusWake();
            b.calls[0].answer.resolve(await statusCallResult(statusSaveFailures[shape]));
            await after(9999);
            expect(b.calls).toHaveLength(1);
            await after(1);

            expect(b.calls.map(call => call.at)).toEqual([1000, 11_000]);
        });

        test("with no failure waiting asks nothing, and a choice still waits out its pause", async () => {
            const b = statusBrowser();
            const a = b.open();
            a.prefs.statusWake();
            await a.prefs.setStatus(hidden);
            a.fire("online", {});
            await after(999);
            expect(b.calls).toHaveLength(0);
            await after(1);
            b.calls[0].answer.resolve(landed);
            await after(0);

            a.prefs.statusWake();
            await after(60_000);
            expect(b.calls).toHaveLength(1);
        });
    });

    describe("one retry clock for the browser", () => {
        const failing = async (): Promise<CallResult> => statusCallResult(statusSaveFailures.maintenance);
        const failedPosts = (b: StatusBrowser): Array<Record<string, unknown>> => b.posts.map(p => p.data as Record<string, unknown>).filter(d => d.kind === "failed");

        /** When the browser opened, which a post's retryAt is read from. */
        let zero = 0;
        const fromZero = (at: unknown): number => (at as number) - zero;

        /** Two tabs of hers, each wait at the middle of its range once their ids are drawn, and A's Appear offline failed at 1 s: asked again at 11 s. */
        async function failedOnce(): Promise<{ b: StatusBrowser, a: StatusTab, t2: StatusTab }> {
            zero = Date.now();
            const b = statusBrowser();
            const a = b.open();
            const t2 = b.open();
            jest.spyOn(Math, "random").mockReturnValue(0.5);
            await a.prefs.setStatus(hidden);
            await after(1000);
            b.calls[0].answer.resolve(await failing());
            await after(0);
            return { b, a, t2 };
        }

        test("two tabs and three 503s: one send for each wait, at the backoff's times, from the tab that failed", async () => {
            const { b, a, t2 } = await failedOnce();
            await after(10_000);
            b.calls[1].answer.resolve(await failing());
            await after(20_000);
            b.calls[2].answer.resolve(await failing());
            await after(40_000);
            b.calls[3].answer.resolve(landed);
            await after(60_000);

            expect(b.calls.map(call => [call.tab, call.at])).toEqual([[a.n, 1000], [a.n, 11_000], [a.n, 31_000], [a.n, 71_000]]);
            // the other tab's copy reached the lock early and let it go, posting nothing
            expect(failedPosts(b).map(post => [fromZero(post.retryAt), post.failures])).toEqual([[11_000, 1], [31_000, 2], [71_000, 3]]);
            expect([a, t2].map(shown)).toEqual([settled(hidden), settled(hidden)]);
        });

        test("the tab that failed closes during the wait: the other sends at the wait's end plus the hand-off, its count carried on", async () => {
            const { b, a, t2 } = await failedOnce();
            await after(4000);
            a.close();
            await after(7999);
            expect(b.calls).toHaveLength(1);
            await after(1);
            expect(b.calls.map(call => [call.tab, call.at])).toEqual([[a.n, 1000], [t2.n, 13_000]]);

            b.calls[1].answer.resolve(await failing());
            await after(0);
            // the second failure of the browser's, so its wait is 20 s
            expect(failedPosts(b).map(post => [fromZero(post.retryAt), post.failures, post.page === failedPosts(b)[0].page])).toEqual([[11_000, 1, true], [33_000, 2, false]]);
        });

        test("the network returning during the wait, heard by both tabs: one send, from the tab that failed", async () => {
            const { b, a, t2 } = await failedOnce();
            await after(4000);
            a.fire("online", {});
            t2.fire("online", {});
            await after(0);
            b.calls[1].answer.resolve(landed);
            await after(60_000);

            expect(b.calls.map(call => [call.tab, call.at])).toEqual([[a.n, 1000], [a.n, 5000]]);
            expect([a, t2].map(shown)).toEqual([settled(hidden), settled(hidden)]);
        });

        test("the network returning after the tab that failed closed: the other sends a hand-off later", async () => {
            const { b, a, t2 } = await failedOnce();
            await after(3000);
            a.close();
            await after(1000);
            t2.fire("online", {});
            await after(1999);
            expect(b.calls).toHaveLength(1);
            await after(1);

            expect(b.calls.map(call => [call.tab, call.at])).toEqual([[a.n, 1000], [t2.n, 7000]]);
        });

        test("a tab opened during the wait takes the clock from its hello and does not send at its 3 s due", async () => {
            const b = statusBrowser();
            const a = b.open();
            jest.spyOn(Math, "random").mockReturnValue(0.5);
            await a.prefs.setStatus(hidden);
            await after(1000);
            b.calls[0].answer.resolve(await failing());
            await after(1000);

            const c = b.open();
            await delivered();
            a.close();
            await after(10_998);
            expect(b.calls).toHaveLength(1);
            await after(1);
            expect(b.calls.map(call => [call.tab, call.at])).toEqual([[a.n, 1000], [c.n, 13_000]]);

            // the hello carried the count, so C's failure is the browser's second, waited 20 s
            b.calls[1].answer.resolve(await failing());
            await after(20_000);
            expect(b.calls.map(call => [call.tab, call.at])).toEqual([[a.n, 1000], [c.n, 13_000], [c.n, 33_000]]);
        });

        test("a failed post for a choice already saved starts no wait", async () => {
            const b = statusBrowser();
            const a = b.open();
            await a.prefs.setStatus(busy);
            await after(1000);
            b.calls[0].answer.resolve(landed);
            await after(0);
            const saved = b.calls[0];
            const choiceAt = (b.posts.map(p => p.data as Record<string, unknown>).find(d => d.kind === "saved")?.choiceAt) as number;

            b.raw().postMessage({ kind: "failed", choiceAt, retryAt: Date.now() + 60_000, failures: 3, page: "another tab" });
            await delivered();
            await a.prefs.setStatus(hidden);
            await after(1000);

            expect(b.calls.filter(call => call !== saved).map(call => call.change)).toEqual([{ status: "appear_offline" }]);
        });

        test("a saved ends the run: the next choice's first failure waits 10 s again", async () => {
            const { b, a } = await failedOnce();
            await after(10_000);
            b.calls[1].answer.resolve(landed);
            await after(0);

            await a.prefs.setStatus(busy);
            await after(1000);
            b.calls[2].answer.resolve(await failing());
            await after(0);

            expect(failedPosts(b).map(post => [fromZero(post.retryAt), post.failures])).toEqual([[11_000, 1], [22_000, 1]]);
        });

        test("a failed post naming an earlier time than the clock holds leaves the later", async () => {
            zero = Date.now();
            const b = statusBrowser();
            const t2 = b.open();
            const raw = b.raw();
            const record = { choice: hidden, at: Date.now() + 1, page: "another tab" };
            raw.postMessage({ kind: "choice", record });
            raw.postMessage({ kind: "failed", choiceAt: record.at, retryAt: zero + 60_000, failures: 2, page: "another tab" });
            raw.postMessage({ kind: "failed", choiceAt: record.at, retryAt: zero + 5000, failures: 1, page: "another tab" });
            await after(61_999);
            expect(b.calls).toHaveLength(0);
            await after(1);

            expect(b.calls.map(call => [call.tab, fromZero(zero + call.at)])).toEqual([[t2.n, 62_000]]);
        });

        test("once stopped, a failed post, the retry's wait and the hand-off send nothing", async () => {
            const { b, a, t2 } = await failedOnce();
            t2.prefs.stop();
            a.close();
            b.raw().postMessage({ kind: "failed", choiceAt: Date.now(), retryAt: Date.now(), failures: 1, page: "another tab" });
            await after(120_000);

            expect(b.calls).toHaveLength(1);
        });
    });

    test.each([
        ["a saved with no choice time", { kind: "saved", me: busy }],
        ["a saved whose choice time is not a number", { kind: "saved", choiceAt: "soon", me: busy }],
        ["a saved whose value is not her status", { kind: "saved", choiceAt: 5, me: { status: "busy" } }],
        ["a refusal with no error", { kind: "saved", choiceAt: 5, page: "another tab", refused: true }],
        ["a choice with no record", { kind: "choice" }],
        ["a choice whose record has no time", { kind: "choice", record: { choice: hidden, page: "another tab" } }],
        ["a choice whose record's choice is not her status", { kind: "choice", record: { choice: "hidden", at: 5, page: "another tab" } }],
        ["a held with no covered", { kind: "held", me: busy, meStamp: 0, retryAt: 0, failures: 0, retryPage: null }],
        ["a failed whose time is not a number", { kind: "failed", choiceAt: 1e15, retryAt: null, failures: 1, page: "another tab" }],
        ["an input with no time", { kind: "input" }],
        ["a kind of a later release", { kind: "status2", choiceAt: 1e15 }]
    ])("%s from another release changes nothing, and her next choice is sent", async (_name, post) => {
        const b = statusBrowser();
        const t2 = b.open();
        t2.prefs.setMe(sidebarMe(automatic));

        b.raw().postMessage(post);
        await delivered();
        expect(shown(t2)).toEqual(settled(automatic));
        expect(t2.prefs.state.ownInputAt).toBe(0);
        await t2.prefs.setStatus(hidden);
        await after(1000);

        expect(b.calls.map(call => call.change)).toEqual([{ status: "appear_offline" }]);
    });

    test("a saved post older than what the tab has covered changes nothing", async () => {
        const b = statusBrowser();
        const t2 = b.open();
        const raw = b.raw();

        raw.postMessage({ kind: "saved", choiceAt: 200, me: busy });
        raw.postMessage({ kind: "saved", choiceAt: 100, me: hidden });
        await after(0);

        expect(shown(t2)).toEqual(settled(busy));
    });

    test("her input in another tab is kept, and an input post older than the newest is ignored", async () => {
        const b = statusBrowser();
        const t2 = b.open();
        const raw = b.raw();

        raw.postMessage({ kind: "input", at: 500 });
        raw.postMessage({ kind: "input", at: 400 });
        await after(0);

        expect(t2.prefs.state.ownInputAt).toBe(500);
    });

    test("her input is posted at once, then at most every 30 s with the newest", async () => {
        const b = statusBrowser();
        const a = b.open();
        const inputs = (): unknown[] => b.posts.map(p => p.data as Record<string, unknown>).filter(d => d.kind === "input").map(d => d.at);

        a.prefs.noteInput(1000);
        a.prefs.noteInput(2000);
        await after(10_000);
        a.prefs.noteInput(3000);
        expect(inputs()).toEqual([1000]);
        await after(20_000);

        expect(inputs()).toEqual([1000, 3000]);
    });

    test("once stopped, a channel message, a queued save, a lock granted and an input send nothing", async () => {
        const b = statusBrowser();
        const a = b.open();
        let free!: () => void;
        void b.locks.tab().request(lockName, {}, () => new Promise<void>(resolve => free = resolve));
        await a.prefs.setStatus(busy);
        await after(1000);
        a.prefs.stop();
        free();
        const raw = b.raw();
        raw.postMessage({ kind: "choice", record: { choice: hidden, at: Date.now() + 1, page: "another tab" } });
        raw.postMessage({ kind: "input", at: 9000 });
        const posts = b.posts.length;
        a.prefs.noteInput(10_000);
        await after(60_000);

        expect(b.calls).toHaveLength(0);
        expect(a.prefs.state.ownInputAt).toBe(0);
        expect(b.posts).toHaveLength(posts);
    });

    test("stop closes the tab's channel", () => {
        const b = statusBrowser();
        const a = b.open();
        b.open();
        expect(b.openChannels()).toBe(2);

        a.prefs.stop();

        expect(b.openChannels()).toBe(1);
    });

    describe("the back-forward cache", () => {
        test("Appear offline failing 503, then the tab leaves and comes back: it shows her saved status and sends nothing more", async () => {
            const b = statusBrowser();
            const a = b.open();
            a.prefs.setMe(sidebarMe(automatic));
            await a.prefs.setStatus(hidden);
            await after(1000);
            b.calls[0].answer.resolve(await statusCallResult(statusSaveFailures.maintenance));
            await after(0);

            a.fire("pagehide", { persisted: true });
            expect(shown(a)).toEqual(settled(automatic));
            await after(5000);
            a.fire("pageshow", { persisted: true });
            await after(120_000);

            expect(shown(a)).toEqual(settled(automatic));
            expect(b.calls).toHaveLength(1);
        });

        test("the call out as the tab leaves is aborted, and its status 0 asks nothing again", async () => {
            const b = statusBrowser();
            const a = b.open();
            await a.prefs.setStatus(hidden);
            await after(1000);

            a.fire("pagehide", { persisted: true });
            await after(0);
            expect(b.calls[0].signal.aborted).toBe(true);
            await after(120_000);

            expect(b.calls).toHaveLength(1);
        });

        test("the holder's call out as it leaves is aborted at once: the lock goes free, the other tab's copy sends at its due, and no failed is posted", async () => {
            const b = statusBrowser();
            const a = b.open();
            const t2 = b.open();
            await a.prefs.setStatus(hidden);
            await after(1000);

            a.fire("pagehide", { persisted: true });
            await after(2000);

            expect(b.calls.map(call => [call.tab, call.at])).toEqual([[a.n, 1000], [t2.n, 3000]]);
            expect(kinds(b)).not.toContain("failed");
        });

        test("the holder waiting out a pause inside the lock as it leaves lets the lock go at once, and the other tab's copy sends", async () => {
            const b = statusBrowser();
            const a = b.open();
            const t2 = b.open();
            let free!: () => void;
            void b.locks.tab().request(lockName, {}, () => new Promise<void>(resolve => free = resolve));
            await a.prefs.setStatus(busy);
            await after(1100);
            await t2.prefs.setStatus(automatic);
            await after(100);
            free();
            await after(0);
            expect(b.locks.holder(lockName)).not.toBeNull();

            // a page in the back-forward cache runs no timer, so a wait left to its timer would hold the lock until it came back
            a.fire("pagehide", { persisted: true });
            await after(0);
            expect(b.locks.holder(lockName)).toBeNull();
            await after(900);

            expect(b.calls.map(call => [call.tab, call.change, call.at])).toEqual([[t2.n, { status: "auto" }, 2100]]);
        });

        test("a tab back from the cache forgot its wait: a choice it makes then goes at its pause", async () => {
            const b = statusBrowser();
            const a = b.open();
            jest.spyOn(Math, "random").mockReturnValue(0.5);
            await a.prefs.setStatus(hidden);
            await after(1000);
            b.calls[0].answer.resolve(await statusCallResult(statusSaveFailures.maintenance));
            await after(0);

            a.fire("pagehide", { persisted: true });
            a.fire("pageshow", { persisted: true });
            await a.prefs.setStatus(busy);
            await after(1000);

            expect(b.calls.map(call => [call.change, call.at])).toEqual([[{ status: "appear_offline" }, 1000], [{ status: "busy" }, 2000]]);
        });

        test("a page closed for good is no visit to the cache: a pagehide that does not persist leaves the choice to be sent", async () => {
            const b = statusBrowser();
            const a = b.open();
            await a.prefs.setStatus(hidden);

            a.fire("pagehide", { persisted: false });
            await after(1000);

            expect(b.calls).toHaveLength(1);
        });

        test("A chooses and leaves inside the pause, B sends it, she chooses again in B, A comes back: nothing sent, and A shows B's choice", async () => {
            const b = statusBrowser();
            const a = b.open();
            const t2 = b.open();
            for (const tab of [a, t2]) {
                tab.prefs.setMe(sidebarMe(automatic));
            }

            await a.prefs.setStatus(busy);
            await after(500);
            a.fire("pagehide", { persisted: true });
            await after(2500);
            expect(b.calls.map(call => [call.tab, call.change])).toEqual([[t2.n, { status: "busy" }]]);
            b.calls[0].answer.resolve(landed);
            await after(0);
            await t2.prefs.setStatus(hidden);
            await after(1000);
            b.calls[1].answer.resolve(landed);
            await after(0);

            a.fire("pageshow", { persisted: true });
            await after(60_000);

            expect(b.calls).toHaveLength(2);
            expect(shown(a)).toEqual(settled(hidden));
        });
    });
});

// #endregion Her status across her tabs
